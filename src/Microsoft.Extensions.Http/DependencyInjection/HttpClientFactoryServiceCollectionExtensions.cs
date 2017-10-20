// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions methods to configure an <see cref="IServiceCollection"/> for <see cref="IHttpClientFactory"/>.
    /// </summary>
    public static class HttpClientFactoryServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddHttpClient(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddLogging();
            services.AddOptions();

            //
            // Core abstractions
            //
            services.TryAddTransient<HttpMessageHandlerBuilder, DefaultHttpMessageHandlerBuilder>();
            services.TryAddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();

            //
            // Misc infrastrure
            //
            services.TryAddSingleton<IHttpMessageHandlerBuilderPostInitializer, DefaultHttpMessageHandlerBuilderPostInitializer>();

            return services;
        }

        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
        /// a named <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
        /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        /// <remarks>
        /// <para>
        /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using 
        /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
        /// </para>
        /// <para>
        /// Use <see cref="Options.Options.DefaultName"/> as the name to configure the default client.
        /// </para>
        /// </remarks>
        public static IServiceCollection AddHttpClient(this IServiceCollection services, string name, Action<HttpClient> configureClient)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (configureClient == null)
            {
                throw new ArgumentNullException(nameof(configureClient));
            }

            AddHttpClient(services);
            services.Configure<HttpClientFactoryOptions>(name, options => options.HttpClientActions.Add(configureClient));

            return services;
        }

        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/> and configures
        /// all <see cref="HttpClient"/> instances produced by the <see cref="IHttpClientFactory"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureClient">
        /// A delegate that is used to configure all <see cref="HttpClient"/> instances produced by the
        /// <see cref="IHttpClientFactory"/>.
        /// </param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="configureClient"/> delegate will be used to configure all <see cref="HttpClient"/> instances
        /// produced by the <see cref="IHttpClientFactory"/>.
        /// </para>
        /// </remarks>
        public static IServiceCollection AddGlobalHttpClient(this IServiceCollection services, Action<HttpClient> configureClient)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureClient == null)
            {
                throw new ArgumentNullException(nameof(configureClient));
            }

            AddHttpClient(services);
            services.ConfigureAll<HttpClientFactoryOptions>(options => options.HttpClientActions.Add(configureClient));

            return services;
        }

        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/>
        /// and adds a delegate that will be used to configure message handlers using 
        /// <see cref="HttpMessageHandlerBuilder"/> for a named <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
        /// <param name="configureHandlers">A delegate that is used to configure an <see cref="HttpMessageHandlerBuilder"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        /// <remarks>
        /// <para>
        /// <see cref="HttpClient"/> instances that apply the provided configuration can be retrieved using 
        /// <see cref="IHttpClientFactory.CreateClient(string)"/> and providing the matching name.
        /// </para>
        /// <para>
        /// Use <see cref="Options.Options.DefaultName"/> as the name to configure the default client.
        /// </para>
        /// </remarks>
        public static IServiceCollection AddHttpMessageHandler(this IServiceCollection services, string name, Action<HttpMessageHandlerBuilder> configureHandlers)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (configureHandlers == null)
            {
                throw new ArgumentNullException(nameof(configureHandlers));
            }

            AddHttpClient(services);
            services.Configure<HttpClientFactoryOptions>(name, options => options.HttpMessageHandlerBuilderActions.Add(configureHandlers));

            return services;
        }

        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/>
        /// and adds a delegate that will be used to configure message handlers using 
        /// <see cref="HttpMessageHandlerBuilder"/> for all <see cref="HttpClient"/> instances produced by 
        /// the <see cref="IHttpClientFactory"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureHandlers">
        /// A delegate that is used to configure an <see cref="HttpMessageHandlerBuilder"/> for each 
        /// <see cref="HttpClient"/> produced by the <see cref="IHttpClientFactory"/>.
        /// </param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="configureHandlers"/> delegate will be used to configure message handlers
        /// for all <see cref="HttpClient"/> instances produced by the <see cref="IHttpClientFactory"/>.
        /// </para>
        /// </remarks>
        public static IServiceCollection AddGlobalHttpMessageHandler(this IServiceCollection services, Action<HttpMessageHandlerBuilder> configureHandlers)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureHandlers == null)
            {
                throw new ArgumentNullException(nameof(configureHandlers));
            }

            AddHttpClient(services);
            services.ConfigureAll<HttpClientFactoryOptions>(options => options.HttpMessageHandlerBuilderActions.Add(configureHandlers));

            return services;
        }
    }
}