using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace ProtoUntyped.Decoders;

internal static class TimeDecoder
{
    private static readonly DateTime _epoch = new DateTime(1970, 1, 1);

    public static DateTime? TryParseDateTime(IReadOnlyList<ProtoWireField> fields, ProtoDecodeOptions options)
    {
        if (TryParseScaledTicks(fields) is not { } ticks || ticks.ToDateTime() is not { } dateTime)
            return null;

        return options.DateTimeValidator.Invoke(dateTime) ? dateTime : null;
    }

    public static TimeSpan? TryParseTimeSpan(IReadOnlyList<ProtoWireField> fields, ProtoDecodeOptions options)
    {
        if (fields.Any(x => x.FieldNumber == 3)) // Kind should not be specified for TimeSpan
            return null;
            
        if (TryParseScaledTicks(fields) is not { } ticks || ticks.ToTimeSpan() is not { } timeSpan)
            return null;

        return options.TimeSpanValidator.Invoke(timeSpan) ? timeSpan : null;
    }

    private static ScaledTicks? TryParseScaledTicks(IReadOnlyList<ProtoWireField> fields)
    {
        if (fields.Count is 0 or > 3)
            return null;

        var ticks = new ScaledTicks();

        foreach (var member in fields)
        {
            if (member.Value.Type != ProtoWireValueType.Int64)
                return null;

            var value = member.Value.Int64Value;
                
            switch (member)
            {
                case { FieldNumber: 1, WireType: WireType.Varint }:
                    ticks.Value = Zag(value);
                    break;
                    
                case { FieldNumber: 2, WireType: WireType.Varint }:
                    ticks.Scale = (TimeSpanScale)value;
                    break;
                    
                case { FieldNumber: 3, WireType: WireType.Varint }:
                    ticks.Kind = (DateTimeKind)value;
                    break;
                    
                default:
                    return null;
            }
        }

        if (!Enum.IsDefined(typeof(TimeSpanScale), ticks.Scale))
            return null;
            
        if (!Enum.IsDefined(typeof(DateTimeKind), ticks.Kind))
            return null;

        return ticks;
    }
        
    private static long Zag(long value)
    {
        return -(value & 0x01L) ^ ((value >> 1) & ~(1L << 63));
    }

    private enum TimeSpanScale
    {
        Days = 0,
        Hours = 1,
        Minutes = 2,
        Seconds = 3,
        Milliseconds = 4,
        Ticks = 5,

        MinMax = 15
    }

    private struct ScaledTicks
    {
        public long Value;
        public TimeSpanScale Scale;
        public DateTimeKind Kind;

        public DateTime? ToDateTime()
        {
            return (Scale, Value) switch
            {
                (TimeSpanScale.Days, _)         => FromTicks(Kind, Value * TimeSpan.TicksPerDay),
                (TimeSpanScale.Hours, _)        => FromTicks(Kind, Value * TimeSpan.TicksPerHour),
                (TimeSpanScale.Minutes, _)      => FromTicks(Kind, Value * TimeSpan.TicksPerMinute),
                (TimeSpanScale.Seconds, _)      => FromTicks(Kind, Value * TimeSpan.TicksPerSecond),
                (TimeSpanScale.Milliseconds, _) => FromTicks(Kind, Value * TimeSpan.TicksPerMillisecond),
                (TimeSpanScale.Ticks, _)        => FromTicks(Kind, Value),
                (TimeSpanScale.MinMax, 1)       => DateTime.MaxValue,
                (TimeSpanScale.MinMax, -1)      => DateTime.MinValue,
                _                               => null,
            };

            static DateTime FromTicks(DateTimeKind kind, long ticks) => DateTime.SpecifyKind(_epoch, kind).AddTicks(ticks);
        }
            
        public TimeSpan? ToTimeSpan()
        {
            return (Scale, Value) switch
            {
                (TimeSpanScale.Days, _)         => TimeSpan.FromDays(Value),
                (TimeSpanScale.Hours, _)        => TimeSpan.FromHours(Value),
                (TimeSpanScale.Minutes, _)      => TimeSpan.FromMinutes(Value),
                (TimeSpanScale.Seconds, _)      => TimeSpan.FromSeconds(Value),
                (TimeSpanScale.Milliseconds, _) => TimeSpan.FromMilliseconds(Value),
                (TimeSpanScale.Ticks, _)        => TimeSpan.FromTicks(Value),
                (TimeSpanScale.MinMax, 1)       => TimeSpan.MaxValue,
                (TimeSpanScale.MinMax, -1)      => TimeSpan.MinValue,
                _                               => null,
            };
        }
    }
}