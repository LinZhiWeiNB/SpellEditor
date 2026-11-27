using System;
using System.Collections.Generic;
using FairyGUI;
using RebornFrame.Spell.Spine;
using GameContentEditor;
using Spine.Unity;
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
        bool _movedUnit;
        Vector2 _lastScreen;
        Vector3 _grab;
        Vector3 _pivot;
        Material _outline;

        public readonly struct SpineClipChoice
        {
            public readonly string Id;
            public readonly string Label;
            public readonly float Seconds;

            public SpineClipChoice(string id, string label, float seconds)
            {
                Id = id ?? "";
                Label = string.IsNullOrEmpty(label) ? Id : label;
                Seconds = seconds;
            }
        }

        public static PreviewActor Selected
        {
            get { return _stage == null ? null : _stage._selected; }
        }

        public static void CopyCasterAnimations(List<SpineClipChoice> into)
        {
            into.Clear();
            var data = CasterSkeleton();
            if (data == null || data.Animations == null) return;
            var animations = data.Animations;
            for (var i = 0; i < animations.Count; i++)
            {
                var item = animations.Items[i];
                if (item == null || string.IsNullOrEmpty(item.Name)) continue;
                into.Add(new SpineClipChoice(item.Name, item.Name, item.Duration));
            }
        }

        public static void CopyCasterTriggers(List<SpineClipChoice> into)
        {
            into.Clear();
            if (_stage == null || string.IsNullOrEmpty(_stage._casterKey)) return;
            var asset = FindSpine(_stage._casterKey);
            if (asset == null || !ResourceLibrary.TryReadSpineSetup(asset, out var text, out _) || string.IsNullOrEmpty(text))
                return;

            var setup = new SpinePreviewSetup();
            setup.Read(text);
            var seconds = new Dictionary<string, float>();
            var data = CasterSkeleton();
            if (data != null && data.Animations != null)
            {
                var animations = data.Animations;
                for (var i = 0; i < animations.Count; i++)
                {
                    var item = animations.Items[i];
                    if (item == null || string.IsNullOrEmpty(item.Name)) continue;
                    seconds[item.Name] = item.Duration;
                }
            }

            for (var i = 0; i < setup.Triggers.Count; i++)
            {
                var trigger = setup.Triggers[i];
                var clip = setup.Clip(trigger.Id, setup.GroupId);
                var length = 0f;
                if (!string.IsNullOrEmpty(clip))
                    seconds.TryGetValue(clip, out length);
                var label = string.IsNullOrEmpty(trigger.Name) ? trigger.Id : trigger.Name;
                into.Add(new SpineClipChoice(trigger.Id, label, length));
            }
        }

        public static bool TryCasterTriggerAnimation(string triggerId, out string animationName)
        {
            animationName = "";
            if (_stage == null || string.IsNullOrEmpty(triggerId) || string.IsNullOrEmpty(_stage._casterKey))
                return false;
            var asset = FindSpine(_stage._casterKey);
            if (asset == null || !ResourceLibrary.TryReadSpineSetup(asset, out var text, out _) || string.IsNullOrEmpty(text))
                return false;

            var setup = new SpinePreviewSetup();
            setup.Read(text);
            animationName = setup.Clip(triggerId, setup.GroupId) ?? "";
            return animationName.Length > 0;
        }

        public static bool TryBindCaster(ulong stableId)
        {
            if (stableId == 0 || _stage == null || _stage._casterView == null) return false;
            var body = _stage._casterView.GameObject;
            if (body == null) return false;
            var animation = body.GetComponent<SkeletonAnimation>();
            if (animation == null || animation.AnimationState == null || animation.Skeleton == null) return false;
            SpineSpellTracks.Bind(stableId, new PreviewSpineSkeleton(animation));
            return true;
        }

        static Spine.SkeletonData CasterSkeleton()
        {
            if (_stage == null || _stage._casterView == null) return null;
            var body = _stage._casterView.GameObject;
            if (body == null) return null;
            var animation = body.GetComponent<SkeletonAnimation>();
            if (animation == null || animation.Skeleton == null) return null;
            return animation.Skeleton.Data;
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

        public static event Action Changed;
        public static bool AllowDrag { get; private set; } = true;

        public static Camera ViewCamera
        {
            get
            {
                if (_stage != null && _stage._camera != null)
                    return _stage._camera;
                return Camera.main;
            }
        }

        public static void SetPlacement(bool allowDrag)
        {
            AllowDrag = allowDrag;
            if (_stage == null || allowDrag)
                return;
            _stage._movedUnit = false;
            _stage._gesture = GestureNone;
        }

        public static void RestoreDefaults()
        {
            if (_stage == null)
                _stage = FindAnyObjectByType<SpellPreviewStage>();
            if (_stage == null)
                return;
            var actors = _stage.GetComponentsInChildren<PreviewActor>(true);
            for (var i = 0; i < actors.Length; i++)
            {
                var actor = actors[i];
                if (actor == null)
                    continue;
                actor.transform.position = actor.HomePosition;
                actor.Tags.Clear();
                actor.Tags.AddRange(actor.DefaultTags);
                actor.Buffs.Clear();
                actor.Sampled.Clear();
                actor.HasSample = false;
                actor.ShowFrameMarks(false);
                actor.SetPick(0);
                var dummy = actor.GetComponent<PreviewDummy>();
                if (dummy == null)
                    continue;
                var names = new List<string>(actor.DefaultTags.Count);
                for (var t = 0; t < actor.DefaultTags.Count; t++)
                    names.Add(TagPath(actor.DefaultTags[t]));
                dummy.SetTags(names);
            }
        }

        public static void AddDummy()
        {
            if (_stage == null)
                _stage = FindAnyObjectByType<SpellPreviewStage>();
            if (_stage == null)
                return;
            _stage.SpawnDummy(0, new Vector3(1.5f * _stage.NextDummyIndex(), 0f, 0f), true);
        }

        public static PreviewActor SpawnDummy(ulong stableId, Vector3 position)
        {
            if (_stage == null)
                _stage = FindAnyObjectByType<SpellPreviewStage>();
            if (_stage == null)
                return null;
            return _stage.SpawnDummy(stableId, position, false);
        }

        public static void CopySeeds(List<SpellPreviewSim.Seed> into)
        {
            into.Clear();
            if (_stage == null)
                return;
            var actors = _stage.GetComponentsInChildren<PreviewActor>(true);
            for (var i = 0; i < actors.Length; i++)
            {
                var actor = actors[i];
                if (actor == null || actor.StableId == 0)
                    continue;
                var seed = new SpellPreviewSim.Seed
                {
                    StableId = actor.StableId,
                    Position = actor.HomePosition,
                };
                seed.Tags.AddRange(actor.DefaultTags);
                seed.CastTags.AddRange(actor.CastTags);
                if (actor.InitialValues.Count > 0)
                {
                    for (var v = 0; v < actor.InitialValues.Count; v++)
                        seed.Values.Add(actor.InitialValues[v]);
                }
                else if (actor.Unit != null)
                {
                    for (var v = 0; v < actor.Unit.Values.Count; v++)
                        seed.Values.Add(actor.Unit.Values[v]);
                }

                into.Add(seed);
            }
        }

        public static void ApplyFrame(SpellPreviewSim.Frame frame)
        {
            if (_stage == null || frame == null)
                return;
            var actors = _stage.GetComponentsInChildren<PreviewActor>(true);
            for (var i = 0; i < frame.Units.Count; i++)
            {
                var unit = frame.Units[i];
                PreviewActor actor = null;
                for (var a = 0; a < actors.Length; a++)
                {
                    if (actors[a] != null && actors[a].StableId == unit.StableId)
                    {
                        actor = actors[a];
                        break;
                    }
                }

                if (actor == null)
                    continue;
                actor.ShowFrameMarks(true);
                actor.SetPick(PickOf(frame, unit.StableId));
                actor.transform.position = unit.Position;
                actor.Tags.Clear();
                actor.Tags.AddRange(unit.Tags);
                actor.Buffs.Clear();
                actor.Buffs.AddRange(unit.Buffs);
                actor.Sampled.Clear();
                actor.Sampled.AddRange(unit.Attributes);
                actor.HasSample = true;
                var dummy = actor.GetComponent<PreviewDummy>();
                if (dummy != null)
                {
                    var names = new List<string>(unit.Tags.Count);
                    for (var t = 0; t < unit.Tags.Count; t++)
                        names.Add(TagPath(unit.Tags[t]));
                    dummy.SetTags(names);
                }
            }

            for (var a = 0; a < actors.Length; a++)
            {
                var actor = actors[a];
                if (actor == null)
                    continue;
                var listed = false;
                for (var i = 0; i < frame.Units.Count; i++)
                {
                    if (frame.Units[i].StableId == actor.StableId)
                    {
                        listed = true;
                        break;
                    }
                }

                if (listed)
                    continue;
                actor.ShowFrameMarks(true);
                actor.SetPick(PickOf(frame, actor.StableId));
            }
        }

        static int PickOf(SpellPreviewSim.Frame frame, ulong stableId)
        {
            for (var i = 0; i < frame.Picks.Count; i++)
            {
                var pick = frame.Picks[i];
                if (pick.StableId != stableId)
                    continue;
                return pick.Passed ? 1 : 2;
            }

            return 0;
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
            PreviewOutline.Clear();
        }

        void LateUpdate()
        {
            PreviewOutline.Clear();
            if (_camera == null)
                return;
            var actors = GetComponentsInChildren<PreviewActor>(true);
            for (var i = 0; i < actors.Length; i++)
            {
                if (actors[i] != null)
                    actors[i].CollectOutline();
            }
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
            _movedUnit = false;
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
            if (actor == null || actor.Locked || !AllowDrag)
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
            var dragged = _movedUnit;
            _movedUnit = false;
            _gesture = GestureNone;
            if (dragged && _selected != null)
                _selected.RememberHome();
            if (dragged && Changed != null)
                Changed();
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
            _movedUnit = true;
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

        int NextDummyIndex()
        {
            var index = 1;
            var actors = GetComponentsInChildren<PreviewActor>(true);
            for (var i = 0; i < actors.Length; i++)
            {
                var actor = actors[i];
                if (actor != null && !actor.Locked && actor.Unit == null)
                    index++;
            }

            return index;
        }

        ulong NextDummyId()
        {
            var max = 0xE100000000000000UL;
            var actors = GetComponentsInChildren<PreviewActor>(true);
            for (var i = 0; i < actors.Length; i++)
            {
                var id = actors[i] == null ? 0UL : actors[i].StableId;
                if (id > max)
                    max = id;
            }

            return max + 1;
        }

        PreviewActor SpawnDummy(ulong stableId, Vector3 position, bool notify)
        {
            var prefab = Resources.Load<GameObject>("Prefabs/TrainingDummy");
            if (prefab == null)
            {
                Debug.LogWarning("没有木桩预制体。");
                return null;
            }

            var index = NextDummyIndex();
            var body = Instantiate(prefab, transform);
            body.name = index == 1 ? "木桩" : "木桩 " + index;
            body.transform.position = position;
            var spawned = body.GetComponent<PreviewActor>();
            if (spawned == null)
                spawned = body.AddComponent<PreviewActor>();
            spawned.Locked = false;
            spawned.StableId = stableId == 0 ? NextDummyId() : stableId;
            spawned.RememberHome();
            spawned.ScheduleFit();
            Select(spawned, true);
            if (notify && Changed != null)
                Changed();
            return spawned;
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
            spawned.StableId = unit.Id;
            spawned.Unit = unit;
            spawned.RememberHome();
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

        static string TagPath(ulong id)
        {
            var path = ComTagEditor.Path(id);
            return string.IsNullOrEmpty(path) ? id.ToString() : path;
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

        sealed class PreviewSpineSkeleton : ISpineSpellSkeleton
        {
            readonly SkeletonAnimation _animation;

            public PreviewSpineSkeleton(SkeletonAnimation animation)
            {
                _animation = animation;
            }

            public void Play(string animationName, int trackIndex, bool loop)
            {
                Show(animationName, trackIndex, loop, 0, true);
            }

            public void Stop(int trackIndex)
            {
                if (_animation == null || _animation.AnimationState == null) return;
                _animation.AnimationState.ClearTrack(trackIndex);
            }

            public void Show(string animationName, int trackIndex, bool loop, float time, bool play)
            {
                if (_animation == null || string.IsNullOrEmpty(animationName)) return;
                var state = _animation.AnimationState;
                var skeleton = _animation.Skeleton;
                if (state == null || skeleton == null) return;

                var entry = state.GetTrack(trackIndex);
                var same = entry != null
                           && entry.Animation != null
                           && entry.Animation.Name == animationName
                           && entry.Loop == loop;
                if (!same)
                {
                    try
                    {
                        entry = state.SetAnimation(trackIndex, animationName, loop);
                    }
                    catch (ArgumentException)
                    {
                        return;
                    }
                }

                if (entry == null) return;
                entry.TrackTime = Mathf.Max(0f, time);
                entry.TimeScale = play ? 1f : 0f;
                _animation.timeScale = play ? 1f : 0f;
                state.Update(0f);
                state.Apply(skeleton);
                skeleton.UpdateWorldTransform(Spine.Physics.Pose);
            }
        }
    }
}
