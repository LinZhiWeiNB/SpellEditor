using System.Collections.Generic;
using RebornFrame.Spell;
using GameContentEditor;

namespace UILogic
{
    static class UnitCatalogs
    {
        public static bool SpineEnabled()
        {
            return ResourceLibrary.SpineImportEnabled();
        }

        public static List<ulong> Attributes()
        {
            var ids = new List<ulong>();
            var seen = new HashSet<ulong>();
            AddAttributes(ProjectSession.OwnModule, ids, seen);
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
                AddAttributes(ProjectSession.Sources[i].Module, ids, seen);
            return ids;
        }

        public static void Spells(List<ulong> active, List<ulong> passive)
        {
            var seen = new HashSet<ulong>();
            AddSpells(ProjectSession.OwnModule, active, passive, seen);
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
                AddSpells(ProjectSession.Sources[i].Module, active, passive, seen);
        }

        public static List<string> SpineKeys()
        {
            var keys = new List<string>();
            var seen = new HashSet<string>();
            var assets = ResourceLibrary.List();
            for (var i = 0; i < assets.Count; i++)
            {
                if (!ResourceLibrary.IsSpine(assets[i].Kind) || !seen.Add(assets[i].Key)) continue;
                keys.Add(assets[i].Key);
            }

            return keys;
        }

        public static string AttributeLabel(ulong id)
        {
            var parsed = new AttributeId(id);
            var name = ProjectSession.AttributeName(id, parsed.LocalId);
            return name + "  " + parsed;
        }

        public static string SpellLabel(ulong id)
        {
            var spell = FindSpell(id);
            var text = new SpellId(id).ToString();
            if (spell == null) return text;
            return text + "  " + (spell.Activation == (int)SpellActivation.Passive ? "被动" : "主动");
        }

        public static uint NextLocalId()
        {
            return UnitLibrary.NextLocalId();
        }

        public static EditorUnit Find(ulong id)
        {
            var units = ProjectSession.Archive == null ? null : ProjectSession.Archive.Units;
            if (units == null) return null;
            for (var i = 0; i < units.Count; i++)
            {
                if (units[i].Id == id) return units[i];
            }

            return null;
        }

        public static long Raw(EditorUnit unit, ulong attributeId)
        {
            for (var i = 0; i < unit.Values.Count; i++)
            {
                if (unit.Values[i].AttributeId == attributeId) return unit.Values[i].Raw;
            }

            return 0;
        }

        public static void SetRaw(EditorUnit unit, ulong attributeId, long raw)
        {
            for (var i = 0; i < unit.Values.Count; i++)
            {
                if (unit.Values[i].AttributeId != attributeId) continue;
                unit.Values[i].Raw = raw;
                return;
            }

            unit.Values.Add(new EditorUnitValue { AttributeId = attributeId, Raw = raw });
        }

        static SpellData FindSpell(ulong id)
        {
            var own = FindSpell(ProjectSession.OwnModule, id);
            if (own != null) return own;
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                var spell = FindSpell(ProjectSession.Sources[i].Module, id);
                if (spell != null) return spell;
            }

            return null;
        }

        static SpellData FindSpell(SpellModuleData module, ulong id)
        {
            if (module == null) return null;
            for (var i = 0; i < module.Spells.Count; i++)
            {
                if (module.Spells[i].Id == id) return module.Spells[i];
            }

            return null;
        }

        static void AddAttributes(SpellModuleData module, List<ulong> ids, HashSet<ulong> seen)
        {
            if (module == null) return;
            for (var i = 0; i < module.Attributes.Count; i++)
            {
                var id = module.Attributes[i].Id;
                if (seen.Add(id)) ids.Add(id);
            }
        }

        static void AddSpells(SpellModuleData module, List<ulong> active, List<ulong> passive, HashSet<ulong> seen)
        {
            if (module == null) return;
            for (var i = 0; i < module.Spells.Count; i++)
            {
                var spell = module.Spells[i];
                if (spell == null || !seen.Add(spell.Id)) continue;
                if (spell.Activation == (int)SpellActivation.Passive) passive.Add(spell.Id);
                else if (spell.Activation == (int)SpellActivation.Active) active.Add(spell.Id);
            }
        }
    }
}
