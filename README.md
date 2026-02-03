# SFM Code Generator

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue)](https://github.com/MOAKIEE/SFMCodeGenerator/releases)

[English](#english) | [中文](#中文)

---

## 中文

**SFM Code Generator** 是一个用于 Minecraft 模组 [Super Factory Manager](https://www.curseforge.com/minecraft/mc-mods/super-factory-manager) 的可视化代码生成工具。

### ✨ 功能特性

- 🎯 **可视化编辑** - 无需手写代码，通过界面配置即可生成 SFML 程序
- 🔄 **触发器配置** - 支持 `EVERY` 定时触发和 `REDSTONE PULSE` 红石脉冲触发
- 📦 **语句生成** - 支持 INPUT、OUTPUT、IF 条件等核心语句
- 📋 **快速模板** - 内置常用自动化模板（物品移动、自动熔炼、自动分类、流体传输）
- 📊 **实时预览** - 代码实时生成，字符数统计（上限 32300 字符）
- 💾 **导出功能** - 一键复制或保存为 `.sfm` 文件

### 📥 下载安装

从 [Releases](https://github.com/MOAKIEE/SFMCodeGenerator/releases) 页面下载最新版本的 `SFMCodeGenerator.exe`，双击运行即可。

> ⚠️ 需要 Windows 10 或更高版本

### 🚀 快速开始

1. 下载并运行 `SFMCodeGenerator.exe`
2. 设置程序名称
3. 配置触发器（间隔时间、类型等）
4. 添加 INPUT/OUTPUT 语句
5. 点击"复制"将代码复制到剪贴板
6. 在游戏中打开 SFM Manager，粘贴代码

### 📖 SFM 语法参考

| 语句 | 说明 | 示例 |
|------|------|------|
| `name` | 程序名称 | `name "我的程序"` |
| `every X ticks do...end` | 定时触发 | `every 20 ticks do...end` |
| `input from` | 从容器输入物品 | `input from a` |
| `output to` | 向容器输出物品 | `output to b` |
| `if...then...end` | 条件判断 | `if a has > 0 iron_ingot then...end` |

### 🤝 贡献

欢迎提交 Issue 和 Pull Request！

### 📄 许可证

本项目采用 [MIT License](LICENSE) 开源。

---

## English

**SFM Code Generator** is a visual code generation tool for the Minecraft mod [Super Factory Manager](https://www.curseforge.com/minecraft/mc-mods/super-factory-manager).

### ✨ Features

- 🎯 **Visual Editor** - Generate SFML programs through UI without writing code
- 🔄 **Trigger Configuration** - Supports `EVERY` timed triggers and `REDSTONE PULSE` triggers
- 📦 **Statement Generation** - Supports INPUT, OUTPUT, IF conditions and more
- 📋 **Quick Templates** - Built-in templates (item transfer, auto-smelting, sorting, fluid transfer)
- 📊 **Live Preview** - Real-time code generation with character count (limit: 32300)
- 💾 **Export** - Copy to clipboard or save as `.sfm` file

### 📥 Download

Download the latest `SFMCodeGenerator.exe` from the [Releases](https://github.com/MOAKIEE/SFMCodeGenerator/releases) page.

> ⚠️ Requires Windows 10 or later

### 🚀 Quick Start

1. Download and run `SFMCodeGenerator.exe`
2. Set program name
3. Configure triggers (interval, type, etc.)
4. Add INPUT/OUTPUT statements
5. Click "Copy" to copy code to clipboard
6. Open SFM Manager in-game and paste the code
### 📄 License

This project is licensed under the [MIT License](LICENSE).
