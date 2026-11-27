using System;
using System.Collections.Generic;
using FairyGUI;
using RebornFrame.Spell.Spine;
using SpellEditor;
using UnityEngine;

namespace UILogic
{
    /// <summary>
    /// 技能预览场地。Awake 记下主相机当时的位置、角度和视野，重置时回到这一帧。
    /// </summary>
    public class SpellPreviewStage : MonoBehaviour
    {
        struct Pose
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public bool Orthographic;
            public float FieldOfView;
            public float OrthographicSize;
        }

        const int GestureNone = 0;
        const int GestureUnit = 1;
        const int GesturePan = 2;
        const int GestureOrbit = 3;

        static SpellPreviewStage _stage;

        Pose _home;
        bool _homeReady;
        Camera _camera;
        GObject _hit;
        Action<PreviewActor> _onSelect;
        PreviewActor _selected;
        ISpineSpellObject _casterView;
        ulong _casterId;
        string _casterKey = "";
        int _gesture;
        Vector2 _lastScreen;
        Vector3 _grab;
        Vector3 _pivot;
        Material _outline;

        public static PreviewActor Selected
        {
            get { return _stage == null ? null : _stage._selected; }
        }

        void Awake()
        {
            _stage = this;
            CaptureHome();
            AdoptActors();
        }

        void OnDestroy()
        {
            if (_stage == this)
                _stage = null;
            ReleaseCaster();
        }

        public static void Open(GObject hit, Action<PreviewActor> onSelect)
        {
            var camera = Camera.main;
            if (_stage == null)
                _stage = FindAnyObjectByType<SpellPreviewStage>();
            if (camera == null || hit == null || _stage == null)
                return;
            _stage.Show(camera, hit, onSelect);
        }

        public static void Close()
        {
            if (_stage == null)
                return;
            _stage._onSelect = null;
            _stage.Hide();
        }

        public static void ResetCamera()
        {
            if (_stage == null)
                _stage = FindAnyObjectByType<SpellPreviewStage>();
            if (_stage == null)
                return;
            if (!_stage._homeReady)
                _stage.CaptureHome();
            _stage.ApplyHome();
        }

        public static void AddDummy()
        {
            if (_stage == null)
                _stage = FindAnyObjectByType<SpellPreviewStage>();
            if (_stage == null)
                return;
            _stage.SpawnDummy();
        }

        public static void SetCaster(EditorUnit unit)
        {
            if (_stage == null)
                _stage = FindAnyObjectByType<SpellPreviewStage>();
            if (_stage == null)
                return;
            _stage.ApplyCaster(unit);
        }

        void CaptureHome()
        {
            var camera = Camera.main;
            if (camera == null)
                camera = FindAnyObjectByType<Camera>();
            if (camera == null)
                return;
            _home = new Pose
            {
                Position = camera.transform.position,
                Rotation = camera.transform.rotation,
                Orthographic = camera.orthographic,
                FieldOfView = camera.fieldOfView,
                OrthographicSize = camera.orthographicSize,
            };
            _homeReady = true;
        }

        void ApplyHome()
        {
            var camera = _camera != null ? _camera : Camera.main;
            if (camera == null || !_homeReady)
                return;
            camera.orthographic = _home.Orthographic;
            if (_home.Orthographic)
                camera.orthographicSize = _home.OrthographicSize;
            else
                camera.fieldOfView = _home.FieldOfView;
            camera.transform.SetPositionAndRotation(_home.Position, _home.Rotation);
        }

        void Show(Camera camera, GObject hit, Action<PreviewActor> onSelect)
        {
            _camera = camera;
            _onSelect = onSelect;
            if (!_homeReady)
                CaptureHome();
            ApplyHome();
            Bind(hit);
            AdoptActors();
        }

        void Hide()
        {
            Unbind();
            _gesture = GestureNone;
            Select(null, false);
            ReleaseCaster();
            _camera = null;
        }

        void Bind(GObject hit)
        {
            Unbind();
            _hit = hit;
            if (_hit == null)
                return;
            _hit.onTouchBegin.Add(OnTouchBegin);
            _hit.onTouchMove.Add(OnTouchMove);
            _hit.onTouchEnd.Add(OnTouchEnd);
        }

        void Unbind()
        {
            if (_hit == null || _hit.isDisposed)
            {
                _hit = null;
                return;
            }

            _hit.onTouchBegin.Remove(OnTouchBegin);
            _hit.onTouchMove.Remove(OnTouchMove);
            _hit.onTouchEnd.Remove(OnTouchEnd);
            _hit = null;
        }

