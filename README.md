# Stack Step (Graveyard Keeper 2)

Gamepad **LB/RB** in the item count window move the quantity by a whole stack at a time — one press lands on the next stack boundary — instead of the d-pad's one-per-press stepping with an accelerating hold.

手柄在数量选择界面按 **LB/RB** 可以**一次跳一整组**（一次按就落到下一个整组数量上），不用再忍受十字键「点一下 ±1、长按越来越快」那种不精确的加减。

- Plugin GUID: `com.gk2mod.stackstep`
- Version: 1.0.0
- Requires: BepInEx 5.4.x (x64) for Graveyard Keeper 2
- Single file: `BepInEx/plugins/GK2StackStep.dll` — no config, no keybinds, no extra folders
- No other mod required (GK2 Mod Framework is **not** needed)

---

## What it does / 功能

The item count window (opening a chest, the player inventory, vendor trades) is the window you get when you press A on a stack of items to choose how many to move. Vanilla only offers the d-pad: left/right is ±1 per press and speeds up while held, up/down jumps straight to the maximum or minimum. Hitting an exact amount with that is fiddly.

数量选择界面（开箱子、整理背包、跟商人买卖时按 A 选数量那个窗口）原版只能用十字键：左右是「按一下 ±1、按住逐渐加速」，上下则是直接拉满或减到最小。想凑一个刚好的数字很难。

This mod adds a coarser pair of buttons:

本 mod 补上一对"大步"按键：

- **RB adds one group, LB removes one group** (the buttons next to the d-pad).
- **The value always lands on a whole group boundary**, so repeated presses give clean numbers instead of drifting.

- **RB 加一组，LB 减一组**（就是十字键旁边那两个肩键）。
- **结果永远落在整组的整数倍上**，连按得到的是干净数字，不会越走越偏。

### What "one group" means / 「一组」是什么

One group is the item's **stack size** — the number the game uses to decide how many of that item fit in a single slot. The mod reads it per item, so it is right for every item in the game.

一组 = 该物品的**堆叠上限**，也就是游戏里"一格最多放多少个"的那个数。mod 会按当前物品实时取值，对所有物品都成立。

With a stack size of 50 the value walks like this:

以堆叠上限 50 为例：

| Current / 当前 | RB (add / 加) | LB (remove / 减) |
| --- | --- | --- |
| 1 | 50 | 1 (already the minimum / 已是最小) |
| 37 | 50 | 1 |
| 50 | 100 | 1 |
| 51 | 100 | 50 |
| 100 | 150 | 50 |
| 137 | 150 | 100 |

The mod never goes outside the window's own range — if the target container cannot take more, or there are not enough items left, the value stops at the limit exactly like the d-pad does. Items with a stack size of 1 behave like the d-pad's ±1, which is the best "one group" can mean for them.

mod 不会超出窗口本身的范围：目标容器装不下、或源容器不够时，数值会停在和十字键一样的位置。堆叠上限为 1 的物品每次 ±1，这已经是它「一组」的全部含义。

### Holding the button / 长按

The first press jumps immediately; keep holding and it repeats every 0.2 s after a 0.6 s pause, moving one group per repeat. The step stays constant, so long presses stay predictable.

按一下立刻跳一次；按住不放则在 0.6 秒后开始每 0.2 秒再跳一组，每次都是一组的量，不会像原版那样越跳越快。

## The hint row / 按键提示

The window gets a small hint showing the two buttons and what they do, drawn right under the game's own tip row at the bottom:

窗口底部会多出一行小提示，显示这两个按键的作用，位置在游戏自己那行提示的下面：

```
[RB] +1 stack   [LB] -1 stack
```

- **The bumper is named for the pad you are using**: `LB`/`RB` on Xbox, `L1`/`R1` on DualShock/DualSense, `L`/`R` on Switch controllers.
- **The wording follows the game language.** Translated: English, German, French, Spanish, Italian, Brazilian Portuguese, Russian, Ukrainian, Polish, Turkish, Simplified and Traditional Chinese. Other languages (Japanese, Korean, Thai, Vietnamese) show the English wording.
- The hint only appears while the game is in gamepad mode, matching how the game hides its own gamepad tips.

