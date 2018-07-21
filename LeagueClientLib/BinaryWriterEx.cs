using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LeagueClientLib
{
    public class BinaryWriterEx : BinaryWriter
    {
        public enum StringMode
        {
            LengthPrefixed,
            NullTerminated,
            Fixed,
        }

        private Encoding enc = Encoding.Default;

        public BinaryWriterEx(Stream output) : base(output)
        {
        }

        public BinaryWriterEx(Stream output, Encoding encoding) : base(output, encoding)
        {
            enc = encoding;
        }

        public BinaryWriterEx(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen)
        {
            enc = encoding;
        }

        protected BinaryWriterEx()
        {
        }

        public void Write(string value, StringMode mode)
        {
            switch(mode)
            {
                case StringMode.LengthPrefixed:
                    Write(value);
                    break;
                case StringMode.NullTerminated:
                    Write(enc.GetBytes(value));
                    Write((byte)0);
                    break;
                case StringMode.Fixed:
                    Write(enc.GetBytes(value));
                    break;
            }
        }
    }
}
