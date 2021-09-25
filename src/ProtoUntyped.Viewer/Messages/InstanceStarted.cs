using System;
using ProtoBuf;

namespace ProtoUntyped.Viewer.Messages
{
    [ProtoContract]
    public class InstanceStarted
    {
        [ProtoMember(1)]
        public string InstanceName { get; set; }

        [ProtoMember(2)]
        public string MachineName { get; set; }

        [ProtoMember(3)]
        public Guid BusId { get; set; }

        [ProtoMember(4)]
        public DateTime StartDateUtc { get; set; }

        [ProtoMember(5, IsRequired = false)]
        public string QueueName { get; set; }
        
        [ProtoMember(7, IsRequired = false)]
        public SupportedOperatingSystem? OperatingSystem { get; set; }

        [ProtoMember(9, IsRequired = false)]
        public string UserName { get; set; }

        [ProtoMember(10, IsRequired = false)]
        public TimeSpan? HeartBeatPeriod { get; set; }

        [ProtoMember(11, IsRequired = false)]
        public string FrameworkDescription { get; set; }
    }
}
