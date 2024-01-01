using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using ProtoUntyped.Tests.Messages;

namespace ProtoUntyped.Tests;

public static class TestData
{
    public static IEnumerable<object> CreateTestMessages()
    {
        foreach (var messageType in typeof(TestMessage).Assembly.GetTypes().Where(x => Attribute.IsDefined(x, typeof(ProtoContractAttribute))))
        {
            yield return ThreadLocalFixture.Create(messageType);
        }
    }

    public static ProtoDecodeOptions GetDecodeOptions(object message)
    {
        return message is IHasRequiredDecodeOptions hasRequiredDecodeOptions ? hasRequiredDecodeOptions.GetRequiredDecodeOptions() : new ProtoDecodeOptions();
    }
}
