using System;
using System.Collections.Generic;
using FairyGUI;
using PackageResource;
using RebornFrame.Spell.Spine;
using GameContentEditor;
using UnityEngine;

namespace UILogic
{
    /// <summary>
    /// 资源预览里的 Spine。界面用 FairyGUI 组件，这里只摆放、填数据和播放。
    /// </summary>
    sealed class ResourceSpinePreview
    {
        readonly SpineLoadSettings _settings = new SpineLoadSettings();
        readonly SpinePreviewSetup _setup = new SpinePreviewSetup();
        ResourceAsset _asset;
        UI_SpinePreview _preview;
        GGraph _holder;
        GGraph _frame;
        GoWrapper _wrapper;
        ISpineSpellObject _view;

        public Action<string> NoteChanged;

        public void Clear()
        {
            Timers.inst.Remove(RefitLater);
            _frame = null;
            PanelLayer.Dismiss<PanelSpineDetail>();
            if (_wrapper != null)
                _wrapper.SetWrapTarget(null, false);
            if (_view != null)
            {
                _view.Dispose();
                _view = null;
            }

            if (_preview != null) _preview.visible = false;
        }

        public bool Show(GComponent parent, GObject insertBefore, GGraph frame, ResourceAsset asset, ResourceLibrary.SpineDocument document, out string error)
        {
            return Show(parent, insertBefore, frame, asset, document, false, out error);
        }

        public bool Show(GComponent parent, GObject insertBefore, GGraph frame, ResourceAsset asset, ResourceLibrary.SpineDocument document, bool playIdle, out string error)
        {
            Clear();
            error = null;
            if (document == null || document.Skeleton == null || document.Skeleton.Length == 0 || string.IsNullOrEmpty(document.Atlas))
            {
                error = "这份 Spine 没有读出来。";
                return false;
            }

            try
            {
                _asset = asset;
                _setup.Clear();
                _setup.Skeleton = document.SkeletonName ?? "";
                if (ResourceLibrary.TryReadSpineSetup(asset, out var saved, out var setupError))
                {
                    if (!string.IsNullOrEmpty(saved)) _setup.Read(saved);
                }
                else if (!string.IsNullOrEmpty(setupError))
                {
                    Report(setupError);
                }

                if (playIdle && _setup.Groups.Count > 0)
                    _setup.GroupId = _setup.Groups[0].Id;

                if (!SpineSpellLoading.TryCreate(SourceOf(document), _settings, _setup.Write(), out _view, out error))
                    return false;

                _frame = frame;
                if (playIdle && _holder != null)
                    _holder.relations.ClearAll();
                if (!Ensure(parent, insertBefore))
                {
                    error = "Spine 预览组件没有加载出来。";
                    Clear();
                    return false;
                }

                Place(frame);
                if (_wrapper == null)
                {
                    _wrapper = new GoWrapper();
                    _holder.SetNativeObject(_wrapper);
                }

                _wrapper.SetWrapTarget(_view.GameObject, false);
                _preview.visible = true;
                _preview.group = frame.group;
                _preview.sortingOrder = frame.sortingOrder + 1;
                if (insertBefore != null && insertBefore.sortingOrder <= _preview.sortingOrder)
                    insertBefore.sortingOrder = _preview.sortingOrder + 1;
                if (playIdle)
                {
                    _preview.m_btnEdit.visible = false;
                    _preview.m_rowGroup.visible = false;
                    _preview.m_rowTrigger.visible = false;
                    _preview.m_slots.visible = false;
                    _holder.relations.ClearAll();
                    _holder.SetXY(0, 0);
                    _holder.SetSize(_preview.width, _preview.height);
                    PresentIdle();
                    Timers.inst.CallLater(RefitLater);
                }
                else
                {
                    RefreshControls();
                    Present();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Clear();
                error = "骨骼没有读出来。";
                return false;
            }
        }

        bool Ensure(GComponent parent, GObject insertBefore)
        {
            if (_preview != null) return true;
            var existing = parent.GetChild("spinePreview") as UI_SpinePreview;
            _preview = existing ?? UI_SpinePreview.CreateInstance();
            if (_preview == null) return false;
            _holder = _preview.m_holder;
            _holder.touchable = false;
            _preview.m_btnEdit.onClick.Set(OpenDetails);
            _preview.m_rowTrigger.m_title.text = "动画触发器";
            if (existing != null) return true;
            var index = insertBefore == null ? parent.numChildren : parent.GetChildIndex(insertBefore);
            if (index < 0) index = parent.numChildren;
            parent.AddChildAt(_preview, index);
            return true;
        }

        void Place(GGraph frame)
        {
            _preview.SetXY(frame.x, frame.y);
            _preview.SetSize(frame.width, frame.height);
        }

        void OpenDetails()
        {
            if (_view == null) return;
            if (!PanelSpineDetail.Show(_setup, _view.Animations, _view.SkinNames, _view.DefaultSkinName, ApplyDetails))
                Report("编辑详情没有打开。");
        }

        void ApplyDetails()
        {
            if (_view == null || _frame == null || _preview == null) return;
            RefreshControls();
            Place(_frame);
            Store(true);
            _view.LoadSetup(_setup.Write());
            Present();
            Store(true);
        }

        void RefreshControls()
        {
            BindChoices(_preview.m_rowGroup, "群组", _setup.Groups, _setup.GroupId, OnGroup);
            BindChoices(_preview.m_rowTrigger, "动画触发器", _setup.Triggers, _setup.TriggerId, OnTrigger);
            var slots = _preview.m_slots;
            slots.autoResizeItem = true;
            slots.itemRenderer = (index, obj) =>
            {
                var slot = _setup.Slots[index];
                BindChoices((UI_ItemChoiceRow)obj, string.IsNullOrEmpty(slot.Name) ? "未命名" : slot.Name, slot.Skins, slot.Chosen, skinId => OnSkin(slot.Id, skinId));
            };
            slots.numItems = _setup.Slots.Count;
        }

        static void BindChoices(UI_ItemChoiceRow row, string caption, List<SpineNamed> items, string selected, Action<string> pick)
        {
            row.m_title.text = caption ?? "";
            var box = row.m_value;
            box.onChanged.Set(() => { });
            if (items == null || items.Count == 0)
            {
                box.items = new[] { "无" };
                box.values = new[] { "" };
                box.value = "";
                return;
            }

            var labels = new string[items.Count];
            var keys = new string[items.Count];
            var index = 0;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                labels[i] = string.IsNullOrEmpty(item.Name) ? "未命名" : item.Name;
                keys[i] = item.Id ?? "";
                if (item.Id == selected) index = i;
            }

            box.items = labels;
            box.values = keys;
            box.selectedIndex = index;
            box.onChanged.Set(() =>
            {
                if (pick != null) pick(box.value ?? "");
            });
        }

