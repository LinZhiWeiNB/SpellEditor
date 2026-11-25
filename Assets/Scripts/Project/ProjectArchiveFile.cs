using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using RebornFrame.Spell;

namespace SpellEditor
{
    /// <summary>
    /// 编辑器项目存档。字段与 Proto/ProjectArchive.proto 一致。
    /// </summary>
    public sealed class ProjectArchiveFile
    {
        public const int CurrentFormat = 1;

        public uint Format = CurrentFormat;
        public string Name = "";
        public uint AuthorId;
        public string RuntimeVersion = "";
        public readonly List<string> LoadedSources = new List<string>();
        public readonly Dictionary<ulong, string> AttributeNames = new Dictionary<ulong, string>();
        public SpellModuleData Module = new SpellModuleData();

        public byte[] Write()
        {
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream);
            output.WriteTag(1, WireFormat.WireType.Varint);
            output.WriteUInt32(Format);
            output.WriteTag(2, WireFormat.WireType.LengthDelimited);
            output.WriteString(Name ?? "");
            output.WriteTag(3, WireFormat.WireType.Varint);
            output.WriteUInt32(AuthorId);
            output.WriteTag(4, WireFormat.WireType.LengthDelimited);
            output.WriteString(RuntimeVersion ?? "");
            for (int i = 0; i < LoadedSources.Count; i++)
            {
                output.WriteTag(5, WireFormat.WireType.LengthDelimited);
                output.WriteString(LoadedSources[i] ?? "");
            }

            foreach (var pair in AttributeNames)
            {
                output.WriteTag(6, WireFormat.WireType.LengthDelimited);
                output.WriteBytes(EncodeName(pair.Key, pair.Value));
            }

            output.WriteTag(7, WireFormat.WireType.LengthDelimited);
            output.WriteBytes(Module == null ? ByteString.Empty : Module.ToByteString());
            output.Flush();
            return stream.ToArray();
        }

        public static ProjectArchiveFile Read(byte[] data)
        {
            var archive = new ProjectArchiveFile();
            var input = new CodedInputStream(data);
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (WireFormat.GetTagFieldNumber(tag))
                {
                    case 1:
                        archive.Format = input.ReadUInt32();
                        break;
                    case 2:
                        archive.Name = input.ReadString();
                        break;
                    case 3:
                        archive.AuthorId = input.ReadUInt32();
                        break;
                    case 4:
                        archive.RuntimeVersion = input.ReadString();
                        break;
                    case 5:
                        archive.LoadedSources.Add(input.ReadString());
                        break;
                    case 6:
                        ReadName(input.ReadBytes(), archive.AttributeNames);
                        break;
                    case 7:
                        archive.Module = SpellModuleData.Parser.ParseFrom(input.ReadBytes());
                        break;
                    default:
                        input.SkipLastField();
                        break;
                }
            }

            if (archive.Module == null)
            {
                archive.Module = new SpellModuleData();
            }

            return archive;
        }

        static ByteString EncodeName(ulong id, string name)
        {
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream);
            output.WriteTag(1, WireFormat.WireType.Varint);
            output.WriteUInt64(id);
            output.WriteTag(2, WireFormat.WireType.LengthDelimited);
            output.WriteString(name ?? "");
            output.Flush();
            return ByteString.CopyFrom(stream.ToArray());
        }

        static void ReadName(ByteString bytes, Dictionary<ulong, string> names)
        {
            var input = bytes.CreateCodedInput();
            ulong id = 0;
            var name = "";
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (WireFormat.GetTagFieldNumber(tag))
                {
                    case 1:
                        id = input.ReadUInt64();
                        break;
                    case 2:
                        name = input.ReadString();
                        break;
                    default:
                        input.SkipLastField();
                        break;
                }
            }

            names[id] = name;
        }
    }
}
