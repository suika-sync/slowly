# slowly

シンプルなバイナリストリーム速度制御ツール。ファイルまたは標準入力から読み込んだデータを、指定した速度で標準出力へ送出します。

## なぜ必要か？

ストリーミングアプリケーションやパイプ接続のテスト時、特定の速度でデータを送る必要があります。`slowly` はその機能のみを提供します。

## インストール

### ダウンロード

[Releases](https://github.com/user/slowly/releases) からダウンロード。

### ソースからビルド

.NET SDK 8.0 以上が必要。

```bash
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r osx-x64 --self-contained
```

## 使い方

```bash
slowly [options] <file>
```

ファイルを指定しない場合は標準入力から読み込み。

## オプション

| オプション | 説明 |
|--------|-------------|
| `-r, --rate <speed>` | 出力速度（例: `2500000`, `24Mbps`） |
| `-d, --delay <seconds>` | 開始遅延（秒） |
| `-l, --loop` | ループモード（ファイル入力のみ） |
| `-c, --count <N>` | ループ回数（0 = 無限） |
| `-b, --buffer <size>` | バッファサイズ（byte）（デフォルト: 18800） |
| `-t, --time <seconds>` | 指定時間で終了（秒） |
| `-v, --verbose` | stderr に進捗表示 |
| `-h, --help` | ヘルプ表示 |
| `-V, --version` | バージョン表示 |

## 速度単位

| 単位 | 意味 |
|------|---------|
| `2500000` | byte/sec |
| `24Mbps` | メガビット/sec |
| `100KB/s` | キロバイト/sec |

内部ではすべて byte/sec に変換して処理。

## 使用例

### ファイルを 24 Mbps で送出

```bash
slowly -r 24Mbps stream.ts | receiver
```

### 3回ループ

```bash
slowly -l -c 3 -r 10Mbps stream.ts | analyzer
```

### ffmpeg からパイプ入力

```bash
ffmpeg -i input.mp4 -f mpegts - | slowly -r 8Mbps
```

### 5秒遅延開始

```bash
slowly -d 5 -r 18Mbps stream.ts | monitor
```

### 60秒間のみ出力

```bash
slowly -t 60 -r 10Mbps stream.ts > output.ts
```

## データ保証

入力と出力は byte 単位で完全に一致。データの変更、順序変更、欠落はありません。

```bash
certutil -hashfile input.ts SHA256
certutil -hashfile output.ts SHA256
# ハッシュ一致
```

## 設計思想

> 何も理解しない。ただ、ゆっくり流す。

`slowly` は入力データを解析・解釈しない。速度制御のみを行う単純なパイプ。

## ライセンス

MIT
