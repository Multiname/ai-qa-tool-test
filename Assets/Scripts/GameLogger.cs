using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
        private const string ConfigFileName = "user_config.json";

        private static readonly object FileLock = new object();

        private static bool _initialized;
        private static bool _isWriting;
        private static string _logPath;
        private static string _playerId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
            Application.quitting -= OnQuitting;
            _initialized = false;
            _isWriting = false;
            _logPath = null;
            _playerId = null;
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

            var configWarning = TryLoadPlayerId(out _playerId);
            WriteEntry("Game started.");

            if (!string.IsNullOrEmpty(configWarning))
            {
                WriteEntry($"CONFIG WARNING: {configWarning}");
                Debug.LogWarning($"[GameLogger] {configWarning}");
            }
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
                    var playerId = string.IsNullOrEmpty(_playerId) ? "unknown" : _playerId;
                    var line = $"[{timestamp}] [playerId={playerId}] {message}{Environment.NewLine}";
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

        private static string TryLoadPlayerId(out string playerId)
        {
            playerId = string.Empty;

            try
            {
                var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                var configPath = Path.Combine(projectRoot, ConfigFileName);

                if (!File.Exists(configPath))
                {
                    return $"{ConfigFileName} was not found at {configPath}.";
                }

                var json = File.ReadAllText(configPath);
                var match = Regex.Match(
                    json,
                    @"[""']?id[""']?\s*:\s*(?:""(?<value>[^""]*)""|'(?<value>[^']*)'|(?<value>[^,\s}]+))",
                    RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    return $"Could not find an id value in {ConfigFileName}.";
                }

                playerId = match.Groups["value"].Value.Trim();
                return string.IsNullOrEmpty(playerId)
                    ? $"The id value in {ConfigFileName} is empty."
                    : null;
            }
            catch (Exception exception)
            {
                return $"Could not read {ConfigFileName}: {exception.Message}";
            }
        }
    }
}
