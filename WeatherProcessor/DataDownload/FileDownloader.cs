using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http;

namespace DataDownload
{
    public class FileDownloader
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task DownloadFileAsync(string fileUrl, string localFilePath)
        {
            try
            {
                // Get the response stream from the URL
                using (Stream responseStream = await _httpClient.GetStreamAsync(fileUrl))
                {
                    // Create a FileStream to write the downloaded content to a local file
                    using (FileStream fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        // Copy the content from the response stream to the file stream
                        await responseStream.CopyToAsync(fileStream);
                    }
                }
                Console.WriteLine($"File downloaded successfully to: {localFilePath}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request error during download: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during file download: {ex.Message}");
            }
        }
    }
}
