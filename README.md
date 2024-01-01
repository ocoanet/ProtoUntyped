[![Build](https://github.com/ocoanet/ProtoUntyped/workflows/build/badge.svg)](https://github.com/ocoanet/ProtoUntyped/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/ProtoUntyped)](https://www.nuget.org/packages/ProtoUntyped/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

# ProtoUntyped

Did you ever want to perfectly decode unknown [protocol buffer](https://developers.google.com/protocol-buffers) encoded data when you do not have access to the message defintions? Well you can't. But at least, with this library, you can try.

## Overview

ProtoUntyped can help you **read unknown protocol buffer encoded data**. The result will not be the same as with the message definitions, because message definitions are required to properly read the binary wire format. ProtoUntyped uses conventions and heuristics to identify the types from the binary data.

ProtoUntype can also be used to **dynamically generate protocol buffer encoded data**.

The core type of the library is `ProtoObject`, which is the protobuf equivalent of the `JsonObject` from the `System.Text.Json` API.

This library is based on the excellent [protobuf-net](https://github.com/protobuf-net/protobuf-net) serializer.

## Decoding

Given the following message definition:

```protobuf
message SearchRequest {
  string query = 1;
  int32 page_number = 2;
  int32 result_per_page = 3;
}
```
and the following message:
```cs
var message = new SearchRequest { Query = "/users", PageNumber = 8, ResultPerPage = 50 };
```

The encoded data can be read using:

```cs
var protoObject = ProtoObject.Decode(bytes);

Console.WriteLine(protoObject.ToString());
```

which will print:

```
message {
- 1 = "/users"
- 2 = 8
- 3 = 50
}
```

ProtoUntyped uses conventions to decode the wire format. Most of them can be configured using the [ProtoDecodeOptions](https://github.com/ocoanet/ProtoUntyped/blob/master/src/ProtoUntyped/ProtoDecodeOptions.cs) class.

## Encoding

Any `ProtoObject` can be encoded back to bytes without data-loss:
```cs
var bytes = protoObject.Encode();
```

But because `ProtoObject` instances can be manually created, it is possible to dynamically generate protobuf encoded data:
```cs
var protoObject = new ProtoObject(
    new ProtoField(1, "/users"),
    new ProtoField(2, 8, WireType.Varint),
    new ProtoField(3, 50, WireType.Varint)
);

var bytes = protoObject.Encode();
```

## BCL types

protobuf-net supports the serialization of a few .NET types like `Guid`, `DateTime` or `decimal`. These types are not part of the protocol buffer format and protobuf-net uses a custom wire format based on embedded messages.

ProtoUntyped does not decode the BCL types by default but it can be activated using the `ProtoDecodeOptions`.

## ProtoWireObject

`ProtoWireObject` is low-level an alternative to `ProtoObject`. Here are the differences between the two types:

| Type | Decodes LEN fields | Converts fixed types to floats | Groups repeated fields | Decodes BCL types |
| --- | :---: | :---: | :---: | :---: |
|ProtoWireObject|:white_check_mark:|:x:|:x:|:x:|
|ProtoObject|:white_check_mark:|:white_check_mark:|:white_check_mark:|:white_check_mark:|


Under the hood, the wire format is always parsed as `ProtoWireObject` first.

## Formatting

`ProtoObject.ToString` generates a proto-like string representation of the object, for example:
```
message {
- 1 = "/users"
- 2 = 8
- 3 = 50
}
```

`ProtoObject` can also be exported as [protoscope](https://github.com/protocolbuffers/protoscope), using `ToProtoscopeString`, for example:
```
1: {"/users"}
2: 8
3: 50
```

Because `ProtoObject` can contain types that are not valid in the protobuf wire-format (e.g.: decimal, Guid),
you should generate protoscope strings from `ProtoWireObject` if you need strict protoscope format.

## Decoding conventions

The protocol buffer wire format contains a sequence of fields. Every field contains the **field number**, the **wire type**, and the **field value**. The field numbers and the wire types can be safely read, so you will always get a valid list of top level field meta-data. Then ProtoUntyped uses heuristics to decode the field values depending on the wire type.

### Wire type 0 (Varint)
Type 0 can be used for `int32, int64, uint32, uint64, sint32, sint64, bool, enum`. The size does not really matter for decoding, but it is required to know if the value is signed. ProtoUntyped always decode type 0 as signed.

### Wire type 1 (64-bit)
Type 1 can be used for `fixed64, sfixed64, double`. Because protobuf-net uses type 0 for integers by default, ProtoUntyped interprets type 1 as `System.Double` by default, but it can be configured using `ProtoDecodeOptions.Fixed64DecodingMode`.

### Wire type 2 (length-delimited)
Type 2 can be used for `string, bytes, embedded messages, packed repeated fields`. ProtoUntyped will try to interpret the data as an embedded message. If the decoding fails, the data will be interpreted as a `System.String`. It can be configured using `ProtoDecodeOptions.PreferredStringDecodingModes`.

### Wire type 5 (32-bit)
Type 5 can be used for `fixed32, sfixed32, float`. Because protobuf-net uses type 0 for integers by default, ProtoUntyped interprets type 5 as `System.Float` by default, but it can be configured using `ProtoDecodeOptions.Fixed32DecodingMode`.
