using System.Collections.Generic;
using System.IO;
using Google.Protobuf;

namespace GameContentEditor
{
    /// <summary>
    /// 编辑器存档里的一条单位。字段与 ProjectArchive.proto 的 EditorUnit 一致。
    /// </summary>
    public sealed class EditorUnit
    {
        public ulong Id;
        public readonly List<ulong> PresetAttributes = new List<ulong>();
        public readonly List<EditorUnitValue> Values = new List<EditorUnitValue>();
        public readonly List<ulong> ActiveSpells = new List<ulong>();
        public readonly List<ulong> PassiveSpells = new List<ulong>();
        public string SpineKey = "";

        public EditorUnit Clone(ulong id)
        {
            var copy = new EditorUnit { Id = id, SpineKey = SpineKey ?? "" };
            copy.PresetAttributes.AddRange(PresetAttributes);
            copy.ActiveSpells.AddRange(ActiveSpells);
            copy.PassiveSpells.AddRange(PassiveSpells);
            for (var i = 0; i < Values.Count; i++)
            {
                copy.Values.Add(new EditorUnitValue
                {
                    AttributeId = Values[i].AttributeId,
                    Raw = Values[i].Raw,
                });
            }

            return copy;
        }

        public ByteString Write()
        {
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream);
            output.WriteTag(1, WireFormat.WireType.Varint);
            output.WriteUInt64(Id);
            for (var i = 0; i < PresetAttributes.Count; i++)
            {
                output.WriteTag(2, WireFormat.WireType.Varint);
                output.WriteUInt64(PresetAttributes[i]);
            }

            for (var i = 0; i < Values.Count; i++)
            {
                output.WriteTag(3, WireFormat.WireType.LengthDelimited);
                output.WriteBytes(Values[i].Write());
            }

            for (var i = 0; i < ActiveSpells.Count; i++)
            {
                output.WriteTag(4, WireFormat.WireType.Varint);
                output.WriteUInt64(ActiveSpells[i]);
            }

            for (var i = 0; i < PassiveSpells.Count; i++)
            {
                output.WriteTag(5, WireFormat.WireType.Varint);
                output.WriteUInt64(PassiveSpells[i]);
            }

            output.WriteTag(6, WireFormat.WireType.LengthDelimited);
            output.WriteString(SpineKey ?? "");
            output.Flush();
            return ByteString.CopyFrom(stream.ToArray());
        }

        public static EditorUnit Read(ByteString bytes)
        {
            var unit = new EditorUnit();
            var input = bytes.CreateCodedInput();
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (WireFormat.GetTagFieldNumber(tag))
                {
                    case 1:
                        unit.Id = input.ReadUInt64();
                        break;
                    case 2:
                        unit.PresetAttributes.Add(input.ReadUInt64());
                        break;
                    case 3:
                        unit.Values.Add(EditorUnitValue.Read(input.ReadBytes()));
                        break;
                    case 4:
                        unit.ActiveSpells.Add(input.ReadUInt64());
                        break;
                    case 5:
                        unit.PassiveSpells.Add(input.ReadUInt64());
                        break;
                    case 6:
                        unit.SpineKey = input.ReadString();
                        break;
                    default:
                        input.SkipLastField();
                        break;
                }
            }

            return unit;
        }
    }

    public sealed class EditorUnitValue
    {
        public ulong AttributeId;
        public long Raw;

        public ByteString Write()
        {
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream);
            output.WriteTag(1, WireFormat.WireType.Varint);
            output.WriteUInt64(AttributeId);
            output.WriteTag(2, WireFormat.WireType.Varint);
            output.WriteInt64(Raw);
            output.Flush();
            return ByteString.CopyFrom(stream.ToArray());
        }

        public static EditorUnitValue Read(ByteString bytes)
        {
            var value = new EditorUnitValue();
            var input = bytes.CreateCodedInput();
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (WireFormat.GetTagFieldNumber(tag))
                {
                    case 1:
                        value.AttributeId = input.ReadUInt64();
                        break;
                    case 2:
                        value.Raw = input.ReadInt64();
                        break;
                    default:
                        input.SkipLastField();
                        break;
                }
            }

            return value;
        }
    }
}
