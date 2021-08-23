using ProtoBuf;

namespace ProtoUntyped.Viewer.Messages
{
    [ProtoContract]
    public class MessagesHandlingStarted
    {
        [ProtoMember(1, IsRequired = true)]
        public string InstanceName { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public string MachineName { get; set; }

        [ProtoMember(3, IsRequired = true)]
        public HandledMessage[] HandledMessages { get; set; }
        
        [ProtoContract]
        public class HandledMessage
        {
            [ProtoMember(1, IsRequired = true)]
            public readonly string MessageFullName;

            [ProtoMember(2, IsRequired = true)]
            public readonly bool IsCommand;

            [ProtoMember(3, IsRequired = true)]
            public readonly string MessageHandlerFullName;
        }
    }
}