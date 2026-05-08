using System.Reflection;
using Bewit.Exceptions;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;

namespace Bewit.Extensions.HotChocolate;

[AttributeUsage(AttributeTargets.Method)]
public sealed class BewitAttribute<T> : ObjectFieldDescriptorAttribute
    where T : notnull
{
    public Type? ExceptionType { get; set; }

    public bool SuppressExceptions { get; set; }

    protected override void OnConfigure(
        IDescriptorContext context,
        IObjectFieldDescriptor descriptor,
        MemberInfo member)
    {
        descriptor.AuthorizeBewit<T>();

        if (ExceptionType is not null || SuppressExceptions)
        {
            descriptor.Use(next => async middlewareContext =>
            {
                if (middlewareContext.Result is IError error
                    && error.Exception is BewitNotFoundException or BewitExpiredException)
                {
                    if (SuppressExceptions)
                    {
                        middlewareContext.Result = null;
                    }
                    else if (ExceptionType is not null)
                    {
                        throw (Exception)Activator.CreateInstance(ExceptionType)!;
                    }
                }

                await next(middlewareContext);
            });
        }
    }
}
