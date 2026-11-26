using System.Collections.Generic;
using RebornFrame.Spell;

namespace SpellEditor
{
    /// <summary>
    /// 单位列表里的一条。本项目新建的单位在存档里。其他数据源的单位改完写进本地差异。
    /// </summary>
    public sealed class UnitRecord
    {
        public EditorUnit Unit;
        public bool Owned;
        public bool Diff;
        public string Source = "";
    }

    public static class UnitLibrary
    {
        public static List<UnitRecord> List()
        {
            var result = new List<UnitRecord>();
            var index = new Dictionary<ulong, int>();
            var archive = ProjectSession.Archive;
            if (archive != null)
            {
                for (var i = 0; i < archive.Units.Count; i++)
                {
                    var unit = archive.Units[i];
                    if (unit == null || index.ContainsKey(unit.Id))
                    {
                        continue;
                    }

                    index[unit.Id] = result.Count;
                    result.Add(new UnitRecord
                    {
                        Unit = unit,
                        Owned = true,
                        Source = "本项目",
                    });
                }
            }

            var diffs = UnitDiffs.Load();
            var diffById = new Dictionary<ulong, EditorUnit>();
            for (var i = 0; i < diffs.Count; i++)
            {
                diffById[diffs[i].Id] = diffs[i];
            }

            for (var s = 0; s < ProjectSession.Sources.Count; s++)
            {
                var source = ProjectSession.Sources[s];
                var units = new List<EditorUnit>();
                UnitDiffs.ReadPackage(source.AbsolutePath, units);
                var title = string.IsNullOrEmpty(source.Title) ? "其他数据源" : source.Title;
                for (var i = 0; i < units.Count; i++)
                {
                    var id = units[i].Id;
                    var hasDiff = diffById.TryGetValue(id, out var diff);
                    var record = new UnitRecord
                    {
                        Unit = hasDiff ? diff.Clone(id) : units[i].Clone(id),
                        Owned = false,
                        Diff = hasDiff,
                        Source = title,
                    };
                    if (index.TryGetValue(id, out var at))
                    {
                        if (result[at].Owned)
                        {
                            continue;
                        }

                        result[at] = record;
                    }
                    else
                    {
                        index[id] = result.Count;
                        result.Add(record);
                    }
                }
            }

            for (var i = 0; i < diffs.Count; i++)
            {
                if (index.ContainsKey(diffs[i].Id))
                {
                    continue;
                }

                index[diffs[i].Id] = result.Count;
                result.Add(new UnitRecord
                {
                    Unit = diffs[i].Clone(diffs[i].Id),
                    Owned = false,
                    Diff = true,
                    Source = "数据源里已经没有这个单位",
                });
            }

            return result;
        }

        public static UnitRecord Find(ulong id)
        {
            var records = List();
            for (var i = 0; i < records.Count; i++)
            {
                if (records[i].Unit.Id == id)
                {
                    return records[i];
                }
            }

            return null;
        }

        public static UnitRecord Create()
        {
            if (!ProjectSession.IsOpen || ProjectSession.Archive == null)
            {
                return null;
            }

            var unit = new EditorUnit
            {
                Id = SpellId.Combine(ProjectSession.AuthorId, NextLocalId()).Value,
            };
            ProjectSession.Archive.Units.Add(unit);
            ProjectSession.Save();
            return new UnitRecord
            {
                Unit = unit,
                Owned = true,
                Source = "本项目",
            };
        }

        public static UnitRecord Copy(UnitRecord record)
        {
            if (record == null || record.Unit == null || !ProjectSession.IsOpen || ProjectSession.Archive == null)
            {
                return null;
            }

            var copy = record.Unit.Clone(SpellId.Combine(ProjectSession.AuthorId, NextLocalId()).Value);
            ProjectSession.Archive.Units.Add(copy);
            ResourceReferences.BindUnitSpine(copy.Id, copy.SpineKey);
            ProjectSession.Save();
            return new UnitRecord
            {
                Unit = copy,
                Owned = true,
                Source = "本项目",
            };
        }

        public static void Commit(UnitRecord record)
        {
            if (record == null || record.Unit == null)
            {
                return;
            }

            if (record.Owned)
            {
                ProjectSession.Save();
                return;
            }

            record.Diff = true;
            UnitDiffs.Upsert(record.Unit);
        }

        public static bool Remove(UnitRecord record)
        {
            if (record == null || record.Unit == null)
            {
                return false;
            }

            if (record.Owned)
            {
                if (ProjectSession.Archive == null)
                {
                    return false;
                }

                ResourceReferences.Unbind(ResourceReferences.UnitSpine(record.Unit.Id));
                ProjectSession.Archive.Units.Remove(record.Unit);
                ProjectSession.Save();
                UnitDiffs.Remove(record.Unit.Id);
                return true;
            }

            if (!record.Diff)
            {
                return false;
            }

            ResourceReferences.Unbind(ResourceReferences.UnitSpine(record.Unit.Id));
            UnitDiffs.Remove(record.Unit.Id);
            return true;
        }

        public static uint NextLocalId()
        {
            var author = ProjectSession.AuthorId;
            var floor = author == 0 ? LibrarySpell.ReservedLocalIdCount : 1u;
            var highest = floor - 1;
            var units = ProjectSession.Archive == null ? null : ProjectSession.Archive.Units;
            if (units != null)
            {
                for (var i = 0; i < units.Count; i++)
                {
                    var id = new SpellId(units[i].Id);
                    if (id.AuthorId == author && id.LocalId > highest)
                    {
                        highest = id.LocalId;
                    }
                }
            }

            var next = highest + 1;
            return next < floor ? floor : next;
        }
    }
}
