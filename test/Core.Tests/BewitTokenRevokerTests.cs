using FluentAssertions;
using Moq;
using Xunit;

namespace Bewit.Tests;

public class BewitTokenRevokerTests
{
    [Fact]
    public async Task RevokeByIdentifierAsync_ShouldDelegateToNonceRepository()
    {
        var mock = new Mock<INonceRepository>();

        mock.Setup(r => r.DeleteIdentifierAsync("test-id", It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var sut = new BewitTokenRevoker<string>(mock.Object);

        await sut.RevokeByIdentifierAsync("test-id", CancellationToken.None);

        mock.Verify(
            r => r.DeleteIdentifierAsync("test-id", CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public void RevokeByIdentifierAsync_WhenRepoThrows_ShouldPropagate()
    {
        var mock = new Mock<INonceRepository>();

        mock.Setup(r => r.DeleteIdentifierAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotSupportedException("no repo"));

        var sut = new BewitTokenRevoker<string>(mock.Object);

        Func<Task> act = () => sut
            .RevokeByIdentifierAsync("test-id", CancellationToken.None).AsTask();

        act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*no repo*");
    }
}
