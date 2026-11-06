using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Writes timestamped gameplay events and runtime exceptions to UserLogs/log.txt.
    /// Initialized automatically before the first scene is loaded.
    /// </summary>
    public static class GameLogger
    {
        private const string LogDirectoryName = "UserLogs";
        private const string LogFileName = "log.txt";

        private static readonly object FileLock = new object();

        private static bool _initialized;
        private static bool _isWriting;
        private static string _logPath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
            Application.quitting -= OnQuitting;
            _initialized = false;
            _isWriting = false;
            _logPath = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
            Application.quitting += OnQuitting;
            WriteEntry("Game started.");
        }

        private static void OnQuitting()
        {
            Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
            Application.quitting -= OnQuitting;
            _initialized = false;
        }

        public static void LogCoinCollected(string coinName, int collected, int remaining)
        {
            WriteEntry($"Coin collected: {coinName}. Collected: {collected}, remaining: {remaining}.");
        }

        public static void LogPortalEntered()
        {
            WriteEntry("Portal entered. Victory shown.");
        }

        public static void LogException(string message)
        {
            WriteEntry($"EXCEPTION: {message}");
        }

        private static void OnLogMessageReceivedThreaded(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Exception)
            {
                return;
            }

            var message = $"EXCEPTION: {condition}";
            if (!string.IsNullOrWhiteSpace(stackTrace))
            {
                message += Environment.NewLine + stackTrace.TrimEnd();
            }

            WriteEntry(message);
        }

        private static void WriteEntry(string message)
        {
            lock (FileLock)
            {
                if (_isWriting)
                {
                    return;
                }

                _isWriting = true;

                try
                {
                    var timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
                    var line = $"[{timestamp}] {message}{Environment.NewLine}";
                    var path = GetLogPath();

                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    File.AppendAllText(path, line, new UTF8Encoding(false));
                }
                catch (Exception exception)
                {
                    // Never allow logging to break gameplay or recurse through the
                    // exception handler while the log file itself is unavailable.
                    Debug.LogError($"[GameLogger] Could not write to UserLogs/log.txt: {exception.Message}");
                }
                finally
                {
                    _isWriting = false;
                }
            }
        }

        private static string GetLogPath()
        {
            if (!string.IsNullOrEmpty(_logPath))
            {
                return _logPath;
            }

            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            _logPath = Path.Combine(projectRoot, LogDirectoryName, LogFileName);
            return _logPath;
        }
    }
}
