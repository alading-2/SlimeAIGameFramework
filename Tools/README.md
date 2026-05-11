# Tools

框架仓库工具目录。

当前入口：

- `run-build.sh`：框架构建。
- `run-tests.sh`：框架测试，先跑 DataOS 验证，再跑 `Tests/SkilmeAI.GameOS.Tests` Runtime 行为测试。
- `run-dataos-validate.sh`：DataOS schema / seed 验证。

Runtime 测试说明见：

```text
../DocsAI/Tests/RuntimeTests.md
```

Godot 场景测试 runner/analyzer 不在本目录维护。事实源是：

```text
../.codex/skills/godot-scene-test/scripts/
```

BrotatoLike 通过 `Games/BrotatoLike/Tools/run-godot-scene.sh` 和 `Games/BrotatoLike/Tools/analyze-godot-scene-logs.sh` 调用 skill 脚本。
