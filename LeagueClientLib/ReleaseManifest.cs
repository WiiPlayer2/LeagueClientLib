using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LeagueClientLib
{
    public class ReleaseManifest
    {
        public class Entry
        {
            protected ReleaseManifest Manifest { get; private set; }

            internal Entry(ReleaseManifest manifest)
            {
                Manifest = manifest;
            }

            public uint NameIndex { get; internal set; }

            public string Name
            {
                get
                {
                    return Manifest.Strings[NameIndex];
                }
            }

            public override string ToString()
            {
                return $"{Name}";
            }
        }

        public class DirectoryEntry : Entry
        {
            internal DirectoryEntry(ReleaseManifest manifest) : base(manifest) { }
            public uint SubDirectoriesStartIndex { get; internal set; }
            public uint SubDirectoriesCount { get; internal set; }
            public uint FilesStartIndex { get; internal set; }
            public uint FilesCount { get; internal set; }

            public IEnumerable<DirectoryEntry> SubDirectories
            {
                get
                {
                    return Manifest.Directories.Skip((int)SubDirectoriesStartIndex).Take((int)SubDirectoriesCount);
                }
            }

            public IEnumerable<FileEntry> Files
            {
                get
                {
                    return Manifest.Files.Skip((int)FilesStartIndex).Take((int)FilesCount);
                }
            }

            internal void Write(BinaryWriterEx writer)
            {
                writer.Write(NameIndex);
                writer.Write(SubDirectoriesStartIndex);
                writer.Write(SubDirectoriesCount);
                writer.Write(FilesStartIndex);
                writer.Write(FilesCount);
            }
        }

        public class FileEntry : Entry
        {
            internal FileEntry(ReleaseManifest manifest) : base(manifest) { }

            public uint Version { get; set; }
            public byte[] Hash { get; set; }
            public uint Flags { get; set; }
            public uint Size { get; set; }
            public uint CompressedSize { get; set; }
            public uint Unknown1 { get; set; }
            public ushort Type { get; set; }
            public byte Unknown2 { get; set; }
            public byte Unknown3 { get; set; }

            public string HashString
            {
                get
                {
                    return string.Concat(Hash.Select(o => o.ToString("X2")));
                }
            }

            internal void Write(BinaryWriterEx writer)
            {
                writer.Write(NameIndex);
                writer.Write(Version);
                writer.Write(Hash);
                writer.Write(Flags);
                writer.Write(Size);
                writer.Write(CompressedSize);
                writer.Write(Unknown1);
                writer.Write(Type);
                writer.Write(Unknown2);
                writer.Write(Unknown3);
            }
        }

        public class Header
        {
            public uint Type { get; set; }
            public uint EntryCount { get; set; }
            public uint Version { get; set; }

            internal void Write(BinaryWriterEx writer)
            {
                writer.Write("RLSM", BinaryWriterEx.StringMode.Fixed);
                writer.Write(Type);
                writer.Write(EntryCount);
                writer.Write(Version);
            }
        }

        private ReleaseManifest()
        {
            ManifestHeader = new Header();
        }

        public static ReleaseManifest Read(string path)
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

        public static ReleaseManifest Read(Stream stream)
        {
            var manifest = new ReleaseManifest();
            manifest.ReadPrivate(stream);
            return manifest;
        }

        private void ReadPrivate(Stream stream)
        {
            var reader = new BinaryReaderEx(stream);

            //Header
            reader.ReadString(4); //Magic number: RLSM
            ManifestHeader.Type = reader.ReadUInt32(); //Type
            ManifestHeader.EntryCount = reader.ReadUInt32(); //Entries
            ManifestHeader.Version = reader.ReadUInt32(); //Version

            var directoryCount = reader.ReadUInt32(); //Directory Count
            Directories = new DirectoryEntry[directoryCount];
            for (var i = 0; i < directoryCount; i++)
            {
                var de = new DirectoryEntry(this);
                Directories[i] = de;
                //Directory Entry
                de.NameIndex = reader.ReadUInt32(); //Name Index
                de.SubDirectoriesStartIndex = reader.ReadUInt32(); //Subdirectories Start Index
                de.SubDirectoriesCount = reader.ReadUInt32(); //Subdirectories Count
                de.FilesStartIndex = reader.ReadUInt32(); //Files Start Index
                de.FilesCount = reader.ReadUInt32(); //Files Count
            }

            var filesCount = reader.ReadUInt32(); //Files Count
            Files = new FileEntry[filesCount];
            for (var i = 0; i < filesCount; i++)
            {
                var fe = new FileEntry(this);
                Files[i] = fe;
                //File Entry
                fe.NameIndex = reader.ReadUInt32(); //Name Index
                fe.Version = reader.ReadUInt32(); //Version
                fe.Hash = reader.ReadBytes(16); //Hash
                fe.Flags = reader.ReadUInt32(); //Flags
                fe.Size = reader.ReadUInt32(); //Size
                fe.CompressedSize = reader.ReadUInt32(); //Compressed Size
                fe.Unknown1 = reader.ReadUInt32(); //unknown
                fe.Type = reader.ReadUInt16(); //Type
                fe.Unknown2 = reader.ReadByte(); //unknown
                fe.Unknown3 = reader.ReadByte(); //unknown
            }

            var stringCount = reader.ReadUInt32();
            StringSize = reader.ReadUInt32();
            Strings = new string[stringCount];
            for (var i = 0; i < stringCount; i++)
            {
                Strings[i] = reader.ReadString0();
            }
        }

        public void Write(Stream stream)
        {
            var writer = new BinaryWriterEx(stream);
            ManifestHeader.EntryCount = (uint)(Directories.Length + Files.Length);
            ManifestHeader.Write(writer);

            writer.Write((uint)Directories.Length);
            foreach(var e in Directories)
            {
                e.Write(writer);
            }

            writer.Write((uint)Files.Length);
            foreach (var e in Files)
            {
                e.Write(writer);
            }

            RecalculateStringSize();
            writer.Write((uint)Strings.Length);
            writer.Write(StringSize);
            foreach(var s in Strings)
            {
                writer.Write(s, BinaryWriterEx.StringMode.NullTerminated);
            }

            writer.Flush();
        }

        public void RecalculateStringSize()
        {
            StringSize = (uint)(Strings.Aggregate(0, (acc, curr) => curr.Length + acc) + Strings.Length);
        }

        public Header ManifestHeader { get; private set; }
        public DirectoryEntry[] Directories { get; private set; }
        public FileEntry[] Files { get; private set; }
        public uint StringSize { get; private set; }
        public string[] Strings { get; private set; }
    }
}