        void OnTouchBegin(EventContext context)
        {
            var input = context.inputEvent;
            var screen = ScreenOf(new Vector2(input.x, input.y));
            _lastScreen = screen;
            if (input.button == 2)
            {
                _gesture = GesturePan;
                context.CaptureTouch();
                return;
            }

            if (input.button == 1)
            {
                _pivot = OrbitPivot();
                _gesture = GestureOrbit;
                context.CaptureTouch();
                return;
            }

            if (input.button != 0)
                return;
            var actor = Pick(screen);
            Select(actor, true);
            if (actor == null || actor.Locked)
            {
                _gesture = GestureNone;
                return;
            }

            _gesture = GestureUnit;
            var ray = _camera.ScreenPointToRay(screen);
            if (GroundAt(ray, actor.transform.position.y, out var point))
                _grab = actor.transform.position - point;
            else
                _grab = Vector3.zero;
            context.CaptureTouch();
        }

        void OnTouchMove(EventContext context)
        {
            if (_gesture == GestureNone || _camera == null)
                return;
            var screen = ScreenOf(new Vector2(context.inputEvent.x, context.inputEvent.y));
            var delta = screen - _lastScreen;
            _lastScreen = screen;
            if (_gesture == GesturePan)
                Pan(delta);
            else if (_gesture == GestureOrbit)
                Orbit(delta);
            else if (_gesture == GestureUnit && _selected != null && !_selected.Locked)
                DragUnit(screen);
        }

        void OnTouchEnd(EventContext context)
        {
            _gesture = GestureNone;
        }

        void Pan(Vector2 delta)
        {
            var distance = Vector3.Distance(_camera.transform.position, Vector3.zero);
            if (distance < 0.5f)
                distance = 8f;
            var span = _camera.orthographic
                ? _camera.orthographicSize * 2f
                : distance * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 2f;
            var scale = span / Mathf.Max(1, Screen.height);
            var move = _camera.transform.right * delta.x * scale + _camera.transform.up * delta.y * scale;
            _camera.transform.position -= move;
        }

        void Orbit(Vector2 delta)
        {
            if (delta.sqrMagnitude < 0.01f)
                return;
            var yaw = Quaternion.AngleAxis(-delta.x * 0.25f, Vector3.up);
            var pitchedRight = yaw * _camera.transform.right;
            var pitch = Quaternion.AngleAxis(delta.y * 0.2f, pitchedRight);
            var turn = pitch * yaw;
            var offset = _camera.transform.position - _pivot;
            var next = turn * offset;
            if (next.y < 0.15f)
                turn = yaw;
            next = turn * offset;
            if (next.y < 0.15f)
                return;
            _camera.transform.position = _pivot + next;
            _camera.transform.rotation = turn * _camera.transform.rotation;
        }

        Vector3 OrbitPivot()
        {
            var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (_hit != null && !_hit.isDisposed)
            {
                var rect = ScreenRect(_hit);
                center = rect.center;
            }

            var ray = _camera.ScreenPointToRay(center);
            if (GroundAt(ray, 0f, out var point))
                return point;
            return Vector3.zero;
        }

        void DragUnit(Vector2 screen)
        {
            var actor = _selected;
            var ray = _camera.ScreenPointToRay(screen);
            if (!GroundAt(ray, actor.transform.position.y, out var point))
                return;
            var next = point + _grab;
            var position = actor.transform.position;
            actor.transform.position = new Vector3(next.x, position.y, next.z);
        }

        PreviewActor Pick(Vector2 screen)
        {
            var ray = _camera.ScreenPointToRay(screen);
            var hits = Physics.RaycastAll(ray, 500f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            PreviewActor best = null;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < hits.Length; i++)
            {
                var actor = hits[i].collider.GetComponentInParent<PreviewActor>();
                if (actor == null || hits[i].distance >= bestDistance)
                    continue;
                best = actor;
                bestDistance = hits[i].distance;
            }

            return best;
        }

        void Select(PreviewActor actor, bool notify)
        {
            if (_selected == actor)
            {
                if (notify && actor != null && _onSelect != null)
                    _onSelect(actor);
                return;
            }

            if (_selected != null)
                _selected.SetSelected(false);
            _selected = actor;
            if (_selected != null)
                _selected.SetSelected(true);
            if (notify && _onSelect != null)
                _onSelect(actor);
        }

        void SpawnDummy()
        {
            var prefab = Resources.Load<GameObject>("Prefabs/TrainingDummy");
            if (prefab == null)
            {
                Debug.LogWarning("没有木桩预制体。");
                return;
            }

            var index = 1;
            var actors = GetComponentsInChildren<PreviewActor>(true);
            for (var i = 0; i < actors.Length; i++)
            {
                var actor = actors[i];
                if (actor != null && !actor.Locked && actor.Unit == null)
                    index++;
            }

            var body = Instantiate(prefab, transform);
            body.name = index == 1 ? "木桩" : "木桩 " + index;
            body.transform.position = new Vector3(1.5f * index, 0f, 0f);
            var spawned = body.GetComponent<PreviewActor>();
            if (spawned == null)
                spawned = body.AddComponent<PreviewActor>();
            spawned.Locked = false;
            spawned.ScheduleFit();
            Select(spawned, true);
        }

