using Bewit.Exceptions;
using Bewit.Validation;
using HotChocolate;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Extensions.HotChocolate;

internal sealed class BewitAuthorizationMiddleware<T>(FieldDelegate next)
    where T : notnull
{
    public async Task InvokeAsync(IMiddlewareContext context)
    {
        IHttpContextAccessor httpContextAccessor = context.Services
            .GetRequiredService<IHttpContextAccessor>();

        HttpContext? httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            context.ReportError(
                ErrorBuilder.New()
                    .SetMessage("No HTTP context available.")
                    .SetCode("BEWIT_NO_HTTP_CONTEXT")
                    .Build());

            return;
        }

        string? tokenString = null;

        if (httpContext.Items.TryGetValue(
                BewitTokenConstants.ContextKey, out var tokenObj))
        {
            tokenString = tokenObj as string;
        }

        if (context.ContextData.TryGetValue(
                BewitTokenConstants.ContextKey, out var contextTokenObj))
        {
            tokenString ??= contextTokenObj as string;
        }

        if (string.IsNullOrWhiteSpace(tokenString))
        {
            context.ReportError(
                ErrorBuilder.New()
                    .SetMessage("Missing bewit token.")
                    .SetCode("BEWIT_MISSING")
                    .Build());

            return;
        }

        try
        {
            var validator = context.Services
                .GetRequiredService<IBewitTokenValidator<T>>();

            var bewitToken = new BewitToken<T>(tokenString);
            T payload = await validator.ValidateBewitTokenAsync(
                bewitToken, context.RequestAborted);

            httpContextAccessor.SetBewitPayload(payload);
        }
        catch (BewitException ex)
        {
            context.ReportError(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode(ex switch
                    {
                        BewitExpiredException => "BEWIT_EXPIRED",
                        BewitNotFoundException => "BEWIT_NOT_FOUND",
                        BewitInvalidException => "BEWIT_INVALID",
                        _ => "BEWIT_ERROR"
                    })
                    .SetException(ex)
                    .Build());

            return;
        }

        await next(context);
    }
}
