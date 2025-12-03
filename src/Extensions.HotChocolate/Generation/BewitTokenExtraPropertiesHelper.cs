using System.Collections.Generic;
using System.Linq;
using HotChocolate.Resolvers;

namespace Bewit.Extensions.HotChocolate.Generation;

public static class BewitTokenExtraPropertiesHelper

{
    private const string ExtraPropertyPrefix = "BewitTokenExtraProperty:";

    public static void AddBewitTokenExtraProperties(
        this IResolverContext resolverContext, Dictionary<string, object> extraProperties)
    {
        if (extraProperties == null)
        {
            return;
        }

        resolverContext.ScopedContextData =
             resolverContext.ScopedContextData.SetItems(
                  extraProperties.ToDictionary(
                      ctx => $"{ExtraPropertyPrefix}{ctx.Key}",
                      ctx => ctx.Value));
    }

    public static Dictionary<string, object> GetBewitTokenExtraProperties(this IMiddlewareContext context)
    {
        Dictionary<string, object> extraProperties = new Dictionary<string, object>();

        foreach (var key in context.ScopedContextData.Keys)
        {
            if (!key.StartsWith(ExtraPropertyPrefix))
            {
                continue;
            }

            object extraPropertyValue = context.ScopedContextData.GetValueOrDefault(key);

            if (extraPropertyValue != null)
            {
                extraProperties.Add(
                    key.Substring(ExtraPropertyPrefix.Length),
                    extraPropertyValue);
            }
        }

        return extraProperties;
    }

}
