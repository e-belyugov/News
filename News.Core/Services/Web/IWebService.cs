using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace News.Core.Services.Web
{
    /// <summary>
    /// WebService interface
    /// </summary>
    public interface IWebService
    {
        /// <summary>
        /// Getting string data from web page
        /// </summary>
        Task<string> GetDataAsync(string url, Encoding encoding);

        /// <summary>
        /// Getting image data from web page
        /// </summary>
        Task<byte[]> GetImageAsync(string url);
    }
}
