using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebServer
{
    public static class Logging
    {
        enum LogLevel
        {
            Verbose = 4,
            Standard = 3,
            Error = 1
        }

        public enum MessageType
        {
            Access = 4,
            Debug = 3,
            Warning = 2,
            Error = 1
        }

        private static LogLevel logLevel = LogLevel.Verbose;

        private static string logName;
        private static string logPath = "";
        private static StreamWriter fileStream;
        private static ConcurrentQueue<string> logMessageQueue;
        private static Thread loggingThread;

        private static bool stop = false;

        public static bool setup()
        {
            logName = generateLogName();
            fileStream = new StreamWriter(logPath + logName);

            logMessageQueue = new ConcurrentQueue<string>();
            return true;
        }

        public static void startLoggingThread()
        {
            loggingThread = new Thread(runLogging);
            Logging.logMessage(Logging.MessageType.Debug, "Starting logging thread");
            loggingThread.Start();
        }

        public static void close()
        {
            fileStream.Close();
        }

        /// <summary>
        /// Logs message to the current log file
        /// </summary>
        /// <param name="messageType">One of MessageType, used to filter logs</param>
        /// <param name="message">The message as string to log</param>
        public static void logMessage(MessageType messageType, String message)
        {
            if ((int)logLevel >= (int)messageType) {

                /*Create the tag [time] MessageType: for the log*/
                string logTag = "[" + DateTime.Now.ToString("HH:mm:ss.ff") + "] " + Enum.GetName(typeof(MessageType), messageType) + ": ";
                logMessageQueue.Enqueue(logTag + message);
            }
        }

        /// <summary>
        /// Helper function to generate logname
        /// </summary>
        /// <returns>bradserver-yyyyMMddHHmmss.log</returns>
        private static string generateLogName()
        {
            return "bradserver-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";
        }

        /// <summary>
        /// The function the logging thread runs to write to file
        /// Currently busy waits, TODO: change to more efficent waiting
        /// </summary>
        private static void runLogging()
        {
            while (!stop) {
                string message = null;
                if (logMessageQueue.TryDequeue(out message)) {
                    fileStream.WriteLine(message);
                    fileStream.Flush();
                }
            }
        }
    }
}
