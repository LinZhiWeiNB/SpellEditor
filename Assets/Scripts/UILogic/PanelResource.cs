using System;
using System.Collections.Generic;
using FairyGUI;
using PackageResource;
using GameContentEditor;
using UnityEngine;

namespace UILogic
{
    public class PanelResource : FguiPanelBase
    {
        sealed class Row
        {
            public bool Folder;
            public bool Source;
            public bool KindRoot;
            public int Depth;
            public string Title;
            public string MetaText;
            public string Kind;
            public string Directory;
            public bool Owned;
            public string SourcePath;
            public string ExpandId;
            public bool Expanded;
            public ResourceAsset Asset;
        }

        UI_PanelResource view;
        readonly List<Row> _rows = new List<Row>();
        readonly List<string> _files = new List<string>();
        readonly List<string> _refs = new List<string>();
        readonly HashSet<string> _open = new HashSet<string>();
        string _selectedKey = "";
        bool _selectedOwned;
        string _selectedSource = "";
        NTexture _preview;
        Texture2D _texture;
        readonly List<string> _kinds = new List<string>();
        readonly List<ResourceLibrary.SpineImportItem> _spineQueue = new List<ResourceLibrary.SpineImportItem>();
        readonly List<string> _spineNotes = new List<string>();
        int _spineIndex;
        int _spineReused;
        string _spineWarning = "";
        ResourceAsset _spineFirst;
        readonly ResourceSpinePreview _spinePreview = new ResourceSpinePreview();
        bool _layoutReady;
        bool _spineLayout;
        float _filesHeight;
        float _labelRefsY;
        float _refsY;
        float _refsHeight;
        float _labelPreviewY;
        float _frameY;
        float _frameHeight;
        bool _seeded;
        bool _editRename;
        string _editKind = "";
        string _editDir = "";
        ResourceAsset _pendingDelete;

        public override void Start(object[] data)
        {
            view = (UI_PanelResource)FGUIComponent;
            view.m_btnBack.onClick.Set(() => FGUIPanelManager.Instance.Back());
            view.m_search.text = "";
            view.m_search.onChanged.Add(Refresh);
            view.m_resources.itemRenderer = RenderRow;
            view.m_resources.onClickItem.Add(OnClick);
            view.m_files.itemRenderer = RenderFile;
            view.m_refs.itemRenderer = RenderRef;
            view.m_btnImport.onClick.Set(OpenImport);
            view.m_btnDelete.onClick.Set(DeleteSelected);
            view.m_btnMove.onClick.Set(MoveSelected);
            _spinePreview.NoteChanged = SetPreviewNote;
            ClearStatus();
            ClearSelection();
            Refresh();
            if (data != null && data.Length > 0 && data[0] is string focus && focus.Length > 0)
            {
                FocusAsset(focus);
            }
        }

        public override void OnClose()
        {
            ClearPreview();
        }

        void Refresh()
        {
            var assets = ResourceLibrary.List();
            var query = (view.m_search.text ?? "").Trim();
            BuildRows(assets, query);
            if (!_seeded)
            {
                for (var i = 0; i < _rows.Count; i++)
                {
                    if (_rows[i].Depth == 0 && _rows[i].Folder)
                    {
                        _open.Add(_rows[i].ExpandId);
                    }
                }

                _seeded = true;
                BuildRows(assets, query);
            }

            view.m_resources.numItems = _rows.Count;
            var selected = FindSelected();
            if (selected == null)
            {
                view.m_cDetail.selectedIndex = (int)UI_PanelResource.EcDetail.未选中;
                view.m_cOwn.selectedIndex = (int)UI_PanelResource.EcOwn.只读;
                ClearPreview();
            }
            else
            {
                ShowDetail(selected);
            }
        }

        void FocusAsset(string key)
        {
            var assets = ResourceLibrary.List();
            ResourceAsset asset = null;
            for (var i = 0; i < assets.Count; i++)
            {
                if (assets[i].Key == key)
                {
                    asset = assets[i];
                    break;
                }
            }

            if (asset == null)
            {
                return;
            }

            if (!asset.Owned)
            {
                _open.Add(ExpandId(false, asset.SourcePath, "", ""));
            }

            var kind = asset.Kind ?? "";
            var source = asset.SourcePath ?? "";
            _open.Add(ExpandId(asset.Owned, source, kind, ""));
            var directory = asset.Directory ?? "";
            while (directory.Length > 0)
            {
                _open.Add(ExpandId(asset.Owned, source, kind, directory));
                directory = ParentOf(directory);
            }

            _selectedKey = asset.Key;
            _selectedOwned = asset.Owned;
            _selectedSource = source;
            if ((view.m_search.text ?? "").Length > 0)
            {
                view.m_search.text = "";
            }

            Refresh();
            for (var i = 0; i < _rows.Count; i++)
            {
                var row = _rows[i];
                if (row.Asset == null || row.Asset.Key != asset.Key || row.Asset.Owned != asset.Owned)
                {
                    continue;
                }

                if ((row.Asset.SourcePath ?? "") != source)
                {
                    continue;
                }

                view.m_resources.ScrollToView(i);
                break;
            }
        }

