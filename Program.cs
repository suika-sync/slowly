using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

class SlowlyOptions
{
    public double Rate { get; set; } = 0;
    public int DelaySeconds { get; set; } = 0;
    public bool Loop { get; set; } = false;
    public int LoopCount { get; set; } = 0;
    public int BufferSize { get; set; } = 18800;
    public bool Verbose { get; set; } = false;
    public bool Help { get; set; } = false;
    public bool Version { get; set; } = false;
    public int TimeSeconds { get; set; } = 0;
    public string InputFile { get; set; } = null;
}

class Program
{
    const string VERSION = "1.0.0";
    static volatile bool _running = true;

    static void Main(string[] args)
    {
        var options = ParseArgs(args);

        if (options.Help)
        {
            ShowHelp();
            return;
        }

        if (options.Version)
        {
            Console.Error.WriteLine($"slowly.exe version {VERSION}");
            return;
        }

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            _running = false;
        };

        try
        {
            Run(options);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    static SlowlyOptions ParseArgs(string[] args)
    {
        var options = new SlowlyOptions();
        int i = 0;

        while (i < args.Length)
        {
            string argOrig = args[i];
            string argLower = argOrig.ToLower();

            switch (argLower)
            {
                case "-r":
                case "--rate":
                    options.Rate = ParseRate(args[++i]);
                    break;
                case "-d":
                case "--delay":
                    options.DelaySeconds = int.Parse(args[++i]);
                    break;
                case "-l":
                case "--loop":
                    options.Loop = true;
                    break;
                case "-c":
                case "--count":
                    options.LoopCount = int.Parse(args[++i]);
                    break;
                case "-b":
                case "--buffer":
                    options.BufferSize = int.Parse(args[++i]);
                    break;
                case "-v":
                case "--verbose":
                    options.Verbose = true;
                    break;
                case "-h":
                case "--help":
                    options.Help = true;
                    break;
                case "--version":
                    options.Version = true;
                    break;
                case "-t":
                case "--time":
                    options.TimeSeconds = int.Parse(args[++i]);
                    break;
                default:
                    if (argOrig == "-V")
                    {
                        options.Version = true;
                    }
                    else if (!argLower.StartsWith("-"))
                    {
                        options.InputFile = args[i];
                    }
                    break;
            }
            i++;
        }

        return options;
    }

    static double ParseRate(string s)
    {
        s = s.Trim().ToLower();

        if (s.EndsWith("mbps"))
            return double.Parse(s.Replace("mbps", "")) * 1_000_000.0 / 8.0;
        if (s.EndsWith("kbps"))
            return double.Parse(s.Replace("kbps", "")) * 1_000.0 / 8.0;
        if (s.EndsWith("bps"))
            return double.Parse(s.Replace("bps", "")) / 8.0;
        if (s.EndsWith("mb/s"))
            return double.Parse(s.Replace("mb/s", "")) * 1_000_000.0;
        if (s.EndsWith("kb/s"))
            return double.Parse(s.Replace("kb/s", "")) * 1_000.0;

        return double.Parse(s);
    }

    static void ShowHelp()
    {
        Console.Error.WriteLine("slowly.exe [options] <file>");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Options:");
        Console.Error.WriteLine("  -r, --rate <speed>     出力速度 (例: 2500000, 24Mbps)");
        Console.Error.WriteLine("  -d, --delay <seconds>  開始遅延 (秒)");
        Console.Error.WriteLine("  -l, --loop             ループモード (ファイル入力のみ)");
        Console.Error.WriteLine("  -c, --count <N>        ループ回数 (0=無限)");
        Console.Error.WriteLine("  -b, --buffer <size>    バッファサイズ (byte)");
        Console.Error.WriteLine("  -v, --verbose          詳細表示");
        Console.Error.WriteLine("  -t, --time <seconds>   実行時間 (秒)");
        Console.Error.WriteLine("  -h, --help             ヘルプ表示");
        Console.Error.WriteLine("  -V, --version          バージョン表示");
    }

    static void Run(SlowlyOptions options)
    {
        Stream inputStream;
        bool ownsStream = false;

        if (options.InputFile != null)
        {
            if (!File.Exists(options.InputFile))
                throw new FileNotFoundException($"File not found: {options.InputFile}");
            inputStream = new FileStream(options.InputFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            ownsStream = true;
        }
        else
        {
            inputStream = Console.OpenStandardInput();
        }

        var outputStream = Console.OpenStandardOutput();
        var buffer = new byte[options.BufferSize];

        if (options.DelaySeconds > 0)
        {
            if (options.Verbose)
                Console.Error.WriteLine($"Waiting {options.DelaySeconds} seconds...");
            Thread.Sleep(options.DelaySeconds * 1000);
        }

        var stopwatch = Stopwatch.StartNew();
        long totalSent = 0;
        int loopIteration = 0;

        try
        {
            while (_running)
            {
                if (options.TimeSeconds > 0 && stopwatch.Elapsed.TotalSeconds >= options.TimeSeconds)
                    break;

                int bytesRead = inputStream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    if (!options.Loop)
                        break;

                    loopIteration++;
                    if (options.LoopCount > 0 && loopIteration >= options.LoopCount)
                        break;

                    if (inputStream is FileStream fs)
                        fs.Seek(0, SeekOrigin.Begin);
                    else
                        break;

                    continue;
                }

                outputStream.Write(buffer, 0, bytesRead);
                outputStream.Flush();
                totalSent += bytesRead;

                if (options.Rate > 0)
                {
                    double targetSeconds = totalSent / options.Rate;
                    double actualSeconds = stopwatch.Elapsed.TotalSeconds;
                    double waitSeconds = targetSeconds - actualSeconds;

                    if (waitSeconds > 0)
                    {
                        int waitMs = (int)(waitSeconds * 1000);
                        if (waitMs > 0)
                            Thread.Sleep(waitMs);
                    }
                }

                if (options.Verbose)
                {
                    double elapsed = stopwatch.Elapsed.TotalSeconds;
                    double speedBps = (elapsed > 0) ? (totalSent * 8.0 / elapsed) : 0;
                    double sentMB = (double)totalSent / 1_000_000.0;
                    double speedMbps = speedBps / 1_000_000.0;
                    Console.Error.Write($"\rSent: {sentMB:F2} MB  Elapsed: {elapsed:F1}s  Speed: {speedMbps:F2} Mbps   ");
                }
            }
        }
        finally
        {
            outputStream.Flush();
            if (ownsStream)
                inputStream.Close();
            if (options.Verbose)
                Console.Error.WriteLine();
        }
    }
}
