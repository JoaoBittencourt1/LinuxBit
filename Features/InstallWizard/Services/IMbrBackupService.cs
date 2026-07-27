namespace LinuxHub.Features.InstallWizard.Services
{
    /// <summary>
    /// Backup e escrita do setor MBR de um disco físico, para instalação do GRUB2 em
    /// sistemas BIOS legado. Ver spec boot-staging — "Instalar bootloader de chainload em
    /// sistemas BIOS legado".
    /// </summary>
    public interface IMbrBackupService
    {
        /// <summary>
        /// Lê os 512 bytes do MBR do disco indicado e salva em <paramref name="backupPath"/>,
        /// de forma recuperável, antes de qualquer escrita (spec — "Backup do MBR é criado
        /// antes da escrita").
        /// </summary>
        string BackupMbr(int diskIndex, string backupPath);

        /// <summary>
        /// Sobrescreve apenas os 440 bytes de código de boot do MBR (o conteúdo de
        /// <paramref name="bootCodeFilePath"/>, que precisa ter exatamente 440 bytes),
        /// preservando a tabela de partição (offset 446-510) e a assinatura 0x55AA —
        /// nunca reescreve o disco inteiro nem a tabela de partição.
        /// </summary>
        void WriteBootCode(int diskIndex, string bootCodeFilePath);

        /// <summary>Restaura o MBR original a partir de um backup feito por <see cref="BackupMbr"/>.</summary>
        void RestoreMbr(int diskIndex, string backupPath);

        /// <summary>Lê os 512 bytes atuais do MBR do disco indicado (para
        /// <see cref="MbrPartitionTableReader"/> localizar o gap pós-MBR antes de embutir o
        /// <c>core.img</c> — nunca escreve nada).</summary>
        byte[] ReadMbr(int diskIndex);

        /// <summary>
        /// Escreve o conteúdo de <paramref name="coreImageFilePath"/> a partir do LBA 1
        /// (logo após o MBR) — o gap pós-MBR onde o <c>core.img</c> do GRUB é embutido em
        /// sistemas BIOS legado. Chamado só depois de <see cref="MbrPartitionTableReader"/>
        /// confirmar que o gap é grande o bastante; nunca sobrescreve o MBR (setor 0) nem
        /// qualquer partição existente.
        /// </summary>
        void WriteCoreImageToGap(int diskIndex, string coreImageFilePath);
    }
}
