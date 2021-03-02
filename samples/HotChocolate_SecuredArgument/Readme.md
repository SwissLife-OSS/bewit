**This project showcases a possible implementation for signed arguments in HotChocolate.**

This HotChocolate Api is generally unsecured. However, one one query field, the supplied argument `input` contains two values: a raw value `name`, and a bewit token that guarantees that the other argument was unaltered.

# Getting Started

Start the application and open the url `http://localhost:5000/` in your browser. 
This will open the [Banana Cakepop GraphQL IDE](https://github.com/ChilliCream/hotchocolate#banana-cake-pop).

Write and execute the following GraphQL Mutation:
```
mutation f {
  createDownloadLink(documentName:"hello_world.pdf")
}
```

This mutation will return a download link for the file `hello_world.pdf`. This link can be used an unlimited number of times for a duration of 5 minutes, since the bewit token is stateless in this sample.

# Expanding this sample

## Making the bewit token stateful 

You can make the bewit token stateful. This means that the bewit token will be stored in a database _and can only be used once_, after which it gets removed from the database.
For this, simply add the project/package `Bewit.Storage.MongoDB` and a Call to `UseMongoPersistance(bewitMongoOptions)` when you register the bewit services:

```
    // Add support for generating bewits in the GraphQL Api
    services.AddBewitGeneration<string>(
        bewitOptions,
        builder => builder.UseHmacSha256Encryption()
            .UseMongoPersistance(
                new BewitMongoOptions {
                    ConnectionString = "myConnectionString",
                    DatabaseName = "myDatabase",
                    CollectionName = "myMongoCollection"
                })
    );

    // Add support for validating bewits in the Mvc Api
    services.AddBewitUrlAuthorizationFilter(
        bewitOptions,
        builder => builder.UseHmacSha256Encryption()
            .UseMongoPersistance(
                new BewitMongoOptions
                {
                    ConnectionString = "myConnectionString",
                    DatabaseName = "myDatabase",
                    CollectionName = "myMongoCollection"
                })
    );
```

## Splitting the Apis into two Applications

You could choose to split this application into two applications:
- A GraphQL App that contains the mutation `createDownloadLink`
- A MVC App that only returns file contents

In this case, the GraphQL App would need the following service registrations:
```
    services.AddBewitGeneration<string>(
        bewitOptions,
        builder => builder.UseHmacSha256Encryption()
    );
```

And the MVC App would need the following service registrations:
```
    services.AddBewitUrlAuthorizationFilter(
        bewitOptions,
        builder => builder.UseHmacSha256Encryption()
    );
```

## Securing the HotChocolate Api

The HotChocolate Api can be secured through ASP.NET Core's standard means. You can read more about this directly on the HotChocolate and microsoft website:
- [HotChocolate security](https://chillicream.com/docs/hotchocolate/v10/security)
- [Policy-based authorization in ASP.NET Core (HotChocolate)](https://chillicream.com/docs/hotchocolate/v10/security#policy-based-authorization-in-aspnet-core)
- [Policy-based authorization in ASP.NET Core (Microsoft)](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-3.1)


