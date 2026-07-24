#!/usr/bin/env bash
set -euo pipefail

######################################
# LinuxHub Universal Installer - CORE
######################################

INSTALL_ROOT="/mnt/linuxhub"
CONF_FILE="/tmp/linuxhub/install.conf"
LOG_FILE="/var/log/linuxhub-install.log"

######################################
# LOGGING
######################################
log() {
    echo "[LINUXHUB] $1"
    echo "[LINUXHUB] $1" >> "$LOG_FILE"
}

fatal() {
    log "ERRO: $1"
    exit 1
}

######################################
# PRIVILEGES
######################################
if [[ $EUID -ne 0 ]]; then
    fatal "Execute como root"
fi

######################################
# CONFIG
######################################
if [[ ! -f "$CONF_FILE" ]]; then
    fatal "Arquivo de configuração não encontrado: $CONF_FILE"
fi

log "Carregando configuração..."
source "$CONF_FILE"

######################################
# BASIC VALIDATION
######################################
[[ -z "${DISTRO:-}" ]] && fatal "DISTRO não definida"
[[ -z "${INSTALL_MODE:-}" ]] && fatal "INSTALL_MODE não definido"
[[ -z "${TARGET_DISK:-}" ]] && fatal "TARGET_DISK não definido"
[[ -z "${USERNAME:-}" ]] && fatal "USERNAME não definido"
[[ -z "${PASSWORD_HASH:-}" ]] && fatal "PASSWORD_HASH não definido"

######################################
# ENV DETECTION
######################################
if [[ -d /sys/firmware/efi ]]; then
    DETECTED_BOOT="uefi"
else
    DETECTED_BOOT="bios"
fi

log "Boot detectado: $DETECTED_BOOT"

if [[ "$DETECTED_BOOT" != "$BOOT_MODE" ]]; then
    fatal "Modo de boot incompatível (esperado $BOOT_MODE)"
fi

######################################
# DISK SAFETY
######################################
log "Disco alvo: $TARGET_DISK"
lsblk "$TARGET_DISK" >/dev/null || fatal "Disco inválido"

######################################
# PREPARE INSTALL ROOT
######################################
log "Preparando ambiente de instalação..."
mkdir -p "$INSTALL_ROOT"

######################################
# DISK SETUP
######################################
if [[ "$INSTALL_MODE" == "replace" ]]; then
    log "Modo: Substituir sistema"
    source lib/disk.sh
    setup_replace
else
    log "Modo: Dual boot"
    [[ -z "${TARGET_PARTITION:-}" ]] && fatal "TARGET_PARTITION não definido"
    source lib/disk.sh
    setup_dualboot
fi

######################################
# MOUNT
######################################
source lib/mount.sh
mount_all "$INSTALL_ROOT"

######################################
# INSTALL BASE SYSTEM
######################################
DISTRO_SCRIPT="distros/${DISTRO}.sh"
[[ ! -f "$DISTRO_SCRIPT" ]] && fatal "Distro não suportada: $DISTRO"

log "Instalando sistema base: $DISTRO"
source "$DISTRO_SCRIPT"
install_base "$INSTALL_ROOT"

######################################
# CHROOT CONFIG
######################################
source lib/chroot.sh
configure_system "$INSTALL_ROOT"

######################################
# USER SETUP
######################################
source lib/user.sh
create_user "$INSTALL_ROOT"

######################################
# BOOTLOADER
######################################
source lib/boot.sh
install_bootloader "$INSTALL_ROOT"

######################################
# FINISH
######################################
log "Instalação concluída com sucesso"
log "Pode reiniciar o sistema"
