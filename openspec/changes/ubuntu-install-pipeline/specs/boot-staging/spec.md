## ADDED Requirements

### Requirement: Preparar a ISO como arquivo acessível ao bootloader de staging
O sistema SHALL garantir que a ISO da distro selecionada esteja disponível
como um arquivo num volume que o bootloader de staging consiga ler no
momento do boot, sem exigir uma partição dedicada nem um pendrive USB.

#### Scenario: ISO já baixada é reaproveitada
- **WHEN** o usuário já baixou a ISO via `install-wizard` para o caminho
  padrão de downloads do LinuxHub
- **THEN** o sistema de boot-staging usa esse arquivo diretamente, sem
  copiá-lo ou movê-lo para uma localização especial

### Requirement: Instalar bootloader de chainload em sistemas UEFI
Em máquinas UEFI, o sistema SHALL instalar um bootloader GRUB2 na EFI
System Partition existente (identificada por `disk-provisioning`) e
registrar uma entrada de firmware via BCD que o disponibiliza como opção de
boot adicional, sem remover a entrada existente do Windows Boot Manager.

#### Scenario: Entrada de boot UEFI aponta para o GRUB instalado
- **WHEN** o sistema termina de instalar o bootloader de staging em UEFI
- **THEN** existe uma entrada de boot no firmware que, quando selecionada,
  carrega o GRUB2 instalado, e a entrada original do Windows Boot Manager
  continua presente e funcional

### Requirement: Instalar bootloader de chainload em sistemas BIOS legado
Em máquinas BIOS legado, o sistema SHALL fazer backup do setor de boot
(MBR) atual antes de instalar o GRUB2 no MBR, e o GRUB2 instalado SHALL
incluir uma entrada de menu que permite bootar o Windows normalmente.

#### Scenario: Backup do MBR é criado antes da escrita
- **WHEN** o sistema instala o bootloader de staging numa máquina BIOS
  legado
- **THEN** uma cópia do MBR original é salva em disco antes de qualquer
  escrita, de forma recuperável

#### Scenario: Windows continua bootável após instalar o GRUB no MBR
- **WHEN** o usuário reinicia a máquina após a instalação do bootloader de
  staging em BIOS legado e escolhe a opção de boot do Windows no menu do
  GRUB
- **THEN** o Windows inicializa normalmente

### Requirement: Bootar a ISO da distro via loopback
O sistema SHALL configurar o bootloader de staging para inicializar o
kernel e initrd contidos na ISO da distro diretamente via loopback,
passando os parâmetros de linha de comando específicos exigidos pela
distro alvo para reconhecer que está sendo iniciada a partir de um arquivo
ISO em disco (não de mídia removível).

#### Scenario: ISO do Ubuntu inicia em ambiente live
- **WHEN** o usuário seleciona a entrada de boot de staging e a distro alvo
  é Ubuntu
- **THEN** o ambiente live do Ubuntu (casper) inicia normalmente a partir
  do arquivo ISO em disco, sem exigir mídia USB
