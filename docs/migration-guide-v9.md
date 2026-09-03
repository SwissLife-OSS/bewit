# Migrating Bewit v8 to v9

## Before deployment

1. Inventory each v8 payload type, its exact `Type.FullName`, secret, expiration mode, nonce usage, maximum lifetime, and MongoDB collection.
2. Choose an explicit stable purpose for each payload.
3. Add a 32-byte-or-longer v9 signing key and key ID.
4. Install `Bewit`, the required v9 integration packages, and the two v8 compatibility packages where old links must survive.
5. Configure v9 MongoDB to use `bewit_tokens`. Do not rename or migrate the old `bewit_nonces` collection.
6. Set an explicit `AcceptUntil` for every v8 mapping.
7. If old records may have a null identifier, add a narrowly scoped validation policy that recognizes only that legacy case, updates by `context.Reference`, and returns `RefreshState`.
8. Change domain expiration updates to call `IBewitTokenRepository<TPayload>.UpdateExpirationByIdentifierAsync` in the same application operation. The repository updates all configured v8/v9 stores and returns per-format counts.

## Deployment

Use a blue-green or atomic cutover, not a rolling deployment with mixed v8 and v9 instances. V9 starts issuing `bwt1` tokens immediately, and v8 instances cannot read them.

At cutover, verify:

- an existing unprefixed v8 link still works;
- a newly created link starts with `bwt1.` and works;
- extending an already expired legacy link repairs it through the temporary policy;
- extending a new link updates its domain record and the v9 `bewit_tokens` record directly;
- invalid `bwt1` input is rejected without a v8 fallback;
- observer failures do not change validation results.

## Rollback

Rollback must restore the whole v8 traffic pool. Links issued while v9 was active will not work on v8. Preserve the v9 signing keys and `bewit_tokens` collection so a later v9 redeployment can validate those links.

## Compatibility removal

After `AcceptUntil`, plus a deliberate operational grace period:

1. confirm that unprefixed-token traffic is zero;
2. remove the legacy null-identifier validation policy;
3. remove `AcceptV8Tokens` and `UseV8MongoDb` registrations;
4. uninstall `Bewit.Compatibility.V8` and `Bewit.Compatibility.V8.MongoDB`;
5. archive or delete `bewit_nonces` only under the application's data-retention process.

The clean end state contains no legacy codec, legacy store, or request-time repair logic.
