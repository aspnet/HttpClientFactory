// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace HttpClientFactorySample
{
    public class Program
    {
        public static void Main(string[] args) => Run().GetAwaiter().GetResult();

        public static async Task Run()
        {
            var services = new ServiceCollection()
                .AddHttpClient("github", c =>
                {
                    c.BaseAddress = new Uri("https://api.github.com/");

                    c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json"); // Github API versioning
                    c.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample"); // Github requires a user-agent
                })

                // Retry requests to github using our retry handler
                .AddHttpMessageHandler("github", b => b.AdditionalHandlers.Add(new RetryHandler()))

                // Add console logger so we can see all the logging produced by the client by default.
                .AddLogging(b => 
                {
                    b.AddFilter((category, level) => true); // Spam the world with logs.
                    b.AddConsole(c => c.IncludeScopes = true);
                })
                .BuildServiceProvider();

            var factory = services.GetRequiredService<IHttpClientFactory>();

            Console.WriteLine("Creating an HttpClient");
            var client = factory.CreateClient("github");

            Console.WriteLine("Creating and sending a request");
            var request = new HttpRequestMessage(HttpMethod.Get, "/");

            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsAsync<JObject>();
            Console.WriteLine("Response data:");
            Console.WriteLine(data);

            Console.WriteLine("Press the ANY key to exit...");
            Console.ReadKey();
        }

        private class RetryHandler : DelegatingHandler
        {
            public int RetryCount { get; set; } = 5;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                for (var i = 0; i < RetryCount; i++)
                {
                    try
                    {
                        return base.SendAsync(request, cancellationToken);
                    }
                    catch (HttpRequestException) when (i == RetryCount - 1)
                    {
                        throw;
                    }
                    catch (HttpRequestException)
                    {
                        // Retry
                        Task.Delay(TimeSpan.FromMilliseconds(50));
                    }
                }

                // Unreachable.
                throw null;
            }
        }
    }
}
