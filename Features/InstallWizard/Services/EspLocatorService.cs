using System.Management;

namespace LinuxHub.Features.InstallWizard.Services
{
    public sealed class EspLocatorService : IEspLocatorService
    {
        private const string EfiSystemPartitionGptType = "{c12a7328-f81f-11d2-ba4b-00a0c93ec93b}";

        public int? FindEfiSystemPartitionIndex(int diskIndex)
        {
            var scope = new ManagementScope(@"root\Microsoft\Windows\Storage");
            var query = new ObjectQuery(
                $"SELECT PartitionNumber, GptType FROM MSFT_Partition WHERE DiskNumber = {diskIndex}");

            using var searcher = new ManagementObjectSearcher(scope, query);

            foreach (ManagementBaseObject partition in searcher.Get())
            {
                string? gptType = partition["GptType"]?.ToString();

                if (string.Equals(gptType, EfiSystemPartitionGptType, StringComparison.OrdinalIgnoreCase))
                    return Convert.ToInt32(partition["PartitionNumber"]);
            }

            return null;
        }
    }
}
