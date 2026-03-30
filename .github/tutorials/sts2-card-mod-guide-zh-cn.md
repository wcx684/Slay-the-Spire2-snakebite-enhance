# Slay the Spire 2 C# 卡牌 Mod 开发指南（BaseLib + STS2 源码对照）

> 目标：让 C# 新手在 30 分钟内做出第一张可运行卡牌。
> 适用：当前仓库中的 BaseLib 与 STS2 反编译源码。

---

## 1. 一页速览：最短路径（10 步）

1. 新建卡类，先继承 `CustomCardModel`（BaseLib 推荐路径）。
2. 给卡类加 `[Pool(typeof(...CardPool))]`，指定卡池。
3. 构造函数里设置费用、类型、稀有度、目标。
4. 覆盖 `CanonicalVars`，先放一个变量（`DamageVar`/`BlockVar`/`PowerVar`）。
5. 覆盖 `OnPlay`，先做一个确定效果（伤害/上毒/格挡）。
6. 覆盖 `OnUpgrade`，只改一个变量（`UpgradeValueBy`）。
7. 进游戏验证“是否显示在正确池”。
8. 验证“打出是否触发效果”。
9. 验证“升级后数值和描述是否一致”。
10. 再加关键词、动画、复杂联动。

实战建议：第一张卡不要做随机、多段、跨牌堆连锁，先做可稳定复现的单效果卡。

---

## 2. 架构关系图：`CardModel` vs `CustomCardModel`

```
CardModel (原版基类)
  ├─ 直接继承：原版卡常见写法
  └─ CustomCardModel (BaseLib)
       ├─ 自动参与 BaseLib 自定义内容注册流程（autoAdd）
       ├─ 可定制边框/肖像（CustomFrame/CustomPortraitPath）
       └─ ConstructedCardModel
            ├─ WithDamage/WithBlock/WithPower 等链式 API
            └─ 适合模板化快速造卡
```

### 什么时候选哪一个

- 选 `CardModel`：你在 1:1 复刻原版写法，且不依赖 BaseLib 自动注入。
- 选 `CustomCardModel`：推荐给 Mod 新手，注入流程更顺。
- 选 `ConstructedCardModel`：你要快速批量写模板卡。

### 关键出处

- `BaseLib.Abstracts.CustomCardModel`
- `BaseLib.Abstracts.ConstructedCardModel`
- `MegaCrit.Sts2.Core.Models.CardModel`

实战建议：新手先用 `CustomCardModel`，后续再迁移到 `ConstructedCardModel` 做模板化。

---

## 3. 卡牌属性字典（作用 / 默认建议 / 常见组合）

## 3.1 核心构造参数

构造签名（原版）：
- `CardModel(int cost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true)`

常见组合：
- 单体攻击：`1费 + Attack + Common + AnyEnemy`
- 基础防御：`1费 + Skill + Basic/Common + Self`
- Debuff：`1费 + Skill + Common + AnyEnemy`

## 3.2 类型/稀有度/目标

- `CardType`：`Attack / Skill / Power / Status / Curse / Quest`
- `CardRarity`：`Basic / Common / Uncommon / Rare / Ancient / Event / Token / Status / Curse / Quest`
- `TargetType`：`Self / AnyEnemy / AllEnemies / RandomEnemy / AnyAlly ...`

## 3.3 动态变量（DynamicVars）

常用变量：
- 伤害：`DamageVar`
- 格挡：`BlockVar`
- 抽牌：`CardsVar`
- 力量类：`PowerVar<TPower>`
- 计算变量：`CalculatedDamageVar`、`CalculatedBlockVar`

升级通常写法：
- `DynamicVars.Damage.UpgradeValueBy(2m);`

注意：
- 同名变量会报重复键错误（`DynamicVarSet`）。

## 3.4 标签/关键词

- 标签：`CardTag.Strike`、`CardTag.Defend` 等
- 关键词：`Exhaust`、`Retain`、`Innate`、`Ethereal` 等

## 3.5 BaseLib 扩展变量（可选）

- `ExhaustiveVar`：打出 N 次后转消耗
- `PersistVar`：本回合内可留手次数
- `RefundVar`：返还能量

实战建议：先做 1 到 2 个变量的卡，变量越多越容易出现描述和逻辑不同步。

---

## 4. 效果实现手册

## 4.1 伤害（单体/全体/随机）

典型骨架：

```csharp
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .WithHitFx("vfx/vfx_attack_slash")
    .Execute(choiceContext);
```

升级：

```csharp
DynamicVars.Damage.UpgradeValueBy(2m);
```

