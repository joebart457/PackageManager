using PM.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Services
{
    internal static class LoggerService
    {
        public static bool LoggingEnabled { get; set; } = true;

        private static void Write(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        private static void LogError(string msg)
        {
            Write(msg, ConsoleColor.Red);
        }

        private static void LogInfo(string msg)
        {
            Write(msg, ConsoleColor.Cyan);
        }

        private static void LogSuccess(string msg)
        {
            Write(msg, ConsoleColor.Green);
        }

        private static void LogWarning(string msg)
        {
            Write(msg, ConsoleColor.DarkYellow);
        }

        private static void LogLogs(string msg)
        {
            Write(msg, ConsoleColor.DarkMagenta);
        }

        public static void Log(string msg, LogSeverity severity = LogSeverity.INFO, bool force = false)
        {
            if (LoggingEnabled || force)
            {
                switch (severity)
                {
                    case LogSeverity.INFO:
                        LogInfo(msg);
                        break;
                    case LogSeverity.WARNING:
                        LogWarning(msg);
                        break;
                    case LogSeverity.ERROR:
                        LogError(msg);
                        break;
                    case LogSeverity.SUCCESS:
                        LogSuccess(msg);
                        break;
                    case LogSeverity.LOGS:
                        LogLogs(msg);
                        break;
                    default:
                        LogInfo(msg);
                        break;
                }
            }
        }
    }
}
