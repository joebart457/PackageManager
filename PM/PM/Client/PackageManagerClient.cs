using Newtonsoft.Json;
using PM.Extensions;
using PM.Models.Manifests;
using PM.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Client
{
    internal static class PackageManagerClient
    {
        private static readonly string? _baseUrl = ConfigService.GetConfig("baseUrl");
        private static async Task<Ty> SendGetRequestAsync<Ty>(string resourceUri)
        {
            HttpClient httpClient = new HttpClient();
            return await httpClient.DownloadToJson<Ty>(new Uri($"{_baseUrl}{resourceUri}"));
        }

        private static async Task SendPostRequestAsync<Ty>(string resourceUri, Ty data)
        {
            HttpClient httpClient = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_baseUrl}{resourceUri}"),
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"),
            };

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        public static async Task<PackageManifest> GetAsync(string name, string tag)
        {
            return await SendGetRequestAsync<PackageManifest>($"/packages/{name}/tag/{tag}");
        }

        public static async Task<List<PackageManifest>> GetAllAsync()
        {
            return await SendGetRequestAsync<List<PackageManifest>>("/packages");
        }

        public static async Task<List<string>> GetAllTagsAsync(string name)
        {
            return await SendGetRequestAsync<List<string>>($"/tags/{name}");
        }

        public static async Task PostAsync(PackageManifest manifest)
        {
            await SendPostRequestAsync($"/packages", manifest);
        }

    }
}
