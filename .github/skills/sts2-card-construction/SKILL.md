---
name: sts2-card-construction
description: "Use when creating or updating Slay the Spire 2 cards with BaseLib. Trigger phrases: build card, create card, card code, card keywords, card portrait, card localization, add cards.json, add card_keywords.json, Snakebite card, STS2 card mod."
---

# STS2 Card Construction Skill

## Purpose
Provide an end-to-end workflow for building STS2 cards with BaseLib:
- card model code,
- keyword wiring,
- portrait path strategy,
- localization text,
- validation,
- reusable task summary writeback.

## Required Context
Before writing code, gather these inputs from user or repository:
1. card class name and desired card fantasy,
2. cost, type, rarity, target,
3. primary and secondary effects,
4. pool choice,
5. localization language(s),
6. whether this should mirror a vanilla card style.

If information is missing, ask concise questions first.

## Source Priority
1. tutorials/sts2-card-mod-guide-zh-cn.md
2. STS2 decompiled source and BaseLib source at C:\Users\wcx\Desktop\mod\code
3. External docs (including https://glitchedreme.github.io/SlayTheSpire2ModdingTutorials/docs/03-baselib/)

When conflicts exist, prefer observed source behavior.

## Build Workflow

### Step 1: Dependencies and registration checks
Confirm both are present:
1. snakebite.csproj references BaseLib.
2. snakebite.json contains dependencies with BaseLib.

### Step 2: Create card model
Default: inherit from CustomCardModel.
Add pool attribute for intended color pool.
Keep constructor semantics aligned with behavior.

Implementation rules:
1. Keep OnPlay deterministic and await each action in logical order.
2. For enemy-target cards, null-check target before effect execution.
3. Keep CanonicalVars, ExtraHoverTips, and CanonicalKeywords in sync with gameplay and text.
4. OnUpgrade should only adjust declared values.
5. First playable version should be minimal and testable.

### Step 3: Dynamic values and hover tips
Use canonical dynamic vars where possible:
- DamageVar, BlockVar, CardsVar, PowerVar.

If using custom dynamic vars:
1. define a unique key,
2. add tooltip binding,
3. add localization in static_hover_tips.json when needed.

Use ExtraHoverTips for:
- powers,
- generated cards,
- custom keywords.

### Step 4: Card keywords
For built-in keywords, use CanonicalKeywords directly.
For custom keywords:
1. create keyword declaration class with CustomEnum and KeywordProperties,
2. reference custom keyword in CanonicalKeywords,
3. add localization file card_keywords.json with title and description.

### Step 5: Card portrait strategy
Implement portrait path explicitly, either per-card or in a shared base class.
Recommended stable pattern:
- use Id.Entry.ToLowerInvariant() in resource path,
- keep images under a predictable folder structure.

Important naming caveat:
- card IDs are often namespaced (`modid-card_entry`).
- if art files are stored without mod prefix (for example `snakebite_storm.png`), strip `modid-` before composing filename, or name file with full ID (`snakebite-snakebite_storm.png`).
- mismatch here causes blank art with log errors like `No loader found for resource`.

Art notes:
- any dimension is accepted,
- official common card art ratio examples are documented by the tutorial,
- keep naming deterministic to avoid missing-resource runtime issues.

### Step 6: Localization text
Create or update:
1. {modId}/localization/zhs/cards.json
2. {modId}/localization/zhs/card_keywords.json when custom keywords are used

Text rules:
1. Use {VarKey:diff()} for dynamic values that should color on change.
2. Ensure text order matches actual OnPlay effect order.
3. Do not duplicate keyword prose if keyword is already auto-rendered by engine behavior.

### Step 7: Validation
At minimum verify:
1. card can be obtained from intended pool,
2. OnPlay resolves without target or null issues,
3. upgrade values and text placeholders match,
4. portrait displays correctly,
5. hover tips and keyword display are correct.

Runtime validation checklist (highly recommended):
1. open `godot.log` and search card ID.
2. if text shows key name (for example `cards.SNAKEBITE-...title`), verify localization key exists in `localization/zhs/cards.json` and resource source matches current packaging mode.
3. if portrait is blank, check for loader errors and compare requested path with actual filename exactly.
4. for generated assets, confirm latest timestamps are deployed (stale pck frequently masks recent fixes).

If full runtime validation is unavailable, explicitly report what was not validated.

## Packaging Modes (Do Not Mix)

Choose exactly one mode per run.

### Mode A: PCK-first (recommended for release)
1. `snakebite.json`: set `has_pck` to `true`.
2. deploy exactly three artifacts: `snakebite.dll`, `snakebite.json`, `snakebite.pck`.
3. pck filename must exactly match mod id (`snakebite.pck`).
4. after resource/text edits, re-export pck before testing.

### Mode B: Loose-file (recommended for rapid iteration)
1. `snakebite.json`: set `has_pck` to `false`.
2. deploy `dll + json + localization + images` directories.
3. do not rely on stale pck from previous builds.

### Common deployment pitfalls
1. `has_pck=true` but missing/wrongly named pck -> resource load failures.
2. `has_pck=false` while assuming pck carries latest assets -> old or missing text/art.
3. building while game process is running -> dll copy fails due to file lock.
4. mixed source trees (for example both project root and nested `snakebite/`) -> copied wrong asset set.

## Output Contract For Agent
When completing a card task with this skill, produce:
1. changed files list,
2. concise behavior summary,
3. validation results with pass/fail,
4. known gaps and next checks.

## Reusable Templates
Use assets in templates/:
1. templates/card_model_template.cs
2. templates/custom_keyword_template.cs
3. templates/cards.localization.zhs.example.json
4. templates/card_keywords.localization.zhs.example.json

## Completion Memory Requirement
After each completed task, append one entry to ai-memory/task-summaries.md.

Use this format:

## [YYYY-MM-DD HH:mm] <task-title>
- Request: <what user asked>
- Changes:
  - <file path>: <what changed>
- Validation:
  - <command or manual check>: <result>
- Decisions:
  - <important implementation choice and why>
- Reuse Notes:
  - <pattern, caveat, or snippet reusable next time>
- Next Options:
  - <optional follow-up 1>
  - <optional follow-up 2>

Rules:
1. append-only; do not overwrite history,
2. if no code changed, use Changes: NO_CHANGE,
3. Reuse Notes must contain at least one concrete reusable point.
