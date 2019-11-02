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

        private static readonly Action<ILogger, string, string, Exception?> _logHandlingAsset =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(5, "HandlingAsset"),
                "Handling asset in prefix {Prefix} with asset sub-path {Path}");

        private static readonly Action<ILogger, string, Exception?> _logNotFoundAsset =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(6, "NotFoundAsset"),
                "Asset {Path} requested but not found: short-circuiting and delivering 404");

        private static readonly Action<ILogger, string, Exception?> _logVariantAcceptableBecauseNoHeaderGiven =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(7, "VariantAcceptableBecauseNoHeaderGiven"),
                "Entity variant {VariantName} is acceptable by default, because no Accept-Encoding header is given");

        private static readonly Action<ILogger, Exception?> _logIdentityExplicitlyUnacceptable =
            LoggerMessage.Define(
                LogLevel.Debug,
                new EventId(8, "IdentityExplicitlyUnacceptable"),
                "The identity variant is explicitly marked as unacceptable");

        private static readonly Action<ILogger, Exception?> _logIdentityUnacceptableBecauseAnyUnacceptable =
            LoggerMessage.Define(
                LogLevel.Debug,
                new EventId(9, "IdentityUnacceptableBecauseAnyUnacceptable"),
                "The identity variant is considered unacceptable because * is explicitly marked as unacceptable");

        private static readonly Action<ILogger, Exception?> _logIdentityAcceptableByDefault =
            LoggerMessage.Define(
                LogLevel.Debug,
                new EventId(10, "IdentityAcceptableByDefault"),
                "The identity variant is considered acceptable by default");

        private static readonly Action<ILogger, string, Exception?> _logVariantExplicitlyUnacceptable =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(11, "VariantExplicitlyUnacceptable"),
                "The variant {VariantName} is explicitly marked as unacceptable");

        private static readonly Action<ILogger, string, Exception?> _logVariantExplicitlyAcceptable =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(12, "VariantExplicitlyAcceptable"),
                "The variant {VariantName} is explicitly marked as acceptable");

        private static readonly Action<ILogger, string, Exception?> _logVariantUnacceptableBecauseNotListed =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(13, "VariantUnacceptableBecauseNotListed"),
                "The variant {VariantName} is considered unacceptable because it is not listed in the Accept-Encoding header");

        private static readonly Func<ILogger, string, string, IDisposable> _logContentNegotiationScope =
            LoggerMessage.DefineScope<string, string>("Content Negotiation for {Path} where Accept-Encoding is \"{AcceptEncoding}\"");

        public static void LogNoEndpointAssigned(this ILogger logger)
            => _logNoEndpointAssigned(logger, null);

        public static void LogUnhandledMethod(this ILogger logger, string method)
            => _logUnhandledMethod(logger, method, null);

        public static void LogUnmatchedPrefix(this ILogger logger, string path, string prefix)
            => _logUnmatchedPrefix(logger, path, prefix, null);

        public static void LogSkipUnknownAsset(this ILogger logger, string name)
            => _logSkipUnknownAsset(logger, name, null);

        public static void LogHandlingAsset(this ILogger logger, string prefix, string subPath)
            => _logHandlingAsset(logger, prefix, subPath, null);

        public static void LogNotFoundAsset(this ILogger logger, string subPath)
            => _logNotFoundAsset(logger, subPath, null);

        public static void LogVariantAcceptableBecauseNoHeaderGiven(this ILogger logger, string variantName)
            => _logVariantAcceptableBecauseNoHeaderGiven(logger, variantName, null);

        public static void LogIdentityExplicitlyUnacceptable(this ILogger logger)
            => _logIdentityExplicitlyUnacceptable(logger, null);

        public static void LogIdentityUnacceptableBecauseAnyUnacceptable(this ILogger logger)
            => _logIdentityUnacceptableBecauseAnyUnacceptable(logger, null);

        public static void LogIdentityAcceptableByDefault(this ILogger logger)
            => _logIdentityAcceptableByDefault(logger, null);

        public static void LogVariantExplicitlyUnacceptable(this ILogger logger, string variantName)
            => _logVariantExplicitlyUnacceptable(logger, variantName, null);

        public static void LogVariantExplicitlyAcceptable(this ILogger logger, string variantName)
            => _logVariantExplicitlyAcceptable(logger, variantName, null);

        public static void LogVariantUnacceptableBecauseNotListed(this ILogger logger, string variantName)
            => _logVariantUnacceptableBecauseNotListed(logger, variantName, null);

        public static IDisposable LogContentNegotiationScope(this ILogger logger, string path, string acceptEncoding)
            => _logContentNegotiationScope(logger, path, acceptEncoding);
    }
}