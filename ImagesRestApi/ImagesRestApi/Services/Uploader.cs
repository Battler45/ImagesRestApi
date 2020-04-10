using ImagesRestApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ImagesRestApi.Services.Interfaces;

namespace ImagesRestApi.Services
{
    public class Uploader : IUploader
    {
        private readonly HttpClient _httpClient;
        public Uploader(HttpClient client)
        {
            _httpClient = client;
        }

        #region Upload item
        public async Task<byte[]> UploadContent(string url)
        {
            using var result = await _httpClient.GetAsync(url);
            if (!result.IsSuccessStatusCode) throw new HttpRequestException();
            return await result.Content.ReadAsByteArrayAsync();
        }
        public async Task<File> UploadFile(string url) => new File()
        {
            Content = await UploadContent(url)
        };
        #endregion
        #region Upload many items
        /*
        foreach (var url in urls)
        {
            using var result = await httpClient.GetAsync(url);
            if (result.IsSuccessStatusCode)
                files.Add(new File() { Content = await result.Content.ReadAsByteArrayAsync() });
            else
                return BadRequest("");
        }
        */
        public async Task<byte[][]> UploadContents(IEnumerable<string> urls)
        {
            var requests = await Task.WhenAll(urls.Select(url => _httpClient.GetAsync(url)).ToArray());
            if (requests.Any(r => !r.IsSuccessStatusCode)) throw new HttpRequestException();
            var contents = await Task.WhenAll(requests.Select(request => request.Content.ReadAsByteArrayAsync()));
            return contents;
        }
        public async Task<List<File>> UploadFiles(IEnumerable<string> urls)
        {
            var requests = await Task.WhenAll(urls.Select(url => _httpClient.GetAsync(url)).ToArray());
            if (requests.Any(r => !r.IsSuccessStatusCode)) throw new HttpRequestException();
            var files = await Task.WhenAll(requests.Select(async request => new File()
            {
                Content = await request.Content.ReadAsByteArrayAsync()
            }));
            return files.ToList();
        }
        #endregion
    }
}
