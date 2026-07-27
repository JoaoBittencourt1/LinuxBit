# Assets/Grub

Binários GRUB2 pré-compilados consumidos por `IGrubAssetProvider`
(`Features/InstallWizard/Services/GrubAssetProvider.cs`). Não são gerados pelo
app em runtime — não há toolchain GRUB (`grub-mkstandalone`, `grub-bios-setup`)
nativo no Windows; foram gerados uma vez via WSL/Ubuntu (pacotes
`grub-efi-amd64-bin`, `grub-pc-bin`, `grub-common`, `grub2-common`) em
2026-07-27 e commitados aqui.

## `uefi/grubx64.efi` — presente, funcional

Imagem GRUB standalone para x86_64-efi (6.7MB), com os módulos `part_gpt`,
`part_msdos`, `ntfs`, `loopback`, `iso9660`, `search`, `chain`, `fat`, `normal`,
`linux`, `configfile` embutidos — não depende de nenhum diretório
`/boot/grub/x86_64-efi` separado na ESP, só do `grub.cfg` que
`BootStagingService` escreve ao lado dele. Gerada com:

```sh
grub-mkstandalone \
  --format=x86_64-efi \
  --output=grubx64.efi \
  --modules="part_gpt part_msdos ntfs loopback iso9660 search chain fat normal linux configfile"
```

Um `.efi` standalone é auto-contido — isso é o que torna o caminho UEFI
testável numa VM comum (VirtualBox EFI, Hyper-V Gen2, VMware EFI) hoje.

## `bios/boot.img` + `bios/core.img` — presentes, embutimento automatizado

Ao contrário da primeira versão deste README, o embutimento do `core.img` no
gap pós-MBR **agora é automatizado** (`MbrPartitionTableReader` +
`MbrBackupService.WriteCoreImageToGap`, chamados por
`BootStagingService.InstallBios`). Como isso foi decidido:

Rodei o `grub-bios-setup` real (via WSL, contra um disco sintético — `losetup`
+ `parted`, formato MBR comum, sem partição `bios_grub` dedicada) e comparei
byte a byte o MBR resultante contra o `boot.img` de fábrica
(`/usr/lib/grub/i386-pc/boot.img`) para entender exatamente o que a ferramenta
real muda:

1. **`core.img` é embutido a partir do LBA 1** (logo após o MBR) — não numa
   posição calculada de forma mais sofisticada; é literalmente "primeiro
   espaço livre depois do setor de boot".
2. **Só dois bytes do `boot.img` mudam**, independente de onde o embutimento
   acontece: offset 102–103 (0-indexed), de `EB 05` (`jmp short +5`) para
   `90 90` (`NOP NOP`) — isso "ativa" o caminho de carregamento via
   `core.img` embutido (presente em toda instalação real, com ou sem
   `--no-rs-codes`, com ou sem partição `bios_grub`).
3. O campo `kernel_sector` (offset 92, `grub_uint64_t` little-endian) que
   `grub-bios-setup` normalmente patcha com o LBA do embutimento **já vem
   como `1` no `boot.img` de fábrica** — ou seja, como sempre embutimos a
   partir do LBA 1 (ponto 1), esse campo não precisa de patch em runtime.

Por isso, `Assets/Grub/bios/boot.img` já é o resultado final (440 bytes, com
o NOP aplicado) — não o `boot.img` cru do pacote — e não precisa de nenhum
patch adicional em C# antes de ser escrito no MBR real.

`Assets/Grub/bios/core.img` foi gerado com prefixo genérico (não fixo a
nenhum disco/partição de teste):

```sh
grub-mkimage -O i386-pc -o core.img -p '/boot/grub' \
  biosdisk part_msdos part_gpt ntfs fat search search_fs_file normal configfile linux
```

`BootStagingService.EnsurePostMbrGapFitsCoreImage` lê o MBR real do disco
alvo, calcula o gap (`MbrPartitionTableReader.GapSectorsAfterMbr`) e **aborta
antes de qualquer escrita** se ele for pequeno demais — discos com
alinhamento pré-Vista (partição 1 no LBA 63, ~31KB de gap) não cabem os
~144KB do `core.img`; discos modernos (alinhamento de 1MiB, LBA 2048, ~1MB de
gap) cabem com folga.

## Estado atual

Ambos os caminhos (UEFI e BIOS legado) têm todo o código e os assets
necessários. **Nada disso foi validado por um boot real** — a comparação
byte a byte contra o `grub-bios-setup` real (WSL, disco sintético em loop
device) é a validação mais forte disponível sem QEMU/hardware, mas não
substitui testar de verdade. Ver `TEST_MATRIX.md`.
