# Matriz de validação — ubuntu-install-pipeline

Tarefa 9.1 (tasks.md). Cobre os cenários das specs `disk-provisioning`,
`boot-staging`, `linux-install-payload` e `install-wizard` (delta) desta
mudança. Nenhuma linha desta matriz foi executada nesta sessão — não há
acesso a QEMU/hardware neste ambiente (Windows sem WSL/toolchain Linux, ver
`Assets/Grub/README.md`). Preencher a coluna Resultado ao rodar cada caso.

## Pré-requisito bloqueante

Todas as linhas de UEFI/BIOS dependem de `Assets/Grub/uefi/grubx64.efi` e
`Assets/Grub/bios/boot.img` existirem (ver `Assets/Grub/README.md`) — sem
eles, `GrubAssetProvider` lança `FileNotFoundException` antes de qualquer
caso poder ser executado.

## Matriz principal — firmware × modo

| # | Firmware | Modo | Passos cobertos | Resultado esperado | Resultado real |
|---|---|---|---|---|---|
| 1 | UEFI (OVMF) | Replace | shrink N/A → boot-staging (ESP+BCD) → reboot → `install.sh` (wipe+GPT+ESP) → debootstrap → grub-install UEFI | Ubuntu instalado, boota direto (sem entrada Windows) | — |
| 2 | UEFI (OVMF) | Dual-boot | shrink NTFS → boot-staging (ESP+BCD) → reboot → `install.sh` (partição no espaço livre) → debootstrap → grub-install UEFI + os-prober | Ubuntu instalado, GRUB lista Windows e Ubuntu, os dois bootam | — |
| 3 | BIOS legado (SeaBIOS) | Replace | boot-staging (MBR+backup) → reboot → `install.sh` (wipe+GPT+bios_grub) → debootstrap → grub-install BIOS | Ubuntu instalado, boota direto | — |
| 4 | BIOS legado (SeaBIOS) | Dual-boot | shrink NTFS → boot-staging (MBR+backup) → reboot → `install.sh` (partição no espaço livre) → debootstrap → grub-install BIOS + os-prober | Ubuntu instalado, GRUB lista Windows e Ubuntu, os dois bootam | — |

## Casos de recusa/cancelamento (spec install-wizard / disk-provisioning)

| # | Cenário | Resultado esperado | Resultado real |
|---|---|---|---|
| 5 | Modo replace selecionado no disco de sistema em uso (ex.: VM com um disco só) | **Permitido** (revisado — ver design.md D2): `TargetSelectionViewModel.IsReplacingSystemDisk=true` liga um `InfoBar` de aviso (`Wizard_ReplaceSystemDiskWarningMessage`) e a confirmação usa o texto mais forte (`Wizard_ConfirmReplaceSystemDiskSummary`), mas a seleção e a instalação prosseguem normalmente | — |
| 6 | Usuário cancela a confirmação destrutiva (`ConfirmationViewModel.Cancelled`) | `PendingConfirmation` volta a `null`; nenhuma operação de disco/boot executada | — |
| 7 | Modo replace, usuário não digita a palavra de confirmação corretamente | `ConfirmationViewModel` não dispara `Confirmed`; instalação não prossegue | — |

## Boot-staging isolado (3.8 — antes de envolver o payload Linux)

| # | Cenário | Resultado esperado | Resultado real |
|---|---|---|---|
| 8 | UEFI: entrada de staging aparece no firmware boot menu | `bcdedit /enum` mostra a entrada criada por `AddFirmwareBootEntry`, Windows Boot Manager continua presente | — |
| 9 | UEFI: selecionar a entrada de staging carrega o GRUB e boota a ISO via loopback | Ambiente live do Ubuntu (casper) inicia | — |
| 10 | BIOS legado: MBR original é salvo antes da escrita | Arquivo de backup existe e tem 512 bytes antes de `WriteBootCode` rodar | — |
| 11 | BIOS legado: selecionar "Windows" no menu do GRUB volta pro Windows | Windows inicializa normalmente via chainload `+1` | — |

## Falha simulada (Fase 4, tarefa 8.2 — não é sobre sucesso, é sobre recuperação)

| # | Cenário | Resultado esperado | Resultado real |
|---|---|---|---|
| 12 | Interromper QEMU no meio de `lib/disk.sh` (após `wipefs`, antes de `mkfs`) | `/var/log/linuxhub-install.log` mostra "Apagando tabela de partição..." sem uma linha de conclusão logo depois — orientação de `installer/README.md` permite diagnosticar | — |
| 13 | Interromper QEMU no meio do `debootstrap` | Log mostra "Rodando debootstrap..." como última linha; reboot volta pro ambiente live staged, não perde acesso a shell | — |

## Hardware real (tarefa 9.3 — depois da matriz em QEMU fechar)

| # | Cenário | Resultado esperado | Resultado real / quirks |
|---|---|---|---|
| 14 | 1 máquina física UEFI | Mesmo resultado dos casos 1/2, documentando Secure Boot/Fast Boot | — |
| 15 | 1 máquina física BIOS legado | Mesmo resultado dos casos 3/4, documentando CSM | — |

## Como preencher esta matriz

1. Gerar `Assets/Grub/{uefi,bios}/*` (bloqueante — ver README do diretório).
2. Rodar os casos 1–4 e 8–11 em QEMU (`qemu-system-x86_64` com `-bios
   OVMF.fd` para UEFI, sem `-bios` para SeaBIOS/BIOS legado).
3. Rodar os casos 5–7 no wizard Windows (não precisa de QEMU).
4. Rodar os casos 12–13 interrompendo o QEMU deliberadamente
   (`Ctrl+Alt+2` no monitor QEMU, ou `quit`) e inspecionando o log.
5. Só depois de 1–13 fecharem, partir para os casos 14–15 em hardware real.
