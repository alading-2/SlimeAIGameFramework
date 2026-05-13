# Tests

`Tests/SlimeAI.GameOS.Tests/` 是 SlimeAI.GameOS 的纯 C# Runtime 行为测试项目。

它可以理解为不启动 Godot 的最小 GameOS 运行沙盘：手动创建 Runtime Entity、写入 DataKey、调用 Runtime service，并断言结果。

## 用途

- 快速验证 GameOS 纯 C# Runtime 规则没有被破坏。
- 在 Godot scene test 之前定位框架层问题。
- 覆盖 Event、Entity、Relationship、Schedule、Movement、Collision、Damage、Ability、Projectile、Effect、Feature、AI、Attack、Observation 等核心行为。

它不覆盖 Godot Node、场景树、PhysicsServer2D、资源加载、UI、动画和 BrotatoLike 主场景。

## 命令

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-tests.sh
```

`Tools/run-build.sh` 会构建测试项目；`Tools/run-tests.sh` 会执行测试项目。

## 文档

长期说明见：

```text
DocsAI/Tests/RuntimeTests.md
```
