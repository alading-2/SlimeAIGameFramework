# Tools

框架仓库工具目录。

当前入口：

- `run-build.sh`：框架构建。
- `run-tests.sh`：框架测试。
- `run-dataos-validate.sh`：DataOS schema / seed 验证。
- `run-pack.sh`：本地包构建。

Godot 场景测试 runner/analyzer 不在本目录维护。事实源是：

```text
../.codex/skills/godot-scene-test/scripts/
```

BrotatoLike 通过 `Games/BrotatoLike/Tools/run-godot-scene.sh` 和 `Games/BrotatoLike/Tools/analyze-godot-scene-logs.sh` 调用 skill 脚本。
