## Context

O wizard Windows-side (catálogo, download de ISO, inventário de disco,
`InstallerConfig`) está funcional e é a fonte da verdade do "plano" de
instalação. Nada do lado Linux existe (`installer/core`, `installer/distros`,
`installer/profiles` vazios) e os dois services que deveriam agir sobre
disco/boot (`DiskPartitioningService`, `BootConfigurationService`) estão
incompletos, incorretos (hardcoded em `select volume C`) e desconectados do
fluxo real (`InstallWizardViewModel.Install()` não os chama).

Restrição física que motiva o design inteiro: **o Windows não consegue
reparticionar/formatar o disco do qual ele mesmo está rodando, enquanto está
rodando.** Isso não é uma limitação de ferramenta (diskpart vs. outra API) —
é o motivo pelo qual "modo replace" nunca pode ser uma operação Windows-side.

Ver `ROADMAP.md` (raiz do repo) para o mapeamento completo e o histórico de
decisões que geraram esta proposta.

## Goals / Non-Goals

**Goals:**
- Uma instalação Ubuntu real, de ponta a ponta, sem USB, funcionando em UEFI
  e em BIOS legado, nos dois modos (dual-boot e replace).
- Separação clara entre "planejar" (Windows, reversível) e "executar"
  (Linux, pós-reboot, é onde está o ponto de não-retorno).
- Cada fase entregável e testável isoladamente antes de avançar (ver
  `tasks.md`) — não uma reescrita monolítica.

**Non-Goals:**
- Suporte a outras distros além do Ubuntu (fica para propostas futuras — a
  interface `distros/<id>.sh` já deixa a porta aberta via OCP, mas nenhuma
  outra família é implementada aqui).
- Rust/C em qualquer camada.
- Gerar `autoinstall.yaml`/usar subiquity — a instalação é bash manual.
- UI polida do passo de boot/staging além do mínimo necessário para
  confirmação e feedback de progresso.

## Decisions

### D1 — Plano (Windows) vs. Execução (Linux) é a fronteira arquitetural central

`install.conf` já é essa fronteira — mas hoje o lado Windows tenta "fazer
demais" (a chamada de `CreatePartition` sugere que o Windows criaria a
partição de fato). Decisão: o Windows só grava **intenção** — disco/partição
alvo, modo, tamanho desejado — e faz **apenas** operações comprovadamente
seguras em disco vivo (shrink de um volume NTFS existente, que é exatamente
o que "Gerenciamento de Disco" nativo do Windows já faz sem reboot). Toda
operação destrutiva ou que exige o disco "livre" (criar partição, mkfs,
`wipefs`/limpar tabela inteira no replace) é responsabilidade exclusiva do
`lib/disk.sh` do lado Linux, executado depois do reboot.

Alternativa considerada: manter a criação de partição no Windows (como o
código atual sugere) e só usar Linux para o `mkfs`. Rejeitada — criar uma
partição de um tipo/GUID que o Linux vai reformatar de qualquer forma não
adiciona segurança nem controle, só duplica lógica de particionamento nos
dois lados (violação de DRY) e ainda deixa o modo replace sem solução.

### D2 — Modo replace no disco de sistema (revisado na Fase 2: permitido, não recusado)

Decisão original da Fase 1: recusar replace quando o disco alvo hospeda o
Windows em execução, com a justificativa "não existe forma segura de tentar
e ver se falha". **Essa justificativa só vale para tentar o wipe enquanto o
Windows está rodando** — e o Windows nunca faz isso; ele só grava o plano em
`install.conf` e prepara o boot-staging (D4). O wipe de verdade
(`setup_replace` em `lib/disk.sh`) só acontece depois do reboot, dentro do
ambiente Ubuntu live carregado via boot-staging — nesse ponto o Windows não
está mais rodando, e apagar o disco (mesmo sendo o único disco da máquina,
mesmo sendo o que tinha o Windows) é tão seguro quanto apagar qualquer disco
a partir de um Linux live comum (mesmo princípio de qualquer instalador
Ubuntu bootado de pendrive substituindo o Windows de um notebook com um
disco só).

