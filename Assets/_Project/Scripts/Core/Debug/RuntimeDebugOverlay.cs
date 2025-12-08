// File: Assets/_Project/Scripts/Core/Debug/RuntimeDebugOverlay.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Lineage.Debug
{
    /// <summary>
    /// Lightweight on-screen debug overlay for runtime builds.
    /// Displays recent log messages by category. Toggle with F3 key.
    /// No Editor dependencies - safe for all builds.
    /// </summary>
    public class RuntimeDebugOverlay : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private bool startEnabled = false;
        [SerializeField] private int maxMessages = 15;
        [SerializeField] private float messageLifetime = 5f;
        [SerializeField] private KeyCode toggleKey = KeyCode.F3;

        private bool isVisible = false;
        private Queue<LogEntry> logMessages = new Queue<LogEntry>();
        private GUIStyle messageStyle;
        private GUIStyle backgroundStyle;
        private bool stylesInitialized = false;

        private struct LogEntry
        {
            public string message;
            public Log.LogLevel level;
            public Log.LogCategory category;
            public float timestamp;
        }

        private void Awake()
        {
            isVisible = startEnabled;
            // Subscribe to Unity's log callback for runtime monitoring
            Application.logMessageReceived += HandleUnityLog;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleUnityLog;
        }

        private void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            Log.LogLevel level = Log.LogLevel.Info;
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    level = Log.LogLevel.Error;
                    break;
                case LogType.Warning:
                    level = Log.LogLevel.Warning;
                    break;
                case LogType.Log:
                    level = Log.LogLevel.Info;
                    break;
            }

            AddMessage(logString, level, Log.LogCategory.General);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                isVisible = !isVisible;
            }

            // Clean up old messages
            while (logMessages.Count > 0 && Time.time - logMessages.Peek().timestamp > messageLifetime)
            {
                logMessages.Dequeue();
            }
        }

        private void AddMessage(string message, Log.LogLevel level, Log.LogCategory category)
        {
            logMessages.Enqueue(new LogEntry
            {
                message = message,
                level = level,
                category = category,
                timestamp = Time.time
            });

            while (logMessages.Count > maxMessages)
            {
                logMessages.Dequeue();
            }
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            messageStyle = new GUIStyle();
            messageStyle.fontSize = 12;
            messageStyle.normal.textColor = Color.white;
            messageStyle.padding = new RectOffset(5, 5, 2, 2);
            messageStyle.wordWrap = true;

            backgroundStyle = new GUIStyle();
            backgroundStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.7f));

            stylesInitialized = true;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void OnGUI()
        {
            if (!isVisible) return;

            InitializeStyles();

            float screenWidth = Screen.width;
            float overlayWidth = Mathf.Min(screenWidth * 0.4f, 600f);
            float overlayHeight = Mathf.Min(Screen.height * 0.5f, 400f);
            Rect overlayRect = new Rect(10, 10, overlayWidth, overlayHeight);

            GUI.Box(overlayRect, "", backgroundStyle);

            GUILayout.BeginArea(overlayRect);
            GUILayout.Label($"<b>Runtime Debug Overlay</b> ({logMessages.Count} messages) [F3 to toggle]", messageStyle);
            GUILayout.Space(5);

            foreach (var entry in logMessages.Reverse())
            {
                Color messageColor = GetColorForLevel(entry.level);
                messageStyle.normal.textColor = messageColor;

                string categoryTag = $"[{entry.category}]";
                string levelTag = $"[{entry.level}]";
                GUILayout.Label($"{levelTag}{categoryTag} {entry.message}", messageStyle);
            }

            GUILayout.EndArea();
        }

        private Color GetColorForLevel(Log.LogLevel level)
        {
            switch (level)
            {
                case Log.LogLevel.Error:
                case Log.LogLevel.Critical:
                    return new Color(1f, 0.3f, 0.3f);
                case Log.LogLevel.Warning:
                    return new Color(1f, 0.9f, 0.3f);
                case Log.LogLevel.Info:
                    return new Color(0.7f, 0.9f, 1f);
                case Log.LogLevel.Debug:
                case Log.LogLevel.Verbose:
                    return new Color(0.6f, 0.6f, 0.6f);
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Programmatically log a message to the overlay
        /// </summary>
        public void LogToOverlay(string message, Log.LogLevel level = Log.LogLevel.Info, Log.LogCategory category = Log.LogCategory.General)
        {
            AddMessage(message, level, category);
        }
    }
}
