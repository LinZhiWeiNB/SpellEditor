# Spine Runtimes

本目录中的运行时来自 Esoteric Software 的官方仓库，未改动源码：

- 源码仓库：https://github.com/EsotericSoftware/spine-runtimes
- 分支：`4.3`
- 提交：`80fc67fa18f33422fc08d13212194f97583807c5`
- 包含：`spine-unity` `4.3.123`（`Runtime`、`Editor`、`Documentation`）和其中的 `spine-csharp` `4.3.44`
- `spine-csharp` 放在 `Runtime/spine-csharp`。该目录的 `package.json` 写明许可证见同目录 `LICENSE`，因此那里也放了同一份协议全文
- 对应的 Spine 编辑器数据：`4.3.74-beta` 或 `4.3.00`（正式版）及更新版本，见 `version.txt`
- 未包含：`Samples~` 示例工程

版权归 Esoteric Software LLC（Copyright (c) 2013-2026）。许可证是 Spine Runtimes License Agreement，全文见同目录的 `LICENSE`。该文本更新日期为 2026 年 11 月 14 日，并取代此前所有版本。官方页面：

https://esotericsoftware.com/spine-runtimes-license

集成这些运行时须同时满足：

- 产品的每位用户自行持有 Spine Editor 许可证，除非集成方已获得 Spine Editor 许可证并遵守 Spine Editor License Agreement 第 2 条（https://esotericsoftware.com/spine-editor-license）。
- 对外提供的产品不能代替 Spine Editor 来创建或编辑供这些运行时使用的骨架和动画。
- 以任何形式再分发时，必须带上 `LICENSE` 和版权声明。

这里的“用户”指除了集成或评估这些运行时之外、实际使用产品的个人或实体。

不要修改本目录里的第三方源码或 `LICENSE`。升级时请整体替换为同一次官方提交，并保留许可证。