- **按键名会跟着你用的手柄变**：Xbox 是 `LB`/`RB`，DualShock/DualSense 是 `L1`/`R1`，Switch 系列是 `L`/`R`。
- **文案跟随游戏语言**：已翻译英语、德语、法语、西班牙语、意大利语、巴西葡语、俄语、乌克兰语、波兰语、土耳其语、简繁中文；其余语言（日、韩、泰、越）显示英文。
- 提示只在游戏处于手柄模式时出现——和游戏自己隐藏手柄提示的逻辑一致。

## Installation / 安装

1. Install **BepInEx 5.4.x (x64)** into the game folder (Steam → Graveyard Keeper 2 → right click → Manage → Browse local files) if you have not already, and run the game once so that `BepInEx/plugins` is created.
2. Drop `GK2StackStep.dll` into `BepInEx/plugins/`.
3. Start the game.

Vortex: install the archive as-is; it contains the `BepInEx/plugins/` folder structure.

中文：装好 BepInEx 5.4 x64（没装的话先运行一次游戏让 `BepInEx/plugins` 生成），把 `GK2StackStep.dll` 放进 `BepInEx/plugins/` 即可；Vortex 可直接安装压缩包。

## How to use / 怎么用

1. Open a chest, your inventory, or a vendor.
2. Press **A** on a stack of items (a stack of 1 just moves directly, as in vanilla) to open the count window.
3. Use **RB / LB** to jump whole stacks, the d-pad to fine-tune, **A** to confirm and **B** to cancel.

There is nothing to configure — the mod is active as soon as the plugin loads.

1. 打开箱子、背包或商人界面。
2. 在某个物品堆上按 **A** 打开数量窗口（数量为 1 的物品和原版一样直接搬走）。
3. 用 **RB / LB** 按整组跳，用十字键微调，**A** 确定、**B** 取消。

不用配置任何东西，插件加载即生效。

## Uninstalling / 卸载

Delete `BepInEx/plugins/GK2StackStep.dll`. Nothing else to clean up: the mod writes nothing to your save and has no config file. Items already moved stay moved, obviously.

删掉 `BepInEx/plugins/GK2StackStep.dll` 即可。没有别的残留：mod 不往存档里写任何东西，也没有配置文件。已经搬走的物品自然保持原样。

## Troubleshooting / 排查

Everything from this mod goes to `BepInEx/LogOutput.log` with the `[StackStep]` prefix.

日志在 `BepInEx/LogOutput.log`，本 mod 的输出都带 `[StackStep]` 前缀。

| Line | Meaning |
| --- | --- |
| `[StackStep] v1.0.0 ready - LB removes one group, RB adds one group, ...` | The mod loaded and both patches were applied |
| `[StackStep] count window opened: <item>, one group = <n>, gamepad = <type>, hint = <text>` | The count window opened; `one group = <n>` is the stack size the mod is using |
| `[StackStep] hint anchored to the game's own +/- tips row: ...` | The hint was placed under the game's tip row (normal) |
| `[StackStep] hint anchored to the ok/cancel row: ...` | The game's tip row was not found; the hint fell back to the buttons row |
| `[StackStep] game members missing: ...` | A game method or field the mod needs was not found — the game build changed; the mod does nothing |
| `[StackStep] Update prefix failed` / `Open postfix failed` | An exception was thrown inside the patch; the mod steps aside and the window behaves vanilla |
| `[StackStep] native font lookup failed` | The game's font mapping could not be read; the hint falls back to the window's title font |

If `one group = 1` shows up for an item you expected to stack, that item's stack size really is 1 — the mod is reading the real value.

如果某个物品的 `one group = 1` 和你预期不符，那说明它堆叠上限确实是 1，mod 读的就是游戏的真实值。

## Known limits / 已知限制