Bloquear a seleção nunca protegeu nada nesse fluxo — só impedia o caso de
uso mais comum do produto (notebook com um disco, trocar Windows por
Linux). **Revisão (Fase 2, após validar a arquitetura do boot-staging):**
`TargetSelectionViewModel.IsReplacingSystemDisk` deixa de bloquear a seleção
e passa a ser só informativo — liga um aviso mais forte na UI
(`Wizard_ReplaceSystemDiskWarningMessage`) e na confirmação destrutiva
(`Wizard_ConfirmReplaceSystemDiskSummary`, deixando explícito que não há
volta pro Windows depois do reboot), mas não impede a operação. Replace
continua permitido em discos secundários também, pelo mesmo motivo de
sempre — e, por consistência, **o wipe real sempre acontece no
`lib/disk.sh`**, nunca no Windows, seja disco de sistema ou não.

### D3 — Detecção de firmware e ESP via API real, não heurística de caminho

`GetFirmwareType` (Win32, `kernel32.dll`) substitui
`Directory.Exists(@"C:\Windows\Boot\EFI")`. A EFI System Partition é
localizada por GUID de tipo de partição GPT (`c12a7328-f81f-11d2-ba4b-
00a0c93ec93b`) via `Win32_DiskPartition`/PowerShell `Get-Partition`, nunca
por índice assumido.

### D4 — Boot sem USB: staging em arquivo + GRUB2 chainloaded via BCD