常见错误：
- `AnyEnemy` 卡没判空 `cardPlay.Target`。

## 4.2 格挡

典型骨架：

```csharp
await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
```

升级：

```csharp
DynamicVars.Block.UpgradeValueBy(3m);
```

常见错误：
- 用了 `BlockVar`，但 `CanonicalVars` 没返回它。

## 4.3 抽牌

典型骨架：

```csharp
await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
```

升级：

```csharp
DynamicVars.Cards.UpgradeValueBy(1m);
```

常见错误：
- 手牌满 10，误以为抽牌没生效。

## 4.4 上 Buff / Debuff（毒、易伤、虚弱）

典型骨架：

```csharp
await PowerCmd.Apply<PoisonPower>(
    cardPlay.Target,
    DynamicVars.Poison.BaseValue,
    Owner.Creature,
    this
);
```

升级：

```csharp
DynamicVars.Poison.UpgradeValueBy(2m);
```

常见错误：
- 没加 `ExtraHoverTips`，玩家看不懂变量含义。

## 4.5 生成牌 / 临时牌

典型骨架：

```csharp
CardModel temp = CombatState.CreateCard<Shiv>(Owner);
await CardPileCmd.AddGeneratedCardToCombat(temp, PileType.Hand, addedByPlayer: true);
```

升级常见改法：
- 生成数量 +1（配合 `CardsVar`）

常见错误：
- 把已有 `Pile` 的卡当“生成卡”再加一次，触发异常。

## 4.6 弃牌 / 消耗 / 保留

- 弃牌：`CardCmd.Discard(...)`
- 消耗：`CardCmd.Exhaust(...)`
- 保留：加 `CardKeyword.Retain`

常见错误：
- 保留逻辑与卡文本不一致（只改了描述没改关键词）。

实战建议：一张卡最多组合 2 种效果，先保证可测试性再追求花哨。

---

## 5. Pool 选择指南（适用场景 + 反例）

可选池：
- 角色池：`IroncladCardPool`、`SilentCardPool`、`DefectCardPool`、`RegentCardPool`、`NecrobinderCardPool`
- 共享池：`ColorlessCardPool`、`StatusCardPool`、`CurseCardPool`、`EventCardPool`、`QuestCardPool`、`TokenCardPool`

推荐：
- 正常可获得卡：角色池 / 无色池
- 战斗临时衍生牌：Token 池
- 负面状态：Status 池
- 诅咒：Curse 池

反例：
- 把“常规奖励卡”放 Token 池。
- 把“临时衍生牌”放角色池，导致进入奖励流。

BaseLib 注入规则：
- 卡类必须带 `[Pool(...)]`。
- `Pool` 类型必须和模型类型匹配，否则会抛异常。

实战建议：第一张卡先放角色池，确认跑通后再细分池策略。

---

## 6. 从原版卡迁移到 Mod 卡的方法论

1. 把继承从 `CardModel` 改成 `CustomCardModel`。
2. 原样迁移构造参数（费用、类型、稀有度、目标）。
3. 原样迁移 `CanonicalVars`。
4. 原样迁移 `OnPlay`、`OnUpgrade`。
5. 加上 `[Pool(typeof(...))]`。
6. 运行验证池显示与效果。
7. 再做 BaseLib 化（比如 `CommonActions` 简化）。

实战建议：先“机械迁移”，后“风格优化”。一次做两件事最容易引入隐性 bug。

---

## 7. 五个可直接改名复用的模板

## 7.1 攻击模板

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Snakebite.Cards;

[Pool(typeof(IroncladCardPool))]
public sealed class TemplateAttack : CustomCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(8m, ValueProp.Move) };

    public TemplateAttack() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
```

## 7.2 技能模板（抽牌）

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Snakebite.Cards;

[Pool(typeof(SilentCardPool))]
public sealed class TemplateSkillDraw : CustomCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new CardsVar(2) };

    public TemplateSkillDraw() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
```

## 7.3 防御模板

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Snakebite.Cards;

[Pool(typeof(IroncladCardPool))]
public sealed class TemplateDefend : CustomCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new BlockVar(7m, ValueProp.Move) };

    public TemplateDefend() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
```

## 7.4 上 Debuff 模板（易伤）

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Snakebite.Cards;

[Pool(typeof(IroncladCardPool))]
public sealed class TemplateDebuff : CustomCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new PowerVar<VulnerablePower>(2m) };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromPower<VulnerablePower>() };

    public TemplateDebuff() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Vulnerable.UpgradeValueBy(1m);
    }
}
```

