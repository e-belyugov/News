using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net;
using News.Core.Services.Logging;

namespace News.Core.Services.Web
{
    /// <summary>
    /// WebService
    /// </summary>
    public class WebService : IWebService
    {
        // Logger
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public WebService(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Getting image data from web page
        /// </summary>
        private WebRequest GetRequest(string url)
        {
            try
            {
                var request = HttpWebRequest.Create(url);
                IWebProxy proxy = HttpWebRequest.GetSystemWebProxy();
                if (proxy != null)
                {
                    string proxyuri = proxy.GetProxy(request.RequestUri).ToString();
                    request.UseDefaultCredentials = true;
                    request.Proxy = new WebProxy(proxyuri, false);
                    request.Proxy.Credentials = new NetworkCredential("e_belyugov", "CrystalPalace22");
                }
                return request;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return null;
            }
        }

        /// <summary>
        /// Getting string data from web page
        /// </summary>
        public async Task<string> GetDataAsync(string url, Encoding encoding)
        {
            string content = "";
            try
            {
                var request = GetRequest(url);

                if (request != null)
                {
                    using (var response = await request.GetResponseAsync())
                    {
                        HttpWebResponse webResponse = response as HttpWebResponse;
                        using (var stream = response.GetResponseStream())
                        using (var reader = new StreamReader(stream, encoding))
                        {
                            content = reader.ReadToEnd();
                        }
                    }
                }

                return content;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return content;
            }
        }

        /// <summary>
        /// Getting image data from web page
        /// </summary>
        public async Task<byte[]> GetImageAsync(string url)
        {
            try
            {
                byte[] image = null;

                var request = GetRequest(url);

                if (request != null)
                {
                    using (var response = await request.GetResponseAsync())
                    {
                        HttpWebResponse webResponse = response as HttpWebResponse;
                        using (var stream = response.GetResponseStream())
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                stream.CopyTo(ms);
                                image = ms.ToArray();
                            }
                        }
                    }
                }

                return image;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return null;
            }
        }
    }
}
