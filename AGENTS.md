# SlimeAI 框架子目录规则

## 定位

本目录是框架实现根目录，负责 `GameOS`、`DataOS`、通用 Agent 协议、Skill 源头、包发布和框架 DocsAI。工作区总入口在上级 `../AGENTS.md`。

## 必读入口

- `DocsAI/INDEX.md`
- `DocsAI/ProjectState.md`
- `DocsAI/Framework/Overview.md`
- `DocsAI/GameOS/Overview.md`
- `DocsAI/DataOS/Overview.md`
- `DocsAI/Agent/Overview.md`

## 修改规则

- 默认中文回复。
- 长期 AI-facing 文档放在 `DocsAI/`，源码目录不再新增说明性 `.md`。
- Capability contract/debug/API/validation 先看 `DocsAI/GameOS/`，再看 owner skill。
- 不从游戏仓库复制业务资产到框架仓库。
- Runtime 修改必须先说明影响范围和验证命令。
- Capability 变更必须同步 DocsAI 下的 contract/debug、manifest、测试和 skill 路由。
- DataOS schema 修改必须同步生成器、snapshot 和验证规则。
- 新增 C# XML 注释默认使用中文；公开 API 保持简洁，不写大段教学型注释。
- 默认不 commit、不 push。

## 验证入口

最小验证：

```bash
Tools/run-build.sh
Tools/run-tests.sh
```
