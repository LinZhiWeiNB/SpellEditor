using System.Collections.Generic;
using FairyGUI;
using GameContentEditor;
using RebornFrame.Spell;
using UnityEngine;

namespace UILogic
{
    /// <summary>
    /// 把单位头顶的空物体换算到屏幕，挂上信息条。标签在上，资源属性在下。
    /// </summary>
    public static class PreviewUnitInfo
    {
        const string Package = "PackageSpell";
        const string Component = "ComUnitInfo";

        static readonly Dictionary<PreviewActor, GComponent> Views = new Dictionary<PreviewActor, GComponent>();
        static readonly List<PreviewActor> Drop = new List<PreviewActor>();
        static readonly List<ulong> Ids = new List<ulong>();
        static readonly List<long> Currents = new List<long>();
        static readonly List<long> Maxes = new List<long>();
        static readonly List<string> Leaves = new List<string>();
        static bool _missing;

        public static void Clear()
        {
            foreach (var pair in Views)
            {
                if (pair.Value != null)
                    pair.Value.Dispose();
            }

            Views.Clear();
        }

        public static void Follow(PreviewActor[] actors, GObject hit, Camera camera, bool editing)
        {
            var parent = hit == null ? null : hit.parent as GComponent;
            if (parent == null || camera == null)
            {
                Clear();
                return;
            }

            Drop.Clear();
            foreach (var pair in Views)
                Drop.Add(pair.Key);
            if (actors != null)
            {
                for (var i = 0; i < actors.Length; i++)
                {
                    var actor = actors[i];
                    if (actor == null || !actor.gameObject.activeInHierarchy)
                        continue;
                    Drop.Remove(actor);
                    var view = ViewOf(parent, actor);
                    if (view == null)
                        continue;
                    Fill(view, actor, editing);
                    Place(view, actor, parent, camera);
                }
            }

            for (var i = 0; i < Drop.Count; i++)
            {
                if (Views.TryGetValue(Drop[i], out var view) && view != null)
                    view.Dispose();
                Views.Remove(Drop[i]);
            }
        }

        static GComponent ViewOf(GComponent parent, PreviewActor actor)
        {
            if (Views.TryGetValue(actor, out var view) && view != null)
                return view;
            var created = UIPackage.CreateObject(Package, Component) as GComponent;
            if (created == null)
            {
                if (!_missing)
                {
                    _missing = true;
                    Debug.LogWarning("PackageSpell 里还没有 ComUnitInfo，先在 FairyGUI 发布技能包。");
                }

                return null;
            }

            created.touchable = false;
            var tags = created.GetChild("tags") as GList;
            if (tags != null)
            {
                tags.touchable = false;
                tags.layout = ListLayoutType.FlowHorizontal;
                tags.autoResizeItem = false;
            }

            var attrs = created.GetChild("attrs") as GList;
            if (attrs != null)
                attrs.touchable = false;
            parent.AddChild(created);
            Views[actor] = created;
            return created;
        }

        static void Fill(GComponent view, PreviewActor actor, bool editing)
        {
            CollectTags(actor, Leaves);
            CollectResources(actor, editing, Ids, Currents, Maxes);
            var tags = view.GetChild("tags") as GList;
            var attrs = view.GetChild("attrs") as GList;
            const float pad = 8f;
            var inner = Mathf.Max(40f, view.width - pad * 2f);
            if (tags != null)
            {
                tags.SetSize(inner, tags.height);
                var leaves = Leaves;
                tags.itemRenderer = (index, obj) => FillTag((GComponent)obj, leaves[index]);
                tags.numItems = leaves.Count;
                tags.ResizeToFit();
            }

            if (attrs != null)
            {
                attrs.SetSize(inner, attrs.height);
                var ids = Ids;
                var currents = Currents;
                var maxes = Maxes;
                attrs.itemRenderer = (index, obj) =>
                {
                    var row = (GComponent)obj;
                    row.SetSize(attrs.width, 18f);
                    FillAttr(row, ids[index], currents[index], maxes[index]);
                };
                attrs.numItems = ids.Count;
                attrs.ResizeToFit();
                for (var i = 0; i < ids.Count && i < attrs.numChildren; i++)
                    FillAttr(attrs.GetChildAt(i).asCom, ids[i], currents[i], maxes[i]);
            }

            var tagsHeight = tags == null || Leaves.Count == 0 ? 0f : tags.height;
            var attrsY = pad + (tagsHeight > 0f && Ids.Count > 0 ? tagsHeight + 6f : tagsHeight);
            if (tags != null)
                tags.SetXY(pad, pad);
            if (attrs != null)
                attrs.SetXY(pad, attrsY);
            var height = attrsY + (attrs == null || Ids.Count == 0 ? 0f : attrs.height) + pad;
            view.SetSize(view.width, Mathf.Max(pad * 2f, height));
            var bg = view.GetChild("bg");
            if (bg != null)
                bg.SetSize(view.width, view.height);
            view.visible = Leaves.Count > 0 || Ids.Count > 0;
        }

