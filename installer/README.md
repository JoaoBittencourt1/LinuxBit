# installer/

Payload Linux-side do pipeline de instalação (executa depois do reboot, no
ambiente live staged pelo `boot-staging`). Ver `openspec/changes/
ubuntu-install-pipeline/design.md` para o raciocínio completo; este arquivo
cobre o que fazer quando algo dá errado no meio do caminho — spec
`linux-install-payload`.

## Visão geral do pipeline

```
Windows (app)                    Linux (boot staged, sem USB)
──────────────                   ─────────────────────────────
1. Wizard: escolhe disco/         5. install.sh carrega install.conf
   modo/distro, confirma             (revalida contra o hardware real
2. Shrink NTFS (se dual-boot)        antes de tocar no disco)
3. Boot-staging: grava GRUB2      6. lib/disk.sh: particiona de verdade
   na ESP/MBR, grub.cfg          7. lib/mount.sh: monta raiz/swap/ESP
4. Usuário reinicia manualmente   8. distros/ubuntu.sh: debootstrap
                                   9. lib/chroot.sh: locale/timezone/keymap
                                  10. lib/user.sh: cria usuário (chpasswd)
                                  11. lib/boot.sh: grub-install + update-grub
```

**Ponto de não-retorno real**: o reboot (passo 4), não o clique em
"Instalar" no Windows. Tudo antes disso é reversível (shrink é uma operação
padrão do Windows; boot-staging só adiciona uma entrada, não remove nada).
Tudo depois do particionamento real (passo 6) mexe em dados de verdade.

## O que acontece se falhar em cada ponto

| Onde | O que acontece | Recuperável? |
|---|---|---|
| Shrink (Windows, passo 2) falha | `DiskPartitioningService` lança exceção com a saída do `diskpart`; nada no disco muda (diskpart só executa se todos os comandos do script forem aceitos) | Sim — Windows continua exatamente como estava, o usuário tenta de novo |
| Boot-staging (Windows, passo 3) falha | `BootStagingService` lança exceção antes ou durante a gravação; no caso BIOS, o MBR original já foi salvo em backup (`MbrBackupService.BackupMbr`) antes de qualquer escrita | Sim — Windows Boot Manager não foi alterado (UEFI) ou pode ser restaurado do backup do MBR (BIOS legado); o usuário nunca reiniciou, então nada mudou na prática |
| Usuário nunca reinicia após o passo 3 | Nada — o shrink já é permanente (espaço não alocado sobra, inofensivo) e a entrada de boot staged só é usada se o usuário escolher reiniciar nela | Sim, trivialmente — o PC continua bootando Windows normalmente por padrão |
| `install.sh` falha antes do particionamento (passos 5, `revalidate_plan`) | Aborta com `fatal()` antes de qualquer escrita no disco alvo | Sim — reiniciar e voltar ao Windows normalmente; nada foi tocado |
| `install.sh` falha **durante** o particionamento/formatação (passo 6) | Tabela de partição do disco alvo pode estar parcialmente escrita | **Não há rollback automático** (limitação conhecida, ver `design.md` Risks) — recuperação manual, ver abaixo |
| `install.sh` falha depois do particionamento, antes de concluir (passos 7–11) | Disco já particionado/formatado, sistema base pode estar parcialmente instalado | Recuperação manual a partir do log — ver abaixo; o disco alvo não volta a ser utilizável por Windows nesse ponto (replace) ou tem uma partição Linux incompleta (dual-boot) |
| `install.sh` conclui, mas o usuário não confia no resultado | `lib/boot.sh` já rodou `update-grub`; dual-boot deveria ter uma entrada do Windows no menu do GRUB definitivo (avisado no log se não encontrar) | Reiniciar e verificar; se o Windows não aparecer no menu, ele ainda existe no disco (dual-boot nunca apaga partições existentes) — corrigível com `os-prober`/`update-grub` manual a partir de um live USB comum |

## Recuperando de uma falha no meio de `install.sh`

Todo passo relevante loga início **e** conclusão em `/var/log/
linuxhub-install.log` (`log()`/`fatal()` em `install.sh`) — a última linha
`[LINUXHUB] ...` do arquivo é o último passo que começou a rodar; se não
houver uma linha de conclusão correspondente logo depois, foi ali que
travou ou falhou. `fatal()` sempre grava `ERRO: <motivo>` antes de abortar —
nunca falha silenciosamente (constitution §6).

O ambiente live (staged na Fase 2, sem USB) continua disponível depois de
qualquer falha — o computador não perde acesso a um shell:

1. Reinicie e escolha de novo a entrada de boot de staging do LinuxHub (ela
   continua lá; nada nesse fluxo a remove).
2. No ambiente live, abra um terminal e leia
   `/var/log/linuxhub-install.log` para ver o último passo concluído.
3. Se falhou antes do particionamento: nada foi tocado, é seguro reiniciar
   normalmente de volta pro Windows e tentar a instalação de novo.
4. Se falhou durante/depois do particionamento: o disco alvo está num
   estado intermediário. Não há automação de rollback nesta versão —
   diagnostique manualmente com `lsblk`, `parted print`, `mount` a partir do
   ambiente live, usando o log para saber até onde o `install.sh` chegou.
   Rodar `install.sh` de novo do zero é seguro no modo **replace** (ele
   sempre recomeça do `wipefs`); no modo **dual-boot**, verifique antes se a
   partição Linux já criada deve ser reaproveitada ou apagada manualmente,
   já que `setup_dualboot` não detecta uma partição LinuxHub anterior.

## Assets pendentes

`installer/` em si (bash) está completo para Ubuntu. O que falta para o
pipeline funcionar de ponta a ponta é externo a este diretório:

- `Assets/Grub/{uefi,bios}/*` (binários GRUB2 pré-compilados) — ver
  `Assets/Grub/README.md`.
- Validação em QEMU/hardware real — nunca executada nesta sessão (sem
  acesso a um ambiente de execução Linux); ver `openspec/changes/
  ubuntu-install-pipeline/tasks.md`, tarefas 3.8/5.8/9.2/9.3.
