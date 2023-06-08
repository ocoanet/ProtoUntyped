using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class SearchRequest
{
    [ProtoMember(1)]
    public string Query { get; set; }

    [ProtoMember(2)]
    public int PageNumber { get; set; }
        
    [ProtoMember(3)]
    public int ResultPerPage { get; set; }
}