        static void FillTag(GComponent row, string leaf)
        {
            if (row == null)
                return;
            var title = row.GetChild("title") as GTextField;
            var bg = row.GetChild("bg") as GGraph;
            if (title != null)
                title.text = leaf;
            var width = title == null ? 48f : Mathf.Max(28f, Mathf.Ceil(title.textWidth) + 16f);
            row.SetSize(width, 22f);
            if (bg != null)
                bg.SetSize(width, 22f);
            if (title != null)
            {
                title.SetXY(8f, 1f);
                title.SetSize(width - 16f, 20f);
            }
        }

        static void FillAttr(GComponent row, ulong id, long current, long max)
        {
            if (row == null)
                return;
            var title = row.GetChild("title") as GTextField;
            var track = row.GetChild("track") as GGraph;
            var fill = row.GetChild("fill") as GGraph;
            var name = ProjectSession.AttributeName(id, new AttributeId(id).LocalId);
            var resource = IsResource(id);
            var number = row.GetChild("value") as GTextField;
            if (title != null)
                title.text = resource || number != null ? name : name + "  " + current;
            if (number != null)
            {
                number.visible = !resource;
                number.text = current.ToString();
            }

            if (track != null)
                track.visible = resource;
            if (fill != null)
                fill.visible = resource;
            if (!resource)
                return;
            var barX = 76f;
            var barW = Mathf.Max(8f, row.width - barX);
            var ratio = max <= 0 ? 0f : Mathf.Clamp01((float)(current / (double)max));
            if (track != null)
            {
                track.SetXY(barX, 5f);
                track.SetSize(barW, 8f);
            }

            if (fill != null)
            {
                fill.SetXY(barX, 5f);
                fill.SetSize(barW * ratio, 8f);
            }
        }

        static void Place(GComponent view, PreviewActor actor, GComponent parent, Camera camera)
        {
            var dummy = actor.GetComponent<PreviewDummy>();
            var world = dummy != null ? dummy.HeadPosition : actor.transform.position + Vector3.up * 1.9f;
            var screen = camera.WorldToScreenPoint(world);
            if (screen.z < 0f)
            {
                view.visible = false;
                return;
            }

            var stagePoint = StageOf(screen);
            var local = parent.GlobalToLocal(stagePoint);
            view.SetXY(local.x - view.width * 0.5f, local.y - view.height);
        }

        static Vector2 StageOf(Vector3 screen)
        {
            var stage = Stage.inst;
            var cam = StageCamera.main;
            if (stage == null || cam == null)
                return new Vector2(screen.x, Screen.height - screen.y);
            var depth = cam.WorldToScreenPoint(stage.cachedTransform.position).z;
            var world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
            var local = stage.cachedTransform.InverseTransformPoint(world);
            return new Vector2(local.x, -local.y);
        }

        static void CollectTags(PreviewActor actor, List<string> leaves)
        {
            leaves.Clear();
            for (var i = 0; i < actor.Tags.Count; i++)
                leaves.Add(Leaf(actor.Tags[i]));
        }

        static string Leaf(ulong id)
        {
            var path = ComTagEditor.Path(id);
            if (string.IsNullOrEmpty(path))
                return id.ToString();
            var cut = path.LastIndexOf('/');
            return cut < 0 ? path : path.Substring(cut + 1);
        }

