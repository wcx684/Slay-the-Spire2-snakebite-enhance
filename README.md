# Snakebite

**我说蛇咬真的很强有人能懂吗？**

安装教程见 [构建与安装](#构建与安装)

一个基于 Slay the Spire 2 + BaseLib 的 C# Mod，围绕原版【蛇咬】构建一组联动卡牌与能力。

## 功能概览

- 新增 6 张以“蛇咬”为核心的 猎煲 卡牌
- 新增 3 个配套能力（Power）
- 通过 Harmony Patch 改写原版【蛇咬】在特定条件下的目标与结算逻辑

## 当前内容

### 卡牌

| 卡牌类 | 稀有度 / 类型 / 费用 | 主要效果 | 升级效果 |
| --- | --- | --- | --- |
| `SnakebiteStorm` | 稀有 / 技能 / 0 | 弃掉手牌全部卡牌，每弃 1 张加入 1 张带【消耗】的【蛇咬】到手牌 | 生成的【蛇咬】会被升级 |
| `SnakebiteTime` | 稀有 / 技能 / 3 | 本回合你的所有【蛇咬】费用为 0（包含本回合后续抽到或生成的【蛇咬】） | 费用 -1 |
| `SnakebiteHellbite` | 稀有 / 能力 / 2 | 施加【地狱狂咬】能力：抽到“蛇咬”相关牌时自动打出 | 费用 -1 |
| `SnakebiteInfinite` | 非凡 / 能力 / 1 | 施加【无尽蛇咬】能力：回合开始在手牌加入【蛇咬】（当回合免费） | 获得固有 |
| `SnakebiteFan` | 稀有 / 能力 / 2 | 施加【蛇咬扇】并生成多张【蛇咬】到手牌 | 额外多生成 1 张 |
| `SnakebitePerfected` | 普通 / 攻击 / 2 | 施加中毒，数量随牌组中“蛇咬”牌数量增加 | 每张“蛇咬”的额外中毒系数提升 |

> 注：卡牌文本以 `snakebite/localization/zhs/cards.json` 为准。

### 能力（Power）

| 能力类 | 核心行为 |
| --- | --- |
| `SnakebiteFanPower` | 作为【蛇咬扇】标记能力，由补丁读取并驱动【蛇咬】改为全体目标 |
| `SnakebiteHellbitePower` | 抽到“蛇咬”相关牌时自动打出该牌，并替换攻击特效/动作 |
| `SnakebiteInfinitePower` | 每回合开始向手牌生成若干【蛇咬】，并设为当回合免费 |
| `SnakebiteTimePower` | 临时能力：本回合内你后续抽到或生成的【蛇咬】会自动变为当回合免费，回合结束后移除 |

### 补丁逻辑

`Patches/SnakebiteFanPatches.cs` 包含两处关键改动：

1. 当玩家拥有 `SnakebiteFanPower` 时，原版 `Snakebite` 的 `TargetType` 改为 `AllEnemies`
2. 拦截原版 `Snakebite.OnPlay`，改为对所有可被命中的敌人逐个施加中毒

## 依赖与环境

- Slay the Spire 2
- BaseLib
- Harmony (`0Harmony.dll`)
- Godot .NET SDK 4.5.1
- .NET SDK（项目目标框架：`net10.0`）

## 构建与安装

### 0) 如何使用

受限于BaseLib，此 mod 需要在杀戮尖塔2**beta**版本下进行游玩

#### 安装

1. 前往 [Baselib releases](https://github.com/Alchyr/BaseLib-StS2/releases) 下载BaseLib mod作为本mod的依赖，解压后将其中的文件复制到杀戮尖塔2的mods目录下（如果没有则创建）

2. 前往本仓库的releases页面下载最新的snakebite mod包，解压后将其中的文件复制到杀戮尖塔2的mods目录下（如果没有则创建）

确保你的文件结构如下：

```txt
Slay the Spire 2/
├── mods/
│   ├── Baselib/
│   │   ├── BaseLib.dll
│   │   ├── BaseLib.json
│   │   └── snakebite.dll
│   └── snakebite/
│       ├── snakebite.dll
│       ├── snakebite.json
│       └── snakebite.pck
```

此时模组应该已经安装完成

### 1) 配置游戏目录

编辑 `snakebite.csproj`，设置你的游戏路径：

```xml
<Sts2Dir>E:\SteamLibrary\steamapps\common\Slay the Spire 2</Sts2Dir>
```

### 2) 构建

```bash
dotnet build snakebite.csproj
```

### 3) 构建后自动复制

构建成功后，MSBuild Target 会自动复制以下文件到：

`<Sts2Dir>/mods/snakebite/`

- `snakebite.dll`
- `snakebite.json`
- `snakebite.pck`（若存在）

### 4) 在游戏中启用

确保 BaseLib 已安装，并在启动器中启用本 Mod。

## 项目结构

```text
Cards/                    # 自定义卡牌（CustomCardModel）
Powers/                   # 能力与行为钩子（PowerModel / CustomPowerModel）
Patches/                  # Harmony 补丁
snakebite/localization/   # 本地化资源（当前为 zhs）
snakebite/images/         # 卡图与能力图标
```

## 开发说明

- 新增卡牌：放在 `Cards/`，使用 `[Pool(typeof(...))]` 指定池，并补齐本地化键
- 新增能力：放在 `Powers/`，根据需要继承 `PowerModel` 或 `CustomPowerModel`
- 需要改写原版行为时：在 `Patches/` 添加 Harmony Patch
- 资源路径统一使用 Godot `res://` 前缀

## CI/CD 自动发布

仓库已添加 GitHub Actions 工作流：`.github/workflows/release-on-push.yml`

- 触发条件：每次 `push`（任意分支）
- 行为：创建一个 `prerelease`，并上传以下 3 个文件作为 Release Assets
	- `snakebite.dll`
	- `snakebite.json`
	- `snakebite.pck`
- Tag 格式：`ci-分支名-运行号`（分支名中的 `/` 会自动替换为 `-`）

如需只在 `main` 分支触发，可将 workflow 的 `on.push.branches` 改为仅 `main`。

## 已知事项

- 本地化文件中存在部分重复键（例如同一能力/卡牌的新旧命名并存），属于兼容性保留现状
- 若图片未生效，优先检查 `.pck` 是否存在且被复制到 Mod 目录

## 许可证

本仓库包含 `LICENSE` 文件，具体条款请参阅仓库根目录。