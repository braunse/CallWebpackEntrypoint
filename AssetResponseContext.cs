using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SBraun.CallWebpackEntrypoint;

namespace SBraun.CallWebpackEntrypoints
{
    internal class AssetResponseContext
    {
        private readonly IFileProvider _fileProvider;
        private readonly WebpackFileData _fileData;
        private readonly HttpContext _context;
        private readonly HttpRequest _request;
        private readonly RequestHeaders _requestHeaders;
        private readonly HttpResponse _response;
        private readonly ResponseHeaders _responseHeaders;
        private readonly ILogger _logger;

        private PreconditionState _ifMatch = PreconditionState.Unchecked;
        private PreconditionState _ifNoneMatch = PreconditionState.Unchecked;
        private PreconditionState _ifModifiedSince = PreconditionState.Unchecked;
        private PreconditionState _ifNotModfiedSince = PreconditionState.Unchecked;
        private PreconditionState _ifRange = PreconditionState.Unchecked;

        private string? _selectedEncoding;
        private WebpackVariantData? _selectedVariant = null;
        private EntityTagHeaderValue? _etag;

        private bool IsGet => HttpMethods.IsGet(_request.Method);
        private bool IsHead => HttpMethods.IsHead(_request.Method);
        private bool IsGetOrHead => IsGet || IsHead;

        internal AssetResponseContext(HttpContext context, IFileProvider fileProvider, WebpackFileData fileData, ILogger logger)
        {
            Contract.Assert(context != null);
            Contract.Assert(fileProvider != null);
            Contract.Assert(fileData != null);

            _fileProvider = fileProvider;
            _fileData = fileData;
            _context = context;
            _request = context.Request;
            _requestHeaders = _request.GetTypedHeaders();
            _response = context.Response;
            _responseHeaders = _response.GetTypedHeaders();
            _logger = logger;
        }

        public Task HandleAsync()
        {
            if (!ChooseVariant())
                return RespondUnacceptable();
            
            CheckMatchPrecondition();
            CheckModifiedSincePrecondition();
            // ComputeRange();
            // CheckRangePrecondition();

            // RFC7232, section 6:
            if (_ifMatch == PreconditionState.Failed)
                return RespondPreconditionFailed();
            else if (_ifMatch == PreconditionState.Unchecked)
            {
                if (_ifNotModfiedSince == PreconditionState.Failed)
                    return RespondPreconditionFailed();
            }

            if (_ifNoneMatch == PreconditionState.Failed)
            {
                if (IsGetOrHead)
                    return RespondNotModified();
                else
                    return RespondPreconditionFailed();
            }
            else if (_ifNoneMatch == PreconditionState.Unchecked)
            {
                if (_ifModifiedSince == PreconditionState.Failed)
                    return RespondNotModified();
            }

            // we don't support range requests yet
            // TODO handle IfRange
            
            return ServeResponse();
        }

        private async Task ServeResponse()
        {
            var fileInfo = _fileProvider.GetFileInfo(_selectedVariant!.FileName);
            if (fileInfo == null)
            {
                // TODO is "Not Found" really the right logic here?
                // We are not finding a file that should be there on the server, and the fact that it isn't
                // is more indicative of a misconfiguration or inconsistent server deployment.
                await RespondNotFound();
                return;
            }

            _response.StatusCode = StatusCodes.Status200OK;
            SetFileHeaders();

            // do not actually send an entity in response to a HEAD request
            if (IsHead)
                return;
            
            if (fileInfo.PhysicalPath != null)
            {
                var sendFile = _context.Features.Get<IHttpResponseBodyFeature>();
                if (sendFile != null)
                {
                    await sendFile.SendFileAsync(fileInfo.PhysicalPath, 0, _selectedVariant.Size);
                    return;
                }
            }

            try
            {
                await using (var readStream = fileInfo.CreateReadStream())
                {
                    await readStream.CopyToAsync(_response.Body, 16384, _context.RequestAborted);
                }
            }
            catch(OperationCanceledException)
            {
                _context.Abort();
            }
        }

        private Task RespondNotFound()
        {
            _response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }

        private Task RespondNotModified()
        {
            SetFileHeaders();
            _response.StatusCode = StatusCodes.Status304NotModified;
            return Task.CompletedTask;
        }

        private void SetFileHeaders()
        {
            if (_response.StatusCode < 400)
            {
                // this is easier than building the value through the Cache-Control typed header
                _response.Headers["Cache-Control"] = $"public,max-age={TimeSpan.FromDays(365).TotalSeconds},immutable";

                _responseHeaders.Date = DateTimeOffset.Now;

                _responseHeaders.ETag = _etag;
                
                _responseHeaders.Expires = DateTimeOffset.Now + TimeSpan.FromDays(365);

                _response.Headers["Vary"] = "Accept-Encoding";

                if("identity" != _selectedEncoding)
                {
                    _response.Headers[HeaderNames.ContentEncoding] = _selectedEncoding;
                }
            }

            if (_response.StatusCode == 200)
            {
                _responseHeaders.ContentLength = _selectedVariant!.Size;
                _responseHeaders.Headers["Conten-Type"] = _fileData.ContentType;
                // TODO _responseHeaders.LastModified = ...
            }
        }

        private Task RespondPreconditionFailed()
        {
            _response.StatusCode = StatusCodes.Status412PreconditionFailed;
            return _response.CompleteAsync();
        }

        private void CheckModifiedSincePrecondition()
        {
            // TODO find out how to deal with modified-since logic.
            // It's 2019, browsers should be able to deal with ETags...
        }

        private void CheckMatchPrecondition()
        {
            // RFC7232, 3.1
            var ifMatch = _requestHeaders.IfMatch;
            if (ifMatch.Count > 0)
            {
                // see if the selected representation matches one of the ETags, or
                // if any of the ETags is *
                if (ifMatch.Any(v => v.Tag == "*" || v.Compare(_etag, true)))
                {
                    _ifMatch = PreconditionState.Fulfilled;
                }
                else
                {
                    _ifMatch = PreconditionState.Failed;
                }
            }

            var ifNoneMatch = _requestHeaders.IfNoneMatch;
            if (ifNoneMatch.Count > 0)
            {
                // see if the selected representation matches one of the ETags, or
                // if any of the ETags is *
                if (ifNoneMatch.Any(v => v.Tag == "*" || v.Compare(_etag, false)))
                {
                    _ifNoneMatch = PreconditionState.Failed;
                }
                else
                {
                    _ifNoneMatch = PreconditionState.Fulfilled;
                }
            }
        }

        private Task RespondUnacceptable()
        {
            _response.StatusCode = StatusCodes.Status406NotAcceptable;
            return Task.CompletedTask;
        }

        private bool ChooseVariant()
        {
            using var logScope = _logger.LogContentNegotiationScope(_request.Path, _request.Headers[HeaderNames.AcceptEncoding]);

            var acceptEncoding = _requestHeaders.AcceptEncoding;

            (_selectedEncoding, _selectedVariant) = _fileData.Variants
                .Where(kv => ContentNegotiationHelpers.IsAcceptableEncoding(acceptEncoding, kv.Key, _logger))
                .OrderBy(kv => kv.Value.Size)
                .FirstOrDefault();

            if (_selectedVariant == null) return false;

            // entity tags must be quoted for the constructor
            _etag = new EntityTagHeaderValue($"\"{_selectedVariant.Hash}\"");
            return true;
        }
    }

    internal enum PreconditionState
    {
        Unchecked,
        Failed,
        Fulfilled,
    }
}