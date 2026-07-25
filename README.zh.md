# slowly

简单的二进制流速度控制工具。从文件或标准输入读取数据，以指定速度输出到标准输出。

## 为什么需要？

测试流媒体应用或管道连接时，常需要以特定速度发送数据。`slowly` 只做这一件事。

## 安装

### 下载

从 [Releases](https://github.com/user/slowly/releases) 下载。

### 从源码编译

需要 .NET SDK 8.0+。

```bash
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r osx-x64 --self-contained
```

## 使用方法

```bash
slowly [options] <file>
```

不指定文件时，从标准输入读取。

## 选项

| 选项 | 说明 |
|--------|-------------|
| `-r, --rate <speed>` | 输出速度（如 `2500000`、`24Mbps`） |
| `-d, --delay <seconds>` | 开始前延迟（秒） |
| `-l, --loop` | 循环模式（仅文件输入） |
| `-c, --count <N>` | 循环次数（0 = 无限） |
| `-b, --buffer <size>` | 缓冲区大小（字节）（默认：18800） |
| `-t, --time <seconds>` | 指定时间后停止（秒） |
| `-v, --verbose` | 在 stderr 显示进度 |
| `-h, --help` | 显示帮助 |
| `-V, --version` | 显示版本 |

## 速度单位

| 单位 | 含义 |
|------|---------|
| `2500000` | 字节/秒 |
| `24Mbps` | 兆比特/秒 |
| `100KB/s` | 千字节/秒 |

内部全部转换为字节/秒处理。

## 示例

### 以 24 Mbps 发送文件

```bash
slowly -r 24Mbps stream.ts | receiver
```

### 循环 3 次

```bash
slowly -l -c 3 -r 10Mbps stream.ts | analyzer
```

### 从 ffmpeg 管道输入

```bash
ffmpeg -i input.mp4 -f mpegts - | slowly -r 8Mbps
```

### 延迟 5 秒开始

```bash
slowly -d 5 -r 18Mbps stream.ts | monitor
```

### 仅输出 60 秒

```bash
slowly -t 60 -r 10Mbps stream.ts > output.ts
```

## 数据保证

输入与输出逐字节完全一致。数据无更改、无重排、无丢失。

```bash
certutil -hashfile input.ts SHA256
certutil -hashfile output.ts SHA256
# 哈希一致
```

## 设计理念

> 不理解任何内容。只是慢慢流动。

`slowly` 不解析或解释输入数据。只是一个带速度限制的简单管道。

## 许可证

MIT
