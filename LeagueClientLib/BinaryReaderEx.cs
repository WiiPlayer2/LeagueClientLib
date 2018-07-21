using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace LeagueClientLib
{
    public class BinaryReaderEx : BinaryReader
    {
        private Encoding enc = Encoding.Default;

        public BinaryReaderEx(Stream input) : base(input)
        {
        }

        public BinaryReaderEx(Stream input, Encoding encoding) : base(input, encoding)
        {
            enc = encoding;
        }

        public BinaryReaderEx(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
            encoding = enc;
        }

        //TODO: Fix appending, should read all bytes and then decode
        public string ReadString0()
        {
            var str = new StringBuilder();
            int curr = 0;
            while((curr = BaseStream.ReadByte()) > 0)
            {
                str.Append((char)(byte)curr);
            }
            if (curr == -1)
                throw new InvalidDataException();
            return str.ToString();
        }

        public string ReadString(int length)
        {
            var bytes = ReadBytes(length);
            return enc.GetString(bytes);
        }

        public string ReadHex(int length, bool lowerCase = false)
        {
            var bytes = ReadBytes(length);
            var format = lowerCase ? "x2" : "X2";
            return string.Concat(bytes.Select(o => o.ToString(format)));
        }
    }
}