        void BuildRows(List<ResourceAsset> assets, string query)
        {
            _rows.Clear();
            AddKindGroup(assets, query, ResourceLibrary.SpineKind, true, "", "本项目", 0);
            AddKindGroup(assets, query, ResourceLibrary.AnimationKind, true, "", "本项目", 0);
            AddKindGroup(assets, query, ResourceLibrary.ParticlesKind, true, "", "本项目", 0);
            AddKindGroup(assets, query, ResourceLibrary.AudioKind, true, "", "本项目", 0);
            AddForeignRows(assets, query);
        }

        void AddForeignRows(List<ResourceAsset> assets, string query)
        {
            var order = new List<string>();
            var titles = new Dictionary<string, string>();
            var groups = new Dictionary<string, List<ResourceAsset>>();
            for (var i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (asset.Owned)
                {
                    continue;
                }

                var key = asset.SourcePath ?? "";
                if (!groups.TryGetValue(key, out var bucket))
                {
                    bucket = new List<ResourceAsset>();
                    groups.Add(key, bucket);
                    order.Add(key);
                    titles.Add(key, string.IsNullOrEmpty(asset.SourceTitle) ? "其他数据源" : asset.SourceTitle);
                }

                bucket.Add(asset);
            }

            for (var i = 0; i < order.Count; i++)
            {
                var source = order[i];
                var bucket = groups[source];
                var title = titles[source];
                var id = ExpandId(false, source, "", "");
                var expanded = query.Length > 0 || _open.Contains(id);
                var mark = _rows.Count;
                _rows.Add(MakeFolder(title, "只读", "", "", false, source, id, expanded, 0, false, true));
                if (expanded)
                {
                    AddKindGroup(bucket, query, ResourceLibrary.SpineKind, false, source, title, 1);
                    AddKindGroup(bucket, query, ResourceLibrary.AnimationKind, false, source, title, 1);
                    AddKindGroup(bucket, query, ResourceLibrary.ParticlesKind, false, source, title, 1);
                    AddKindGroup(bucket, query, ResourceLibrary.AudioKind, false, source, title, 1);
                    AddOtherKinds(bucket, query, source, title);
                }

                if (query.Length > 0 && _rows.Count == mark + 1)
                {
                    _rows.RemoveAt(mark);
                }
            }
        }

        void AddOtherKinds(List<ResourceAsset> assets, string query, string source, string title)
        {
            var seen = new HashSet<string>();
            for (var i = 0; i < assets.Count; i++)
            {
                var kind = assets[i].Kind ?? "";
                if (KnownKind(kind) || !seen.Add(kind))
                {
                    continue;
                }

                AddKindGroup(assets, query, kind, false, source, title, 1);
            }
        }

        void AddKindGroup(List<ResourceAsset> assets, string query, string kind, bool owned, string sourcePath, string sourceTitle, int depth)
        {
            var mine = new List<ResourceAsset>();
            for (var i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (asset.Owned != owned || !KindEquals(asset.Kind, kind))
                {
                    continue;
                }

                if (!owned && (asset.SourcePath ?? "") != (sourcePath ?? ""))
                {
                    continue;
                }

                mine.Add(asset);
            }

            var folders = new List<string>();
            if (owned)
            {
                ResourceLibrary.CollectOwnFolders(kind, folders);
            }
            else
            {
                FillFolders(mine, folders);
            }

            var children = Visible(mine, folders, kind, "", depth + 1, owned, sourcePath, sourceTitle, query);
            var label = ResourceLibrary.Label(kind);
            var show = query.Length > 0
                ? children.Count > 0 || (mine.Count > 0 && ContainsText(label, query))
                : children.Count > 0 || folders.Count > 0 || mine.Count > 0 || (owned && KindEnabled(kind));
            if (!show)
            {
                return;
            }

            var id = ExpandId(owned, sourcePath, kind, "");
            var expanded = query.Length > 0 || _open.Contains(id);
            _rows.Add(MakeFolder(label, owned ? "本项目" : sourceTitle, kind, "", owned, sourcePath, id, expanded, depth, true, false));
            if (expanded)
            {
                _rows.AddRange(children);
            }
        }

        List<Row> Visible(List<ResourceAsset> mine, List<string> folders, string kind, string directory, int depth, bool owned, string sourcePath, string sourceTitle, string query)
        {
            var rows = new List<Row>();
            var subs = new List<string>();
            for (var i = 0; i < folders.Count; i++)
            {
                if (ParentOf(folders[i]) == directory)
                {
                    subs.Add(folders[i]);
                }
            }

            subs.Sort(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < subs.Count; i++)
            {
                var sub = subs[i];
                var nested = Visible(mine, folders, kind, sub, depth + 1, owned, sourcePath, sourceTitle, query);
                var name = LeafOf(sub);
                if (query.Length > 0 && nested.Count == 0 && !ContainsText(name, query) && !ContainsText(sub, query))
                {
                    continue;
                }

                var id = ExpandId(owned, sourcePath, kind, sub);
                var expanded = query.Length > 0 || _open.Contains(id);
                var meta = owned ? sub : sourceTitle + "  ·  " + sub;
                rows.Add(MakeFolder(name, meta, kind, sub, owned, sourcePath, id, expanded, depth, false, false));
                if (expanded)
                {
                    rows.AddRange(nested);
                }
            }

            var files = new List<ResourceAsset>();
            for (var i = 0; i < mine.Count; i++)
            {
                if ((mine[i].Directory ?? "") != directory)
                {
                    continue;
                }

                if (query.Length > 0 && !ResourceLibrary.Matches(mine[i], query))
                {
                    continue;
                }

                files.Add(mine[i]);
            }

            files.Sort(CompareAsset);
            for (var i = 0; i < files.Count; i++)
            {
                rows.Add(new Row
                {
                    Depth = depth,
                    Title = files[i].Name,
                    MetaText = Meta(files[i]),
                    Kind = kind,
                    Owned = files[i].Owned,
                    Asset = files[i],
                });
            }

            return rows;
        }

