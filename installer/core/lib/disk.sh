#!/usr/bin/env bash
######################################
# LinuxHub Installer - Disk provisioning
######################################
# Responsabilidade única: particionamento definitivo (replace/dual-boot) e
# criação dos filesystems. Toda operação destrutiva do pipeline vive aqui —
# nada disso roda no lado Windows (design.md D1). Nunca chame setup_replace/
# setup_dualboot sem antes ter revalidado o plano (spec linux-install-payload
# — "Revalidar o plano antes do ponto de não-retorno").
#
# Requer as variáveis do install.conf já carregadas: TARGET_DISK_INDEX,
# BOOT_MODE, INSTALL_MODE, e (modo dual-boot) TARGET_PARTITION_INDEX,
# LINUX_PARTITION_SIZE_GB. SWAP_SIZE_MB precisa estar setado por quem chama
# (install.sh) antes de invocar setup_replace/setup_dualboot.
#
# Exporta para os scripts seguintes (mount.sh etc.): TARGET_DISK,
# ROOT_PARTITION, SWAP_PARTITION, ESP_PARTITION (vazio se BIOS legado).

# Resolve TARGET_DISK_INDEX (índice do Win32_DiskDrive gravado pelo lado
# Windows) para um device Linux (/dev/sdX, /dev/nvmeXn1, ...), assumindo que
# a ordem de enumeração dos discos é a mesma nos dois lados. Essa suposição
# não é garantida em 100% do hardware — é uma limitação conhecida (ver
# design.md), mitigada por revalidate_plan() abortar se o device resolvido
# não existir ou não bater com o esperado, em vez de seguir com um disco
# errado.
resolve_target_disk_device() {
    local index="$1"
    local devices=()
    mapfile -t devices < <(lsblk -dno PATH,TYPE | awk '$2 == "disk" {print $1}' | sort)

    if (( index < 0 || index >= ${#devices[@]} )); then
        fatal "Disco de índice $index não encontrado (${#devices[@]} disco(s) detectado(s))."
    fi

    echo "${devices[$index]}"
}

# Caminho de device de uma partição de um disco (ex.: /dev/sda + 2 ->
# /dev/sda2; /dev/nvme0n1 + 2 -> /dev/nvme0n1p2).
partition_device() {
    local disk="$1" index="$2"
    if [[ "$disk" =~ (nvme|mmcblk) ]]; then
        echo "${disk}p${index}"
    else
        echo "${disk}${index}"
    fi
}

# Compara o plano gravado em install.conf contra o hardware observado antes
# de qualquer operação destrutiva — spec "Revalidar o plano antes do ponto
# de não-retorno". Define TARGET_DISK como efeito colateral (necessário
# antes de setup_replace/setup_dualboot).
revalidate_plan() {
    TARGET_DISK="$(resolve_target_disk_device "$TARGET_DISK_INDEX")"

    [[ -b "$TARGET_DISK" ]] \
        || fatal "Disco alvo '$TARGET_DISK' (índice $TARGET_DISK_INDEX) não existe mais — abortando antes de particionar."

    if [[ "$INSTALL_MODE" == "dualboot" ]]; then
        [[ -z "${TARGET_PARTITION_INDEX:-}" ]] && fatal "TARGET_PARTITION_INDEX não definido em modo dual-boot."

        local target_partition
        target_partition="$(partition_device "$TARGET_DISK" "$TARGET_PARTITION_INDEX")"
        [[ -b "$target_partition" ]] \
            || fatal "Partição alvo '$target_partition' não existe mais — abortando antes de particionar."
    fi

    log "Plano revalidado: disco alvo $TARGET_DISK (modo $INSTALL_MODE) confere com o hardware observado."
}

# Modo substituir: apaga a tabela de partição existente e cria uma nova,
# compatível com o firmware detectado. GPT em ambos os casos (evita o limite
# de 2TB do MBR clássico) — BIOS legado usa uma partição bios_grub em vez de
# ESP, conforme o boot-staging (design.md D4).
setup_replace() {
    revalidate_plan

    log "Apagando tabela de partição existente de $TARGET_DISK..."
    wipefs --all "$TARGET_DISK" || fatal "Falha ao limpar assinaturas de partição em $TARGET_DISK."
    parted -s "$TARGET_DISK" mklabel gpt || fatal "Falha ao criar tabela GPT em $TARGET_DISK."

    local swap_mib="${SWAP_SIZE_MB:-0}"

    if [[ "$BOOT_MODE" == "uefi" ]]; then
        local esp_end=513
        local swap_end=$(( esp_end + swap_mib ))

        parted -s "$TARGET_DISK" mkpart ESP fat32 1MiB "${esp_end}MiB" \
            || fatal "Falha ao criar a ESP."
        parted -s "$TARGET_DISK" set 1 esp on
        parted -s "$TARGET_DISK" mkpart primary linux-swap "${esp_end}MiB" "${swap_end}MiB" \
            || fatal "Falha ao criar a partição de swap."
        parted -s "$TARGET_DISK" mkpart primary ext4 "${swap_end}MiB" 100% \
            || fatal "Falha ao criar a partição raiz."

        ESP_PARTITION="$(partition_device "$TARGET_DISK" 1)"
        SWAP_PARTITION="$(partition_device "$TARGET_DISK" 2)"
        ROOT_PARTITION="$(partition_device "$TARGET_DISK" 3)"
    else
        local grub_end=2
        local swap_end=$(( grub_end + swap_mib ))

        parted -s "$TARGET_DISK" mkpart bios_grub 1MiB "${grub_end}MiB" \
            || fatal "Falha ao criar a partição bios_grub."
        parted -s "$TARGET_DISK" set 1 bios_grub on
        parted -s "$TARGET_DISK" mkpart primary linux-swap "${grub_end}MiB" "${swap_end}MiB" \
            || fatal "Falha ao criar a partição de swap."
        parted -s "$TARGET_DISK" mkpart primary ext4 "${swap_end}MiB" 100% \
            || fatal "Falha ao criar a partição raiz."

        ESP_PARTITION=""
        SWAP_PARTITION="$(partition_device "$TARGET_DISK" 2)"
        ROOT_PARTITION="$(partition_device "$TARGET_DISK" 3)"
    fi

    partprobe "$TARGET_DISK"
    format_new_partitions
    log "Particionamento (replace) concluído: raiz=$ROOT_PARTITION swap=${SWAP_PARTITION:-N/A} esp=${ESP_PARTITION:-N/A}"
}

# Modo dual-boot: cria a partição Linux só no espaço não alocado deixado
# pelo shrink executado no lado Windows — nunca apaga nem redimensiona
# nenhuma partição existente.
setup_dualboot() {
    revalidate_plan

    local size_gb="${LINUX_PARTITION_SIZE_GB:?LINUX_PARTITION_SIZE_GB não definido}"
    local swap_mib="${SWAP_SIZE_MB:-0}"
    local requested_mib=$(( size_gb * 1024 ))

    if (( requested_mib <= swap_mib )); then
        fatal "LINUX_PARTITION_SIZE_GB ($size_gb GB) pequeno demais para caber raiz + swap (${swap_mib}MiB)."
    fi

    # A única região "livre" na tabela de partição existente é o espaço
    # deixado pelo shrink do Windows-side — nunca há mais de uma nesse
    # fluxo, já que o Windows só encolhe uma partição por instalação.
    local free_line
    free_line="$(parted -sm "$TARGET_DISK" unit MiB print free | grep ':free;' | tail -n 1)"
    [[ -z "$free_line" ]] && fatal "Nenhum espaço não alocado encontrado em $TARGET_DISK — o shrink do Windows não deixou espaço livre?"

    local free_start
    free_start="$(echo "$free_line" | cut -d: -f2 | tr -d 'MiB')"
    local free_size
    free_size="$(echo "$free_line" | cut -d: -f4 | tr -d 'MiB')"

    if (( $(printf '%.0f' "$requested_mib") > $(printf '%.0f' "$free_size") )); then
        fatal "Espaço não alocado (${free_size}MiB) menor que o solicitado (${requested_mib}MiB)."
    fi

    local swap_end root_end
    swap_end="$(awk -v s="$free_start" -v m="$swap_mib" 'BEGIN { printf "%.0f", s + m }')"
    root_end="$(awk -v s="$free_start" -v m="$requested_mib" 'BEGIN { printf "%.0f", s + m }')"

    log "Criando partições Linux em $TARGET_DISK: swap ${free_start}-${swap_end}MiB, raiz ${swap_end}-${root_end}MiB."

    if (( swap_mib > 0 )); then
        parted -s "$TARGET_DISK" mkpart primary linux-swap "${free_start}MiB" "${swap_end}MiB" \
            || fatal "Falha ao criar a partição de swap no espaço não alocado."
    fi
    parted -s "$TARGET_DISK" mkpart primary ext4 "${swap_end}MiB" "${root_end}MiB" \
        || fatal "Falha ao criar a partição raiz no espaço não alocado."

    partprobe "$TARGET_DISK"

    local partition_count
    partition_count="$(lsblk -no NAME "$TARGET_DISK" | tail -n +2 | wc -l)"
    if (( swap_mib > 0 )); then
        SWAP_PARTITION="$(partition_device "$TARGET_DISK" "$((partition_count - 1))")"
    else
        SWAP_PARTITION=""
    fi
    ROOT_PARTITION="$(partition_device "$TARGET_DISK" "$partition_count")"
    ESP_PARTITION=""

    format_new_partitions
    log "Particionamento (dual-boot) concluído: raiz=$ROOT_PARTITION swap=${SWAP_PARTITION:-N/A}"
}

format_new_partitions() {
    log "Formatando as novas partições..."

    mkfs.ext4 -F "$ROOT_PARTITION" || fatal "Falha ao formatar a partição raiz $ROOT_PARTITION."

    if [[ -n "${SWAP_PARTITION:-}" ]]; then
        mkswap "$SWAP_PARTITION" || fatal "Falha ao formatar a partição de swap $SWAP_PARTITION."
    fi

    if [[ -n "${ESP_PARTITION:-}" ]]; then
        mkfs.fat -F32 "$ESP_PARTITION" || fatal "Falha ao formatar a ESP $ESP_PARTITION."
    fi
}