A ISO do Ubuntu fica como arquivo num volume NTFS existente (o
`IsoDownloadService` já baixa para `%APPDATA%\LinuxHub\ISOs\` — não precisa
de partição dedicada). Um GRUB2 é instalado como um boot entry adicional:
- **UEFI**: um `.efi` do GRUB é copiado para a ESP existente (reaproveitada,
  não recriada) e registrado via `bcdedit` como uma entrada de firmware
  (`bcdedit /set {id} device partition=<ESP>`, `path
  \EFI\linuxhub\grubx64.efi`, `bcdedit /displayorder {id} /addlast`).
- **BIOS legado**: GRUB2 não pode coexistir como boot entry do BCD da mesma
  forma (BCD é um mecanismo UEFI-first / NT bootmgr); a via realista é
  instalar GRUB no MBR como bootloader de primeiro estágio que chaina para
  o `bootmgr` do Windows como uma entrada de menu — isso é uma inversão do
  fluxo normal (GRUB assume o MBR, Windows vira uma opção de menu) e precisa
  de um passo de "restaurar MBR do Windows" como plano de rollback caso o
  usuário desista antes do reboot.

GRUB então boota o `.iso` via `loopback` + parâmetros de cmdline específicos
do Ubuntu/casper (`iso-scan/filename=`, `boot=casper`). Decisão: investigar
reaproveitar os scripts de boot do Ventoy (open source) para essa parte em
vez de escrever o `grub.cfg` de loopback do zero — reduz risco de
compatibilidade, mas ainda precisa adaptação para o caso "arquivo já está no
disco interno" em vez de "pendrive Ventoy-formatado". Fica como item de
investigação na Fase 2 (ver `tasks.md`), não uma decisão fechada aqui.

Alternativa considerada e rejeitada: boot via VHD (estilo Wubi). Mais
simples de implementar (usa suporte nativo do BCD para VHD), mas não produz
um dual-boot "de verdade" (Linux dentro de um arquivo de disco virtual) e
tem suporte incerto em distros modernas — não atende ao objetivo de
paridade com uma instalação real.

### D5 — Payload Linux em bash manual, não autoinstall/subiquity

Decisão do usuário: controle explícito sobre cada passo destrutivo, mesmo
custando mais código próprio. `lib/disk.sh` chama `parted`/`sgdisk`
diretamente; `lib/chroot.sh` usa `debootstrap`; `lib/boot.sh` roda
`grub-install` + `update-grub`, e adiciona (não substitui) a entrada de
chainload de volta para o `bootmgr` do Windows, para BIOS legado e UEFI.

### D6 — Confirmação destrutiva é um passo de UI dedicado, não um `MessageBox` genérico

Conforme constitution §6, toda operação que toca o disco do usuário (mesmo
o shrink) precisa de confirmação explícita antes de executar. Decisão: um
novo passo no wizard (após "Alvo" e antes de "Instalar") que resume em texto
simples o que vai acontecer ("Vamos encolher a unidade C: em X GB..." /
"Vamos apagar TODOS os dados do Disco N..."), com um campo de confirmação
que exige digitar algo (não só um clique) quando o modo é replace — o custo
de um clique acidental num "apagar disco inteiro" é desproporcional ao
atrito extra de digitar uma confirmação.

## Risks / Trade-offs

- [Risco] Firmware UEFI varia muito entre fabricantes (Secure Boot, Fast
  Boot, CSM) → Mitigação: validar primeiro em QEMU+OVMF (Fase 5) antes de
  hardware real; documentar explicitamente que Secure Boot pode precisar
  estar desabilitado na primeira versão (GRUB não assinado).
- [Risco] Falha no meio da instalação Linux-side deixa o disco num estado
  inconsistente (parcialmente particionado, chroot incompleto) → Mitigação:
  Fase 4 dedicada a verificação pré-ponto-de-não-retorno e diagnóstico
  recuperável; fora de escopo desta proposta um rollback automático completo
  (documentar como limitação conhecida).
- [Risco] Chainload GRUB em BIOS legado sobrescreve o MBR atual do Windows →
  Mitigação: `boot-staging` deve fazer backup do MBR atual antes de
  qualquer escrita, e `linux-install-payload`/`lib/boot.sh` deve
  obrigatoriamente incluir uma entrada de menu para bootar o Windows de
  volta.
- [Trade-off] Bash manual (D5) é mais superfície de bug em operação
  destrutiva do que reaproveitar `debootstrap`+instalador nativo testado em
  escala — aceito conscientemente pela decisão do usuário de priorizar
  controle/transparência sobre cada passo.
- [Risco] Reaproveitar scripts do Ventoy (D4) traz uma dependência externa
  cuja licença/formato pode não se encaixar limpo no fluxo "arquivo já no
  disco" — se a investigação da Fase 2 mostrar que não compensa, cair para
  GRUB `grub.cfg` de loopback escrito à mão é o fallback.

## Migration Plan

Não há usuários em produção hoje (o pipeline nunca funcionou de ponta a
ponta), então não há dado existente a migrar. A "migração" é sequencial por
fase, cada uma com checkpoint de avaliação humana antes de prosseguir (ver
`tasks.md`) — o rollback de cada fase é, na prática, não mergear/reverter o
commit daquela fase, já que nenhuma fase isolada expõe o caminho completo de
instalação a um usuário final antes da Fase 5.

## Open Questions

- ~~Reaproveitar o motor de boot do Ventoy (D4) é viável para o caso "ISO em
  disco interno, não pendrive"?~~ **Resolvido (spike Fase 2, tarefa 3.1):**
  não reaproveitar. O motor do Ventoy é construído em torno de detectar e
  montar uma partição/volume formatado como Ventoy (reserva de disco própria,
  sistema de plugins em JSON, bloco de variáveis de ambiente persistido via
  device-mapper) — a premissa central é "qualquer ISO num pendrive
  Ventoy-formatado", não "uma ISO fixa já em arquivo num volume NTFS comum,
  com GRUB chainloaded via BCD/MBR". Adaptar exigiria arrancar a detecção de
  USB e o framework de plugins do Ventoy só para reaproveitar a parte de
  loopback — mais trabalho de integração do que economia. O próprio Ubuntu
  (casper) já documenta os parâmetros de boot para exatamente esse cenário
  ("ISO em disco, não em mídia removível"): `boot=casper
  iso-scan/filename=<caminho>`. Decisão: escrever um `grub.cfg` mínimo próprio
  (módulos stock do GRUB2 — `loopback`, `ntfs`, `part_gpt`/`part_msdos`) em
  vez de adotar o motor do Ventoy; ver `GrubConfigBuilder` em
  `Features/InstallWizard/Services`.
- O backup/restauração do MBR em BIOS legado (D4, risco de chainload) deve
  ser automático ou exigir confirmação/ação manual do usuário no primeiro
  boot pós-instalação? A decidir durante a Fase 2.
- Disco secundário no modo replace (D2): vale a pena o Windows fazer `clean`
  antecipado, ou é preferível, por simplicidade, que **todo** wipe — mesmo
  em disco secundário — só aconteça no `lib/disk.sh`? Design atual escolhe a
  segunda opção (uma única implementação de wipe); revisar se isso se provar
  impraticável na Fase 1.