        static Row MakeFolder(string title, string meta, string kind, string directory, bool owned, string sourcePath, string expandId, bool expanded, int depth, bool kindRoot, bool source)
        {
            return new Row
            {
                Folder = true,
                Source = source,
                KindRoot = kindRoot,
                Depth = depth,
                Title = title,
                MetaText = meta,
                Kind = kind,
                Directory = directory ?? "",
                Owned = owned,
                SourcePath = sourcePath ?? "",
                ExpandId = expandId,
                Expanded = expanded,
            };
        }

        void RenderRow(int index, GObject obj)
        {
            var row = _rows[index];
            var item = (UI_ItemResource)obj;
            var indent = 12f + row.Depth * 16f;
            var title = item.GetChild("title");
            if (title != null)
            {
                title.x = indent;
                title.width = Math.Max(80f, 456f - indent);
            }

            var meta = item.GetChild("meta");
            if (meta != null)
            {
                meta.x = indent;
                meta.width = Math.Max(80f, 464f - indent);
            }

            item.title = row.Folder ? (row.Expanded ? "▾ " : "▸ ") + row.Title : row.Title;
            item.m_meta.text = row.MetaText ?? "";
            item.m_cTone.selectedIndex = row.Owned ? 0 : 1;
            item.selected = row.Asset != null && Same(row.Asset);
            item.data = row;
            item.draggable = row.Asset != null && row.Asset.Owned;
            item.onDragStart.Set(OnDragAsset);
            item.onDrop.Set(OnDropAsset);
            item.onRightClick.Set(OnFolderMenu);
        }

        void OnClick(EventContext context)
        {
            if (context.inputEvent != null && context.inputEvent.button == 1)
            {
                return;
            }

            var index = view.m_resources.GetChildIndex((GObject)context.data);
            if (index < 0 || index >= _rows.Count)
            {
                return;
            }

            var row = _rows[index];
            if (row.Folder)
            {
                if (!_open.Add(row.ExpandId))
                {
                    _open.Remove(row.ExpandId);
                }

                Refresh();
                return;
            }

            if (row.Asset == null)
            {
                return;
            }

            var asset = row.Asset;
            _selectedKey = asset.Key;
            _selectedOwned = asset.Owned;
            _selectedSource = asset.SourcePath ?? "";
            ShowDetail(asset);
            view.m_resources.numItems = _rows.Count;
        }

        void OnDragAsset(EventContext context)
        {
            var row = (context.sender as GObject)?.data as Row;
            if (row == null || row.Asset == null || !row.Asset.Owned)
            {
                return;
            }

            context.PreventDefault();
            var touch = context.inputEvent == null ? -1 : context.inputEvent.touchId;
            DragDropManager.inst.StartDrag((GObject)context.sender, "", row.Asset, touch);
        }

        void OnDropAsset(EventContext context)
        {
            var row = (context.sender as GObject)?.data as Row;
            var asset = context.data as ResourceAsset;
            if (row == null || !row.Folder || !row.Owned || row.Source || asset == null || !asset.Owned)
            {
                return;
            }

            if (!KindEquals(asset.Kind, row.Kind))
            {
                Status("只能拖到同类型的目录。", true);
                return;
            }

            if ((asset.Directory ?? "") == (row.Directory ?? ""))
            {
                return;
            }

            if (!ResourceLibrary.Move(asset, row.Directory, out var newKey, out var error))
            {
                Status(error, true);
                return;
            }

            _selectedKey = newKey;
            _selectedOwned = true;
            _selectedSource = "";
            _open.Add(row.ExpandId);
            var where = row.Directory.Length == 0 ? "根目录" : row.Directory;
            Status("已移动到 " + where + "。", false);
            Refresh();
        }

        void OnFolderMenu(EventContext context)
        {
            var row = (context.sender as GObject)?.data as Row;
            if (row == null || !row.Folder || !row.Owned || row.Source)
            {
                return;
            }

            context.StopPropagation();
            if (!PanelFolderMenu.Show(new PanelFolderMenu.Request
            {
                Rename = !row.KindRoot,
                Create = true,
                Remove = !row.KindRoot,
                OnRename = () => BeginRename(row),
                OnCreate = () => BeginCreate(row),
                OnRemove = () => BeginDeleteFolder(row),
            }))
            {
                Status("请重新发布 PackageResource 后再管理目录。", true);
            }
        }

