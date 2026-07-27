using LinuxHub.Features.InstallWizard.Services;
using Xunit;

namespace LinuxHub.Tests.Features.InstallWizard.Services
{
    public class MbrPartitionTableReaderTests
    {
        private static byte[] BuildMbr(params (byte type, uint startLba)[] partitions)
        {
            var mbr = new byte[512];

            for (int i = 0; i < partitions.Length && i < 4; i++)
            {
                int entryOffset = 446 + i * 16;
                mbr[entryOffset + 4] = partitions[i].type;
                BitConverter.GetBytes(partitions[i].startLba).CopyTo(mbr, entryOffset + 8);
            }

            return mbr;
        }

        [Fact]
        public void FindFirstPartitionStartLba_ReturnsSmallestAmongOccupiedEntries()
        {
            byte[] mbr = BuildMbr((0x07, 2048), (0x0C, 4096));

            Assert.Equal(2048u, MbrPartitionTableReader.FindFirstPartitionStartLba(mbr));
        }

        [Fact]
        public void FindFirstPartitionStartLba_IgnoresEmptyEntries()
        {
            byte[] mbr = BuildMbr((0x00, 0), (0x07, 2048));

            Assert.Equal(2048u, MbrPartitionTableReader.FindFirstPartitionStartLba(mbr));
        }

        [Fact]
        public void FindFirstPartitionStartLba_ReturnsNullWhenNoPartitions()
        {
            byte[] mbr = new byte[512];

            Assert.Null(MbrPartitionTableReader.FindFirstPartitionStartLba(mbr));
        }

        [Fact]
        public void GapSectorsAfterMbr_ModernWindowsAlignment_Returns2047()
        {
            // Alinhamento de 1MiB padrão desde o Windows Vista SP1 — partição 1 em LBA 2048.
            byte[] mbr = BuildMbr((0x07, 2048));

            Assert.Equal(2047u, MbrPartitionTableReader.GapSectorsAfterMbr(mbr));
        }

        [Fact]
        public void GapSectorsAfterMbr_LegacyAlignment_ReturnsSmallGap()
        {
            // Alinhamento antigo (pré-Vista), partição 1 em LBA 63 — típico de discos antigos.
            byte[] mbr = BuildMbr((0x07, 63));

            Assert.Equal(62u, MbrPartitionTableReader.GapSectorsAfterMbr(mbr));
        }

        [Fact]
        public void GapSectorsAfterMbr_NoGap_ReturnsZero()
        {
            byte[] mbr = BuildMbr((0x07, 1));

            Assert.Equal(0u, MbrPartitionTableReader.GapSectorsAfterMbr(mbr));
        }
    }
}