        void OnGroup(string id)
        {
            if (_view == null || string.IsNullOrEmpty(id) || id == _setup.GroupId) return;
            _setup.GroupId = id;
            Store(false);
            _view.SetGroup(id);
            Present();
        }

        void OnTrigger(string id)
        {
            if (_view == null || string.IsNullOrEmpty(id)) return;
            _setup.TriggerId = id;
            Store(false);
            _view.PlayTrigger(id, 0, true);
            Present();
        }

        void OnSkin(string slotId, string skinId)
        {
            if (_view == null) return;
            for (var i = 0; i < _setup.Slots.Count; i++)
            {
                var slot = _setup.Slots[i];
                if (slot.Id != slotId || slot.Chosen == skinId) continue;
                slot.Chosen = skinId;
                Store(false);
                _view.LoadSetup(_setup.Write());
                Present();
                return;
            }
        }

        void RefitLater(object param)
        {
            if (_view == null || _holder == null || _preview == null || _frame == null || !_preview.visible) return;
            Place(_frame);
            _holder.relations.ClearAll();
            _holder.SetXY(0, 0);
            _holder.SetSize(_preview.width, _preview.height);
            PresentIdle();
        }

        void PresentIdle()
        {
            var trigger = IdleTrigger(_setup.Triggers);
            string clip = "";
            try
            {
                if (trigger != null)
                {
                    _setup.TriggerId = trigger.Id;
                    _view.PlayTrigger(trigger.Id, 0, true);
                    clip = _setup.ResolveClip();
                }
                else
                {
                    _view.Play(0, 0, true);
                    clip = _view.Animations.Count > 0 ? _view.Animations[0] : "";
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            Fit();
            if (_holder != null && _holder.displayObject != null)
                _holder.displayObject.SetSize(_holder.width, _holder.height);
            if (_wrapper != null) _wrapper.CacheRenderers();
            Report(string.IsNullOrEmpty(clip) ? "这份 Spine 没有可播放的动画。" : "循环播放  ·  " + clip);
        }

        static SpineNamed IdleTrigger(List<SpineNamed> triggers)
        {
            if (triggers == null) return null;
            for (var i = 0; i < triggers.Count; i++)
            {
                var item = triggers[i];
                if (item == null) continue;
                if (string.Equals(item.Name, "idle", StringComparison.OrdinalIgnoreCase))
                    return item;
            }

            return null;
        }

        void Present()
        {
            Fit();
            if (_holder != null && _holder.displayObject != null)
                _holder.displayObject.SetSize(_holder.width, _holder.height);
            if (_wrapper != null) _wrapper.CacheRenderers();
            var clip = _setup.ResolveClip();
            if (string.IsNullOrEmpty(clip))
            {
                Report(_setup.Triggers.Count == 0 ? "点编辑详情，设置群组、触发器和外观槽位。" : "这一组不播");
                return;
            }

            Report("循环播放  ·  " + clip);
        }

        void Store(bool tell)
        {
            if (_asset == null || !_asset.Owned)
            {
                if (tell) Report("这份 Spine 不在本项目里，改动留在这次预览里。");
                return;
            }

            if (ResourceLibrary.TryWriteSpineSetup(_asset, _setup.Write(), out var error)) return;
            Report(string.IsNullOrEmpty(error) ? "外观设置没有写进去。" : error);
        }

        void Report(string text)
        {
            if (NoteChanged != null) NoteChanged(text);
        }

        void Fit()
        {
            var target = _view == null ? null : _view.GameObject;
            if (target == null || _holder == null) return;
            if (!_view.Measure(null, out var x, out var y, out var width, out var height)) return;

            var pad = 12f;
            var fit = Mathf.Min((_holder.width - pad * 2f) / width, (_holder.height - pad * 2f) / height);
            if (float.IsNaN(fit) || float.IsInfinity(fit) || fit <= 0f) fit = 1f;
            var centerX = x + width * 0.5f;
            var centerY = y + height * 0.5f;
            var transform = target.transform;
            transform.localRotation = Quaternion.identity;
            transform.localScale = new Vector3(fit, fit, 1f);
            transform.localPosition = new Vector3(
                _holder.width * 0.5f - centerX * fit,
                -_holder.height * 0.5f - centerY * fit,
                0f);
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
    }
}
