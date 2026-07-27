#!/usr/bin/env bash
######################################
# LinuxHub Installer - Chroot base configuration
######################################
# Configura locale/timezone/keymap no sistema recém-instalado, a partir dos
# valores já resolvidos pelo lado Windows (ISystemInfoProvider) e gravados
# em install.conf. Requer install_base já ter rodado (debootstrap +
# bind-mounts de /dev, /proc, /sys — ver distros/ubuntu.sh).

configure_system() {
    local mountpoint="$1"

    log "Configurando hostname, locale, timezone e keymap..."

    echo "${HOSTNAME:?HOSTNAME não definido}" > "$mountpoint/etc/hostname"
    cat > "$mountpoint/etc/hosts" <<EOF
127.0.0.1   localhost
127.0.1.1   ${HOSTNAME}
EOF

    chroot "$mountpoint" /bin/bash -c "
        set -e
        locale-gen '${LOCALE:?LOCALE não definido}'
        update-locale LANG='${LOCALE}'
        ln -sf '/usr/share/zoneinfo/${TIMEZONE:?TIMEZONE não definido}' /etc/localtime
        dpkg-reconfigure -f noninteractive tzdata
        echo 'keyboard-configuration keyboard-configuration/xkb-keymap select ${KEYMAP:?KEYMAP não definido}' | debconf-set-selections
        dpkg-reconfigure -f noninteractive keyboard-configuration
    " || fatal "Falha ao configurar locale/timezone/keymap no sistema instalado."

    log "Configuração básica do sistema concluída."
}
