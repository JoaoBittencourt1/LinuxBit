## ADDED Requirements

### Requirement: Revalidar o plano antes do ponto de não-retorno
Antes de executar qualquer operação destrutiva (particionamento definitivo,
formatação, wipe de disco), o sistema SHALL revalidar que o disco/partição
alvo descritos em `install.conf` ainda correspondem ao estado real do disco
observado a partir do ambiente Linux, e SHALL abortar com uma mensagem
clara se houver divergência.

#### Scenario: Plano válido prossegue
- **WHEN** o disco/partição alvo em `install.conf` corresponde ao que o
  ambiente Linux observa no hardware
- **THEN** a instalação prossegue para a etapa de particionamento

#### Scenario: Divergência aborta antes de qualquer escrita
- **WHEN** o disco/partição alvo descrito em `install.conf` não é mais
  encontrado, ou o tamanho observado diverge do esperado
- **THEN** o sistema aborta antes de qualquer operação destrutiva e grava o
  motivo no log de instalação

### Requirement: Particionar o disco conforme o modo de instalação
No modo substituir, o sistema SHALL apagar a tabela de partição existente
do disco alvo e criar uma nova (GPT para UEFI, MBR ou GPT com partição
`bios_grub` para BIOS legado). No modo dual-boot, o sistema SHALL criar a
partição Linux dentro do espaço não alocado deixado pelo shrink executado
no lado Windows, sem alterar as partições existentes.

#### Scenario: Replace cria tabela de partição nova
- **WHEN** o modo de instalação é substituir
- **THEN** o sistema apaga a tabela de partição existente do disco alvo e
  cria uma nova, compatível com o modo de firmware detectado

#### Scenario: Dual-boot preserva partições existentes
- **WHEN** o modo de instalação é dual-boot
- **THEN** o sistema cria a partição Linux apenas no espaço não alocado
  deixado pelo shrink, sem apagar ou redimensionar qualquer partição
  existente

### Requirement: Montar o sistema de arquivos alvo
O sistema SHALL formatar e montar as partições recém-criadas (raiz, e swap
quando habilitado) no diretório de instalação antes de iniciar a instalação
do sistema base.

#### Scenario: Ponto de montagem pronto para instalação
- **WHEN** o particionamento é concluído com sucesso
- **THEN** a partição raiz está formatada, montada em `/mnt/linuxhub`, e a
  partição de swap (se habilitada) está ativa

### Requirement: Instalar o sistema base do Ubuntu
O sistema SHALL instalar um sistema Ubuntu base funcional no ponto de
montagem alvo usando `debootstrap`, com a versão/codinome correspondente à
distro selecionada em `install.conf`.

#### Scenario: Sistema base instalado com sucesso
- **WHEN** a montagem do sistema de arquivos alvo está pronta
- **THEN** o sistema executa `debootstrap` para a versão do Ubuntu
  especificada, deixando um sistema base utilizável no ponto de montagem

### Requirement: Criar a conta de usuário configurada
O sistema SHALL criar, dentro do sistema instalado (via chroot), o usuário,
senha (a partir do hash já gerado pelo Windows-side) e hostname
especificados em `install.conf`.

#### Scenario: Usuário criado com as credenciais configuradas
- **WHEN** o sistema base do Ubuntu está instalado
- **THEN** o usuário especificado em `install.conf` existe no sistema
  instalado, com a senha correspondente ao hash fornecido e privilégios
  administrativos

### Requirement: Instalar o bootloader preservando o boot do Windows
O sistema SHALL instalar o GRUB2 definitivo no sistema recém-instalado
(`grub-install` + geração de `grub.cfg`) e SHALL garantir que uma entrada
para bootar o Windows (via chainload, ou via NVRAM em UEFI) continue
disponível após a instalação, exceto quando o modo for substituir no disco
que continha o único Windows da máquina.

#### Scenario: Dual-boot mantém entrada do Windows no menu
- **WHEN** a instalação é concluída em modo dual-boot
- **THEN** o menu de boot do GRUB definitivo inclui uma entrada funcional
  para iniciar o Windows existente

#### Scenario: Replace não referencia um Windows que não existe mais
- **WHEN** a instalação é concluída em modo substituir, apagando o único
  Windows presente na máquina
- **THEN** o menu de boot do GRUB definitivo não inclui uma entrada de
  Windows órfã