        void BeginRename(Row row)
        {
            if (row == null || row.KindRoot)
            {
                return;
            }

            _editRename = true;
            _editKind = row.Kind;
            _editDir = row.Directory;
            if (!PanelPickPath.Show(new PanelPickPath.Request
            {
                Title = "重命名目录",
                Hint = row.Directory,
                Text = LeafOf(row.Directory),
                Prompt = "新的目录名",
                Confirm = "重命名",
                Commit = ApplyFolder,
            }))
            {
                Status("请重新发布 PackageResource 后再管理目录。", true);
            }
        }

        void BeginCreate(Row row)
        {
            if (row == null)
            {
                return;
            }

            _editRename = false;
            _editKind = row.Kind;
            _editDir = row.Directory ?? "";
            if (!PanelPickPath.Show(new PanelPickPath.Request
            {
                Title = "新建子目录",
                Hint = _editDir.Length == 0 ? "建在该类型的根目录下。" : "建在 " + _editDir + " 下。",
                Text = "",
                Prompt = "目录名",
                Confirm = "创建",
                Commit = ApplyFolder,
            }))
            {
                Status("请重新发布 PackageResource 后再管理目录。", true);
            }
        }

        void BeginDeleteFolder(Row row)
        {
            if (row == null || row.KindRoot)
            {
                return;
            }

            _pendingDelete = null;
            _editKind = row.Kind;
            _editDir = row.Directory;
            var count = CountUnder(row.Kind, row.Directory);
            var refs = ReferencesUnder(row.Kind, row.Directory);
            if (!PanelPickConfirm.Show(new PanelPickConfirm.Request
            {
                Title = "删除目录",
                Hint = count == 0
                    ? "这个目录是空的。"
                    : refs == 0
                        ? "同时删除其中的 " + count + " 个资源。"
                        : "同时删除其中的 " + count + " 个资源，还有 " + refs + " 处引用。",
                Confirm = "删除",
                Commit = ConfirmDeleteFolder,
            }))
            {
                Status("请重新发布 PackageResource 后再管理目录。", true);
            }
        }

        string ApplyFolder(string name)
        {
            bool ok;
            string directory;
            string error;
            if (_editRename)
            {
                ok = ResourceLibrary.RenameFolder(_editKind, _editDir, name, out directory, out error);
            }
            else
            {
                ok = ResourceLibrary.CreateFolder(_editKind, _editDir, name, out directory, out error);
            }

            if (!ok)
            {
                return error ?? "没有改成。";
            }

            if (_editRename)
            {
                MoveOpen(_editKind, _editDir, directory);
                if (_selectedOwned && UnderDirectory(_selectedKey, _editDir))
                {
                    _selectedKey = directory + _selectedKey.Substring(_editDir.Length);
                }
            }
            else
            {
                _open.Add(ExpandId(true, "", _editKind, _editDir));
                _open.Add(ExpandId(true, "", _editKind, directory));
            }

            Status(_editRename ? "目录已改为 " + directory + "。" : "已新建 " + directory + "。", false);
            Refresh();
            return null;
        }

        string ConfirmDeleteFolder()
        {
            if (!ResourceLibrary.DeleteFolder(_editKind, _editDir, out var removed, out var error))
            {
                return error ?? "没有删掉。";
            }

            ForgetOpen(_editKind, _editDir);
            if (_selectedOwned && UnderDirectory(_selectedKey, _editDir))
            {
                _selectedKey = "";
                _selectedOwned = false;
                _selectedSource = "";
            }

            Status(removed == 0 ? "已删除目录。" : "已删除目录，其中 " + removed + " 个资源。", false);
            Refresh();
            return null;
        }

        int CountUnder(string kind, string directory)
        {
            var assets = ResourceLibrary.List();
            var count = 0;
            for (var i = 0; i < assets.Count; i++)
            {
                if (assets[i].Owned && KindEquals(assets[i].Kind, kind) && UnderDirectory(assets[i].Key, directory))
                {
                    count++;
                }
            }

            return count;
        }

        int ReferencesUnder(string kind, string directory)
        {
            var assets = ResourceLibrary.List();
            var count = 0;
            for (var i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (asset.Owned && KindEquals(asset.Kind, kind) && UnderDirectory(asset.Key, directory))
                {
                    count += ResourceLibrary.ReferenceCount(asset);
                }
            }

            return count;
        }

        void MoveOpen(string kind, string oldDir, string newDir)
        {
            var head = "own|" + kind + "|";
            var next = new List<string>();
            var drop = new List<string>();
            foreach (var id in _open)
            {
                if (!id.StartsWith(head, StringComparison.Ordinal))
                {
                    continue;
                }

                var rest = id.Substring(head.Length);
                if (rest != oldDir && !rest.StartsWith(oldDir + "/", StringComparison.Ordinal))
                {
                    continue;
                }

                drop.Add(id);
                next.Add(head + newDir + rest.Substring(oldDir.Length));
            }

            for (var i = 0; i < drop.Count; i++)
            {
                _open.Remove(drop[i]);
                _open.Add(next[i]);
            }
        }

        void ForgetOpen(string kind, string directory)
        {
            var head = "own|" + kind + "|";
            var drop = new List<string>();
            foreach (var id in _open)
            {
                if (!id.StartsWith(head, StringComparison.Ordinal))
                {
                    continue;
                }

                var rest = id.Substring(head.Length);
                if (rest == directory || rest.StartsWith(directory + "/", StringComparison.Ordinal))
                {
                    drop.Add(id);
                }
            }

            for (var i = 0; i < drop.Count; i++)
            {
                _open.Remove(drop[i]);
            }
        }

