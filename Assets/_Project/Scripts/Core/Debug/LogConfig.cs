// File: Assets/_Project/Scripts/Core/Debug/LogConfig.cs
using UnityEngine;

namespace Lineage.Debug
{
    /// <summary>
    /// Scriptable configuration for Log system settings.
    /// Allows runtime and build-time control over logging behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "LogConfig", menuName = "Lineage/Debug/Log Configuration")]
    public class LogConfig : ScriptableObject
    {
        [Header("Log Level")]
        [Tooltip("Minimum log level to display (lower levels will be filtered)")]
        public Log.LogLevel minLogLevel = Log.LogLevel.Debug;

        [Header("Output Targets")]
        [Tooltip("Enable Unity console output")]
        public bool enableUnityConsole = true;

        [Tooltip("Enable in-game console/overlay output")]
        public bool enableInGameConsole = false;

        [Tooltip("Enable file logging to persistent data path")]
        public bool enableFileLogging = false;

        [Header("Message Formatting")]
        [Tooltip("Include timestamp in log messages")]
        public bool includeTimestamp = true;

        [Tooltip("Include frame number in log messages")]
        public bool includeFrameNumber = true;

        [Header("Category Filtering")]
        [Tooltip("Enable category-based filtering (if empty, all categories allowed)")]
        public Log.LogCategory[] allowedCategories = new Log.LogCategory[0];

        [Header("Performance")]
        [Tooltip("Max log file size in MB (0 = unlimited)")]
        public float maxLogFileSizeMB = 50f;

        [Tooltip("Auto-flush to file after each message (slower but safer)")]
        public bool autoFlushFileLog = false;

        /// <summary>
        /// Apply this configuration to the Log system
        /// </summary>
        public void Apply()
        {
            Log.MinLogLevel = minLogLevel;
            Log.EnableUnityConsole = enableUnityConsole;
            Log.EnableInGameConsole = enableInGameConsole;
            Log.EnableFileLogging = enableFileLogging;
            Log.IncludeTimestamp = includeTimestamp;
            Log.IncludeFrameNumber = includeFrameNumber;

            UnityEngine.Debug.Log($"[LogConfig] Applied configuration: MinLevel={minLogLevel}, FileLogging={enableFileLogging}");
        }

        /// <summary>
        /// Check if a category is allowed based on filter settings
        /// </summary>
        public bool IsCategoryAllowed(Log.LogCategory category)
        {
            if (allowedCategories == null || allowedCategories.Length == 0)
                return true; // No filter = all allowed

            foreach (var allowed in allowedCategories)
            {
                if (allowed == category)
                    return true;
            }

            return false;
        }

        private void OnValidate()
        {
            // Ensure sensible defaults
            if (maxLogFileSizeMB < 0f)
                maxLogFileSizeMB = 0f;
        }
    }
}
