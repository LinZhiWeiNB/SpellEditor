using System.Collections.Generic;
using System.IO;
using Google.Protobuf;

namespace GameContentEditor
{
    /// <summary>
    /// 编辑器存档里的一条单位预设。字段与 ProjectArchive.proto 的 UnitPreset 一致。
    /// </summary>
    public sealed class UnitPreset
    {
        public ulong Id;
        public string Name = "";
        public readonly List<ulong> Attributes = new List<ulong>();

        public ByteString Write()
        {
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream);
            output.WriteTag(1, WireFormat.WireType.Varint);
            output.WriteUInt64(Id);
            output.WriteTag(2, WireFormat.WireType.LengthDelimited);
            output.WriteString(Name ?? "");
            for (var i = 0; i < Attributes.Count; i++)
            {
                output.WriteTag(3, WireFormat.WireType.Varint);
                output.WriteUInt64(Attributes[i]);
            }

            output.Flush();
            return ByteString.CopyFrom(stream.ToArray());
        }

        public static UnitPreset Read(ByteString bytes)
        {
            var preset = new UnitPreset();
            var input = bytes.CreateCodedInput();
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (WireFormat.GetTagFieldNumber(tag))
                {
                    case 1:
                        preset.Id = input.ReadUInt64();
                        break;
                    case 2:
                        preset.Name = input.ReadString();
                        break;
                    case 3:
                        preset.Attributes.Add(input.ReadUInt64());
                        break;
                    default:
                        input.SkipLastField();
                        break;
                }
            }

            return preset;
        }
    }
}
