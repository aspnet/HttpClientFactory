﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.Extensions.Http
{
    internal class DefaultHttpMessageHandlerBuilder : HttpMessageHandlerBuilder
    {
        private readonly IHttpMessageHandlerBuilderPostInitializer _postInitializer;

        public DefaultHttpMessageHandlerBuilder()
        {
        }

        public DefaultHttpMessageHandlerBuilder(IHttpMessageHandlerBuilderPostInitializer postInitializer)
        {
            _postInitializer = postInitializer;
        }

        public override HttpMessageHandler PrimaryHandler { get; set; } = new HttpClientHandler();

        public override IList<DelegatingHandler> AdditionalHandlers { get; } = new List<DelegatingHandler>();

        public override HttpMessageHandler Build()
        {
            if (PrimaryHandler == null)
            {
                var message = Resources.FormatHttpMessageHandlerBuilder_PrimaryHandlerIsNull(nameof(PrimaryHandler));
                throw new InvalidOperationException(message);
            }

            _postInitializer?.Apply(this);

            return CreateHandlerPipeline(PrimaryHandler, AdditionalHandlers);
        }
    }
}
