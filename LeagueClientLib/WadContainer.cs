using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace LeagueClientLib
{
    public abstract class WadContainer
    {
        public abstract class Header
        {
            internal Header(byte majorVersion, byte minorVersion)
            {
                MajorVersion = majorVersion;
                MinorVersion = minorVersion;
            }

            public byte MajorVersion { get; set; }
            public byte MinorVersion { get; set; }
            public uint FilesCount { get; set; }

            internal static Header Read(BinaryReaderEx reader)
            {
                var magic = reader.ReadString(2);
                if (magic != "RW")
                    throw new InvalidDataException("Not a valid WAD file");

                var majorVersion = reader.ReadByte();
                var minorVersion = reader.ReadByte();

                Header header = null;

                switch(majorVersion)
                {
                    case 3:
                        header = new WadContainerV3.HeaderV3(majorVersion, minorVersion);
                        break;
                    default:
                        throw new NotSupportedException($"WAD version {majorVersion}.{minorVersion} is not supported.");
                }

                header.ReadEx(reader);
                return header;
            }

            internal abstract void ReadEx(BinaryReaderEx reader);

            internal abstract void Write(BinaryWriterEx writer);
        }

        public abstract class FileEntry
        {
            internal FileEntry() { }

            public byte[] Data { get; set; }
            public ulong PathHash { get; set; }
            public uint Offset { get; set; }
            public uint CompressedSize { get; set; }
            public uint Size { get; set; }

            internal static FileEntry Read(BinaryReaderEx reader, Header header)
            {
                FileEntry entry = null;
                switch(header.MajorVersion)
                {
                    case 3:
                        entry = new WadContainerV3.FileEntryV3();
                        break;
                }
                entry.ReadEx(reader);
                return entry;
            }

            internal abstract void ReadEx(BinaryReaderEx reader);

            internal abstract void Write(BinaryWriterEx writer);

            public void Extract(Stream stream)
            {
                stream.Write(Data, 0, Data.Length);
                stream.Flush();
            }

            public override string ToString()
            {
                return $"{PathHash:X16} <0x{Offset:X8}>";
            }
        }

        protected WadContainer(Header header)
        {
            ContainerHeader = header;
        }

        public Header ContainerHeader { get; }

        public FileEntry[] Files { get; protected set; }

        public static WadContainer Read(string path)
        {
            var f = File.OpenRead(path);
            try
            {
                return Read(f);
            }
            finally
            {
                f.Close();
            }
        }

        public static WadContainer Read(Stream stream)
        {
            if (!stream.CanSeek)
                throw new NotSupportedException();

            var reader = new BinaryReaderEx(stream);
            var header = Header.Read(reader);

            WadContainer wad = null;

            switch(header.MajorVersion)
            {
                case 3:
                    wad = new WadContainerV3(header);
                    break;
            }

            wad.Read(reader);
            return wad;
        }

        protected void Read(BinaryReaderEx reader)
        {
            Files = new FileEntry[ContainerHeader.FilesCount];
            for (var i = 0; i < Files.Length; i++)
            {
                Files[i] = FileEntry.Read(reader, ContainerHeader);
            }

            foreach(var f in Files)
            {
                reader.BaseStream.Seek(f.Offset, SeekOrigin.Begin);
                f.Data = reader.ReadBytes((int)f.CompressedSize);
            }
        }

        public void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
