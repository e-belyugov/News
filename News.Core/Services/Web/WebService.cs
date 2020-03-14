using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net;
using News.Core.Services.Logging;
using System.Drawing;

namespace News.Core.Services.Web
{
    /// <summary>
    /// Web service
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
                (request as HttpWebRequest).UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";

                //IWebProxy proxy = HttpWebRequest.GetSystemWebProxy();
                //if (proxy != null)
                //{
                //    string proxyuri = proxy.GetProxy(request.RequestUri).ToString();
                //    if (!proxyuri.Contains(url))
                //    {
                //        request.UseDefaultCredentials = true;
                //        request.Proxy = new WebProxy(proxyuri, false)
                //        {
                //            Credentials = new NetworkCredential("e_belyugov", "CrystalPalace22")
                //        };
                //    }
                //}
                request.Proxy = null;
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
                    //_logger.Info("--- WEBSERVICE : Begin GETRESPONSE");
                    request.Proxy = null;
                    using (var response = await request.GetResponseAsync())
                    {
                        //_logger.Info("--- WEBSERVICE : End GETRESPONSE");

                        HttpWebResponse webResponse = response as HttpWebResponse;

                        //_logger.Info("--- WEBSERVICE : Begin GETRESPONSESTREAM");
                        using (var stream = response.GetResponseStream())
                        {
                            //using (var reader = new StreamReader(stream, encoding))
                            //{
                            //    content = reader.ReadToEnd();
                            //}
                            //_logger.Info("--- WEBSERVICE : End GETRESPONSESTREAM");

                            if (stream != null)
                                using (var buffer = new BufferedStream(stream))
                                {
                                    //_logger.Info("--- WEBSERVICE : Begin READING");
                                    using (StreamReader reader = new StreamReader(buffer, encoding))
                                    {
                                        content = reader.ReadToEnd();
                                    }
                                    //_logger.Info("--- WEBSERVICE : End READING");
                                }
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
