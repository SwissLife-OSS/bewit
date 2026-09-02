using FluentAssertions;
using Xunit;

namespace Bewit.Tests;

public class DefaultNonceRepositoryTests
{
    private readonly DefaultNonceRepository _sut = new();

    [Fact]
    public async Task InsertOneAsync_ShouldThrowNotSupportedException()
    {
        var token = Token.Create(Guid.NewGuid(), DateTime.UtcNow);

        Func<Task> act = () => _sut
            .InsertOneAsync(token, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*persistent nonce repository*");
    }

    [Fact]
    public async Task TakeOneAsync_ShouldThrowNotSupportedException()
    {
        Func<Task> act = () => _sut
            .TakeOneAsync(Guid.NewGuid(), CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*persistent nonce repository*");
    }

    [Fact]
    public async Task DeleteIdentifierAsync_ShouldThrowNotSupportedException()
    {
        Func<Task> act = () => _sut
            .DeleteIdentifierAsync("id", CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*persistent nonce repository*");
    }

    [Fact]
    public async Task UpdateExpiryAsync_ShouldThrowNotSupportedException()
    {
        Func<Task> act = () => _sut
            .UpdateExpiryAsync(Guid.NewGuid(), DateTime.UtcNow, CancellationToken.None)
            .AsTask();

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*persistent nonce repository*");
    }

    [Fact]
    public async Task UpdateExpiryByIdentifierAsync_ShouldThrowNotSupportedException()
    {
        Func<Task> act = () => _sut
            .UpdateExpiryByIdentifierAsync("share-123", DateTime.UtcNow, CancellationToken.None)
            .AsTask();

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*persistent nonce repository*");
    }

    [Fact]
    public async Task UpdateExpiryByIdentifierAsync_CustomRepositoryWithoutOverride_ShouldRemainCompatible()
    {
        INonceRepository repository = new ExistingCustomNonceRepository();

        Func<Task> act = () => repository
            .UpdateExpiryByIdentifierAsync("share-123", DateTime.UtcNow, CancellationToken.None)
            .AsTask();

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*supports it*");
    }

    private sealed class ExistingCustomNonceRepository : INonceRepository
    {
        public ValueTask InsertOneAsync(Token token, CancellationToken cancellationToken) =>
            ValueTask.CompletedTask;

        public ValueTask<Token?> TakeOneAsync(Guid nonce, CancellationToken cancellationToken) =>
            ValueTask.FromResult<Token?>(null);

        public ValueTask DeleteIdentifierAsync(
            string identifier,
            CancellationToken cancellationToken) =>
            ValueTask.CompletedTask;

        public ValueTask<bool> UpdateExpiryAsync(
            Guid nonce,
            DateTime newExpiry,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult(false);
    }
}
