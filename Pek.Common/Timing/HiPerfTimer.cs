using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pek.Timing;

/// <summary>
/// 纳秒级计时器（跨平台兼容）
/// </summary>
public class HiPerfTimer
{
    private Int64 _startTime;
    private Int64 _stopTime;
    private readonly Int64 _freq;
    private Stopwatch? _sw;
    private static readonly Boolean IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [DllImport("Kernel32.dll")]
    private static extern Boolean QueryPerformanceCounter(out Int64 lpPerformanceCount);

    [DllImport("Kernel32.dll")]
    private static extern Boolean QueryPerformanceFrequency(out Int64 lpFrequency);

    public HiPerfTimer()
    {
        if (IsWindows)
        {
            if (!QueryPerformanceFrequency(out _freq))
                throw new Win32Exception();
        }
        else
        {
            _sw = new Stopwatch();
        }
    }

    public void Start()
    {
        Thread.Sleep(0);
        if (IsWindows)
            QueryPerformanceCounter(out _startTime);
        else
            _sw?.Start();
    }

    public static HiPerfTimer StartNew()
    {
        var timer = new HiPerfTimer();
        timer.Start();
        return timer;
    }

    public void Stop()
    {
        if (IsWindows)
            QueryPerformanceCounter(out _stopTime);
        else
            _sw?.Stop();
    }

    public Double Duration
    {
        get
        {
            if (IsWindows)
                return (_stopTime - _startTime) / (Double)_freq;
            else
                return _sw?.Elapsed.TotalSeconds ?? 0;
        }
    }

    public static Double Execute(Action action)
    {
        var timer = new HiPerfTimer();
        timer.Start();
        action();
        timer.Stop();
        return timer.Duration;
    }
}