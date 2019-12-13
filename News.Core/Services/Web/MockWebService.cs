using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Reflection;

namespace News.Core.Services.Web
{
    public class MockWebService : IWebService
    {
        // Resource name array
        string[] resourceNames;

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

                //if (!String.IsNullOrEmpty(resourceName))
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
            catch (Exception ex)
            {
                Debug.WriteLine("\tERROR {0}", ex.Message);
                return content;
            }
        }

        /// <summary>
        /// Getting image data from web page
        /// </summary>
        public Task<byte[]> GetImageAsync(string url)
        {
            throw new NotImplementedException();
        }
    }
}
