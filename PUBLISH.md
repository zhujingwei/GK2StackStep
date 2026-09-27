# Nexus 发布文案（Stack Step 1.0.0）

下面是可以直接复制粘贴到 Nexus 上传页的内容。上传步骤和检查清单在最下面。

**待补**：首图。

---

## Name（Mod 名称）

```
Stack Step
```

## Summary（短简介，约 200 字符）

```
The item count window gets gamepad LB/RB to jump a whole stack at a time instead of the d-pad's fiddly ±1 stepping. Values land on clean stack boundaries, holding repeats, and a hint row shows the buttons. Chests, player inventory and vendor trades. No config needed.
```

## Category / Tags

- Category: `Gameplay`（或 `Quality of Life`，视 Nexus 上的分类而定）
- Tags: `BepInEx`, `Quality of Life`, `Inventory`, `User Interface`, `Gamepad`
- Requires: `BepInEx 5.4.x (x64)`
- Optional: 无（不依赖 GK2 Mod Framework 或任何其它 mod）

## Description（正文，BBCode）

```
[size=5][b]Stack Step[/b][/size]

The [b]item count window[/b] — the one you get when you press A on a stack of items to choose how many to move — gets a pair of coarse buttons: [b]RB adds one group, LB removes one group[/b].

[line]

[size=4][b]The problem[/b][/size]
Vanilla only gives you the d-pad in that window. Left/right is ±1 per press and speeds up the longer you hold it; up/down jumps straight to the maximum or the minimum. Landing on an exact amount with that is fiddly, and on a full chest it is the slowest part of sorting.

[size=4][b]What this mod does[/b][/size]
[list]
[*][b]RB adds one group, LB removes one group.[/b] One press is one group, no acceleration to fight.
[*][b]The value always lands on a whole group boundary[/b], so repeated presses give clean numbers — with a stack of 50 the value walks 50, 100, 150, and going down 137 → 100 → 50.
[*][b]Holding repeats.[/b] The first press jumps immediately; keep holding and it repeats every 0.2 s after a 0.6 s pause, always by exactly one group.
[*][b]Nothing to configure.[/b] No settings file, no keybinds, active as soon as the plugin loads.
[/list]

[size=4][b]What "one group" means[/b][/size]
One group is the item's [b]stack size[/b] — the number of that item the game fits into a single slot. The mod reads it per item, so it is correct for every item in the game. With a stack size of 50:

[list]
[*]1 → RB 50, LB 1 (already the minimum)
[*]50 → RB 100, LB 1
[*]51 → RB 100, LB 50
[*]137 → RB 150, LB 100
[/list]

The mod never leaves the window's own range: if the target container cannot take more, or the source does not hold enough, the value stops at the limit exactly like the d-pad does. Items that stack to 1 simply step by 1, which is all "one group" can mean for them.

[size=4][b]The hint row[/b][/size]
A small hint appears under the game's own tip row at the bottom of the window, showing both buttons:

[b][RB] +1 stack     [LB] -1 stack[/b]

[list]
[*]The bumper is [b]named for the pad you are using[/b]: LB/RB on Xbox, L1/R1 on DualShock/DualSense, L/R on Switch controllers.
[*]The wording follows the [b]game language[/b]: English, German, French, Spanish, Italian, Brazilian Portuguese, Russian, Ukrainian, Polish, Turkish and both Chinese variants. Other languages show the English wording.
[*]The hint only appears while the game is in gamepad mode — the same rule the game uses for its own gamepad tips.
[/list]

[size=4][b]Installation[/b][/size]
[list=1]
[*]Install [b]BepInEx 5.4.x (x64)[/b] into the game folder if you have not already, and run the game once so that the plugins folder is created.
[*]Drop [b]GK2StackStep.dll[/b] into [b]BepInEx/plugins/[/b].
[*]Start the game. Mod manager (Vortex) install also works — the archive already contains the BepInEx/plugins folder structure.
[/list]

[size=4][b]How to use[/b][/size]
[list=1]
[*]Open a chest, your inventory or a vendor.
[*]Press [b]A[/b] on a stack of items (a stack of 1 moves directly, as in vanilla) to open the count window.
[*]Use [b]RB / LB[/b] to jump whole stacks, the d-pad to fine-tune, [b]A[/b] to confirm and [b]B[/b] to cancel.
[/list]

[size=4][b]Troubleshooting[/b][/size]
Log file: [b]BepInEx/LogOutput.log[/b] — everything from this mod is prefixed with [b][StackStep][/b].
[list]
[*][i]v1.0.0 ready - LB removes one group, RB adds one group, ...[/i] — the mod loaded and both patches applied
[*][i]count window opened: item, one group = N, gamepad = ..., hint = ...[/i] — the window opened; [b]one group = N[/b] is the stack size being used
[*][i]hint anchored to the game's own +/- tips row[/i] — the hint was placed under the game's tip row (normal)
[*][i]game members missing: ...[/i] — the game build changed and a method/field was not found; the mod does nothing
[*][i]Update prefix failed[/i] / [i]Open postfix failed[/i] — an error inside the patch; the window falls back to vanilla behaviour
[/list]

[size=4][b]Known limits[/b][/size]
[list]
[*][b]Seeds in the vendor window are a special case.[/b] The game itself snaps seed purchases in fours, and that snapping runs on top of the group jump, so a seed boundary can be off by a few. Chests, the inventory and normal vendor goods are exact.
[*][b]The hint row sits just below the window frame[/b], because it is anchored under the game's own "+ -" tip row and the game draws that row there.
[*]No rebinding — the bumpers are read from the game's own controller mapping.
[/list]

[size=4][b]Uninstalling[/b][/size]
Delete the DLL. The mod writes nothing to your save and has no config file, so there is nothing else to clean up.

[size=4][b]Compatibility[/b][/size]
Built for Graveyard Keeper 2 (Steam) on BepInEx 5.4.23.4. Two Harmony patches, both on the quantity window: a prefix on [b]SmartSlider.Update[/b] and a postfix on [b]UIItemCountWindow.Open[/b]. [b]SmartSlider[/b] is used by UIItemCountWindow alone, so no other screen is affected. Crafting, inventories, zombies and saves are untouched. No known conflicts, load order is irrelevant, and no other mod is required.

[size=4][b]Source[/b][/size]
Source code and build instructions: https://github.com/zhujingwei/GK2StackStep (MIT licensed). Built with netstandard2.1 against the game's own assemblies.
```