        void ShowDetail(ResourceAsset asset)
        {
            view.m_cDetail.selectedIndex = (int)UI_PanelResource.EcDetail.已选中;
            view.m_cOwn.selectedIndex = asset.Owned
                ? (int)UI_PanelResource.EcOwn.本源
                : (int)UI_PanelResource.EcOwn.只读;
            view.m_valueName.text = asset.Name;
            view.m_valueKind.text = asset.KindLabel;
            view.m_valueSource.text = asset.Owned ? "本项目" : asset.SourceTitle;
            var folder = asset.Directory.Length == 0 ? "（类别根目录）" : asset.Directory;
            view.m_valueFolder.text = folder;
            view.m_inputFolder.text = asset.Directory;
            var lines = ResourceLibrary.DetailLines(asset);
            if (lines.Count == 0)
            {
                lines.Add(asset.Owned ? "没有文件。" : "数据包条目  ·  " + asset.Length + " 字节");
            }

            view.m_labelFiles.text = ResourceLibrary.IsBundle(asset.Kind) ? "预制体" : "文件";
            Fill(view.m_files, lines);
            Fill(view.m_refs, ResourceLibrary.DetailReferences(asset));
            ShowPreview(asset);
        }

        void Fill(GList list, List<string> lines)
        {
            _files.Clear();
            if (list == view.m_refs)
            {
                _refs.Clear();
                _refs.AddRange(lines);
                list.numItems = _refs.Count;
                return;
            }

            _files.AddRange(lines);
            list.numItems = _files.Count;
        }

        void RenderFile(int index, GObject obj)
        {
            ((UI_ItemResourceLine)obj).m_title.text = _files[index];
        }

        void RenderRef(int index, GObject obj)
        {
            ((UI_ItemResourceLine)obj).m_title.text = _refs[index];
        }

        void ShowPreview(ResourceAsset asset)
        {
            ClearPreview();
            if (ResourceLibrary.IsBundle(asset.Kind))
            {
                view.m_cPreview.selectedIndex = (int)UI_PanelResource.EcPreview.无;
                view.m_previewHint.text = "预制体列在上面。技能里的键是包路径加预制体名。";
                view.m_previewNote.text = "";
                return;
            }

            if (ResourceLibrary.IsAudio(asset.Kind))
            {
                view.m_cPreview.selectedIndex = (int)UI_PanelResource.EcPreview.无;
                view.m_previewHint.text = "音频文件。技能里的片段键就是这条资源的路径。";
                view.m_previewNote.text = asset.Key;
                return;
            }

            if (ResourceLibrary.IsSpine(asset.Kind))
            {
                ShowSpine(asset);
                return;
            }

            var png = ResourceLibrary.FirstPng(asset, out var pages);
            if (png == null || png.Length == 0)
            {
                view.m_cPreview.selectedIndex = (int)UI_PanelResource.EcPreview.无;
                view.m_previewHint.text = "这个资源没有可显示的预览。";
                view.m_previewNote.text = "";
                return;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(png))
            {
                UnityEngine.Object.Destroy(texture);
                view.m_cPreview.selectedIndex = (int)UI_PanelResource.EcPreview.无;
                view.m_previewHint.text = "这张贴图没有读出来。";
                view.m_previewNote.text = "";
                return;
            }

            _texture = texture;
            _preview = new NTexture(_texture);
            view.m_preview.texture = _preview;
            view.m_cPreview.selectedIndex = (int)UI_PanelResource.EcPreview.有;
            view.m_previewNote.text = pages > 1 ? "共 " + pages + " 张贴图，显示第一张。" : "";
        }

        void ShowSpine(ResourceAsset asset)
        {
            if (!ResourceLibrary.TryOpenSpine(asset, out var document, out var error))
            {
                Hint(error ?? "这份 Spine 没有读出来。");
                return;
            }

            ApplySpineLayout();
            if (!_spinePreview.Show(view, null, view.m_previewFrame, asset, document, out error))
            {
                RestoreLayout();
                Hint(error ?? "这份 Spine 没有读出来。");
                return;
            }

            view.m_cPreview.selectedIndex = (int)UI_PanelResource.EcPreview.有;
            view.m_preview.visible = false;
        }

        void Hint(string text)
        {
            view.m_cPreview.selectedIndex = (int)UI_PanelResource.EcPreview.无;
            view.m_previewHint.text = text ?? "";
            view.m_previewNote.text = "";
        }

        void SetPreviewNote(string text)
        {
            if (view != null && view.m_previewNote != null)
            {
                view.m_previewNote.text = text ?? "";
            }
        }

        void RememberLayout()
        {
            if (_layoutReady)
            {
                return;
            }

            _layoutReady = true;
            _filesHeight = view.m_files.height;
            _labelRefsY = view.m_labelRefs.y;
            _refsY = view.m_refs.y;
            _refsHeight = view.m_refs.height;
            _labelPreviewY = view.m_labelPreview.y;
            _frameY = view.m_previewFrame.y;
            _frameHeight = view.m_previewFrame.height;
        }

