using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using RebornFrame.GameData;

namespace SpellEditor
{
    /// <summary>
    /// 对其他数据源单位的本地覆盖。文件在项目存档旁边，名字是「存档名.unit-diffs.byte」。
    /// 游戏按玩家设置的模组载入顺序决定哪一份生效，所以导出先留空。
    /// </summary>
    public static class UnitDiffs
    {
        public const string PackageKey = "units";

        public static string FilePath()
        {
            var archive = ProjectSession.ArchivePath;
            if (string.IsNullOrEmpty(archive))
            {
                return null;
            }

            var directory = Path.GetDirectoryName(archive);
            var stem = Path.GetFileNameWithoutExtension(archive);
            return Path.Combine(directory, stem + ".unit-diffs.byte");
        }

        public static List<EditorUnit> Load()
        {
            var list = new List<EditorUnit>();
            var path = FilePath();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return list;
            }

            try
            {
                ReadList(File.ReadAllBytes(path), list);
            }
            catch (Exception)
            {
                list.Clear();
            }

            return list;
        }

        public static void Upsert(EditorUnit unit)
        {
            if (unit == null)
            {
                return;
            }

            var list = Load();
            var found = false;
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].Id != unit.Id)
                {
                    continue;
                }

                list[i] = unit.Clone(unit.Id);
                found = true;
                break;
            }

            if (!found)
            {
                list.Add(unit.Clone(unit.Id));
            }

            Write(list);
        }

        public static void Remove(ulong id)
        {
            var list = Load();
            var removed = false;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].Id != id)
                {
                    continue;
                }

                list.RemoveAt(i);
                removed = true;
            }

            if (removed)
            {
                Write(list);
            }
        }

        public static void Retarget(string oldKey, string newKey)
        {
            if (string.IsNullOrEmpty(oldKey) || oldKey == newKey)
            {
                return;
            }

            var list = Load();
            var changed = false;
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].SpineKey != oldKey)
                {
                    continue;
                }

                list[i].SpineKey = newKey ?? "";
                changed = true;
            }

            if (changed)
            {
                Write(list);
            }
        }

        public static void ReadPackage(string path, List<EditorUnit> into)
        {
            if (string.IsNullOrEmpty(path) || into == null)
            {
                return;
            }

            if (!GameDataFile.TryReadEntry(path, PackageKey, out var data, out _))
            {
                return;
            }

            ReadList(data, into);
        }

        /// <summary>
        /// 把本地单位差异写进数据包。玩家的模组载入顺序决定游戏里哪一份生效，这里先不写。
        /// </summary>
        public static void Export()
        {
        }

        static void Write(List<EditorUnit> units)
        {
            var path = FilePath();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (units == null || units.Count == 0)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                return;
            }

            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream);
            for (var i = 0; i < units.Count; i++)
            {
                output.WriteTag(1, WireFormat.WireType.LengthDelimited);
                output.WriteBytes(units[i].Write());
            }

            output.Flush();
            File.WriteAllBytes(path, stream.ToArray());
        }

        static void ReadList(byte[] data, List<EditorUnit> into)
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
                    into.Add(EditorUnit.Read(input.ReadBytes()));
                }
                else
                {
                    input.SkipLastField();
                }
            }
        }
    }
}