        void AdoptActors()
        {
            var bodies = GetComponentsInChildren<PreviewDummy>(true);
            for (var i = 0; i < bodies.Length; i++)
            {
                if (bodies[i].GetComponent<PreviewActor>() == null)
                    bodies[i].gameObject.AddComponent<PreviewActor>();
            }
        }

        void ApplyCaster(EditorUnit unit)
        {
            var key = unit == null ? "" : unit.SpineKey ?? "";
            var id = unit == null ? 0UL : unit.Id;
            if (_casterView != null && id == _casterId && key == _casterKey)
            {
                var current = _casterView.GameObject;
                if (current != null)
                {
                    var actor = current.GetComponent<PreviewActor>();
                    if (actor != null)
                        actor.Unit = unit;
                }

                return;
            }

            ReleaseCaster();
            _casterId = id;
            _casterKey = key;
            if (unit == null || string.IsNullOrEmpty(key))
                return;

            var asset = FindSpine(key);
            if (asset == null)
                return;
            if (!ResourceLibrary.TryOpenSpine(asset, out var document, out var error))
            {
                if (!string.IsNullOrEmpty(error))
                    Debug.LogWarning(error);
                return;
            }

            var setup = "";
            if (ResourceLibrary.TryReadSpineSetup(asset, out var saved, out _) && !string.IsNullOrEmpty(saved))
                setup = saved;
            if (!SpineSpellLoading.TryCreate(SourceOf(document), new SpineLoadSettings(), setup, out var view, out error) || view == null)
            {
                if (!string.IsNullOrEmpty(error))
                    Debug.LogWarning(error);
                return;
            }

            var body = view.GameObject;
            if (body == null)
            {
                view.Dispose();
                return;
            }

            body.name = "PreviewCaster";
            body.transform.SetParent(transform, false);
            body.transform.position = Vector3.zero;
            body.transform.rotation = Quaternion.identity;
            var spawned = body.AddComponent<PreviewActor>();
            spawned.Locked = true;
            spawned.Unit = unit;
            spawned.BindOutline(OutlineMaterial());
            spawned.ScheduleFit();
            _casterView = view;
        }

        void ReleaseCaster()
        {
            if (_selected != null && _casterView != null && _selected.gameObject == _casterView.GameObject)
                Select(null, true);
            if (_casterView != null)
            {
                _casterView.Dispose();
                _casterView = null;
            }

            _casterId = 0;
            _casterKey = "";
        }

        Material OutlineMaterial()
        {
            if (_outline != null)
                return _outline;
#if UNITY_EDITOR
            _outline = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/PreviewOutline.mat");
#endif
            return _outline;
        }

        static ResourceAsset FindSpine(string key)
        {
            var assets = ResourceLibrary.List();
            for (var i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (asset != null && asset.Key == key && ResourceLibrary.IsSpine(asset.Kind))
                    return asset;
            }

            return null;
        }

        static SpineSkeletonBytes SourceOf(ResourceLibrary.SpineDocument document)
        {
            var source = new SpineSkeletonBytes();
            source.Skeleton = document.Skeleton;
            source.Binary = document.Binary;
            source.Atlas = document.Atlas;
            for (var i = 0; i < document.Pages.Count; i++)
            {
                var page = document.Pages[i];
                source.Pages.Add(new SpineAtlasPage { Name = page.Name, Png = page.Bytes });
            }

            return source;
        }

        static bool GroundAt(Ray ray, float y, out Vector3 point)
        {
            point = Vector3.zero;
            if (Mathf.Abs(ray.direction.y) < 0.0001f)
                return false;
            var distance = (y - ray.origin.y) / ray.direction.y;
            if (distance < 0f)
                return false;
            point = ray.origin + ray.direction * distance;
            return true;
        }

        static Vector2 ScreenOf(Vector2 stagePoint)
        {
            var stage = Stage.inst;
            var cam = StageCamera.main;
            if (stage == null || cam == null)
                return stagePoint;
            var world = stage.cachedTransform.TransformPoint(stagePoint.x, -stagePoint.y, 0f);
            var screen = cam.WorldToScreenPoint(world);
            return new Vector2(screen.x, screen.y);
        }

        static Rect ScreenRect(GObject hole)
        {
            var topLeft = ScreenOf(hole.LocalToGlobal(Vector2.zero));
            var bottomRight = ScreenOf(hole.LocalToGlobal(new Vector2(hole.width, hole.height)));
            var x = Mathf.Min(topLeft.x, bottomRight.x);
            var y = Mathf.Min(topLeft.y, bottomRight.y);
            return new Rect(x, y, Mathf.Abs(bottomRight.x - topLeft.x), Mathf.Abs(bottomRight.y - topLeft.y));
        }
    }
}
