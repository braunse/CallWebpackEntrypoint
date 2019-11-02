using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using SBraun.CallWebpackEntrypoint;

namespace SBraun.CallWebpackEntrypoints
{
    public class AssetServerMiddleware
    {
        private readonly IFileProvider _fileProvider;
        private readonly RequestDelegate _next;
        private readonly ILogger<AssetServerMiddleware> _logger;
        private readonly WebpackEntrypoints _entrypoints;
        private readonly PathString _prefix;
        private readonly bool _claimWholePrefix;

        public AssetServerMiddleware(AssetServerOptions options, RequestDelegate next,
            ILogger<AssetServerMiddleware> logger, WebpackEntrypoints entrypoints,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _entrypoints = entrypoints;
            _fileProvider = options.FileProvider ?? DefaultFileProvider(env, options.Prefix);
            _prefix = options.Prefix;
            _claimWholePrefix = options.ClaimWholePrefix;
        }

        private IFileProvider DefaultFileProvider(IWebHostEnvironment env, string prefix)
            => new PhysicalFileProvider(Path.Join(env.WebRootPath, prefix.TrimStart('/')));

        public async Task Invoke(HttpContext context)
        {
            var entrypointsData = await _entrypoints.GetRawData();
            
            if (!ValidateShouldHandle(context, entrypointsData, out var subPath, out var notFound, out var fileData))
            {
                await _next(context);
                return;
            }

            if (notFound)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
            else
            {
                await HandleAsync(context, subPath, fileData);
            }
        }

        private Task HandleAsync(HttpContext context, PathString subPath, WebpackFileData fileData)
        {
            // it is now officially our responsibility to handle the request or error out.
            // since file serving is surprisingly involved, delegate the rest to the AssetResponseContext.
            var arc = new AssetResponseContext(context, _fileProvider, fileData, _logger);
            return arc.HandleAsync();
        }

        private bool ValidateShouldHandle(HttpContext context, WebpackEntrypointsData entrypointsData,
            out PathString subPath,
            out bool notFound,
            [NotNullWhen(true)] out WebpackFileData? fileData)
        {
            notFound = false;
            fileData = null;
            if (!ValidateNoEndpointAssigned(context))
            {
                _logger.LogNoEndpointAssigned();
                return false;
            }

            if (!ValidateMethodIsReadonly(context))
            {
                _logger.LogUnhandledMethod(context.Request.Method);
                return false;
            }

            if (!ValidatePathPrefixMatch(context, out subPath))
            {
                _logger.LogUnmatchedPrefix(context.Request.Path, _prefix);
                return false;
            }

            var slashless = subPath.Value.TrimStart('/');

            if (!entrypointsData.Files!.TryGetValue(slashless, out fileData))
            {
                if (_claimWholePrefix)
                {
                    _logger.LogNotFoundAsset(slashless);
                    notFound = true;
                    return true;
                }
                else
                {
                    _logger.LogSkipUnknownAsset(slashless);
                }
                return false;
            }
            
            _logger.LogHandlingAsset(_prefix, subPath);
            return true;
        }

        private bool ValidateMethodIsReadonly(HttpContext context)
        {
            var method = context.Request.Method;
            return HttpMethods.IsGet(method) || HttpMethods.IsHead(method);
        }

        private bool ValidatePathPrefixMatch(HttpContext context, out PathString subPath)
            => context.Request.Path.StartsWithSegments(_prefix, out subPath);

        private bool ValidateNoEndpointAssigned(HttpContext context)
            => context.GetEndpoint() == null;
    }

    public class AssetServerOptions
    {
        public IFileProvider? FileProvider { get; set; }
        public string Prefix { get; set; } = "/assets";
        public bool ClaimWholePrefix { get; set; } = true;
    }
}