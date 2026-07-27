## Why

O LinuxHub tem um wizard de instalação Windows-side maduro (catálogo, download
de ISO, inventário de disco, coleta de conta, geração de `install.conf`), mas
nada do outro lado consome esse arquivo: `installer/core`, `installer/distros`
e `installer/profiles` estão vazios, `install.sh` referencia seis scripts que
não existem, e os dois services que deveriam preparar o disco e o boot
(`DiskPartitioningService`, `BootConfigurationService`) estão incompletos e
sequer conectados ao fluxo do wizard. O projeto não instala nada hoje — só
gera um arquivo de configuração que ninguém lê.

Além disso, o design atual do particionamento parte de uma premissa que não
se sustenta: o Windows não consegue reparticionar/formatar o disco de onde
ele mesmo está rodando enquanto está rodando. "Modo replace" (substituir
disco inteiro) precisa ser reformulado para acontecer do lado Linux, depois
do reboot — não como uma operação diskpart disparada a partir do app.

Esta proposta destrava a primeira instalação Ubuntu de ponta a ponta (UEFI e
BIOS legado, dual-boot e replace), organizada em fases sequenciais e
verificáveis — não uma reescrita monolítica. Ver `ROADMAP.md` (raiz do repo)
para o mapeamento completo do estado atual e o raciocínio por trás de cada
decisão referenciada aqui.

## What Changes

- Reescrever `DiskPartitioningService` para agir sobre o disco/partição
  realmente selecionado no wizard (hoje hardcoded em `select volume C`), e
  restringir sua responsabilidade à única operação segura em disco vivo:
  encolher uma partição NTFS existente para liberar espaço não alocado
  (modo dual-boot). Criação de partição, filesystem e qualquer operação no
  modo replace deixam de ser responsabilidade do Windows.
- **BREAKING (comportamento)**: bloquear o modo "replace" na UI quando o
  disco alvo hospeda a instalação Windows em execução — não é mais
  "tentado e falha", é recusado explicitamente com uma mensagem que explica
  o porquê.
- Corrigir a detecção de UEFI (trocar a heurística de pasta por
  `GetFirmwareType`, API Win32) e a localização da EFI System Partition
  (trocar o índice fixo assumido por lookup real via GUID de tipo de
  partição GPT).
- Conectar `IDiskPartitioningService` e o novo mecanismo de boot ao fluxo
  real de `InstallWizardViewModel.Install()` — hoje esses services existem
  mas não são chamados.
- Adicionar confirmação explícita na UI antes de qualquer operação de disco
  (mesmo o shrink, que é reversível, mas ainda assim toca o disco do
  usuário), conforme constitution §6.
- Introduzir um mecanismo de boot sem USB: staging da ISO do Ubuntu em uma
  área alocada no disco físico, com um GRUB2 chainloaded a partir do BCD do
  Windows que boota a ISO via loopback — funcionando tanto em UEFI quanto
  em BIOS legado, com caminhos de implementação distintos para cada um.
- Escrever o payload Linux-side (bash manual, sem instalador nativo
  gerado): `lib/disk.sh` (particionamento real via `parted`/`sgdisk`,
  incluindo o wipe completo do modo replace — agora seguro porque roda fora
  do disco sendo apagado), `lib/mount.sh`, `lib/chroot.sh` (`debootstrap`
  do Ubuntu), `lib/user.sh`, `lib/boot.sh` (`grub-install` + entrada de
  chainload de volta para o Windows Boot Manager, para não sequestrar o
  boot do Windows em BIOS legado).
- Fora de escopo nesta proposta: qualquer distro além do Ubuntu, Rust/C em
  qualquer camada, e instalação via `autoinstall`/subiquity gerado
  (decisão: bash manual dá mais controle sobre operações destrutivas).

## Capabilities

### New Capabilities
- `disk-provisioning`: planejamento e execução segura de operações de disco
  do lado Windows — inventário, shrink no alvo correto, detecção real de
  UEFI/ESP, bloqueio de replace em disco de sistema, confirmação destrutiva
  explícita. É a fonte da verdade do "plano" que o lado Linux executa.
- `boot-staging`: staging da ISO no disco e chainload de um GRUB2 via BCD
  para bootar a ISO via loopback sem USB, cobrindo UEFI e BIOS legado.
- `linux-install-payload`: scripts bash que executam a instalação real do
  Ubuntu após o reboot — particionamento definitivo, montagem, base do
  sistema via `debootstrap`, criação de usuário, bootloader — consumindo o
  plano gravado em `install.conf`.

### Modified Capabilities
- `install-wizard`: o fluxo de instalação passa a executar de fato as
  etapas de disco e boot (hoje só grava config e para), passa a exigir
  confirmação explícita antes de operações de disco, e passa a recusar o
  modo replace quando o alvo é o disco de sistema em uso.

## Impact

- **Código afetado**: `Features/InstallWizard/Services/*`
  (`DiskPartitioningService`, `BootConfigurationService`,
  `InstallerConfigBuilder`, `PartitionInventoryService`,
  `DiskInventoryService`), `Features/InstallWizard/ViewModels/*`
  (`TargetSelectionViewModel`, `InstallWizardViewModel`),
  `Features/InstallWizard/Views/InstallWizardView.xaml` (novo passo de
  confirmação destrutiva).
- **Novo código**: `installer/core/lib/{disk,mount,chroot,user,boot}.sh`,
  `installer/distros/ubuntu.sh`, componente de staging/boot (localização a
  definir em design.md — pode viver em `Features/InstallWizard/Services`
  como um novo service, ou numa feature própria dado o tamanho).
- **Sistemas externos tocados**: BCD do Windows (via `bcdedit`/API), tabela
  de partição do disco do usuário, firmware UEFI/BIOS (NVRAM boot entries
  via `efibootmgr` do lado Linux).
- **Dependências novas**: GRUB2 (binários/módulos para chainload e
  loopback), possivelmente reaproveitando scripts/motor do Ventoy para
  compatibilidade de boot da ISO do Ubuntu.
- **Risco**: alto — envolve particionamento de disco real e boot de
  firmware; validação primária em QEMU (OVMF/SeaBIOS) antes de qualquer
  teste em hardware físico (ver Fase 5 em tasks.md).
