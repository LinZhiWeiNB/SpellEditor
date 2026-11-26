using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RebornFrame.Spell;

namespace SpellEditor
{
    /// <summary>
    /// 本项目资源引用。写在项目目录的 resource-references.txt，不进导出的数据包。
    /// 一条引用有稳定的 owner，改键时用同一个 owner 覆盖。
    /// </summary>
    public static class ResourceReferences
    {
        public const string FileName = "resource-references.txt";

        struct Row
        {
            public string Key;
            public string Owner;
            public string Label;
        }

        static readonly List<Row> Rows = new List<Row>();
        static string _loadedPath = "";

        public static string UnitSpine(ulong id)
        {
            return "unit/" + id + "/spine";
        }

        public static string SpellTrack(ulong spellId, int index)
        {
            return "spell/" + spellId + "/track/" + index;
        }

        public static void BindUnitSpine(ulong id, string key)
        {
            var owner = UnitSpine(id);
            if (string.IsNullOrEmpty(key))
            {
                Unbind(owner);
                return;
            }

            Bind(owner, key, "单位 " + new SpellId(id));
        }

        public static void Bind(string owner, string key, string label)
        {
            if (string.IsNullOrEmpty(owner))
            {
                return;
            }

            Ensure();
            RemoveOwner(owner);
            if (!string.IsNullOrEmpty(key))
            {
                Rows.Add(new Row { Key = key, Owner = owner, Label = label ?? "" });
            }

            Write();
        }

        public static void Unbind(string owner)
        {
            if (string.IsNullOrEmpty(owner))
            {
                return;
            }

            Ensure();
            if (!RemoveOwner(owner))
            {
                return;
            }

            Write();
        }

        public static void Retarget(string oldKey, string newKey)
        {
            if (string.IsNullOrEmpty(oldKey) || oldKey == newKey)
            {
                return;
            }

            Ensure();
            var changed = false;
            for (var i = 0; i < Rows.Count; i++)
            {
                if (Rows[i].Key != oldKey)
                {
                    continue;
                }

                var row = Rows[i];
                row.Key = newKey ?? "";
                Rows[i] = row;
                changed = true;
            }

            if (changed)
            {
                Write();
            }
        }

        public static List<string> Labels(string key)
        {
            var lines = new List<string>();
            if (string.IsNullOrEmpty(key) || !ProjectSession.IsOpen)
            {
                return lines;
            }

            Ensure();
            var seen = new HashSet<string>();
            for (var i = 0; i < Rows.Count; i++)
            {
                if (Rows[i].Key != key || !seen.Add(Rows[i].Label))
                {
                    continue;
                }

                lines.Add(Rows[i].Label);
            }

            return lines;
        }

        public static bool MoveFile(string oldDirectory, string newDirectory, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(oldDirectory) || string.IsNullOrEmpty(newDirectory))
            {
                return true;
            }

            var from = Path.Combine(oldDirectory, FileName);
            var to = Path.Combine(newDirectory, FileName);
            if (!File.Exists(from))
            {
                _loadedPath = "";
                return true;
            }

            if (File.Exists(to))
            {
                error = "新目录里已经有 " + FileName + "，原来的引用记录还留在旧目录。";
                return false;
            }

            try
            {
                File.Move(from, to);
            }
            catch (Exception)
            {
                error = "新存档已经写好，引用记录还留在原来的位置。";
                return false;
            }

            _loadedPath = to;
            return true;
        }

        public static void Forget()
        {
            Rows.Clear();
            _loadedPath = "";
        }

        static void Ensure()
        {
            if (!ProjectSession.IsOpen)
            {
                return;
            }

            var path = FilePath();
            if (path == _loadedPath)
            {
                return;
            }

            Rows.Clear();
            _loadedPath = path;
            if (File.Exists(path))
            {
                Read(path);
                return;
            }

            Seed();
            Write();
        }

        static void Seed()
        {
            var units = ProjectSession.Archive == null ? null : ProjectSession.Archive.Units;
            if (units != null)
            {
                for (var i = 0; i < units.Count; i++)
                {
                    var unit = units[i];
                    if (unit == null || string.IsNullOrEmpty(unit.SpineKey))
                    {
                        continue;
                    }

                    Rows.Add(new Row
                    {
                        Key = unit.SpineKey,
                        Owner = UnitSpine(unit.Id),
                        Label = "单位 " + new SpellId(unit.Id),
                    });
                }
            }

            var diffs = UnitDiffs.Load();
            for (var i = 0; i < diffs.Count; i++)
            {
                var unit = diffs[i];
                if (unit == null || string.IsNullOrEmpty(unit.SpineKey))
                {
                    continue;
                }

                Rows.Add(new Row
                {
                    Key = unit.SpineKey,
                    Owner = UnitSpine(unit.Id),
                    Label = "单位 " + new SpellId(unit.Id),
                });
            }

            ResourceLibrary.CollectSpellReferences(RowsAdd);
        }

        static void RowsAdd(string owner, string key, string label)
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(key))
            {
                return;
            }

            Rows.Add(new Row { Key = key, Owner = owner, Label = label ?? "" });
        }

        static bool RemoveOwner(string owner)
        {
            var removed = false;
            for (var i = Rows.Count - 1; i >= 0; i--)
            {
                if (Rows[i].Owner != owner)
                {
                    continue;
                }

                Rows.RemoveAt(i);
                removed = true;
            }

            return removed;
        }

        static void Read(string path)
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(path);
            }
            catch (Exception)
            {
                return;
            }

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrEmpty(line) || line[0] == '#')
                {
                    continue;
                }

                var parts = line.Split('\t');
                if (parts.Length < 3 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
                {
                    continue;
                }

                Rows.Add(new Row { Key = parts[0], Owner = parts[1], Label = parts[2] });
            }
        }

        static void Write()
        {
            if (!ProjectSession.IsOpen)
            {
                return;
            }

            var path = FilePath();
            var text = new StringBuilder();
            text.Append("# resource references v1\n");
            for (var i = 0; i < Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(Rows[i].Key) || string.IsNullOrEmpty(Rows[i].Owner))
                {
                    continue;
                }

                text.Append(Rows[i].Key);
                text.Append('\t');
                text.Append(Rows[i].Owner);
                text.Append('\t');
                text.Append(Rows[i].Label ?? "");
                text.Append('\n');
            }

            try
            {
                File.WriteAllText(path, text.ToString(), new UTF8Encoding(false));
                _loadedPath = path;
            }
            catch (Exception)
            {
            }
        }

        static string FilePath()
        {
            return Path.Combine(Path.GetDirectoryName(ProjectSession.ArchivePath) ?? "", FileName);
        }
    }
}
