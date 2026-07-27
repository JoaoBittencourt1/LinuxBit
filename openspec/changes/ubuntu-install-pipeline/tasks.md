## 1. Fase 1 — Corrigir e conectar o plano de disco (Windows)

- [x] 1.1 Trocar `TargetSelectionViewModel`/`DiskInventoryService` para expor
      qual disco físico hospeda o volume de boot do Windows em execução
      (via `Win32_LogicalDiskToPartition` → `Win32_DiskPartition` →
      `Win32_DiskDrive`, não um índice assumido), adicionando
      `DiskInfo.IsSystemDisk`.
- [x] 1.2 ~~Bloquear a seleção desse disco no modo substituir~~ **Revisado na
      Fase 2**: a seleção não é mais bloqueada — o wipe real só acontece
      depois do reboot (Linux), quando o Windows não está mais rodando,
      então bloquear não protegia nada e impedia o caso de uso mais comum
      (notebook com um disco só). `TargetSelectionViewModel.
      IsReplacingSystemDisk` agora só liga um aviso mais forte na UI e na
      confirmação destrutiva. Ver design.md D2 (revisado) para o raciocínio
      completo.
- [x] 1.3 Reescrever `IDiskPartitioningService`/`DiskPartitioningService`
      para receber o disco/partição alvo real do wizard (não hardcoded em
      `select volume C`) e restringir sua única operação a shrink de uma
      partição existente — remover a lógica de `create partition` /
      `assign letter` do lado Windows.
- [x] 1.4 Trocar a detecção de UEFI (`Directory.Exists(@"C:\Windows\Boot\
      EFI")`) por `GetFirmwareType` via P/Invoke em
      `TargetSelectionViewModel`/novo helper dedicado.
- [x] 1.5 Trocar `InstallerConfigBuilder.EfiPartitionIndex = 1` fixo por
      lookup real da EFI System Partition por GUID de tipo GPT
      (`PartitionInventoryService` ou novo service dedicado).
- [x] 1.6 Adicionar o passo de confirmação destrutiva no wizard (view +
      viewmodel novos): resume a operação de disco, exige confirmação
      simples para shrink e confirmação reforçada (digitar palavra) para
      replace.
- [x] 1.7 Conectar `IDiskPartitioningService` (shrink) e o novo passo de
      confirmação ao fluxo real de `InstallWizardViewModel.Install()` —
      hoje esse service não é chamado.
- [x] 1.8 Escrever/atualizar testes unitários (services são puros, sem
      `System.Windows.*`, conforme constitution §5):
      `TargetSelectionViewModel` (bloqueio de disco de sistema),
      `DiskPartitioningService` (shrink no alvo correto),
      `InstallerConfigBuilder` (EFI por GUID).
- [ ] 1.9 Testar manualmente o fluxo completo até a confirmação (sem
      reboot ainda) numa VM com um disco secundário: shrink acontece na
      partição certa, replace é bloqueado no disco de sistema, mensagens
      de confirmação aparecem corretamente.

## 2. Fase 1 — Checkpoint

- [ ] 2.1 **Resumir a Fase 1**: o que foi feito (arquivos criados/alterados,
      bugs corrigidos, decisões tomadas), o que ficou de fora/adiado, e o
      resultado dos testes (1.8/1.9) — sucesso, falha, ou parcial.
- [ ] 2.2 **PARAR.** Apresentar o resumo de 2.1 ao usuário e aguardar
      avaliação explícita antes de iniciar a Fase 2. Não prosseguir para o
      mecanismo de boot sem antes essa base de disco estar validada como
      correta e segura.

## 3. Fase 2 — Mecanismo de boot sem USB (staging)

- [x] 3.1 Spike: investigar se o motor de boot do Ventoy é reaproveitável
      para bootar uma ISO já presente em disco interno (não pendrive
      Ventoy-formatado); documentar a decisão em `design.md` (Open
      Questions) antes de seguir.