        void ApplySpineLayout()
        {
            RememberLayout();
            if (_spineLayout)
            {
                return;
            }

            _spineLayout = true;
            var fileCut = Mathf.Min(68f, Mathf.Max(0f, _filesHeight - 72f));
            var refCut = Mathf.Min(36f, Mathf.Max(0f, _refsHeight - 48f));
            var lift = fileCut + refCut;
            view.m_files.height = _filesHeight - fileCut;
            view.m_labelRefs.y = _labelRefsY - fileCut;
            view.m_refs.y = _refsY - fileCut;
            view.m_refs.height = _refsHeight - refCut;
            view.m_labelPreview.y = _labelPreviewY - lift;
            view.m_previewFrame.y = _frameY - lift;
            view.m_previewFrame.height = _frameHeight + lift;
        }

        void RestoreLayout()
        {
            if (!_layoutReady || !_spineLayout)
            {
                return;
            }

            _spineLayout = false;
            view.m_files.height = _filesHeight;
            view.m_labelRefs.y = _labelRefsY;
            view.m_refs.y = _refsY;
            view.m_refs.height = _refsHeight;
            view.m_labelPreview.y = _labelPreviewY;
            view.m_previewFrame.y = _frameY;
            view.m_previewFrame.height = _frameHeight;
        }

        void ClearPreview()
        {
            RestoreLayout();
            _spinePreview.Clear();
            if (view != null && view.m_preview != null)
            {
                view.m_preview.visible = true;
                view.m_preview.texture = null;
            }

            if (_preview != null)
            {
                _preview.Dispose();
                _preview = null;
            }

            if (_texture != null)
            {
                UnityEngine.Object.Destroy(_texture);
                _texture = null;
            }
        }

        void OpenImport()
        {
            _kinds.Clear();
            if (ResourceLibrary.SpineImportEnabled())
            {
                _kinds.Add("Spine");
            }

            if (ResourceLibrary.AnimationImportEnabled())
            {
                _kinds.Add("动画");
            }

            if (ResourceLibrary.ParticlesImportEnabled())
            {
                _kinds.Add("粒子");
            }

            _kinds.Add("音频");
            if (!PanelPickList.Show(new PanelPickList.Request
            {
                Title = "导入资源",
                Hint = "按当前项目设置列出。",
                Items = new List<string>(_kinds),
                OnPick = ImportKind,
            }))
            {
                Status("请重新发布 PackageResource 后再导入。", true);
            }
        }

        void ImportKind(string kind)
        {
            if (kind == "Spine")
            {
                ImportSpine();
            }
            else if (kind == "动画")
            {
                ImportAnimation();
            }
            else if (kind == "粒子")
            {
                ImportParticles();
            }
            else
            {
                ImportAudio();
            }
        }

        void CancelSpineImport()
        {
            _pendingDelete = null;
            _spineQueue.Clear();
            if (_spineNotes.Count == 0)
            {
                Status("已取消导入。", false);
                return;
            }

            Status(SpineSummary() + " 其余已取消。", false);
            Refresh();
        }

        void ImportSpine()
        {
            var folder = EditorDialog.OpenFolder("选择 Spine 导出目录");
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            if (!ResourceLibrary.TryPlanSpineImport(folder, out var items, out var warning, out var error))
            {
                Status(error ?? "没有导入。", true);
                return;
            }

            _spineQueue.Clear();
            _spineQueue.AddRange(items);
            _spineNotes.Clear();
            _spineIndex = 0;
            _spineReused = 0;
            _spineWarning = warning ?? "";
            _spineFirst = null;
            ContinueSpine();
        }

        void ContinueSpine()
        {
            while (_spineIndex < _spineQueue.Count)
            {
                var item = _spineQueue[_spineIndex];
                var existing = ResourceLibrary.SameNameKeys(item.Stem);
                if (existing.Count > 0)
                {
                    ShowSameName(item, existing);
                    return;
                }

                if (!AcceptSpine(item.SuggestedKey, false))
                {
                    return;
                }
            }

            if (_spineFirst != null)
            {
                _selectedKey = _spineFirst.Key;
                _selectedOwned = true;
                _selectedSource = "";
            }

            Status(SpineSummary(), _spineWarning.Length > 0);
            _spineQueue.Clear();
            Refresh();
        }

        void ShowSameName(ResourceLibrary.SpineImportItem item, List<string> existing)
        {
            var slash = item.SuggestedKey.LastIndexOf('/');
            var request = new PanelPickName.Request
            {
                Title = "同名骨骼 " + item.Stem,
                Hint = "点一条覆盖该路径。填写新路径则另外保留一份。",
                Items = new List<string>(existing),
                Path = slash < 0 ? "" : item.SuggestedKey.Substring(0, slash),
                Prompt = "新路径，例如 heroes/player",
                Confirm = "使用新路径",
                OnPick = key =>
                {
                    if (AcceptSpine(key, true))
                    {
                        ContinueSpine();
                    }
                },
                OnCancel = CancelSpineImport,
            };
            request.OnPlace = path => PlaceSpine(request, path);
            if (!PanelPickName.Show(request))
            {
                Status("请重新发布 PackageResource 后再导入。", true);
            }
        }

