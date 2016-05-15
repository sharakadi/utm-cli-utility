using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace UtmCliUtility
{
    public class TransportWrapper
    {
        public Uri Address { get; private set; }

        private Lazy<string> _version;
        public string Version
        {
            get
            {
                return _version.Value;
            }
        }

        public string GetPageContent(Uri uri, Encoding encoding = null)
        {
            var wc = new WebClient();
            var mainPage = wc.DownloadData(uri);
            var enc = encoding ?? Encoding.UTF8;
            return WebUtility.HtmlDecode(enc.GetString(mainPage));
        }

        public IEnumerable<Uri> ParseXsdUrls(string pageContent)
        {
            var urlRegex = new Regex(@"(?<url>info\/(\w.?(\/)?)*\.xsd)");
            var matches = urlRegex.Matches(pageContent);
            foreach (Match m in matches)
            {
                var g = m.Groups["url"];
                if (g.Captures.Count > 0)
                {
                    var builder = new UriBuilder();
                    builder.Host = Address.Host;
                    builder.Port = Address.Port;
                    builder.Path = g.Captures[0].Value;
                    yield return builder.Uri;
                }
            }
        }

        public string ParseXsdFromWebPage(string pageContent)
        {
            int start = pageContent.IndexOf("<pre>");
            int end = pageContent.LastIndexOf("</pre>");
            if (end <= start) return null;
            return pageContent.Substring(start + 5, end - start - 5);
        }

        private string GetTransportVersion()
        {
            var versionRegex = new Regex(@"version:([0-9]{1,}\.[0-9]{1,}\.[0-9]{1,}){1}");
            var match = versionRegex.Match(GetPageContent(Address));
            if (match.Groups.Count == 0) return null;
            var capture = match.Groups[1].Captures.Cast<Capture>().First();
            if (capture == null) return null;
            return capture.Value;
        }

        public TransportWrapper(string url)
        {
            Address = new Uri(url);
            _version = new Lazy<string>(() => GetTransportVersion());
        }
    }
}
