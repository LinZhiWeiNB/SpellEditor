using System;
using System.Collections.Generic;
using RebornFrame.Spell;
using SpellEditor;
using Unity.Entities;
using UnityEngine;

namespace UILogic
{
    /// <summary>
    /// 用一份临时世界把技能逻辑跑完，记下每一帧的单位状态，然后丢掉这个世界。
    /// </summary>
    public static class SpellPreviewSim
    {
        public sealed class Seed
        {
            public ulong StableId;
            public Vector3 Position;
            public readonly List<ulong> Tags = new List<ulong>();
            public readonly List<EditorUnitValue> Values = new List<EditorUnitValue>();
        }

        public sealed class Frame
        {
            public readonly List<Unit> Units = new List<Unit>();
        }

        public sealed class Unit
        {
            public ulong StableId;
            public Vector3 Position;
            public readonly List<ulong> Tags = new List<ulong>();
            public readonly List<EditorUnitValue> Attributes = new List<EditorUnitValue>();
            public readonly List<ulong> Buffs = new List<ulong>();
        }

        public static List<Frame> Run(ulong spellId, int frames, IReadOnlyList<Seed> seeds)
        {
            var samples = new List<Frame>();
            if (spellId == 0 || frames < 1 || seeds == null || seeds.Count == 0)
                return samples;

            var own = ProjectSession.OwnModule;
            if (own == null)
                return samples;

            var world = new World("SpellPreview");
            var startup = new SpellStartup();
            try
            {
                for (var i = 0; i < ProjectSession.Sources.Count; i++)
                {
                    var module = ProjectSession.Sources[i].Module;
                    if (module != null)
                        startup.AddModuleData(WithoutPresentation(module));
                }

                startup.AddModuleData(WithoutPresentation(own));
                startup.Start(world);

                var manager = world.EntityManager;
                var entities = new List<Entity>(seeds.Count);
                var owner = 0UL;
                for (var i = 0; i < seeds.Count; i++)
                {
                    var seed = seeds[i];
                    if (seed.StableId == 0)
                        continue;
                    var entity = manager.CreateEntity();
                    manager.AddComponentData(entity, new PreviewPose
                    {
                        X = seed.Position.x,
                        Y = seed.Position.y,
                        Z = seed.Position.z,
                    });
                    startup.InitializeUnit(entity, seed.StableId, Overrides(seed));
                    for (var t = 0; t < seed.Tags.Count; t++)
                        BuffAccess.TryAddIntrinsicTag(manager, entity, new TagId(seed.Tags[t]));
                    entities.Add(entity);
                    if (owner == 0)
                        owner = seed.StableId;
                }

                if (owner != 0)
                {
                    var spell = new SpellId(spellId);
                    var passive = false;
                    var data = ProjectSession.EnsureOwnSpell(spellId);
                    if (data != null && data.Activation == (int)SpellActivation.Passive)
                        passive = true;
                    if (passive)
                        SpellAccess.TryGrant(manager, spell, owner);
                    else
                        SpellAccess.TryBeginCast(manager, spell, owner);
                }

                var tags = new List<ulong>();
                var attributes = new List<AttributeSnapshot>();
                var buffs = new List<SpellBuffSample>();
                for (var frame = 0; frame < frames; frame++)
                {
                    startup.Tick(frame);
                    var sample = new Frame();
                    for (var i = 0; i < entities.Count; i++)
                    {
                        var entity = entities[i];
                        if (!manager.Exists(entity))
                            continue;
                        var pose = manager.GetComponentData<PreviewPose>(entity);
                        var unit = new Unit
                        {
                            StableId = manager.GetComponentData<SpellStableId>(entity).Value,
                            Position = new Vector3(pose.X, pose.Y, pose.Z),
                        };
                        SpellUnitSnapshot.CopyTags(manager, entity, tags);
                        SpellUnitSnapshot.CopyAttributes(manager, entity, attributes);
                        SpellUnitSnapshot.CopyBuffs(manager, entity, buffs);
                        unit.Tags.AddRange(tags);
                        for (var a = 0; a < attributes.Count; a++)
                        {
                            unit.Attributes.Add(new EditorUnitValue
                            {
                                AttributeId = attributes[a].Id.Value,
                                Raw = attributes[a].CurrentBits,
                            });
                        }

                        for (var b = 0; b < buffs.Count; b++)
                            unit.Buffs.Add(buffs[b].Id);
                        sample.Units.Add(unit);
                    }

                    samples.Add(sample);
                }
            }
            catch (Exception error)
            {
                Debug.LogWarning(error.Message);
                samples.Clear();
            }
            finally
            {
                startup.Dispose();
                if (world.IsCreated)
                    world.Dispose();
            }

            return samples;
        }

        static SpellModuleData WithoutPresentation(SpellModuleData source)
        {
            var copy = source.Clone();
            for (var i = 0; i < copy.Spells.Count; i++)
                copy.Spells[i].PresentationTracks.Clear();
            return copy;
        }

        static List<UnitResourceCurrentOverride> Overrides(Seed seed)
        {
            var list = new List<UnitResourceCurrentOverride>();
            for (var i = 0; i < seed.Values.Count; i++)
            {
                var value = seed.Values[i];
                if (!TryKind(value.AttributeId, out var kind))
                    continue;
                list.Add(new UnitResourceCurrentOverride
                {
                    Id = new AttributeId(value.AttributeId),
                    CurrentValue = new AttributeValue(kind, value.Raw),
                });
            }

            return list;
        }

        static bool TryKind(ulong id, out AttributeValueKind kind)
        {
            kind = AttributeValueKind.Int32;
            var found = false;
            for (var s = 0; s < ProjectSession.Sources.Count; s++)
                found |= TakeKind(ProjectSession.Sources[s].Module, id, ref kind);
            found |= TakeKind(ProjectSession.OwnModule, id, ref kind);
            return found && InLayout(id);
        }

        static bool TakeKind(SpellModuleData module, ulong id, ref AttributeValueKind kind)
        {
            if (module == null)
                return false;
            var found = false;
            for (var i = 0; i < module.Attributes.Count; i++)
            {
                var attribute = module.Attributes[i];
                if (attribute.Id != id)
                    continue;
                kind = (AttributeValueKind)attribute.ValueKind;
                found = true;
            }

            return found;
        }

        static bool InLayout(ulong id)
        {
            for (var s = 0; s < ProjectSession.Sources.Count; s++)
            {
                if (HasLayout(ProjectSession.Sources[s].Module, id))
                    return true;
            }

            return HasLayout(ProjectSession.OwnModule, id);
        }

        static bool HasLayout(SpellModuleData module, ulong id)
        {
            if (module == null)
                return false;
            for (var i = 0; i < module.AttributeLayout.Count; i++)
            {
                if (module.AttributeLayout[i].AttributeId == id)
                    return true;
            }

            return false;
        }

        struct PreviewPose : IComponentData
        {
            public float X;
            public float Y;
            public float Z;
        }
    }
}
