using System;
using System.Collections.Generic;
using System.Text;

namespace LeagueClientLib
{
    public class WadContainerV3 : WadContainer
    {
        internal WadContainerV3(Header header) : base(header)
        {
        }

        public class HeaderV3 : Header
        {
            internal HeaderV3(byte majorVersion, byte minorVersion) : base(majorVersion, minorVersion)
            {
            }

            public byte[] ECDSA { get; set; }
            public ulong FilesChecksum { get; set; }

            internal override void ReadEx(BinaryReaderEx reader)
            {
                ECDSA = reader.ReadBytes(256);
                FilesChecksum = reader.ReadUInt64();
                FilesCount = reader.ReadUInt32();
            }

            internal override void Write(BinaryWriterEx writer)
            {
                throw new NotImplementedException();
            }
        }

        public class FileEntryV3 : FileEntry
        {
            public byte Type { get; set; }
            public byte Duplicate { get; set; }
            public byte Unknown1 { get; set; }
            public byte Unknown2 { get; set; }
            public ulong Sha256 { get; set; }

            internal override void ReadEx(BinaryReaderEx reader)
            {
                PathHash = reader.ReadUInt64(); //Path Hash
                Offset = reader.ReadUInt32(); //Offset
                CompressedSize = reader.ReadUInt32(); //Compressed Size
                Size = reader.ReadUInt32(); //File Size
                Type = reader.ReadByte(); //Type
                Duplicate = reader.ReadByte(); //Duplicate
                Unknown1 = reader.ReadByte(); //Unknown
                Unknown2 = reader.ReadByte(); //Unknown
                Sha256 = reader.ReadUInt64(); //First 8 bytes of fileEntry sha256
            }

            internal override void Write(BinaryWriterEx writer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
