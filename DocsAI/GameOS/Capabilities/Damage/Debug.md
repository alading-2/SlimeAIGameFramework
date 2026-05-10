# Damage Debug Guide

## 日志开关

设置环境变量启用 Damage 调试日志：



或在代码中临时启用：



## 关键日志关键字

| 关键字 | 含义 |
|--------|------|
| [Damage] | Damage 能力日志前缀 |
| Error | 错误 |
| Warning | 警告 |

## 状态 Dump



## 常见排查步骤

1. 确认 DataKey 已注册（调用 DamageDataKeys.RegisterAll()）
2. 确认相关 System 已加入 Schedule
3. 检查 capability.json 中的依赖 Capability 是否已启用
4. 查看 Godot smoke 日志中的 Damage 相关输出

## Godot 场景测试

[   0% ] [90m[1mfirst_scan_filesystem[22m | Started 项目初始化 (5 steps)[39m[0m
[   0% ] [90m[1mfirst_scan_filesystem[22m | 正在扫描文件结构……[39m[0m
[  16% ] [90m[1mfirst_scan_filesystem[22m | 正在加载全局类名……[39m[0m
[  33% ] [90m[1mfirst_scan_filesystem[22m | 正在校验 GDExtension……[39m[0m
[  50% ] [90m[1mfirst_scan_filesystem[22m | 正在创建自动加载脚本……[39m[0m
[  66% ] [90m[1mfirst_scan_filesystem[22m | 正在初始化插件……[39m[0m
[  83% ] [90m[1mfirst_scan_filesystem[22m | 正在启动文件扫描……[39m[0m
[92m[ DONE ][39m [1mfirst_scan_filesystem[22m
[0m
[   0% ] [90m[1mdotnet_build_project[22m | Started Building .NET project... (1 steps)[39m[0m
[   0% ] [90m[1mdotnet_build_project[22m | Building project[39m[0m
[92m[ DONE ][39m [1mdotnet_build_project[22m
[0m
[   0% ] [90m[1mloading_editor_layout[22m | Started 正在加载编辑器 (5 steps)[39m[0m
[   0% ] [90m[1mloading_editor_layout[22m | 正在加载编辑器布局……[39m[0m
[  16% ] [90m[1mloading_editor_layout[22m | 正在加载停靠面板……[39m[0m
[  33% ] [90m[1mloading_editor_layout[22m | 正在重新打开场景……[39m[0m
[  50% ] [90m[1mloading_editor_layout[22m | 正在加载中央编辑器布局……[39m[0m
[  66% ] [90m[1mloading_editor_layout[22m | 正在加载插件窗口布局……[39m[0m
[  83% ] [90m[1mloading_editor_layout[22m | 编辑器布局就绪。[39m[0m
[92m[ DONE ][39m [1mloading_editor_layout[22m
[0m
BrotatoLike main scene initialized
BrotatoLike GameOS smoke: brotato-like-smoke res://Scenes/Main.tscn bridge:True pool:True dataos:True main:True
BrotatoLike GameOS smoke PASS
Scene test run: .ai-temp/scene-tests/runs/2026-05-06/13-15-57
Status: PASS marker found
Error markers: none

Stdout tail:
BrotatoLike main scene initialized
BrotatoLike GameOS smoke: brotato-like-smoke res://Scenes/Main.tscn bridge:True pool:True dataos:True main:True
BrotatoLike GameOS smoke PASS
