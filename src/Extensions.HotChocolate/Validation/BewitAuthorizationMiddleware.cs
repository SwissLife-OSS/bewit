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
            if (context.ContextData.TryGetValue(BewitTokenHeader.Value, out var objectToken) &&
                objectToken is string bewitToken)
            {
                try
                {
                    object payload = await _tokenValidator.ValidateBewitTokenAsync(
                        new BewitToken<T>(bewitToken),
                        context.RequestAborted);

                    _httpContextAccessor.SetBewitPayload(payload);
                }
                catch (Exception ex)
                {
                    CreateError(context, ex);
                }

                await _next(context);
            }
            else
            {
                CreateError(context);
            }
        }

        private void CreateError(IMiddlewareContext context, Exception ex = default)
        {
            IErrorBuilder errorBuilder = ErrorBuilder.New()
                .SetMessage("NotAuthorized")
                .SetPath(context.Path)
                .AddLocation(context.FieldSelection);

            if (ex != default)
            {
                errorBuilder.SetException(ex);
            }

            context.Result = errorBuilder.Build();
        }
    }
}