## 7.5 生成临时牌模板

```csharp
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Snakebite.Cards;

[Pool(typeof(SilentCardPool))]
public sealed class TemplateCreateTempCard : CustomCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new CardsVar(2) };

    public TemplateCreateTempCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            CardModel shiv = CombatState.CreateCard<Shiv>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(shiv, PileType.Hand, addedByPlayer: true);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
```

实战建议：把模板类名改掉后，先只改构造参数和一个变量，再跑游戏确认。

---

## 8. 新手踩坑清单 + 排错流程

## 8.1 常见坑

1. 编译过了但游戏里没有卡
- 漏了 `[Pool]` 或池类型写错。

2. 卡在错误池出现
- 普通卡误放 `Token/Status/Curse`。

3. 目标为空崩溃
- `AnyEnemy` 卡没判 `cardPlay.Target`。

4. 数值变量不更新
- 升级没 `UpgradeValueBy`，或变量名重复。

5. 文本和效果不一致
- 描述用了某变量名，但 `CanonicalVars` 没这个键。

6. 生牌异常
- 对已在牌堆中的卡再次走“生成卡”流程。

## 8.2 排错流程（推荐顺序）

1. 看卡是否进池（`[Pool]` + 池语义）。
2. 看 `OnPlay` 是否执行（先用最小效果验证）。
3. 看 `DynamicVars` 是否齐全且无重名。
4. 看升级后变量值是否改变。
5. 最后再看动画、音效、复杂联动。

## 8.3 版本限制 + 替代方案

限制：`ConstructedCardModel` 当前只支持一个 calculated 变量组。  
替代：继承 `CustomCardModel` 手写 `CanonicalVars`，可放多个计算变量。

实战建议：每次只改一处并进游戏验证，不要一口气改 5 个点。

---

## 9. 最终完整示例：新建一张 Debuff 卡（带注释）

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Snakebite.Cards;

// 放进静默角色池，你也可以改成自己的角色池
[Pool(typeof(SilentCardPool))]
public sealed class ToxicNeedle : CustomCardModel
{
    // 用 PowerVar<PoisonPower> 让描述和预览自动拿到中毒值
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new PowerVar<PoisonPower>(5m) };

    // 提示中毒效果，避免新手玩家看不懂
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromPower<PoisonPower>() };

    public ToxicNeedle() : base(
        baseCost: 1,
        type: CardType.Skill,
        rarity: CardRarity.Common,
        target: TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // AnyEnemy 卡务必判空，避免边界条件崩溃
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 可选：播放中毒视觉
        NPoisonImpactVfx vfx = NPoisonImpactVfx.Create(cardPlay.Target);
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);

        // 可选：角色施法动作
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 核心效果：施加中毒
        await PowerCmd.Apply<PoisonPower>(
            target: cardPlay.Target,
            amount: DynamicVars.Poison.BaseValue,
            applier: Owner.Creature,
            cardSource: this
        );
    }

    protected override void OnUpgrade()
    {
        // 升级：中毒 +2
        DynamicVars.Poison.UpgradeValueBy(2m);
    }
}
```

实战建议：先让这张卡稳定出现并可打出，再加第二效果（例如“抽 1 张牌”）。

---

## 10. 参考索引（关键类/方法）

BaseLib：
- `BaseLib.Abstracts.CustomCardModel`
- `BaseLib.Abstracts.ConstructedCardModel`
- `BaseLib.Utils.PoolAttribute`
- `BaseLib.Patches.Content.CustomContentDictionary.AddModel`
- `BaseLib.Utils.CommonActions`
- `BaseLib.Cards.Variables.ExhaustiveVar / PersistVar / RefundVar`

STS2：
- `MegaCrit.Sts2.Core.Models.CardModel`
- `MegaCrit.Sts2.Core.Commands.DamageCmd`
- `MegaCrit.Sts2.Core.Commands.CreatureCmd`
- `MegaCrit.Sts2.Core.Commands.PowerCmd`
- `MegaCrit.Sts2.Core.Commands.CardPileCmd`
- `MegaCrit.Sts2.Core.Commands.CardCmd`
- `MegaCrit.Sts2.Core.Commands.CardSelectCmd`
- `MegaCrit.Sts2.Core.Models.CardPoolModel`
- `MegaCrit.Sts2.Core.Factories.CardFactory`
- `MegaCrit.Sts2.Core.Runs.CardCreationOptions`

---

如果你希望，我可以下一步直接在 `snakebite/Cards/` 下给你创建这 5 张模板卡的真实 `.cs` 文件（按你当前命名空间自动填好）。
