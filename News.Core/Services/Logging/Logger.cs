using System;
using System.Diagnostics;

namespace News.Core.Services.Logging
{
    /// <summary>
    /// Logger interface
    /// </summary>
    public class Logger : ILogger
    {
        /// <summary>
        /// Last error string
        /// </summary>
        private string _lastError;
        public string LastError { get => _lastError; }

        /// <summary>
        /// Info logging
        /// </summary>
        public void Info(string infoMessage)
        {

        }

        /// <summary>
        /// Error logging
        /// </summary>
        public void Error(Exception exception)
        {
            string errorMessage = exception.Message;
            _lastError = errorMessage;
            Debug.WriteLine("\tERROR {0}", errorMessage);
        }
    }
}
