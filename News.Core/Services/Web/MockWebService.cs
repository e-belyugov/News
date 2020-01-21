using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Reflection;
using System.Resources;
using News.Core.Services.Logging;


namespace News.Core.Services.Web
{
    /// <summary>
    /// Mock web service
    /// </summary>
    public class MockWebService : IWebService
    {
        // Logger
        private readonly ILogger _logger;

        // Resource name array
        string[] resourceNames;

        /// <summary>
        /// Constructor
        /// </summary>
        public MockWebService(ILogger logger)
        {
            _logger = logger;
        }

        // Checking if resource exists
        private bool ResourceExists(string resourceName)
        {
            if (resourceNames == null) resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            int pos = Array.IndexOf(resourceNames, resourceName);
            bool exists = pos > -1 ? true : false;
            return exists;
        }

        /// <summary>
        /// Getting string data from web page
        /// </summary>
        public async Task<string> GetDataAsync(string url, Encoding encoding)
        {
            string content = "";
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(WebService)).Assembly;

                url = url.Replace("http://","").Replace("https://", "").Replace("/", "").Replace(".ru", "").Replace(".html","");
                string resourceName = "News.Core.Services.Web.TestData." + url + ".html";

                if (ResourceExists(resourceName))
                {
                    Stream stream = assembly.GetManifestResourceStream(resourceName);
                    using (var reader = new StreamReader(stream, encoding))
                    {
                        content = await reader.ReadToEndAsync();
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
            byte[] image = null;
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(WebService)).Assembly;

                url = url.Replace("http://", "").Replace("https://", "").Replace("/", "").Replace(".ru", "").Replace(".jpg", "");
                string resourceName = "News.Core.Services.Resources.TestData." + url + ".jpg";

                if (ResourceExists(resourceName))
                {
                    using (var stream = await Task.Run(() => assembly.GetManifestResourceStream(resourceName)))
                    {
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        image = buffer;
                    }
                }

                return image;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return image;
            }
        }
    }
}
