using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Extensions
{
    public static class HttpClientUtils
    {
        public static async Task DownloadFileTaskAsync(this HttpClient client, Uri uri, string FileName)
        {
            using (var s = await client.GetStreamAsync(uri))
            {
                
                using (var fs = new FileStream(FileName, FileMode.Create))
                {
                    await s.CopyToAsync(fs);
                }
            }
        }

        public static async Task<Ty> DownloadToJson<Ty>(this HttpClient client, Uri uri)
        {
            var response = await client.GetAsync(uri);
     
            if (response != null && response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Ty? finalObject = JsonConvert.DeserializeObject<Ty>(result);
                if (finalObject == null) throw new Exception($"unable to convert response to valid package object. Response was {result}");
                return finalObject;
            }
            throw new Exception($"received [Status:{response?.StatusCode}] when attempting to retrieve resource at {uri.OriginalString}");
        }
    }
}
