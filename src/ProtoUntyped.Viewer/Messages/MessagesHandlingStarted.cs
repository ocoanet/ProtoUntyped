using ProtoBuf;

namespace ProtoUntyped.Viewer.Messages;

[ProtoContract]
public class MessagesHandlingStarted
{
    [ProtoMember(1)]
    public string InstanceName { get; set; }

    [ProtoMember(2)]
    public string MachineName { get; set; }

    [ProtoMember(3)]
    public HandledMessage[] HandledMessages { get; set; }
        
    [ProtoContract]
    public class HandledMessage
    {
        [ProtoMember(1)]
        public string MessageFullName { get; set; }

        [ProtoMember(2)]
        public bool IsCommand { get; set; }

        [ProtoMember(3)]
        public string MessageHandlerFullName { get; set; }
    }
}