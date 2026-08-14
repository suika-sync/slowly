# slowly

A simple binary stream rate controller. Sends data from file or stdin to stdout at a specified speed.

## Why?

When testing streaming applications or piped processes, you often need to send data at a specific rate. `slowly` does exactly that — nothing more, nothing less.

## Install

### Download

Download from [Releases](https://github.com/suika-sync/slowly/releases/).

### Build from source

Requires .NET SDK 8.0+.

```bash
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r osx-x64 --self-contained
```

## Usage

```bash
slowly [options] <file>
```

If no file is specified, reads from stdin.

## Options

| Option | Description |
|--------|-------------|
| `-r, --rate <speed>` | Output speed (e.g., `2500000`, `24Mbps`) |
| `-d, --delay <seconds>` | Delay before starting |
| `-l, --loop` | Loop input (file only) |
| `-c, --count <N>` | Loop count (0 = infinite) |
| `-b, --buffer <size>` | Buffer size in bytes (default: 18800) |
| `-t, --time <seconds>` | Stop after N seconds |
| `-v, --verbose` | Show progress on stderr |
| `-h, --help` | Show help |
| `-V, --version` | Show version |

## Rate Units

| Unit | Meaning |
|------|---------|
| `2500000` | bytes/sec |
| `24Mbps` | megabits/sec |
| `100KB/s` | kilobytes/sec |

Internal conversion: all rates are converted to bytes/sec.

## Examples

### Send file at 24 Mbps

```bash
slowly -r 24Mbps stream.ts | receiver
```

### Loop 3 times

```bash
slowly -l -c 3 -r 10Mbps stream.ts | analyzer
```

### Pipe from ffmpeg

```bash
ffmpeg -i input.mp4 -f mpegts - | slowly -r 8Mbps
```

### Delay 5 seconds

```bash
slowly -d 5 -r 18Mbps stream.ts | monitor
```

### Time-limited output

```bash
slowly -t 60 -r 10Mbps stream.ts > output.ts
```

## Data Integrity

Input and output are byte-for-byte identical. No data is modified, reordered, or lost.

```bash
certutil -hashfile input.ts SHA256
certutil -hashfile output.ts SHA256
# Hashes match
```

## Design

> Understand nothing. Just flow slowly.

`slowly` does not parse or interpret input data. It is a dumb pipe with a speed limiter.

## License

MIT
