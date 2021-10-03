namespace ProtoUntyped
{
    public class ProtoDecodeOptions
    {
        public bool DecodeGuids { get; set; }
        public ProtoStringDecoder StringDecoder { get; set; } = ProtoStringDecoder.Default;
    }
}
