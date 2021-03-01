using System;
using System.Threading.Tasks;
using Bewit.Core;
using Bewit.Validation;
using HotChocolate;
using HotChocolate.Resolvers;

namespace Bewit.Extensions.HotChocolate.Validation
{
    public class BewitAuthorizationMiddleware
    {
        private readonly FieldDelegate _next;
        private readonly IBewitContext _bewitContext;
        private readonly IBewitTokenValidator<object> _tokenValidator;

        public BewitAuthorizationMiddleware(
            FieldDelegate next,
            IBewitContext bewitContext,
            IBewitTokenValidator<object> tokenValidator)
        {
            _next = next
                ?? throw new ArgumentNullException(nameof(next));
            _bewitContext = bewitContext
                ?? throw new ArgumentNullException(nameof(bewitContext));
            _tokenValidator = tokenValidator
                ?? throw new ArgumentNullException(nameof(tokenValidator));
        }

        public async Task InvokeAsync(
            IDirectiveContext context)
        {
            try
            {
                if (context.ContextData.TryGetValue(
                        BewitTokenHeader.Value, out var objectToken) &&
                    objectToken is string bewitToken)
                {
                    object payload = await
                        _tokenValidator.ValidateBewitTokenAsync(
                            new BewitToken<object>(bewitToken),
                            context.RequestAborted);

                    await _bewitContext.SetAsync(payload);

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
