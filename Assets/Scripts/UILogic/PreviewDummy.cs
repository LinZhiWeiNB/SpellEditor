using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace UILogic
{
    public class PreviewDummy : MonoBehaviour
    {
        [SerializeField] TextMesh _label;
        [SerializeField] MeshRenderer _plate;
        [SerializeField] Material _outline;
        [SerializeField] List<string> _tags = new List<string>();
        [SerializeField] bool _highlighted;
        [SerializeField] Color _highlight = new Color(0.91f, 0.72f, 0.28f, 1f);
        [SerializeField] float _outlineWidth = 0.025f;

        readonly List<GameObject> _outlines = new List<GameObject>();
        TextMesh[] _strokes;
        bool _anchored;
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
            EnsurePlate();
            _ready = true;
            ApplyTags();
            ApplyHighlight();
        }

        void OnEnable()
        {
            ApplyTags();
            if (_ready)
                ApplyHighlight();
        }

        void LateUpdate()
        {
            if (_label == null)
                return;
            FaceCamera();
            if (_label.GetComponent<Renderer>().enabled)
                FitPlate();
        }

        public void SetTags(IReadOnlyList<string> tags)
        {
            _tags.Clear();
            if (tags != null)
            {
                for (var i = 0; i < tags.Count; i++)
                    _tags.Add(tags[i]);
            }

            ApplyTags();
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

        void ApplyTags()
        {
            if (_label == null)
                return;
            var text = Join(_tags);
            _label.characterSize = 0.035f;
            _label.fontSize = 32;
            _label.anchor = TextAnchor.LowerCenter;
            _label.alignment = TextAlignment.Center;
            _label.color = new Color(0.96f, 0.94f, 0.88f, 1f);
            _label.text = text;
            EnsureStrokes();
            for (var i = 0; i < _strokes.Length; i++)
            {
                var stroke = _strokes[i];
                stroke.text = text;
                stroke.characterSize = _label.characterSize;
                stroke.fontSize = _label.fontSize;
                stroke.anchor = _label.anchor;
                stroke.alignment = _label.alignment;
            }

            var show = text.Length > 0;
            var renderer = _label.GetComponent<Renderer>();
            renderer.sortingOrder = 1;
            renderer.enabled = show;
            for (var i = 0; i < _strokes.Length; i++)
                _strokes[i].GetComponent<Renderer>().enabled = show;
            if (_plate != null)
                _plate.enabled = show;
            if (show)
                FitPlate();
        }

        void FaceCamera()
        {
            var cam = SpellPreviewStage.ViewCamera;
            if (cam == null)
                cam = Camera.main;
            if (cam == null)
                return;
            AnchorLabel();
            _label.transform.localScale = Vector3.one;
            var toward = cam.transform.position - _label.transform.position;
            if (toward.sqrMagnitude < 0.0001f)
                toward = -cam.transform.forward;
            _label.transform.rotation = Quaternion.LookRotation(toward, cam.transform.up)
                * Quaternion.Euler(0f, 0f, 180f);
        }

        void AnchorLabel()
        {
            if (_anchored || _label.transform.parent == transform)
            {
                _anchored = true;
                return;
            }

            _label.transform.SetParent(transform, true);
            _anchored = true;
        }

        void EnsureStrokes()
        {
            if (_strokes != null)
                return;
            var offsets = new[]
            {
                new Vector3(-1f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, -1f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(-0.7f, -0.7f, 0f),
                new Vector3(-0.7f, 0.7f, 0f),
                new Vector3(0.7f, -0.7f, 0f),
                new Vector3(0.7f, 0.7f, 0f),
            };
            var material = _label.GetComponent<Renderer>().sharedMaterial;
            _strokes = new TextMesh[offsets.Length];
            for (var i = 0; i < offsets.Length; i++)
            {
                var body = new GameObject("TagStroke");
                body.transform.SetParent(_label.transform, false);
                body.transform.localPosition = offsets[i] * 0.008f + new Vector3(0f, 0f, -0.002f);
                body.transform.localRotation = Quaternion.identity;
                body.transform.localScale = Vector3.one;
                var mesh = body.AddComponent<TextMesh>();
                mesh.font = _label.font;
                mesh.color = Color.black;
                var renderer = mesh.GetComponent<Renderer>();
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.sortingOrder = 0;
                renderer.sharedMaterial = material;
                _strokes[i] = mesh;
            }
        }

        void FitPlate()
        {
            if (_plate == null || _label == null)
                return;
            var filter = _label.GetComponent<MeshFilter>();
            var mesh = filter != null ? filter.sharedMesh : null;
            if (mesh == null || mesh.vertexCount == 0)
            {
                _plate.enabled = false;
                return;
            }

            var bounds = mesh.bounds;
            _plate.transform.localScale = new Vector3(
                Mathf.Max(0.01f, bounds.size.x + 0.08f),
                Mathf.Max(0.01f, bounds.size.y + 0.05f),
                1f);
            _plate.transform.localPosition = new Vector3(bounds.center.x, bounds.center.y, -0.01f);
        }

        void EnsurePlate()
        {
            if (_plate == null)
                return;
            var filter = _plate.GetComponent<MeshFilter>();
            if (filter == null)
                filter = _plate.gameObject.AddComponent<MeshFilter>();
            if (filter.sharedMesh == null)
                filter.sharedMesh = PlateQuad();
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

        static string Join(List<string> tags)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];
                if (string.IsNullOrEmpty(tag))
                    continue;
                if (builder.Length > 0)
                    builder.Append(" · ");
                builder.Append(tag);
            }

            return builder.ToString();
        }

        static Mesh _plateQuad;

        static Mesh PlateQuad()
        {
            if (_plateQuad != null)
                return _plateQuad;
            var mesh = new Mesh();
            mesh.name = "PreviewPlateQuad";
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f)
            };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateBounds();
            mesh.hideFlags = HideFlags.HideAndDontSave;
            _plateQuad = mesh;
            return mesh;
        }
    }
}
