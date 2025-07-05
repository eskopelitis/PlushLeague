using UnityEngine;

namespace PlushLeague.Debug
{
    /// <summary>
    /// Static Debug methods that forward to UnityEngine.Debug
    /// </summary>
    public static class Debug
    {
        /// <summary>
        /// Logs a message to the console
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }
        
        /// <summary>
        /// Logs a warning message to the console
        /// </summary>
        /// <param name="message">The warning message to log</param>
        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        
        /// <summary>
        /// Logs an error message to the console
        /// </summary>
        /// <param name="message">The error message to log</param>
        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}
