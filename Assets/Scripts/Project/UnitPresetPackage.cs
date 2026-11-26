using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using RebornFrame.GameData;

namespace SpellEditor
{
    /// <summary>
    /// 单位预设写在数据包的 unit-presets 条目里。其他项目载入这份数据包后，从这里读出预设。
    /// </summary>
    public static class UnitPresetPackage
    {
        public const string Key = "unit-presets";
        public const string Kind = "spell.unit-presets";

        public static void Read(string path, List<UnitPreset> into)
        {
            if (string.IsNullOrEmpty(path) || into == null)
            {
                return;
            }

            if (!GameDataFile.TryReadEntry(path, Key, out var data, out _))
            {
                return;
            }

            ReadList(data, into);
        }

        public static byte[] Encode(List<UnitPreset> presets)
        {
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream);
            if (presets != null)
            {
                for (var i = 0; i < presets.Count; i++)
                {
                    if (presets[i] == null)
                    {
                        continue;
                    }

                    output.WriteTag(1, WireFormat.WireType.LengthDelimited);
                    output.WriteBytes(presets[i].Write());
                }
            }

            output.Flush();
            return stream.ToArray();
        }

        static void ReadList(byte[] data, List<UnitPreset> into)
        {
            if (data == null || data.Length == 0)
            {
                return;
            }

            var input = new CodedInputStream(data);
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                if (WireFormat.GetTagFieldNumber(tag) == 1)
                {
                    into.Add(UnitPreset.Read(input.ReadBytes()));
                }
                else
                {
                    input.SkipLastField();
                }
            }
        }
    }
}
