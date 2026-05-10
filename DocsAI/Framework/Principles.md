# SkilmeAI Framework Principles

## AI-first

文档是 AI 执行协议，不是源码目录装饰。每份长期文档必须帮助 AI 判断：

- 何时使用。
- 先读什么。
- 禁止什么。
- 修改哪些文件。
- 跑哪些验证。
- 产出哪些 artifact。

## 少入口

AI 路由优先级：

```text
AGENTS.md -> DocsAI/INDEX.md -> domain 文档 -> owner skill -> 验证命令
```

同一事实不在多个目录重复维护。源码旁文档迁入 DocsAI 后，旧路径只能作为短指针或历史归档。

## 小内核，可选能力

GameOS runtime kernel 保持小而稳定；能力包以 Capability 形式接入。Capability 不应隐式修改其他能力的所有权边界。

## 框架和游戏分离

框架只接收可复用的运行时抽象、协议、数据生成、验证机制。BrotatoLike 专属行为保留在 `Games/BrotatoLike`，除非新的 OpenSpec proposal 证明其可复用价值。

## 验证优先

每个框架变更都要说明验证命令。能跑就跑；不能跑要记录具体原因。Godot 场景测试以 artifact 为事实源，不只看无错误日志。

