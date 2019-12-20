using System;
using System.Collections.Generic;
using System.Text;

namespace News.Core.Services.Logging
{
    /// <summary>
    /// Logger interface
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Info logging
        /// </summary>
        void Info(string infoMessage);

        /// <summary>
        /// Error logging
        /// </summary>
        void Error(Exception exception);

        /// <summary>
        /// Last error description
        /// </summary>
        string LastError { get; }
    }
}
