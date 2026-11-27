using System.Collections.Generic;
using GameContentEditor;
using UnityEngine;

namespace UILogic
{
    /// <summary>
    /// 预览场上的一个单位。预览单位可以选中，位置由 Locked 锁住。
    /// </summary>
    public class PreviewActor : MonoBehaviour
    {
        public bool Locked;

        [System.NonSerialized] public ulong StableId;
        [System.NonSerialized] public ulong PresetId;
        [System.NonSerialized] public EditorUnit Unit;
        [System.NonSerialized] public bool HasSample;
        [System.NonSerialized] public Vector3 HomePosition;
        [System.NonSerialized] public readonly List<ulong> DefaultTags = new List<ulong>();
        [System.NonSerialized] public readonly List<ulong> CastTags = new List<ulong>();
        [System.NonSerialized] public readonly List<ulong> Tags = new List<ulong>();
        [System.NonSerialized] public readonly List<ulong> Buffs = new List<ulong>();
        [System.NonSerialized] public readonly List<EditorUnitValue> InitialValues = new List<EditorUnitValue>();
        [System.NonSerialized] public readonly List<EditorUnitValue> Sampled = new List<EditorUnitValue>();

        PreviewDummy _highlight;
        int _fit;

        public void RememberHome()
        {
            HomePosition = transform.position;
        }

        public void RememberTags()
        {
            DefaultTags.Clear();
            DefaultTags.AddRange(Tags);
        }

        public void BindOutline(Material material)
        {
            _highlight = GetComponent<PreviewDummy>();
            if (_highlight == null)
                _highlight = gameObject.AddComponent<PreviewDummy>();
            _highlight.BindOutline(material);
        }

        public void SetSelected(bool on)
        {
            if (_highlight == null)
                _highlight = GetComponent<PreviewDummy>();
            if (_highlight != null)
                _highlight.SetHighlight(on);
        }

        public void ScheduleFit()
        {
            _fit = 2;
            FitCollider();
        }

        void LateUpdate()
        {
            if (_fit <= 0)
                return;
            _fit--;
            FitCollider();
        }

        public void FitCollider()
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            var has = false;
            var bounds = new Bounds(transform.position, Vector3.zero);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer.gameObject.name == "Outline")
                    continue;
                if (renderer.GetComponent<TextMesh>() != null)
                    continue;
                if (!has)
                {
                    bounds = renderer.bounds;
                    has = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            var box = GetComponent<BoxCollider>();
            if (box == null)
                box = gameObject.AddComponent<BoxCollider>();
            if (!has)
            {
                box.center = new Vector3(0f, 1f, 0f);
                box.size = new Vector3(0.6f, 2f, 0.6f);
                return;
            }

            box.center = transform.InverseTransformPoint(bounds.center);
            var size = bounds.size;
            var scale = transform.lossyScale;
            box.size = new Vector3(
                size.x / Mathf.Max(0.0001f, Mathf.Abs(scale.x)),
                size.y / Mathf.Max(0.0001f, Mathf.Abs(scale.y)),
                size.z / Mathf.Max(0.0001f, Mathf.Abs(scale.z)));
        }
    }
}
