﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Bewit.Core;
using Bewit.Generation;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.WebUtilities;

namespace Bewit.HotChocolate
{
    public class BewitUrlMiddleware
    {
        private readonly FieldDelegate _next;

        public BewitUrlMiddleware(FieldDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(
            IMiddlewareContext context, 
            IBewitTokenGenerator<string> tokenGenerator)
        {
            await _next(context).ConfigureAwait(false);

            if (context.Result is string result)
            {
                var uri = new Uri(result);

                BewitToken<string> bewit =
                    await tokenGenerator.GenerateBewitTokenAsync(
                        uri.PathAndQuery, context.RequestAborted);

                var parametersToAdd = new Dictionary<string, string>
                {
                    { "bewit", WebUtility.UrlEncode((string) bewit) }
                };
                string newUri =
                    QueryHelpers.AddQueryString(result, parametersToAdd);

                context.Result = newUri;
            }
        }
    }
}
