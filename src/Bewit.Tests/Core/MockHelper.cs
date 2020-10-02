using System;
using Bewit.Core;
using Moq;
using Newtonsoft.Json;

namespace Bewit.Tests.Core
{
    internal static class MockHelper
    {
        internal class MockedVariablesProvider : IVariablesProvider
        {
            public DateTime UtcNow =>
                new DateTime(2017, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);

            public Guid NextToken =>
                new Guid("724e7acc-be57-49a1-8195-46a03c6271c6");
        }

        internal class MockedVariablesProvider2 : IVariablesProvider
        {
            public DateTime UtcNow =>
                new DateTime(2018, 6, 6, 1, 1, 1, 1, DateTimeKind.Utc);

            public Guid NextToken =>
                new Guid("724e7acc-be57-49a1-8195-46a03c6271c6");
        }

        internal static ICryptographyService GetMockedCrpytoService<T>()
        {
            var cryptoService = new Mock<ICryptographyService>();
            cryptoService.Setup(s => s.GetHash(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<T>()
                ))
                .Returns((string s, DateTime exp, T payload) =>
                    $"{s}__{exp:O}__{JsonConvert.SerializeObject(payload)}");

            return cryptoService.Object;
        }
    }
}
