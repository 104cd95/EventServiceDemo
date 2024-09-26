using System;
using UnityEngine;

namespace EventServiceDemo.Utility
{
    // Simple wrapper for Unity's Debug.Log that notifies
    // all subscribers about logging
    public static class DebugUtility
    {
        public static event Action<LogType, string> LogEvent;

        public static void Log(LogType logType, string message)
        {
            Debug.unityLogger.Log(logType, message);
            LogEvent?.Invoke(logType, message);
        }
        
        public static void LogFormat(LogType logType, string format, params object[] args)
        {
            Log(logType, string.Format(format, args));
        }
    }
}