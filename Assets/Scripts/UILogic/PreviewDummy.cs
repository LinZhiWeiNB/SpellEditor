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
            _label.text = text;
            var show = text.Length > 0;
            var renderer = _label.GetComponent<Renderer>();
            renderer.enabled = show;
            if (_plate != null)
                _plate.enabled = show;
            if (show)
                FitPlate();
        }

        void FaceCamera()
        {
            var cam = Camera.main;
            if (cam == null)
                return;
            var toward = cam.transform.position - _label.transform.position;
            if (toward.sqrMagnitude < 0.0001f)
                return;
            _label.transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
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
            if (_outline == null)
                return;
            var bodies = GetComponentsInChildren<MeshRenderer>(true);
            for (var i = 0; i < bodies.Length; i++)
            {
                var body = bodies[i];
                if (!IsBody(body))
                    continue;
                var existing = body.transform.Find("Outline");
                if (existing != null)
                {
                    _outlines.Add(existing.gameObject);
                    continue;
                }

                var source = body.GetComponent<MeshFilter>().sharedMesh;
                var shell = new GameObject("Outline");
                shell.transform.SetParent(body.transform, false);
                var filter = shell.AddComponent<MeshFilter>();
                filter.sharedMesh = Smoothed(source);
                var renderer = shell.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = _outline;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                shell.SetActive(false);
                _outlines.Add(shell);
            }
        }

        bool IsBody(MeshRenderer renderer)
        {
            if (renderer == null || renderer == _plate)
                return false;
            if (_label != null && renderer.gameObject == _label.gameObject)
                return false;
            if (renderer.GetComponent<TextMesh>() != null)
                return false;
            if (renderer.gameObject.name == "Outline")
                return false;
            var filter = renderer.GetComponent<MeshFilter>();
            return filter != null && filter.sharedMesh != null;
        }

        void ApplyHighlight()
        {
            for (var i = 0; i < _outlines.Count; i++)
            {
                var outline = _outlines[i];
                if (outline == null)
                    continue;
                outline.SetActive(_highlighted);
                if (!_highlighted)
                    continue;
                var renderer = outline.GetComponent<MeshRenderer>();
                if (renderer == null)
                    continue;
                var block = new MaterialPropertyBlock();
                block.SetColor("_Color", _highlight);
                block.SetFloat("_Width", _outlineWidth);
                renderer.SetPropertyBlock(block);
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

        static Mesh Smoothed(Mesh source)
        {
            var vertices = source.vertices;
            if (vertices == null || vertices.Length == 0)
                return source;
            var normals = source.normals;
            if (normals == null || normals.Length != vertices.Length)
            {
                var temp = Instantiate(source);
                temp.RecalculateNormals();
                normals = temp.normals;
                if (Application.isPlaying)
                    Destroy(temp);
                else
                    DestroyImmediate(temp);
            }

            var sums = new Dictionary<long, Vector3>();
            var counts = new Dictionary<long, int>();
            for (var i = 0; i < vertices.Length; i++)
            {
                var key = Quantize(vertices[i]);
                Vector3 sum;
                sums.TryGetValue(key, out sum);
                sums[key] = sum + normals[i];
                int count;
                counts.TryGetValue(key, out count);
                counts[key] = count + 1;
            }

            var smooth = new Vector3[vertices.Length];
            for (var i = 0; i < vertices.Length; i++)
            {
                var key = Quantize(vertices[i]);
                var averaged = sums[key] / counts[key];
                smooth[i] = averaged.sqrMagnitude < 1e-8f ? normals[i] : averaged.normalized;
            }

            var mesh = new Mesh();
            mesh.name = source.name + " Outline";
            mesh.indexFormat = source.indexFormat;
            mesh.vertices = vertices;
            mesh.normals = smooth;
            mesh.subMeshCount = source.subMeshCount;
            for (var sub = 0; sub < source.subMeshCount; sub++)
                mesh.SetTriangles(source.GetTriangles(sub), sub);
            mesh.hideFlags = HideFlags.HideAndDontSave;
            return mesh;
        }

        static long Quantize(Vector3 point)
        {
            var x = (long)Mathf.RoundToInt(point.x * 1000f);
            var y = (long)Mathf.RoundToInt(point.y * 1000f);
            var z = (long)Mathf.RoundToInt(point.z * 1000f);
            return (x & 0x1fffff) | ((y & 0x1fffff) << 21) | ((z & 0x1fffff) << 42);
        }

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
