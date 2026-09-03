# Hot Chocolate secured argument sample

This sample shows three stable token purposes: one self-contained payload and two server-controlled payloads backed by the shared `bewit_tokens` MongoDB collection. It also demonstrates identifier-based revocation through `IBewitTokenRepository<TPayload>`.

Start MongoDB on `localhost:27017`, then run the application and use its GraphQL endpoint. The complete v9 registration is in `Startup.cs`.
