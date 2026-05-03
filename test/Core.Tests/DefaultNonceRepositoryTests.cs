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
    public async Task ExtendExpiryAsync_ShouldThrowNotSupportedException()
    {
        Func<Task> act = () => _sut
            .ExtendExpiryAsync(Guid.NewGuid(), TimeSpan.FromMinutes(1), CancellationToken.None)
            .AsTask();

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
}
