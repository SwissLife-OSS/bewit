using System;
using Bewit.Core;
using Moq;

namespace Bewit.Extensions.Mvc.Tests.Integration
{
    internal static class MockHelper
    {
        internal static ICryptographyService GetMockedCrpytoService<T>()
        {
            var cryptoService = new Mock<ICryptographyService>();
            cryptoService.Setup(s => s.GetHash(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<T>()
                ))
                .Returns("serkjhbfujhesnbfuhesnbf");

            return cryptoService.Object;
        }
    }
}