        string PlaceSpine(PanelPickName.Request request, string path)
        {
            if (_spineIndex < 0 || _spineIndex >= _spineQueue.Count)
            {
                return "这一份已经不在导入队列里。";
            }

            if (!ResourceLibrary.TrySpineKey(_spineQueue[_spineIndex].Stem, path, out var key, out var error))
            {
                return error ?? "路径没有生效。";
            }

            var existed = false;
            var names = ResourceLibrary.SameNameKeys(_spineQueue[_spineIndex].Stem);
            for (var i = 0; i < names.Count; i++)
            {
                if (string.Equals(names[i], key, StringComparison.OrdinalIgnoreCase))
                {
                    existed = true;
                    break;
                }
            }

            if (AcceptSpine(key, existed))
            {
                request.After = ContinueSpine;
            }

            return null;
        }

        bool AcceptSpine(string key, bool overwrite)
        {
            if (_spineIndex < 0 || _spineIndex >= _spineQueue.Count)
            {
                return false;
            }

            var item = _spineQueue[_spineIndex];
            var asset = ResourceLibrary.CommitSpine(item, key, out var reused, out var error);
            if (asset == null)
            {
                Status(error ?? "没有导入。", true);
                _spineQueue.Clear();
                return false;
            }

            _spineReused += reused;
            if (_spineFirst == null)
            {
                _spineFirst = asset;
            }

            var recorded = (overwrite ? "覆盖 " : "导入 ") + item.Label + " → " + asset.Key;
            var pluginNote = ImportAutomation.Apply(asset, item.SkeletonPath);
            if (pluginNote.Length > 0)
            {
                recorded += "（" + pluginNote + "）";
            }

            _spineNotes.Add(recorded);
            _spineIndex++;
            return true;
        }

        string SpineSummary()
        {
            var summary = _spineNotes.Count == 1
                ? "已处理 " + _spineNotes[0] + "。"
                : "已处理 " + _spineNotes.Count + " 个骨骼：" + string.Join("；", _spineNotes) + "。";
            if (_spineReused > 0)
            {
                summary += " 其中 " + _spineReused + " 张贴图项目里已经有了。";
            }

            if (!string.IsNullOrEmpty(_spineWarning))
            {
                summary += " 未导入：" + _spineWarning + "。";
            }

            return summary;
        }

        void ImportAnimation()
        {
            ImportPicked("选择动画 AssetBundle", true, ResourceLibrary.ImportAnimation);
        }

        void ImportParticles()
        {
            ImportPicked("选择粒子 AssetBundle", true, ResourceLibrary.ImportParticles);
        }

        void ImportAudio()
        {
            ImportPicked("选择音频", false, ResourceLibrary.ImportAudio);
        }

        delegate ResourceAsset FileImporter(string path, out string error);

        void ImportPicked(string title, bool bundle, FileImporter import)
        {
            var path = bundle ? EditorDialog.OpenAssetBundle(title) : EditorDialog.OpenAudioFile(title);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var asset = import(path, out var error);
            if (asset == null)
            {
                Status(error ?? "没有导入。", true);
                return;
            }

            _selectedKey = asset.Key;
            _selectedOwned = true;
            _selectedSource = "";
            var recorded = "已导入 " + asset.Name + "，放在项目目录的 " + ResourceLibrary.StorageName(asset) + "/" + asset.Key + "。";
            var pluginNote = ImportAutomation.Apply(asset, path);
            if (pluginNote.Length > 0)
            {
                recorded += pluginNote;
            }

            Status(recorded, false);
            Refresh();
        }

        void DeleteSelected()
        {
            var asset = FindSelected();
            if (asset == null || !asset.Owned)
            {
                return;
            }

            var refs = ResourceLibrary.ReferenceCount(asset);
            if (refs == 0)
            {
                if (!ResourceLibrary.Delete(asset, out var error))
                {
                    Status(error, true);
                    return;
                }

                NoteDeleted(asset, 0);
                return;
            }

            _pendingDelete = asset;
            if (!PanelPickConfirm.Show(new PanelPickConfirm.Request
            {
                Title = "删除资源",
                Hint = "还有 " + refs + " 处引用，删除后仍指向原来的键。",
                Confirm = "删除",
                Commit = ConfirmDeleteAsset,
            }))
            {
                Status("请重新发布 PackageResource 后再删除。", true);
            }
        }

        string ConfirmDeleteAsset()
        {
            var asset = _pendingDelete;
            if (asset == null)
            {
                return null;
            }

            var refs = ResourceLibrary.ReferenceCount(asset);
            if (!ResourceLibrary.Delete(asset, out var error))
            {
                return error ?? "没有删掉。";
            }

            _pendingDelete = null;
            NoteDeleted(asset, refs);
            return null;
        }

        void NoteDeleted(ResourceAsset asset, int refs)
        {
            var name = asset.Name;
            _selectedKey = "";
            ClearSelection();
            Status(refs == 0
                ? "已删除 " + name + "。"
                : "已删除 " + name + "。还有 " + refs + " 处引用指向原来的键。", refs > 0);
            Refresh();
        }

        void MoveSelected()
        {
            var asset = FindSelected();
            if (asset == null || !asset.Owned)
            {
                return;
            }

            if (!ResourceLibrary.Move(asset, view.m_inputFolder.text, out var newKey, out var error))
            {
                Status(error, true);
                return;
            }

            _selectedKey = newKey;
            Status(newKey == asset.Key ? "目录没有变化。" : "已移动到 " + ResourceLibrary.StorageName(asset) + "/" + newKey + "。", false);
            Refresh();
        }

