using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace LinuxHub.Models
{
    class IsoService
    {
        public async Task DownloadIso(string url, string output)
        {
            using var client = new HttpClient();
            using var stream = await client.GetStreamAsync(url);
            using var file = File.Create(output);
            await stream.CopyToAsync(file);
        }
    }
}