        static void CollectResources(PreviewActor actor, bool editing, List<ulong> ids, List<long> currents, List<long> maxes)
        {
            ids.Clear();
            currents.Clear();
            maxes.Clear();
            if (actor.Unit == null)
            {
                for (var i = 0; i < actor.InitialValues.Count; i++)
                    AddTracked(actor, ids, actor.InitialValues[i].AttributeId);
            }
            else
            {
                AddTracked(actor, ids, actor.Unit.PresetAttributes);
                for (var i = 0; i < actor.Unit.Values.Count; i++)
                    AddTracked(actor, ids, actor.Unit.Values[i].AttributeId);
                for (var i = 0; i < actor.InitialValues.Count; i++)
                    AddTracked(actor, ids, actor.InitialValues[i].AttributeId);
            }

            var framed = !editing && actor.HasSample;
            for (var i = 0; i < ids.Count; i++)
            {
                var id = ids[i];
                if (framed)
                {
                    SampleOf(actor, id, out var current, out var max);
                    currents.Add(current);
                    maxes.Add(IsResource(id) ? max : current);
                    continue;
                }

                var hasDefault = TryLayout(id, out var fallback);
                var raw = hasDefault ? fallback : 0L;
                if (actor.Unit != null && HasValue(actor.Unit.Values, id))
                    raw = UnitCatalogs.Raw(actor.Unit, id);
                raw = Override(actor.InitialValues, id, raw);
                currents.Add(raw);
                maxes.Add(ResourceMax(actor, id, hasDefault ? fallback : 0L));
            }
        }

        static void SampleOf(PreviewActor actor, ulong id, out long current, out long max)
        {
            current = 0;
            max = 0;
            for (var i = 0; i < actor.Sampled.Count; i++)
            {
                var value = actor.Sampled[i];
                if (value.AttributeId != id)
                    continue;
                current = value.Raw;
                max = value.Max;
                return;
            }
        }

        static void AddTracked(PreviewActor actor, List<ulong> ids, List<ulong> source)
        {
            for (var i = 0; i < source.Count; i++)
                AddTracked(actor, ids, source[i]);
        }

        static void AddTracked(PreviewActor actor, List<ulong> ids, ulong id)
        {
            if (id == 0 || Contains(ids, id) || !actor.Tracks(id))
                return;
            ids.Add(id);
        }

        static bool HasValue(List<EditorUnitValue> values, ulong id)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (values[i].AttributeId == id)
                    return true;
            }

            return false;
        }

        static long ResourceMax(PreviewActor actor, ulong id, long layout)
        {
            for (var i = 0; i < actor.InitialValues.Count; i++)
            {
                var value = actor.InitialValues[i];
                if (value.AttributeId == id && value.HasMax)
                    return value.Max;
            }

            return layout;
        }

        static long Override(List<EditorUnitValue> values, ulong id, long current)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (values[i].AttributeId == id)
                    return values[i].Raw;
            }

            return current;
        }

        static bool Contains(List<ulong> ids, ulong id)
        {
            for (var i = 0; i < ids.Count; i++)
            {
                if (ids[i] == id)
                    return true;
            }

            return false;
        }

        static bool IsResource(ulong id)
        {
            return Behavior(ProjectSession.OwnModule, id) == AttributeBehavior.Resource
                || BehaviorOfSources(id) == AttributeBehavior.Resource;
        }

        static AttributeBehavior BehaviorOfSources(ulong id)
        {
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                var behavior = Behavior(ProjectSession.Sources[i].Module, id);
                if (behavior != AttributeBehavior.None)
                    return behavior;
            }

            return AttributeBehavior.None;
        }

        static AttributeBehavior Behavior(SpellModuleData module, ulong id)
        {
            if (module == null)
                return AttributeBehavior.None;
            for (var i = 0; i < module.Attributes.Count; i++)
            {
                var attribute = module.Attributes[i];
                if (attribute.Id == id)
                    return (AttributeBehavior)attribute.Behavior;
            }

            return AttributeBehavior.None;
        }

        static bool TryLayout(ulong id, out long raw)
        {
            if (LayoutOf(ProjectSession.OwnModule, id, out raw))
                return true;
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                if (LayoutOf(ProjectSession.Sources[i].Module, id, out raw))
                    return true;
            }

            raw = 0;
            return false;
        }

        static bool LayoutOf(SpellModuleData module, ulong id, out long raw)
        {
            raw = 0;
            if (module == null)
                return false;
            for (var i = 0; i < module.AttributeLayout.Count; i++)
            {
                var entry = module.AttributeLayout[i];
                if (entry.AttributeId != id)
                    continue;
                raw = entry.Raw;
                return true;
            }

            return false;
        }
    }
}
