using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UtmCliUtility
{
    internal class HttpHelper
    {
        private byte[] UploadData(Uri location, byte[] data)
        {
            var filename = DateTime.Now.Ticks + ".xml";

            var requestContent = new MultipartFormDataContent();
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");

            requestContent.Add(content, "xml_file", filename);
            var client = new HttpClient();
            var responce = client.PostAsync(location, requestContent);
            responce.Start();
            responce.Wait();
            var responceData = responce.Result.Content.ReadAsByteArrayAsync();
            responceData.Start();
            responceData.Wait();
            return responceData.Result;
        }

        private byte[] DownloadData(Uri resource)
        {
            WebClient wc = new WebClient();
            return wc.DownloadData(resource);
        }
    }
}
