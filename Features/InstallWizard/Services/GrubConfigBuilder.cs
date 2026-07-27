using System.Text;

namespace LinuxHub.Features.InstallWizard.Services
{
    /// <summary>
    /// Gera o grub.cfg de staging que boota a ISO via loopback (spec boot-staging —
    /// "Bootar a ISO da distro via loopback"). Lógica pura de texto, sem I/O.
    /// Usa <c>search --file</c> para localizar a ISO e o bootmgr do Windows em vez de
    /// numeração de disco/partição assumida — mesmo princípio de D3 (design.md): nunca
    /// um índice fixo, sempre uma busca real. Ver design.md, Open Questions, sobre a
    /// decisão de não reaproveitar o motor de boot do Ventoy.
    /// </summary>
    public static class GrubConfigBuilder
    {
        public static string BuildConfig(string distroName, string isoWindowsPath, bool includeWindowsChainload)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(distroName);
            ArgumentException.ThrowIfNullOrWhiteSpace(isoWindowsPath);

            var sb = new StringBuilder();
            sb.AppendLine("set timeout=10");
            sb.AppendLine("set default=0");
            sb.AppendLine();
            sb.Append(BuildIsoBootEntry(distroName, isoWindowsPath));

            if (includeWindowsChainload)
            {
                sb.AppendLine();
                sb.Append(BuildWindowsChainloadEntry());
            }

            return sb.ToString();
        }

        internal static string BuildIsoBootEntry(string distroName, string isoWindowsPath)
        {
            string isoPath = ToGrubPath(isoWindowsPath);

            return $@"menuentry ""Instalar {distroName} (staging LinuxHub)"" {{
    insmod part_gpt
    insmod part_msdos
    insmod ntfs
    insmod loopback
    insmod iso9660
    set isofile=""{isoPath}""
    search --no-floppy --file --set=root $isofile
    loopback loop $isofile
    linux (loop)/casper/vmlinuz boot=casper iso-scan/filename=$isofile noeject noprompt splash ---
    initrd (loop)/casper/initrd
}}
";
        }

        internal static string BuildWindowsChainloadEntry() => @"menuentry ""Windows"" {
    insmod part_msdos
    insmod ntfs
    search --no-floppy --file --set=root /bootmgr
    chainloader +1
}
";

        /// <summary>
        /// Converte um caminho absoluto do Windows (<c>C:\Users\...\ubuntu.iso</c>) no
        /// caminho unix-style que o GRUB usa dentro do volume localizado por
        /// <c>search --file</c> (<c>/Users/.../ubuntu.iso</c>) — GRUB não conhece letras
        /// de unidade, só caminhos relativos à raiz do volume.
        /// </summary>
        internal static string ToGrubPath(string windowsAbsolutePath)
        {
            string path = windowsAbsolutePath.Replace('\\', '/');

            int colon = path.IndexOf(':');
            if (colon >= 0)
                path = path[(colon + 1)..];

            return path.StartsWith('/') ? path : "/" + path;
        }
    }
}