- **In the vendor window, seeds are a special case.** The game itself snaps seed purchases in fours, and that snapping runs on top of the group jump, so a seed group boundary can be off by a few. Chests, the player inventory and normal vendor goods are exact.
- **The hint row sits just below the window frame**, because it is anchored under the game's own tip row and the game draws that row there. That is where the game's "+ -" tips already are.
- The mod does not add keybindings and cannot be rebound; the bumpers are read directly from the game's own controller mapping.

- **商人窗口里买种子是个例外**：游戏自己对种子按 4 个一买做吸附，这个吸附会叠加在整组跳转之上，所以买种子时整组边界可能差几个。箱子、背包、普通商品都是精确的。
- **提示行在窗框下方一点**，因为它是挂在游戏自己那行提示下面的，而游戏把那行画在那里（你自己的 `+ -` 提示本来就在那儿）。
- mod 不提供改键，按键直接读游戏自己的手柄映射。

## Compatibility / 兼容性

- Built for Graveyard Keeper 2 (Steam, app id 4358690) with BepInEx 5.4.23.4, Unity 6000.3.x, Mono.
- Two Harmony patches, both on the quantity window: a prefix on `SmartSlider.Update` (where the buttons are read) and a postfix on `UIItemCountWindow.Open` (where the stack size and hint are set up). `SmartSlider` is used by `UIItemCountWindow` alone, so no other screen is touched.
- The bumpers are read from the same controller mapping the game uses for them, and the jump goes through the game's own `SmartSlider.ChangeValue`, so the range clamp, the number field and the vendor price stay in sync.
- Nothing about crafting, inventories, zombies or saves is patched. No known conflicts; load order is irrelevant. No other mod is required.

- 面向 Graveyard Keeper 2（Steam，app id 4358690），环境 BepInEx 5.4.23.4 / Unity 6000.3.x / Mono。
- 两处 Harmony 补丁，都只作用在数量窗口上：`SmartSlider.Update` 的 prefix（读按键）和 `UIItemCountWindow.Open` 的 postfix（取堆叠上限、建提示）。`SmartSlider` 全游戏只有 `UIItemCountWindow` 在用，所以不会波及其它界面。
- 按键读的是游戏自己那份手柄映射；跳转走游戏自己的 `SmartSlider.ChangeValue`，所以范围夹取、数字输入框、商人价格都会同步。
- 不碰合成、背包、僵尸、存档。无已知冲突，加载顺序无关，不依赖任何其它 mod。

## Changelog / 更新日志

### 1.0.0
- First release. Gamepad LB/RB jump the quantity to the next whole stack (item stack size) in the chest / inventory / vendor count window, with hold-to-repeat.
- Hint row under the game's own tips showing the two bumpers; the bumper name follows the controller, the wording follows the game language.

### 1.0.0
- 首次发布。手柄 LB/RB 在箱子/背包/商人的数量窗口里按整组（物品堆叠上限）跳转，支持长按连跳。
- 游戏提示行下方显示一行按键提示；按键名随手柄变化，文案随游戏语言变化。

## Source / 源码

https://github.com/zhujingwei/GK2StackStep — MIT licensed.

Built with `dotnet build -c Release` (netstandard2.1) against the game's own assemblies: `Assembly-CSharp.dll`, `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `LazyBearTechnology.dll`, `Rewired_Core.dll`, plus `Unity.TextMeshPro.dll`, `UnityEngine.UI.dll` and `UnityEngine.UIModule.dll` for the hint row. The `<HintPath>` entries in the `.csproj` point at a local Steam install — adjust them for your own.

源码：https://github.com/zhujingwei/GK2StackStep ，MIT 协议。

用 `dotnet build -c Release` 构建（netstandard2.1），引用游戏自己的程序集：`Assembly-CSharp.dll`、`UnityEngine.dll`、`UnityEngine.CoreModule.dll`、`LazyBearTechnology.dll`、`Rewired_Core.dll`，提示行另外需要 `Unity.TextMeshPro.dll`、`UnityEngine.UI.dll`、`UnityEngine.UIModule.dll`。`.csproj` 里的 `<HintPath>` 指向本机 Steam 安装路径，按自己的路径改。