- [x] 3.2 Implementar staging da ISO do Ubuntu já baixada
      (`%APPDATA%\LinuxHub\ISOs\`) como origem direta do boot — sem copiar
      para partição dedicada. (`BootStagingService.InstallStagingBootloader`
      valida que o arquivo existe e usa o caminho direto, sem cópia.)
- [x] 3.3 Implementar instalação do GRUB2 + entrada BCD em sistemas UEFI
      (grava na ESP existente, `bcdedit /set {id} device/path`,
      `/displayorder /addlast`), sem remover a entrada existente do
      Windows Boot Manager. (Orquestração completa em C#: `EspLocatorService`
      + montagem temporária da ESP + `BootConfigurationService.
      AddFirmwareBootEntry`. `Assets/Grub/uefi/grubx64.efi` gerado via WSL em
      2026-07-27 e commitado — standalone, sem dependências externas.
      **Testável em VM agora**, ainda não validado em execução real.)
- [x] 3.4 Implementar instalação do GRUB2 no MBR em sistemas BIOS legado,
      incluindo backup do MBR original antes de qualquer escrita e uma
      entrada de menu para bootar o Windows. `MbrBackupService` faz
      backup/restore e escreve os 440 bytes de código de boot preservando a
      tabela de partição. Embutimento do `core.img` no gap pós-MBR
      automatizado: `MbrPartitionTableReader` acha o gap real (lendo a
      tabela de partição do disco), `BootStagingService.
      EnsurePostMbrGapFitsCoreImage` aborta antes de qualquer escrita se o
      gap for pequeno demais, `MbrBackupService.WriteCoreImageToGap` grava.
      O offset/formato exato do patch necessário em `boot.img` foi
      determinado comparando byte a byte a saída de um `grub-bios-setup`
      real (rodado via WSL contra um disco sintético) contra o `boot.img`
      de fábrica — ver `Assets/Grub/README.md` para o raciocínio completo.
      **Ainda não validado por um boot real** (nunca rodou contra hardware
      nem QEMU).
- [x] 3.5 Gerar o `grub.cfg` de loopback com os parâmetros de boot do
      Ubuntu/casper (`iso-scan/filename=`, `boot=casper`) apontando para o
      arquivo ISO staged. (`GrubConfigBuilder`, lógica pura testada em
      `GrubConfigBuilderTests` — usa `search --file` em vez de numeração de
      disco assumida.)
- [x] 3.6 Criar novo service (`IBootStagingService`/feature própria,
      conforme SRP/ISP da constitution — não inchar
      `BootConfigurationService` atual) e substituir/completar
      `BootConfigurationService.AddBootEntry`, que hoje cria uma entrada
      BCD incompleta. (`IBootStagingService`/`BootStagingService` orquestra
      `EspLocatorService`, `GrubAssetProvider`, `MbrBackupService` e o novo
      `BootConfigurationService.AddFirmwareBootEntry`.)
- [x] 3.7 Conectar o novo boot-staging ao fluxo de
      `InstallWizardViewModel.Install()`, depois da confirmação da Fase 1.
- [ ] 3.8 Testar em QEMU+OVMF (UEFI) e QEMU+SeaBIOS (BIOS legado): a
      entrada de staging aparece no boot, carrega o GRUB, e o GRUB
      consegue bootar a ISO do Ubuntu em modo live via loopback.

## 4. Fase 2 — Checkpoint

- [ ] 4.1 **Resumir a Fase 2**: caminho escolhido no spike do Ventoy (3.1) e
      por quê, o que foi implementado (staging, GRUB UEFI, GRUB BIOS
      legado + backup de MBR, cmdline de loopback), e o resultado dos
      testes em QEMU (3.8) — o que bootou, o que não bootou.
- [ ] 4.2 **PARAR.** Apresentar o resumo de 4.1 (com gravação/log da
      demonstração do boot em QEMU, UEFI e BIOS legado, até o ambiente live
      do Ubuntu) e aguardar avaliação explícita antes de iniciar a Fase 3 —
      o payload Linux só faz sentido se o boot até ele já está provado.

## 5. Fase 3 — Payload Linux (bash manual, Ubuntu)

- [x] 5.1 Implementar `installer/core/lib/disk.sh`: `setup_replace()`
      (apaga tabela de partição existente, cria nova compatível com o modo
      de firmware) e `setup_dualboot()` (cria partição Linux só no espaço
      não alocado deixado pelo shrink), usando `parted`/`sgdisk`.
      **Limitação conhecida**: `resolve_target_disk_device` assume que a
      ordem de enumeração de disco é a mesma em Windows (`TARGET_DISK_INDEX`)
      e Linux (`lsblk`) — não garantido em 100% do hardware; mitigado por
      `revalidate_plan` abortar se o device resolvido não existir, mas não
      elimina o risco de resolver o disco errado quando ambos existem.
- [x] 5.2 Implementar a revalidação do plano antes do particionamento
      (spec `linux-install-payload` — "Revalidar o plano antes do ponto de
      não-retorno"): compara disco/partição de `install.conf` contra o
      hardware observado, aborta com log claro se divergir.
      (`revalidate_plan()` em `disk.sh`.)
- [x] 5.3 Implementar `installer/core/lib/mount.sh`: formata e monta
      raiz/swap em `/mnt/linuxhub`.
- [x] 5.4 Implementar `installer/distros/ubuntu.sh`
      (`install_base(mountpoint)`): `debootstrap` da versão do Ubuntu
      especificada em `install.conf`.
- [x] 5.5 Implementar `installer/core/lib/chroot.sh`: configuração básica
      do sistema instalado (locale, timezone, keymap a partir de
      `install.conf`).
- [x] 5.6 Implementar `installer/core/lib/user.sh`: cria usuário/senha
      (a partir do hash)/hostname configurados. **Bug corrigido durante a
      implementação**: `CryptoHelper.GenerateSha512Hash`
      (removido — `Common/Helpers/CryptoHelper.cs`) gerava um digest
      SHA-512 hex puro, não um hash crypt(3) (`$6$salt$hash`) —
      `usermod -p`/`/etc/shadow` não aceitam esse formato, login ficaria
      quebrado. Decisão (com o usuário): o Windows não tenta mais
      pré-hashear a senha (não tem crypt(3)/glibc SHA-512-crypt
      disponível); `install.conf` grava `PASSWORD` em texto puro, e
      `user.sh` usa `chpasswd` dentro do chroot para gerar o hash com o
      glibc do próprio sistema instalado. `install.sh` apaga `install.conf`
      ao final para limitar a janela de exposição do texto puro.
- [x] 5.7 Implementar `installer/core/lib/boot.sh`: `grub-install` +
      `update-grub` no sistema instalado, garantindo entrada de chainload
      de volta pro Windows (exceto quando o único Windows foi apagado no
      modo replace).
- [ ] 5.8 Testar o `install.sh` completo em QEMU (UEFI e BIOS legado, replace
      e dual-boot — 4 combinações) partindo do boot staged na Fase 2, até
      um Ubuntu instalado e bootável.

## 6. Fase 3 — Checkpoint

- [ ] 6.1 **Resumir a Fase 3**: scripts escritos em `installer/core/lib/*` e
      `installer/distros/ubuntu.sh`, e o resultado de cada uma das 4
      combinações testadas (UEFI/BIOS legado × replace/dual-boot) — qual
      terminou num Ubuntu instalado e bootável, qual falhou e onde.
- [ ] 6.2 **PARAR.** Apresentar o resumo de 6.1 (com logs de instalação e
      prints do sistema instalado bootando) e aguardar avaliação explícita
      antes da Fase 4 — a essa altura a instalação já é "real" e qualquer
      ajuste de segurança deve ser discutido antes de formalizar o
      tratamento de falhas.

## 7. Fase 4 — Segurança e recuperação

- [x] 7.1 Revisar todos os pontos de falha entre o shrink (Windows) e o
      particionamento definitivo (Linux) e documentar o que acontece em
      cada um (ex.: usuário não reinicia após o shrink — o app deve deixar
      isso claro e recuperável). (Tabela completa em `installer/README.md`,
      "O que acontece se falhar em cada ponto".)
- [x] 7.2 Garantir que uma falha no meio de `install.sh` (após
      particionamento, antes de concluir) deixe um log claro em
      `/var/log/linuxhub-install.log` com o último passo concluído, para
      diagnóstico manual. (Auditado: todo passo relevante em `lib/*.sh`
      agora loga início e conclusão; adicionados os logs que faltavam em
      `lib/boot.sh` e `lib/disk.sh::format_new_partitions`.)
- [x] 7.3 Documentar (README/mensagem final do instalador) como o usuário
      recupera acesso a um shell caso a instalação falhe no meio, dado que
      não há rollback automático completo nesta fase (limitação conhecida,
      registrada em `design.md`). (`installer/README.md`, seção
      "Recuperando de uma falha no meio de install.sh"; mensagem de
      sucesso/erro do wizard atualizada em `Strings.resx`/
      `Strings.en-US.resx` para orientar o próximo passo real — reiniciar e
      onde achar o log — em vez do texto genérico antigo "Configuração
      gerada com sucesso!".)
- [x] 7.4 Revisar mensagens de erro do lado Windows (services da Fase 1) e
      do lado Linux (`fatal()` em `install.sh`) quanto a clareza — nunca
      silenciar exceção em operação de disco/boot, conforme constitution
      §6. (Auditoria: nenhum `catch` vazio nem `|| true`/supressão de erro
      encontrado em `Features/InstallWizard/Services` nem em
      `installer/**/*.sh`.)

## 8. Fase 4 — Checkpoint

- [ ] 8.1 **Resumir a Fase 4**: pontos de falha revisados e como cada um se
      comporta hoje, o que ficou registrado em log, e onde a recuperação
      ainda depende de intervenção manual do usuário (limitação conhecida).
- [ ] 8.2 **PARAR.** Apresentar o resumo de 8.1, junto do comportamento em
      cenários de falha simulada (interromper o QEMU no meio do
      `install.sh`, revisar o log e a orientação de recuperação), e
      aguardar avaliação explícita antes da Fase 5.

## 9. Fase 5 — Validação (QEMU antes de hardware real)

- [x] 9.1 Montar a matriz de teste completa: {UEFI, BIOS legado} ×
      {replace, dual-boot}, cobrindo todos os cenários descritos nas specs
      `disk-provisioning`, `boot-staging`, `linux-install-payload` e
      `install-wizard` (delta) desta mudança. (`TEST_MATRIX.md` — 15 casos,
      nenhum executado ainda, coluna "Resultado real" em aberto.)
- [ ] 9.2 Rodar a matriz em QEMU (OVMF para UEFI, SeaBIOS para BIOS
      legado), incluindo o caso de disco de sistema recusado e o de
      confirmação cancelada. **Não executável nesta sessão** — sem acesso a
      QEMU/ambiente de execução Linux; bloqueado também pelos binários GRUB
      pendentes (ver 3.3/3.4).
- [ ] 9.3 Validar em pelo menos uma máquina física real por tipo de
      firmware (UEFI e BIOS legado), documentando quirks encontrados
      (Secure Boot, Fast Boot, CSM) como itens de acompanhamento — não
      necessariamente resolvidos nesta proposta. **Não executável nesta
      sessão** — precisa de hardware físico e de 9.2 já ter passado.
- [x] 9.4 Atualizar `ROADMAP.md` com o resultado da validação e o estado
      real do pipeline após esta mudança. (Seção 1 reescrita: o que foi
      corrigido nesta mudança, o que está bloqueado — binários GRUB — e o
      que nunca foi executado — QEMU/hardware real.)

## 10. Fase 5 — Checkpoint / Conclusão

- [ ] 10.1 **Resumir a Fase 5**: resultado completo da matriz de validação
      (cada combinação UEFI/BIOS legado × replace/dual-boot, QEMU e
      hardware real), quirks de firmware encontrados (Secure Boot, Fast
      Boot, CSM) e se ficaram como itens de acompanhamento, e o estado
      final do pipeline vs. o que o `ROADMAP.md` previa.
- [ ] 10.2 **PARAR.** Apresentar o resumo de 10.1 e aguardar avaliação
      explícita antes de considerar esta mudança pronta para arquivar
      (`/opsx:archive`). Esta é a primeira vez que o pipeline é exposto
      como "funcional" — tratar como um marco que merece revisão humana
      deliberada, não uma formalidade.
