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
        /// Last error description
        /// </summary>
        public Logger()
        {
        }

        /// <summary>
        /// Last error description
        /// </summary>
        public string LastError { get; private set; } = "";

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
            //if (LastError == "") LastError = errorMessage;
            if (LastError == "") LastError = exception.StackTrace + Environment.NewLine + errorMessage;
            Debug.WriteLine("\tERROR {0}", errorMessage);
        }
    }
}
