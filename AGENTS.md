# SkilmeAI 框架仓库规则

## 定位

本仓库是 `SkilmeAI` AI 框架主仓库，负责 `GameOS`、`DataOS`、通用 Agent 协议、Skill 源头、包发布和框架文档。

## 必读入口

- `DocsAI/INDEX.md`
- `DocsAI/ProjectState.md`
- `GameOS/README.md`
- `DataOS/README.md`

## 修改规则

- 默认中文回复。
- 不从游戏仓库复制业务资产到框架仓库。
- Runtime 修改必须先说明影响范围和验证命令。
- Capability 必须带 `Contract.md`、manifest、测试和 Debug 说明。
- DataOS schema 修改必须同步生成器、snapshot 和验证规则。
- 新增 C# XML 注释默认使用中文；公开 API 保持简洁，不写大段教学型注释。
- 默认不 commit、不 push。

## 验证入口

最小验证：

```bash
Tools/run-build.sh
Tools/run-tests.sh
```
