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
# Nomes de variável precisam bater exatamente com o que
# InstallerConfigWriter.Save() grava (Features/InstallWizard/Services/
# InstallerConfigWriter.cs) — não com o que seria intuitivo aqui.
[[ -z "${DISTRO_ID:-}" ]] && fatal "DISTRO_ID não definida"
[[ -z "${INSTALL_MODE:-}" ]] && fatal "INSTALL_MODE não definido"
[[ -z "${TARGET_DISK_INDEX:-}" ]] && fatal "TARGET_DISK_INDEX não definido"
[[ -z "${USERNAME:-}" ]] && fatal "USERNAME não definido"
[[ -z "${PASSWORD:-}" ]] && fatal "PASSWORD não definido"

SWAP_SIZE_MB=0
if [[ "${SWAP_ENABLED:-false}" == "true" ]]; then
    SWAP_SIZE_MB=$(( ${SWAP_SIZE_GB:-0} * 1024 ))
fi

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
# PREPARE INSTALL ROOT
######################################
log "Preparando ambiente de instalação..."
mkdir -p "$INSTALL_ROOT"

######################################
# DISK SETUP
######################################
# setup_replace/setup_dualboot chamam revalidate_plan (lib/disk.sh) antes de
# qualquer escrita — resolve TARGET_DISK_INDEX para um device real e aborta
# se o plano gravado em install.conf não bater mais com o hardware
# observado (spec linux-install-payload, "Revalidar o plano antes do ponto
# de não-retorno").
source core/lib/disk.sh
if [[ "$INSTALL_MODE" == "replace" ]]; then
    log "Modo: Substituir sistema"
    setup_replace
else
    log "Modo: Dual boot"
    [[ -z "${TARGET_PARTITION_INDEX:-}" ]] && fatal "TARGET_PARTITION_INDEX não definido"
    setup_dualboot
fi

######################################
# MOUNT
######################################
source core/lib/mount.sh
mount_all "$INSTALL_ROOT"

######################################
# INSTALL BASE SYSTEM
######################################
DISTRO_SCRIPT="distros/${DISTRO_ID}.sh"
[[ ! -f "$DISTRO_SCRIPT" ]] && fatal "Distro não suportada: $DISTRO_ID"

log "Instalando sistema base: $DISTRO_ID"
source "$DISTRO_SCRIPT"
install_base "$INSTALL_ROOT"

######################################
# CHROOT CONFIG
######################################
source core/lib/chroot.sh
configure_system "$INSTALL_ROOT"

######################################
# USER SETUP
######################################
source core/lib/user.sh
create_user "$INSTALL_ROOT"

######################################
# BOOTLOADER
######################################
source core/lib/boot.sh
install_bootloader "$INSTALL_ROOT"

######################################
# FINISH
######################################
# install.conf carrega PASSWORD em texto puro (ver core/lib/user.sh) —
# apaga o arquivo assim que ele não é mais necessário, para limitar a janela
# de exposição.
shred -u "$CONF_FILE" 2>/dev/null || rm -f "$CONF_FILE"

log "Instalação concluída com sucesso"
log "Pode reiniciar o sistema"
