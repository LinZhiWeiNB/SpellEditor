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
        public string ExportLocation = "";
        public string ExportModDirectory = "";
        public string ExportName = "";
        public string ExportDescription = "";
        public string ExportDirectoryName = "";
        public uint RenderPlugins;
        public readonly List<EditorUnit> Units = new List<EditorUnit>();
        public readonly Dictionary<ulong, string> TagNames = new Dictionary<ulong, string>();
        public readonly List<UnitPreset> UnitPresets = new List<UnitPreset>();

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
            WriteText(output, 8, ExportLocation);
            WriteText(output, 9, ExportModDirectory);
            WriteText(output, 10, ExportName);
            WriteText(output, 11, ExportDescription);
            WriteText(output, 12, ExportDirectoryName);
            output.WriteTag(13, WireFormat.WireType.Varint);
            output.WriteUInt32(RenderPlugins);
            for (int i = 0; i < Units.Count; i++)
            {
                output.WriteTag(14, WireFormat.WireType.LengthDelimited);
                output.WriteBytes(Units[i].Write());
            }

            foreach (var pair in TagNames)
            {
                output.WriteTag(15, WireFormat.WireType.LengthDelimited);
                output.WriteBytes(EncodeName(pair.Key, pair.Value));
            }

            for (int i = 0; i < UnitPresets.Count; i++)
            {
                output.WriteTag(16, WireFormat.WireType.LengthDelimited);
                output.WriteBytes(UnitPresets[i].Write());
            }

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
                    case 8:
                        archive.ExportLocation = input.ReadString();
                        break;
                    case 9:
                        archive.ExportModDirectory = input.ReadString();
                        break;
                    case 10:
                        archive.ExportName = input.ReadString();
                        break;
                    case 11:
                        archive.ExportDescription = input.ReadString();
                        break;
                    case 12:
                        archive.ExportDirectoryName = input.ReadString();
                        break;
                    case 13:
                        archive.RenderPlugins = input.ReadUInt32();
                        break;
                    case 14:
                        archive.Units.Add(EditorUnit.Read(input.ReadBytes()));
                        break;
                    case 15:
                        ReadName(input.ReadBytes(), archive.TagNames);
                        break;
                    case 16:
                        archive.UnitPresets.Add(UnitPreset.Read(input.ReadBytes()));
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

        static void WriteText(CodedOutputStream output, int field, string value)
        {
            output.WriteTag(field, WireFormat.WireType.LengthDelimited);
            output.WriteString(value ?? "");
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
