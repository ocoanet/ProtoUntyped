using System;
using System.Text;

namespace ProtoUntyped
{
    public class ProtoStringDecoder
    {
        private static readonly Encoding _encoding = new UTF8Encoding(true, true);

        public static readonly ProtoStringDecoder Default = new(_ => true);
        public static readonly ProtoStringDecoder AsciiOnly = new(IsValidAsciiString);

        private readonly Func<string, bool> _validator;

        public ProtoStringDecoder(Func<string, bool> validator)
        {
            _validator = validator;
        }

        public virtual bool TryDecode(byte[] bytes, out string result)
        {
            try
            {
                result = _encoding.GetString(bytes);
                return _validator.Invoke(result);
            }
            catch (Exception)
            {
                result = string.Empty;
                return false;
            }
        }

        private static bool IsValidAsciiString(string s)
        {
            foreach (var c in s)
            {
                if (c >= 128)
                    return false;
            }

            return true;
        }
    }
}
