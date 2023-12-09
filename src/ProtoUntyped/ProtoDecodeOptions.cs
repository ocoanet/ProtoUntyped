using System;
using System.Collections.Generic;
using ProtoBuf;

namespace ProtoUntyped;

public class ProtoDecodeOptions
{
    /// <summary>
    /// Enable <see cref="Guid"/> decoding (using protobuf-net format).
    /// </summary>
    /// <remarks>
    /// protobuf-net uses the following nested message to encode Guid:
    /// <code>
    /// message Guid {
    ///   fixed64 lo = 1;
    ///   fixed64 hi = 2;
    /// }
    /// </code>
    /// Any nested message that uses the same format might be incorrectly identified as a <see cref="Guid"/>.
    /// You can use <see cref="GuidValidator"/> to reject incorrect values.
    /// </remarks>
    public bool DecodeGuid { get; set; }

    /// <summary>
    /// Specify the delegate that will used to identify valid <see cref="Guid"/> values.
    /// </summary>
    public Func<(Guid Guid, byte Version), bool> GuidValidator { get; set; } = DefaultGuidValidator;

    /// <summary>
    /// Enable <see cref="DateTime"/> decoding (using protobuf-net format).
    /// </summary>
    /// <remarks>
    /// protobuf-net uses the following nested message to encode DateTime:
    /// <code>
    /// message DateTime {
    ///   sint64 value = 1;
    ///   TimeSpanScale scale = 2;
    ///   DateTimeKind kind = 3;
    ///   enum TimeSpanScale {
    ///     DAYS = 0;
    ///     HOURS = 1;
    ///     MINUTES = 2;
    ///     SECONDS = 3;
    ///     MILLISECONDS = 4;
    /// 	TICKS = 5;
    ///     MINMAX = 15;
    ///   }
    ///   enum DateTimeKind {     
    ///      UNSPECIFIED = 0;
    ///      UTC = 1;
    ///      LOCAL = 2;
    ///    }
    /// }
    /// </code>
    /// Any nested message that uses the same format might be incorrectly identified as a <see cref="DateTime"/>.
    /// You can use <see cref="DateTimeValidator"/> to reject incorrect values.
    /// </remarks>
    public bool DecodeDateTime { get; set; }

    /// <summary>
    /// Specify the delegate that will used to identify valid <see cref="DateTime"/> values.
    /// </summary>
    public Func<DateTime, bool> DateTimeValidator { get; set; } = DefaultDateTimeValidator;

    /// <summary>
    /// Enable <see cref="TimeSpan"/> decoding (using protobuf-net format).
    /// </summary>
    /// <remarks>
    /// protobuf-net uses the following nested message to encode TimeSpan:
    /// <code>
    /// message TimeSpan {
    ///   sint64 value = 1;
    ///   TimeSpanScale scale = 2;
    ///   enum TimeSpanScale {
    ///     DAYS = 0;
    ///     HOURS = 1;
    ///     MINUTES = 2;
    ///     SECONDS = 3;
    ///     MILLISECONDS = 4;
    /// 	TICKS = 5;
    ///     MINMAX = 15;
    ///   }
    /// }
    /// </code>
    /// Any nested message that uses the same format might be incorrectly identified as a <see cref="TimeSpan"/>.
    /// You can use <see cref="TimeSpanValidator"/> to reject incorrect values.
    /// </remarks>
    public bool DecodeTimeSpan { get; set; }

    /// <summary>
    /// Specify the delegate that will used to identify valid <see cref="TimeSpan"/> values.
    /// </summary>
    public Func<TimeSpan, bool> TimeSpanValidator { get; set; } = DefaultTimeSpanValidator;

    /// <summary>
    /// Enable <see cref="decimal"/> decoding (using protobuf-net format).
    /// </summary>
    /// <remarks>
    /// protobuf-net uses the following nested message to encode decimal:
    /// <code>
    /// message Decimal {
    ///     uint64 lo = 1;
    ///     uint32 hi = 2;
    ///     uint32 signScale = 3;
    /// }
    /// </code>
    /// Any nested message that uses the same format might be incorrectly identified as a <see cref="decimal"/>.
    /// You can use <see cref="DecimalValidator"/> to reject incorrect values.
    /// </remarks>
    public bool DecodeDecimal { get; set; }

    /// <summary>
    /// Specify the delegate that will used to identify valid <see cref="decimal"/> values.
    /// </summary>
    public Func<decimal, bool> DecimalValidator { get; set; } = DefaultDecimalValidator;


    /// <summary>
    /// Specify the delegate that will used to identify valid <see cref="String"/> values.
    /// </summary>
    public Func<Memory<byte>, bool> StringValidator { get; set; } = DefaultStringValidator;

    /// <summary>
    /// Specify how empty strings should are decoded.
    /// </summary>
    /// <remarks>
    /// "Empty strings" refers to records with the wire type 2 (LEN) with an empty payload.
    /// </remarks>
    public StringWireTypeDecodingMode EmptyStringDecodingMode { get; set; }

    /// <summary>
    /// Specifies how the <see cref="WireType.Fixed32"/> wire type should be decoded.
    /// </summary>
    public FixedWireTypeDecodingMode Fixed32DecodingMode { get; set; }

    /// <summary>
    /// Specifies how the <see cref="WireType.Fixed64"/> wire type should be decoded.
    /// </summary>
    public FixedWireTypeDecodingMode Fixed64DecodingMode { get; set; }

    public static bool DefaultGuidValidator((Guid Guid, byte Version) value)
    {
        return value.Version is >= 1 and <= 5;
    }

    public static bool DefaultDateTimeValidator(DateTime value)
    {
        return value >= DateTime.Today.AddYears(-30) && value <= DateTime.Today.AddYears(20) && value.Kind == DateTimeKind.Unspecified;
    }

    public static bool DefaultTimeSpanValidator(TimeSpan value)
    {
        return value <= TimeSpan.FromDays(60);
    }

    public static ReadOnlySpan<byte> DefaultStringValidatorInvalidBytes => new byte[] { 0, 1, 2, 3, 4, 5, 6 };

    public static bool DefaultStringValidator(Memory<byte> value)
    {
        return value.Span.IndexOfAny(DefaultStringValidatorInvalidBytes) == -1;
    }

    public static bool DefaultDecimalValidator(decimal value)
    {
        return true;
    }
}