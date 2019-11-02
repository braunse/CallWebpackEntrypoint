using System;
using Microsoft.Extensions.Logging;

namespace SBraun.CallWebpackEntrypoints
{
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception?> _logNoEndpointAssigned =
            LoggerMessage.Define(
                LogLevel.Debug,
                new EventId(1, "SkipBecauseNoEndpointAssigned"),
                "Skipping asset server, as an endpoint was already assigned");

        private static readonly Action<ILogger, string, Exception?> _logUnhandledMethod =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(2, "SkipBecauseUnhandledMethod"),
                "Skipping asset server, as method {Method} is not handled");

        private static readonly Action<ILogger, string, string, Exception?> _logUnmatchedPrefix =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(3, "SkipBecauseUnmatchedPrefix"),
                "Skipping asset server, as path {Path} does not match configured prefix {Prefix}");

        private static readonly Action<ILogger, string, Exception?> _logSkipUnknownAsset =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(4, "SkipBecauseUnknownAsset"),
                "Skipping asset server because unknown asset {Name} requested and prefix not claimed completely");

        private static readonly Action<ILogger, string, Exception?> _logNotFoundAsset =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(6, "NotFoundAsset"),
                "Asset {Path} requested but not found: short-circuiting and delivering 404");

        public static void LogNoEndpointAssigned(this ILogger logger)
            => _logNoEndpointAssigned(logger, null);

        public static void LogUnhandledMethod(this ILogger logger, string method)
            => _logUnhandledMethod(logger, method, null);

        public static void LogUnmatchedPrefix(this ILogger logger, string path, string prefix)
            => _logUnmatchedPrefix(logger, path, prefix, null);

        public static void LogSkipUnknownAsset(this ILogger logger, string name)
            => _logSkipUnknownAsset(logger, name, null);

        public static void LogNotFoundAsset(this ILogger logger, string subPath)
            => _logNotFoundAsset(logger, subPath, null);
    }
}