using System.Collections.Generic;
using System.IO;
using Google.Protobuf;

namespace SpellEditor
{
    /// <summary>
    /// 一个技能的预览布置。只写在项目存档里。
    /// </summary>
    public sealed class SpellPreviewLayout
    {
        public ulong SpellId;
        public readonly List<SpellPreviewActorRecord> Actors = new List<SpellPreviewActorRecord>();

        public ByteString Write()
        {
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream);
            output.WriteTag(1, WireFormat.WireType.Varint);
            output.WriteUInt64(SpellId);
            for (var i = 0; i < Actors.Count; i++)
            {
                output.WriteTag(2, WireFormat.WireType.LengthDelimited);
                output.WriteBytes(Actors[i].Write());
            }

            output.Flush();
            return ByteString.CopyFrom(stream.ToArray());
        }

        public static SpellPreviewLayout Read(ByteString bytes)
        {
            var layout = new SpellPreviewLayout();
            var input = bytes.CreateCodedInput();
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (WireFormat.GetTagFieldNumber(tag))
                {
                    case 1:
                        layout.SpellId = input.ReadUInt64();
                        break;
                    case 2:
                        layout.Actors.Add(SpellPreviewActorRecord.Read(input.ReadBytes()));
                        break;
                    default:
                        input.SkipLastField();
                        break;
                }
            }

            return layout;
        }
    }

    /// <summary>预览场上的施法者或木桩。</summary>
    public sealed class SpellPreviewActorRecord
    {
        public ulong StableId;
        public ulong UnitId;
        public float X;
        public float Y;
        public float Z;
        public ulong PresetId;
        public readonly List<ulong> Tags = new List<ulong>();
        public readonly List<EditorUnitValue> Values = new List<EditorUnitValue>();

        public ByteString Write()
        {
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream);
            output.WriteTag(1, WireFormat.WireType.Varint);
            output.WriteUInt64(StableId);
            output.WriteTag(2, WireFormat.WireType.Varint);
            output.WriteUInt64(UnitId);
            output.WriteTag(3, WireFormat.WireType.Fixed32);
            output.WriteFloat(X);
            output.WriteTag(4, WireFormat.WireType.Fixed32);
            output.WriteFloat(Y);
            output.WriteTag(5, WireFormat.WireType.Fixed32);
            output.WriteFloat(Z);
            output.WriteTag(6, WireFormat.WireType.Varint);
            output.WriteUInt64(PresetId);
            for (var i = 0; i < Tags.Count; i++)
            {
                output.WriteTag(7, WireFormat.WireType.Varint);
                output.WriteUInt64(Tags[i]);
            }

            for (var i = 0; i < Values.Count; i++)
            {
                output.WriteTag(8, WireFormat.WireType.LengthDelimited);
                output.WriteBytes(Values[i].Write());
            }

            output.Flush();
            return ByteString.CopyFrom(stream.ToArray());
        }

        public static SpellPreviewActorRecord Read(ByteString bytes)
        {
            var actor = new SpellPreviewActorRecord();
            var input = bytes.CreateCodedInput();
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (WireFormat.GetTagFieldNumber(tag))
                {
                    case 1:
                        actor.StableId = input.ReadUInt64();
                        break;
                    case 2:
                        actor.UnitId = input.ReadUInt64();
                        break;
                    case 3:
                        actor.X = input.ReadFloat();
                        break;
                    case 4:
                        actor.Y = input.ReadFloat();
                        break;
                    case 5:
                        actor.Z = input.ReadFloat();
                        break;
                    case 6:
                        actor.PresetId = input.ReadUInt64();
                        break;
                    case 7:
                        actor.Tags.Add(input.ReadUInt64());
                        break;
                    case 8:
                        actor.Values.Add(EditorUnitValue.Read(input.ReadBytes()));
                        break;
                    default:
                        input.SkipLastField();
                        break;
                }
            }

            return actor;
        }
    }
}
