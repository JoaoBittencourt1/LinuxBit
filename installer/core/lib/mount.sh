#!/usr/bin/env bash
######################################
# LinuxHub Installer - Mount target filesystems
######################################
# Requer ROOT_PARTITION/SWAP_PARTITION/ESP_PARTITION já definidos por
# lib/disk.sh (setup_replace ou setup_dualboot).

mount_all() {
    local mountpoint="$1"

    [[ -z "${ROOT_PARTITION:-}" ]] && fatal "mount_all chamado antes do particionamento (ROOT_PARTITION vazio)."

    log "Montando partição raiz $ROOT_PARTITION em $mountpoint..."
    mount "$ROOT_PARTITION" "$mountpoint" || fatal "Falha ao montar $ROOT_PARTITION em $mountpoint."

    if [[ -n "${ESP_PARTITION:-}" ]]; then
        mkdir -p "$mountpoint/boot/efi"
        mount "$ESP_PARTITION" "$mountpoint/boot/efi" || fatal "Falha ao montar a ESP $ESP_PARTITION."
    fi

    if [[ -n "${SWAP_PARTITION:-}" ]]; then
        swapon "$SWAP_PARTITION" || fatal "Falha ao ativar a partição de swap $SWAP_PARTITION."
    fi

    log "Sistema de arquivos alvo montado em $mountpoint."
}
