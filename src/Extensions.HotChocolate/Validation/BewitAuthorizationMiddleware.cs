using System;
using System.Threading.Tasks;
using Bewit.Validation;
using HotChocolate;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Http;

namespace Bewit.Extensions.HotChocolate.Validation
{
    public class BewitAuthorizationMiddleware<T>
    {
        private readonly FieldDelegate _next;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBewitTokenValidator<T> _tokenValidator;

        public BewitAuthorizationMiddleware(
            FieldDelegate next,
            IHttpContextAccessor httpContextAccessor,
            IBewitTokenValidator<T> tokenValidator)
        {
            _next = next
                ?? throw new ArgumentNullException(nameof(next));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _tokenValidator = tokenValidator
                ?? throw new ArgumentNullException(nameof(tokenValidator));
        }

        public async Task InvokeAsync(IMiddlewareContext context)
        {
            try
            {
                if (context.ContextData.TryGetValue(
                        BewitTokenHeader.Value, out var objectToken) &&
                    objectToken is string bewitToken)
                {
                    object payload = await _tokenValidator.ValidateBewitTokenAsync(
                        new BewitToken<T>(bewitToken),
                        context.RequestAborted);

                    _httpContextAccessor.SetBewitPayload(payload);

                    await _next(context);
                }
                else
                {
                    context.Result = ErrorBuilder.New()
                        .SetMessage("NotAuthorized")
                        .SetPath(context.Path)
                        .AddLocation(context.FieldSelection)
                        .Build();
                }
            }
            catch (Exception ex)
            {
                context.Result = ErrorBuilder.New()
                    .SetMessage("NotAuthorized")
                    .SetPath(context.Path)
                    .SetException(ex)
                    .AddLocation(context.FieldSelection)
                    .Build();
            }
        }
    }
}
