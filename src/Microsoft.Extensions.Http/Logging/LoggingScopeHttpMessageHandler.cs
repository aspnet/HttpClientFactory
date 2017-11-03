// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Http.Logging
{
    public class LoggingScopeHttpMessageHandler : DelegatingHandler
    {
        private ILogger _logger;

        public LoggingScopeHttpMessageHandler(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (Log.BeginRequestPipelineScope(_logger, request))
            {
                Log.RequestPipelineStart(_logger, request);
                var response = await base.SendAsync(request, cancellationToken);
                Log.RequestPipelineEnd(_logger, response);

                return response;
            }
        }
        
        private static class Log
        {
            private static readonly Func<ILogger, HttpMethod, Uri, IDisposable> _beginRequestPipelineScope;
            private static readonly Action<ILogger, HttpMethod, Uri, Exception> _requestPipelineStart;
            private static readonly Action<ILogger, HttpMethod, Uri, HttpStatusCode, Exception> _requestPipelineEnd;

            private static readonly string FormatRequestPipelineScope ="HTTP {HttpMethod} {Uri}";
            private static readonly string MessageRequestPipelineStart = "Start processing HTTP request {HttpMethod} {Uri}";
            private static readonly string MessageRequestPipelineEnd = "End processing HTTP request {HttpMethod} {Uri} - {StatusCode}";

            static Log()
            {
                _beginRequestPipelineScope = LoggerMessage.DefineScope<HttpMethod, Uri>(FormatRequestPipelineScope);

                _requestPipelineStart = LoggerMessage.Define<HttpMethod, Uri>(LogLevel.Information, EventIds.RequestPipelineStart, MessageRequestPipelineStart);
                _requestPipelineEnd = LoggerMessage.Define< HttpMethod, Uri, HttpStatusCode>(LogLevel.Information, EventIds.RequestPipelineEnd, MessageRequestPipelineEnd);
            }

            public static IDisposable BeginRequestPipelineScope(ILogger logger, HttpRequestMessage request)
            {
                return _beginRequestPipelineScope(logger, request.Method, request.RequestUri);
            }

            public static void RequestPipelineStart(ILogger logger, HttpRequestMessage request)
            {
                _requestPipelineStart(logger, request.Method, request.RequestUri, null);
            }

            public static void RequestPipelineEnd(ILogger logger, HttpResponseMessage response)
            {
                _requestPipelineEnd(logger, response.RequestMessage.Method, response.RequestMessage.RequestUri, response.StatusCode, null);
            }
        }
    }
}