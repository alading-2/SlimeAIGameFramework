# SkillsSource Index

本文记录已迁入 SkilmeAI 框架仓库的 Skill 源头。

## Active Source

这些 Skill 同步安装在仓库根目录 `.codex/skills/`，可由 Codex 在框架仓库中直接触发：

- `ability-system`
- `ai-system`
- `attack-system`
- `collision-system`
- `damage-system`
- `data-authoring`
- `ecs-component`
- `ecs-data`
- `ecs-entity`
- `ecs-event`
- `feature-system`
- `godot-scene-test`
- `movement-system`
- `project-index`
- `projectile-effect-system`
- `research-reference-framework`
- `test-system`
- `tools`
- `ui-bind`

## Bundled Resources

- `ability-system/references/ability-logic-parameters.md`
- `godot-scene-test/scripts/analyze-logs.sh`
- `godot-scene-test/scripts/godot-scene-runner.mjs`
- `godot-scene-test/scripts/run-test.sh`

## Boundary

- 框架仓库 Skill 面向 GameOS / DataOS / Agent / Validation。
- BrotatoLike 游戏仓库只保留 `project-index / game-development / gameos-reference / data-authoring / godot-scene-test` 五个入口型 Skill。
- 旧仓库 `/home/slime/Code/Godot/Games/MyGames/brotato-my` 只作为迁移输入，不再作为 Skill 权威源头。