## Changelog（Nexus 的 Changelog 字段）

```
1.0.0
- First release. Gamepad LB/RB in the item count window jump the quantity to the next whole stack (the item's stack size), with hold-to-repeat. Works in chests, the player inventory and vendor trades.
- A hint row under the game's own tips shows both bumpers; the bumper name follows the controller (LB/RB, L1/R1, L/R) and the wording follows the game language (12 languages translated, others fall back to English).
```

---

## 上传前检查清单

1. **主文件**：`dist/GK2StackStep-1.0.0.zip` —— **已生成**：8634 字节，SHA256 `A5F094CB80ACA7406AAF03DAA2C26E8CB4F8FC07CCF0F4FE4D5F9BD246F39B8B`。内部只有一项 `BepInEx/plugins/GK2StackStep.dll`（17408 字节，SHA256 `f0576a1028ff4fdb…b8fec5`），Vortex 可直接部署。重新打包用 `tools/makezip.ps1`（工作区那份是单文件版：`-Source` / `-EntryName`（**必须用正斜杠**）/ `-Destination`，脚本会解包回验 SHA256；若换用参数为数组的版本，注意 `powershell -File` 不会把 `a,b` 拆成数组，要改用 `-Command "& '...' -Source 'x','y'"`）。
2. **游戏页**：Graveyard Keeper 2（Steam app id 4358690，Nexus 上选游戏时不要选成一代 Graveyard Keeper）。
3. **License / permissions**：**MIT** —— 仓库里已有 `LICENSE`（建 GitHub 仓库时生成的，Copyright (c) 2026 wergel）。Nexus 上传时选 MIT。
4. **图片**：Nexus 页面首图不是必填但强烈建议（1600×900 或相近 16:9）。**可以截「数量窗口 + 底部提示行」的画面**，一眼就能说明这个 mod 干什么。需要的话可以让我生成一张标题图。
5. **Requirements 字段**：填 `BepInEx 5.4.x (x64)`（Nexus 上若已有 BepInEx 条目可直接链接；没有就写在 Description 里）。**本 mod 不需要 GK2 Mod Framework**，别误加。
6. **Related mods**：可以和 GK2AutoStationService（`GK2AutoStationService`）、Caretaker Priority 互相填 Related，但三者互不依赖、单独可用。
7. **Source 字段**：填 `https://github.com/zhujingwei/GK2StackStep`（Description 里也写了）。
8. **首次发布**：Nexus 普通 mod 上传后立即发布（仅成人/敏感内容才需审核）。上传后建议在 Description 里保留排查用的日志关键字，减少问答量。
9. **GitHub 侧可选**：给 `v1.0.0` 打 tag / 建 Release 并附上发布包，方便从源码页下载（Nexus 仍作为主要下载渠道）。
10. **发布前最后确认**：确认游戏内实机验证通过（按键跳转 + 提示行的位置/文案 + 语言 + 手柄名），再动上传物——见 [[feedback-verify-before-publishing]]。版本号保持 `1.0.0`，不要因为改文案就往上加——见 [[feedback-mod-version-policy]]。