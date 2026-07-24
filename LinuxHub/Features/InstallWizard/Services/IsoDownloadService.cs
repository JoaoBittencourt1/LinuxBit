using System.IO;
using System.Net.Http;
using LinuxHub.Common.Models;

namespace LinuxHub.Features.InstallWizard.Services
{
    public sealed class IsoDownloadService : IIsoDownloadService
    {
        public async Task<string> DownloadAsync(DistroInfo distro, IProgress<IsoDownloadProgress> progress, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(distro);

            string baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LinuxHub", "ISOs"
            );
            Directory.CreateDirectory(baseDir);

            string downloadPath = Path.Combine(baseDir, $"{distro.Id}.iso");

            try
            {
                using var client = new HttpClient();
                using var response = await client.GetAsync(
                    distro.DirectDownloadLink,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 1L;
                var buffer = new byte[8192];
                long totalRead = 0;
                var startTime = DateTime.Now;

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var fileStream = File.Create(downloadPath);

                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalRead += bytesRead;

                    double percent = (double)totalRead / totalBytes * 100;
                    var elapsed = DateTime.Now - startTime;
                    double speed = totalRead / elapsed.TotalSeconds;
                    double remainingSeconds = (totalBytes - totalRead) / speed;

                    progress.Report(new IsoDownloadProgress(percent, remainingSeconds));
                }

                return downloadPath;
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(downloadPath))
                    File.Delete(downloadPath);
                throw;
            }
        }
    }
}
