﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Http
{
    public class DefaultHttpMessageHandlerBuilderTest
    {
        // Testing this because it's an important design detail. If someone wants to globally replace the handler
        // they can do so by replacing this service. It's important that the Factory isn't the one to instantiate
        // the handler. The factory has no defaults - it only applies options.
        [Fact] 
        public void Ctor_SetsPrimaryHandler()
        {
            // Arrange & Act
            var builder = new DefaultHttpMessageHandlerBuilder();

            // Act
            Assert.IsType<HttpClientHandler>(builder.PrimaryHandler);
        }


        [Fact]
        public void Build_NoAdditionalHandlers_ReturnsPrimaryHandler()
        {
            // Arrange
            var builder = new DefaultHttpMessageHandlerBuilder()
            {
                PrimaryHandler = Mock.Of<HttpMessageHandler>(),
            };

            // Act
            var handler = builder.Build();

            // Assert
            Assert.Same(builder.PrimaryHandler, handler);
        }

        [Fact]
        public void Build_SomeAdditionalHandlers_PutsTogetherDelegatingHandlers()
        {
            // Arrange
            var builder = new DefaultHttpMessageHandlerBuilder()
            {
                PrimaryHandler = Mock.Of<HttpMessageHandler>(),
                AdditionalHandlers =
                {
                    Mock.Of<DelegatingHandler>(), // Outer
                    Mock.Of<DelegatingHandler>(), // Middle
                }
            };

            // Act
            var handler = builder.Build();

            // Assert
            Assert.Same(builder.AdditionalHandlers[0], handler);

            handler = Assert.IsAssignableFrom<DelegatingHandler>(handler).InnerHandler;
            Assert.Same(builder.AdditionalHandlers[1], handler);

            handler = Assert.IsAssignableFrom<DelegatingHandler>(handler).InnerHandler;
            Assert.Same(builder.PrimaryHandler, handler);
        }

        [Fact]
        public void Build_WithHandlersAndPostInitializer_CallsPositInitializerAtEnd()
        {
            // Arrange
            var postInitializerHandler = Mock.Of<DelegatingHandler>();

            var postInitializer = new Mock<IHttpMessageHandlerBuilderPostInitializer>();
            postInitializer
                .Setup(p => p.Apply(It.IsAny<HttpMessageHandlerBuilder>()))
                .Callback<HttpMessageHandlerBuilder>(b => b.AdditionalHandlers.Add(postInitializerHandler));

            var builder = new DefaultHttpMessageHandlerBuilder(postInitializer.Object)
            {
                PrimaryHandler = Mock.Of<HttpMessageHandler>(),
                AdditionalHandlers =
                {
                    Mock.Of<DelegatingHandler>(), // Outer
                }
            };

            // Act
            var handler = builder.Build();

            // Assert
            Assert.Same(builder.AdditionalHandlers[0], handler);

            handler = Assert.IsAssignableFrom<DelegatingHandler>(handler).InnerHandler;
            Assert.Same(builder.AdditionalHandlers[1], handler);
            Assert.Same(postInitializerHandler, handler);

            handler = Assert.IsAssignableFrom<DelegatingHandler>(handler).InnerHandler;
            Assert.Same(builder.PrimaryHandler, handler);
        }

        [Fact]
        public void Build_PrimaryHandlerIsNull_ThrowsException()
        {
            // Arrange
            var builder = new DefaultHttpMessageHandlerBuilder()
            {
                PrimaryHandler = null,
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());
            Assert.Equal("The 'PrimaryHandler' must not be null.", exception.Message);
        }

        [Fact]
        public void Build_AdditionalHandlerIsNull_ThrowsException()
        {
            // Arrange
            var builder = new DefaultHttpMessageHandlerBuilder()
            {
                AdditionalHandlers =
                {
                    null,
                }
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());
            Assert.Equal("The 'additionalHandlers' must not contain a null entry.", exception.Message);
        }

        [Fact]
        public void Build_AdditionalHandlerHasNonNullInnerHandler_ThrowsException()
        {
            // Arrange
            var builder = new DefaultHttpMessageHandlerBuilder()
            {
                AdditionalHandlers =
                {
                    Mock.Of<DelegatingHandler>(h => h.InnerHandler == Mock.Of<DelegatingHandler>()),
                }
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());
            Assert.Equal(
                "The 'InnerHandler' property must be null. " +
                "'DelegatingHandler' instances provided to 'HttpMessageHandlerBuilder' must not be reused or cached." + Environment.NewLine +
                $"Handler: '{builder.AdditionalHandlers[0].ToString()}'",
                exception.Message);
        }
    }
}