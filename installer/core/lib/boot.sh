#!/usr/bin/env bash
######################################
# LinuxHub Installer - Definitive bootloader
######################################
# grub-install + update-grub no sistema recém-instalado. Em dual-boot,
# habilita os-prober para que o GRUB definitivo detecte e inclua uma entrada
# para o Windows existente; em replace, nunca roda os-prober (não há
# Windows para detectar) — spec linux-install-payload, "Instalar o
# bootloader preservando o boot do Windows".

install_bootloader() {
    local mountpoint="$1"

    log "Instalando o bootloader definitivo (grub-install, modo $BOOT_MODE)..."

    if [[ "$BOOT_MODE" == "uefi" ]]; then
        chroot "$mountpoint" /bin/bash -c "
            set -e
            grub-install --target=x86_64-efi --efi-directory=/boot/efi --bootloader-id=ubuntu --recheck
        " || fatal "grub-install (UEFI) falhou."
    else
        chroot "$mountpoint" /bin/bash -c "
            set -e
            grub-install --target=i386-pc --recheck '${TARGET_DISK:?TARGET_DISK não definido}'
        " || fatal "grub-install (BIOS legado) falhou em ${TARGET_DISK}."
    fi

    if [[ "$INSTALL_MODE" == "dualboot" ]]; then
        log "Habilitando os-prober para detectar o Windows existente..."
        chroot "$mountpoint" /bin/bash -c "
            set -e
            apt-get install -y os-prober
            sed -i '/GRUB_DISABLE_OS_PROBER/d' /etc/default/grub
            echo 'GRUB_DISABLE_OS_PROBER=false' >> /etc/default/grub
        " || fatal "Falha ao habilitar o os-prober para detectar o Windows existente."
    fi

    chroot "$mountpoint" /bin/bash -c "set -e; update-grub" \
        || fatal "update-grub falhou ao gerar o grub.cfg definitivo."

    if [[ "$INSTALL_MODE" == "dualboot" ]] && ! grep -qi "windows" "$mountpoint/boot/grub/grub.cfg"; then
        log "AVISO: o grub.cfg definitivo não parece ter uma entrada do Windows — verifique manualmente antes de reiniciar."
    fi

    log "Bootloader definitivo instalado."
}
