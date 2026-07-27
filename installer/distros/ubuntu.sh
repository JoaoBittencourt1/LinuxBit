#!/usr/bin/env bash
######################################
# LinuxHub Installer - Ubuntu distro payload
######################################
# Interface distros/<id>.sh: install_base(mountpoint). Único ponto de
# acoplamento a uma distro específica — trocar/adicionar uma distro é
# adicionar um novo arquivo aqui, sem tocar em install.sh (OCP, ver
# design.md Non-Goals: só Ubuntu é implementado, mas a interface já permite
# outras famílias no futuro).

# install.conf grava DISTRO_VERSION como o número de versão do catálogo
# (ex.: "24.04.3"), não o codinome que debootstrap espera — mapeamento
# explícito em vez de heurística, para nunca instalar a suite errada.
resolve_ubuntu_codename() {
    local version="$1"
    local major_minor
    major_minor="$(echo "$version" | cut -d. -f1,2)"

    case "$major_minor" in
        24.04) echo "noble" ;;
        22.04) echo "jammy" ;;
        20.04) echo "focal" ;;
        *) fatal "Versão do Ubuntu não mapeada para um codinome debootstrap: $version" ;;
    esac
}

install_base() {
    local mountpoint="$1"
    local codename
    codename="$(resolve_ubuntu_codename "${DISTRO_VERSION:?DISTRO_VERSION não definido}")"

    log "Rodando debootstrap ($codename) em $mountpoint..."
    # --include garante locales/keyboard-configuration/grub-pc|grub-efi-amd64 no
    # sistema base — um debootstrap mínimo não traz esses pacotes por padrão, e
    # lib/chroot.sh (locale-gen, dpkg-reconfigure keyboard-configuration) e
    # lib/boot.sh (grub-install) dependem deles existirem depois.
    local grub_package="grub-pc"
    [[ "${BOOT_MODE:-}" == "uefi" ]] && grub_package="grub-efi-amd64"

    debootstrap --arch=amd64 --include="locales,keyboard-configuration,${grub_package}" \
        "$codename" "$mountpoint" http://archive.ubuntu.com/ubuntu/ \
        || fatal "debootstrap falhou para $codename."

    mount --bind /dev "$mountpoint/dev" || fatal "Falha ao bind-mount /dev."
    mount --bind /dev/pts "$mountpoint/dev/pts" || fatal "Falha ao bind-mount /dev/pts."
    mount -t proc proc "$mountpoint/proc" || fatal "Falha ao montar /proc."
    mount -t sysfs sysfs "$mountpoint/sys" || fatal "Falha ao montar /sys."

    log "Sistema base Ubuntu ($codename) instalado em $mountpoint."
}
