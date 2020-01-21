using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using News.Core.Services.Logging;
using News.Core.Services.Web;

namespace News.Core.Helpers
{
    /// <summary>
    /// Resource helper
    /// </summary>
    public class ResourceHelper
    {
        // Logger
        private readonly ILogger _logger;

        // Resource name array
        private string[] _resourceNames;

        /// <summary>
        /// Constructor
        /// </summary>
        public ResourceHelper(ILogger logger)
        {
            _logger = logger;
        }

        // Checking if resource exists
        private bool ResourceExists(string resourceName)
        {
            if (_resourceNames == null) _resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            int pos = Array.IndexOf(_resourceNames, resourceName);
            bool exists = pos > -1 ? true : false;
            return exists;
        }

        /// <summary>
        /// Getting image from resource
        /// </summary>
        public byte[] GetResourceImage(string resourceName)
        {
            byte[] image = null;
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(WebService)).Assembly;

                if (ResourceExists(resourceName))
                {
                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            byte[] buffer = new byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);
                            image = buffer;
                        }
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

        /// <summary>
        /// Getting parser logo from resource
        /// </summary>
        public byte[] GetParserLogo(string parserSourceTitle)
        {
            byte[] image = null;
            try
            {
                parserSourceTitle = parserSourceTitle.Replace(".ru","");
                string resourceName = "News.Core.Resources.Logos." + parserSourceTitle + ".jpg";
                image = GetResourceImage(resourceName);

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
