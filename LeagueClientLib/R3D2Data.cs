using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LeagueClientLib
{
    public class R3D2Data
    {
        public class WemFragment
        {
            internal WemFragment() { }

            public byte[] Data { get; set; }
            
            public uint Size { get; set; }

            public uint Offset { get; set; }

            internal static WemFragment Read(BinaryReaderEx reader)
            {
                var wem = new WemFragment();
                wem.ReadEx(reader);
                return wem;
            }

            internal void ReadEx(BinaryReaderEx reader)
            {
                Offset = reader.ReadUInt32();
                Size = reader.ReadUInt32();
                var shortNumbers = reader.ReadUInt32(); // following ushort numbers, including 'w.e.m.'
                for(var i = 0; i < shortNumbers; i++)
                {
                    reader.ReadUInt16();
                }
                reader.ReadPadding(4 + 4 + 4 + (int)shortNumbers * 2, 8);
            }
        }

        private R3D2Data() { }

        public WemFragment[] Fragments { get; private set; }

        public static R3D2Data Read(string path)
        {
            using (var f = File.OpenRead(path))
            {
                return Read(f);
            }
        }

        public static R3D2Data Read(byte[] data)
        {
            using (var memStream = new MemoryStream(data))
            {
                memStream.Seek(0, SeekOrigin.Begin);
                return Read(memStream);
            }
        }

        public static R3D2Data Read(Stream stream)
        {
            if (!stream.CanSeek)
                throw new NotSupportedException();

            stream.Seek(0, SeekOrigin.Begin);
            var data = new R3D2Data();
            data.ReadEx(stream);
            return data;
        }

        private void ReadEx(Stream stream)
        {
            var reader = new BinaryReaderEx(stream);

            var magic = reader.ReadString(4); //Magic Number: r3d2
            if (magic != "r3d2")
                throw new InvalidDataException();
            reader.ReadUInt32();
            var wemFragmentCount = reader.ReadUInt32(); //wem Fragment Count

            for(var i = 0; i < 25; i++)
            {
                reader.ReadUInt32();
            }

            Fragments = new WemFragment[wemFragmentCount];
            for(var i = 0; i < wemFragmentCount; i++)
            {
                Fragments[i] = WemFragment.Read(reader);
            }

            foreach(var wem in Fragments)
            {
                reader.BaseStream.Seek(wem.Offset, SeekOrigin.Begin);
                wem.Data = reader.ReadBytes((int)wem.Size);
            }
        }
    }
}
