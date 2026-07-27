namespace LinuxHub.Features.InstallWizard.Services
{
    /// <summary>
    /// Lê a tabela de partição MBR clássica (offset 446, 4 entradas de 16 bytes) para achar
    /// o "gap" pós-MBR onde o <c>core.img</c> do GRUB é embutido em sistemas BIOS legado —
    /// o espaço entre o setor 1 (logo após o MBR) e o LBA de início da primeira partição.
    /// Lógica pura, sem I/O — a leitura do disco físico é responsabilidade de
    /// <see cref="IMbrBackupService"/>.
    /// </summary>
    public static class MbrPartitionTableReader
    {
        private const int PartitionTableOffset = 446;
        private const int PartitionEntrySize = 16;
        private const int PartitionEntryCount = 4;
        private const int TypeOffsetWithinEntry = 4;
        private const int StartLbaOffsetWithinEntry = 8;

        /// <summary>
        /// Retorna o menor LBA de início entre as partições MBR ocupadas (tipo != 0x00),
        /// ou <c>null</c> se nenhuma partição estiver definida. <paramref name="mbr"/>
        /// precisa ter pelo menos 512 bytes.
        /// </summary>
        public static uint? FindFirstPartitionStartLba(byte[] mbr)
        {
            ArgumentNullException.ThrowIfNull(mbr);
            if (mbr.Length < 512)
                throw new ArgumentException("MBR precisa ter pelo menos 512 bytes.", nameof(mbr));

            uint? smallest = null;

            for (int i = 0; i < PartitionEntryCount; i++)
            {
                int entryOffset = PartitionTableOffset + i * PartitionEntrySize;
                byte type = mbr[entryOffset + TypeOffsetWithinEntry];

                if (type == 0x00)
                    continue;

                uint startLba = BitConverter.ToUInt32(mbr, entryOffset + StartLbaOffsetWithinEntry);

                if (startLba == 0)
                    continue;

                if (smallest is null || startLba < smallest)
                    smallest = startLba;
            }

            return smallest;
        }

        /// <summary>
        /// Quantos setores de 512 bytes estão livres entre o LBA 1 (logo após o MBR) e o
        /// início da primeira partição — onde o <c>core.img</c> é embutido. Retorna 0 se não
        /// houver nenhuma partição (não deveria acontecer num disco Windows real) ou se a
        /// primeira partição começar no LBA 1 (sem gap).
        /// </summary>
        public static uint GapSectorsAfterMbr(byte[] mbr)
        {
            uint? firstPartitionStart = FindFirstPartitionStartLba(mbr);
            if (firstPartitionStart is null || firstPartitionStart <= 1)
                return 0;

            return firstPartitionStart.Value - 1;
        }
    }
}
