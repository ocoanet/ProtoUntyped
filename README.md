[![Build](https://github.com/ocoanet/ProtoUntyped/workflows/build-and-test/badge.svg)](https://github.com/ocoanet/ProtoUntyped/actions/workflows/build-and-test.yml)
[![NuGet](https://img.shields.io/nuget/v/ProtoUntyped)](https://www.nuget.org/packages/ProtoUntyped/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

# ProtoUntyped

Did you ever want to perfectly decode unknown [protocol buffer](https://developers.google.com/protocol-buffers) encoded data when you do not have access to the message defintions? Well you can't. But at least, with this library, you can try.

## Overview

ProtoUntyped can help you **read unknown protocol buffer encoded data**. The result will not be the same as with the message definitions, because message definitions are required to properly read the binary wire format. ProtoUntyped uses conventions and heuristics to identify the types from the binary data.

This library is based on the excellent [protobuf-net](https://github.com/protobuf-net/protobuf-net) serializer.

ProtoUntyped is not designed to be used in critical production code. However, it can be really helpful for data analysis and debugging.

## Usage

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
var message = new SearchRequest { Query = "/users", PageNamer = 2, ResultPerPage = 50 };
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
- 2 = 2
- 3 = 50
}
```

ProtoUntyped uses a few heuristics to map wire types to .NET types. A few of them can be configured using the `ProtoDecodeOptions` class:

```cs
var options = new ProtoDecodeOptions { DecodeGuid = true };

var protoObject = ProtoObject.Decode(bytes, options);
```

## Implementation

The protocol buffer wire format contains a sequence of fields. Every field contains the **field number**, the **wire type**, and the **field value**. The field numbers and the wire types can be safely read, so you will always get a valid list of top level field meta-data. Then ProtoUntyped uses heuristics to decode the field values depending on the wire type.

### Wire type 0 (Varint)
Type 0 can be used for `int32, int64, uint32, uint64, sint32, sint64, bool, enum`. The size does not really matter for decoding, but it is required to know if the value is signed. ProtoUntyped always decode type 0 as signed.

### Wire type 1 (64-bit)
Type 1 can be used for `fixed64, sfixed64, double`. Because protobuf-net uses type 0 for integers by default, ProtoUntyped interprets type 1 as `System.Double`.

### Wire type 2 (length-delimited)
Type 2 can be used for `string, bytes, embedded messages, packed repeated fields`. ProtoUntyped will try to interpret the data as an embedded message. If the decoding fails, the data will be interpreted as a `System.String`.

### Wire type 5 (32-bit)
Type 5 can be used for `fixed32, sfixed32, float`. Because protobuf-net uses type 0 for integers by default, ProtoUntyped interprets type 5 as `System.Float`.

## BCL types

protobuf-net supports the serialization of a few .NET types like `Guid`, `DateTime` or `decimal`. These types are not part of the protocol buffer format and protobuf-net uses a custom wire format based on embedded messages.

ProtoUntyped does not decode the BCL types by default but it can be activated using the `ProtoDecodeOptions`.
