#!/usr/bin/env bash
######################################
# LinuxHub Installer - User account creation
######################################
# Cria o usuário/senha/hostname configurados no wizard. PASSWORD chega em
# texto puro em install.conf — o Windows não tem crypt(3) (glibc SHA-512-
# crypt) disponível para pré-hashear a senha, então o hash real é gerado
# aqui, via `chpasswd` dentro do chroot, que usa o próprio glibc do sistema
# recém-instalado (correto por construção, em vez de reimplementar
# SHA-512-crypt no lado Windows). PASSWORD nunca é logado nem ecoado.
create_user() {
    local mountpoint="$1"
    local username="${USERNAME:?USERNAME não definido}"
    local password="${PASSWORD:?PASSWORD não definido}"

    log "Criando usuário $username..."

    chroot "$mountpoint" /bin/bash -c "useradd -m -s /bin/bash -G sudo '${username}'" \
        || fatal "Falha ao criar o usuário $username no sistema instalado."

    printf '%s:%s\n' "$username" "$password" | chroot "$mountpoint" chpasswd \
        || fatal "Falha ao definir a senha do usuário $username no sistema instalado."

    log "Usuário $username criado com privilégios administrativos."
}