        void ClearSelection()
        {
            _selectedKey = "";
            _selectedOwned = false;
            _selectedSource = "";
            view.m_cDetail.selectedIndex = (int)UI_PanelResource.EcDetail.未选中;
            view.m_cOwn.selectedIndex = (int)UI_PanelResource.EcOwn.只读;
            ClearPreview();
        }

        ResourceAsset FindSelected()
        {
            if (_selectedKey.Length == 0)
            {
                return null;
            }

            for (var i = 0; i < _rows.Count; i++)
            {
                var asset = _rows[i].Asset;
                if (asset != null && Same(asset))
                {
                    return asset;
                }
            }

            return null;
        }

        bool Same(ResourceAsset asset)
        {
            return asset.Owned == _selectedOwned
                   && asset.Key == _selectedKey
                   && (asset.SourcePath ?? "") == _selectedSource;
        }

        static string Meta(ResourceAsset asset)
        {
            var source = asset.Owned ? "本项目" : asset.SourceTitle;
            if (ResourceLibrary.IsBundle(asset.Kind))
            {
                var text = source + "  ·  预制体 " + ResourceLibrary.EntryCount(asset);
                return asset.Directory.Length == 0 ? text : text + "  ·  " + asset.Directory;
            }

            if (asset.Directory.Length == 0)
            {
                return source;
            }

            return source + "  ·  " + asset.Directory;
        }

        static int CompareAsset(ResourceAsset left, ResourceAsset right)
        {
            if (left.Owned != right.Owned)
            {
                return left.Owned ? -1 : 1;
            }

            var source = string.Compare(left.SourceTitle, right.SourceTitle, StringComparison.OrdinalIgnoreCase);
            if (source != 0)
            {
                return source;
            }

            return string.Compare(left.Key, right.Key, StringComparison.OrdinalIgnoreCase);
        }

        static void FillFolders(List<ResourceAsset> mine, List<string> folders)
        {
            folders.Clear();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < mine.Count; i++)
            {
                var dir = mine[i].Directory ?? "";
                while (dir.Length > 0 && seen.Add(dir))
                {
                    folders.Add(dir);
                    dir = ParentOf(dir);
                }
            }
        }

        static string ParentOf(string directory)
        {
            var slash = (directory ?? "").LastIndexOf('/');
            return slash < 0 ? "" : directory.Substring(0, slash);
        }

        static string LeafOf(string directory)
        {
            var slash = (directory ?? "").LastIndexOf('/');
            return slash < 0 ? directory ?? "" : directory.Substring(slash + 1);
        }

        static string ExpandId(bool owned, string source, string kind, string directory)
        {
            if (!owned && string.IsNullOrEmpty(kind))
            {
                return "src|" + (source ?? "");
            }

            return (owned ? "own|" : "frn|" + (source ?? "") + "|") + (kind ?? "") + "|" + (directory ?? "");
        }

        static bool UnderDirectory(string key, string directory)
        {
            return !string.IsNullOrEmpty(directory)
                   && (key ?? "").StartsWith(directory + "/", StringComparison.Ordinal);
        }

        static bool ContainsText(string text, string query)
        {
            return (text ?? "").IndexOf(query ?? "", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool KnownKind(string kind)
        {
            return ResourceLibrary.IsSpine(kind)
                   || ResourceLibrary.IsAnimation(kind)
                   || ResourceLibrary.IsParticles(kind)
                   || ResourceLibrary.IsAudio(kind);
        }

        static bool KindEnabled(string kind)
        {
            if (ResourceLibrary.IsSpine(kind))
            {
                return ResourceLibrary.SpineImportEnabled();
            }

            if (ResourceLibrary.IsAnimation(kind))
            {
                return ResourceLibrary.AnimationImportEnabled();
            }

            if (ResourceLibrary.IsParticles(kind))
            {
                return ResourceLibrary.ParticlesImportEnabled();
            }

            return ResourceLibrary.IsAudio(kind);
        }

        static bool KindEquals(string assetKind, string kind)
        {
            if (ResourceLibrary.IsSpine(kind))
            {
                return ResourceLibrary.IsSpine(assetKind);
            }

            if (ResourceLibrary.IsAnimation(kind))
            {
                return ResourceLibrary.IsAnimation(assetKind);
            }

            if (ResourceLibrary.IsParticles(kind))
            {
                return ResourceLibrary.IsParticles(assetKind);
            }

            if (ResourceLibrary.IsAudio(kind))
            {
                return ResourceLibrary.IsAudio(assetKind);
            }

            return string.Equals(assetKind, kind, StringComparison.Ordinal);
        }

        void Status(string text, bool warning)
        {
            view.m_status.text = text ?? "";
            view.m_cMessage.selectedIndex = string.IsNullOrEmpty(text)
                ? (int)UI_PanelResource.EcMessage.空
                : warning
                    ? (int)UI_PanelResource.EcMessage.警告
                    : (int)UI_PanelResource.EcMessage.提示;
        }

        void ClearStatus()
        {
            view.m_status.text = "";
            view.m_cMessage.selectedIndex = (int)UI_PanelResource.EcMessage.空;
        }
    }
}
