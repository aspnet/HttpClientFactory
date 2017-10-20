// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Extensions.Http
{
    /// <summary>
    /// Used by the <see cref="DefaultHttpMessageHandlerBuilder"/> to apply additional initialization
    /// after user code runs to the configure the <see cref="HttpMessageHandlerBuilder"/> immediately
    /// before <see cref="HttpMessageHandlerBuilder.Build()"/> is called.
    /// </summary>
    public interface IHttpMessageHandlerBuilderPostInitializer
    {
        /// <summary>
        /// Applies additional initialization to the <see cref="HttpMessageHandlerBuilder"/>
        /// </summary>
        /// <param name="builder">The <see cref="HttpMessageHandlerBuilder"/>.</param>
        void Apply(HttpMessageHandlerBuilder builder);
    }
}
