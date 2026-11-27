using System.Collections.Generic;
using UnityEngine;

namespace UILogic
{
    public class PreviewDummy : MonoBehaviour
    {
        [SerializeField] TextMesh _label;
        [SerializeField] MeshRenderer _plate;
        [SerializeField] Material _outline;
        [SerializeField] bool _highlighted;
        [SerializeField] Color _highlight = new Color(0.91f, 0.72f, 0.28f, 1f);
        [SerializeField] float _outlineWidth = 0.025f;

        readonly List<GameObject> _outlines = new List<GameObject>();
        bool _ready;

        public bool Highlighted
        {
            get { return _highlighted; }
            set { SetHighlight(value); }
        }

        public Color HighlightColor
        {
            get { return _highlight; }
            set
            {
                _highlight = value;
                ApplyHighlight();
            }
        }

        void Awake()
        {
            EnsureOutlines();
            HideLabel();
            _ready = true;
            ApplyHighlight();
        }

        void OnEnable()
        {
            HideLabel();
            if (_ready)
                ApplyHighlight();
        }

        public Vector3 HeadPosition
        {
            get
            {
                var head = Head();
                if (head != null)
                    return head.position;
                var top = transform.position + Vector3.up * 1.9f;
                var renderers = GetComponentsInChildren<Renderer>(true);
                var found = false;
                for (var i = 0; i < renderers.Length; i++)
                {
                    var renderer = renderers[i];
                    if (renderer == null || !renderer.enabled || renderer == _plate)
                        continue;
                    if (renderer.GetComponent<TextMesh>() != null)
                        continue;
                    var bounds = renderer.bounds;
                    if (!found || bounds.max.y > top.y)
                    {
                        found = true;
                        top = new Vector3(bounds.center.x, bounds.max.y + 0.08f, bounds.center.z);
                    }
                }

                return top;
            }
        }

        public void SetHighlight(bool on)
        {
            _highlighted = on;
            if (_ready)
                ApplyHighlight();
        }

        public void BindOutline(Material material)
        {
            if (material == null)
                return;
            _outline = material;
            if (_outlines.Count == 0)
                EnsureOutlines();
            if (_ready)
                ApplyHighlight();
        }

        void HideLabel()
        {
            if (_label != null)
            {
                var renderer = _label.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.enabled = false;
                var strokes = _label.GetComponentsInChildren<TextMesh>(true);
                for (var i = 0; i < strokes.Length; i++)
                {
                    if (strokes[i] == _label)
                        continue;
                    strokes[i].gameObject.SetActive(false);
                }
            }

            if (_plate != null)
                _plate.enabled = false;
        }

        Transform Head()
        {
            if (_label != null)
                return _label.transform;
            var children = GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < children.Length; i++)
            {
                if (children[i] != null && children[i].name == "Tag")
                    return children[i];
            }

            return null;
        }

        void EnsureOutlines()
        {
            if (_outlineWidth < 0f && _outline == null)
                return;
            var children = GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < children.Length; i++)
            {
                var child = children[i];
                if (child != null && child.name == "Outline")
                    child.gameObject.SetActive(false);
            }
        }

        public bool IsPlate(Renderer renderer)
        {
            return renderer != null && renderer == _plate;
        }

        void ApplyHighlight()
        {
            for (var i = 0; i < _outlines.Count; i++)
            {
                var outline = _outlines[i];
                if (outline != null)
                    outline.SetActive(false);
            }
        }
    }
}
