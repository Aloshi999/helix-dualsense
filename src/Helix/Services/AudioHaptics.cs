using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Helix.Services;

public sealed class AudioHaptics : IDisposable
{
    private readonly object _gate = new();
    private WasapiLoopbackCapture? _capture;
    private float _rms;
    private bool _loggedSkip;

    public bool IsRunning => _capture is not null;

    public void SetEnabled(bool enabled)
    {
        if (enabled)
        {
            Start();
        }
        else
        {
            Stop();
        }
    }

    public (byte Large, byte Small) Sample()
    {
        lock (_gate)
        {
            var large = (byte)Math.Clamp((int)(Math.Clamp(_rms * 3.4f, 0f, 1f) * 255f), 0, 255);
            return (large, (byte)(large * 3 / 4));
        }
    }

    private void Start()
    {
        if (_capture is not null)
        {
            return;
        }

        if (!OperatingSystem.IsWindows())
        {
            if (!_loggedSkip)
            {
                AppLog.Info("Audio-to-haptics is Windows-only.");
                _loggedSkip = true;
            }

            return;
        }

        try
        {
            var capture = new WasapiLoopbackCapture();
            capture.DataAvailable += OnData;
            capture.RecordingStopped += (_, e) =>
            {
                if (e.Exception is not null)
                {
                    AppLog.Warn("Audio capture stopped: " + e.Exception.Message);
                }
            };
            capture.StartRecording();
            _capture = capture;
            AppLog.Info("Audio-to-haptics started.");
        }
        catch (Exception ex)
        {
            AppLog.Warn("Audio-to-haptics unavailable: " + ex.Message);
        }
    }

    private void OnData(object? sender, WaveInEventArgs e)
    {
        if (e.BytesRecorded < 8 || _capture?.WaveFormat is not { } format)
        {
            return;
        }

        double energy = 0;
        var samples = 0;
        if (format.BitsPerSample == 32 && format.Encoding == WaveFormatEncoding.IeeeFloat)
        {
            var count = e.BytesRecorded / 4;
            for (var i = 0; i < count; i++)
            {
                var sample = BitConverter.ToSingle(e.Buffer, i * 4);
                energy += sample * sample;
                samples++;
            }
        }
        else if (format.BitsPerSample == 16)
        {
            var count = e.BytesRecorded / 2;
            for (var i = 0; i < count; i++)
            {
                var sample = BitConverter.ToInt16(e.Buffer, i * 2) / 32768.0;
                energy += sample * sample;
                samples++;
            }
        }

        if (samples == 0)
        {
            return;
        }

        var rms = (float)Math.Sqrt(energy / samples);
        lock (_gate)
        {
            _rms = _rms * 0.65f + rms * 0.35f;
        }
    }

    private void Stop()
    {
        var capture = _capture;
        _capture = null;
        if (capture is null)
        {
            return;
        }

        try
        {
            capture.DataAvailable -= OnData;
            capture.StopRecording();
            capture.Dispose();
        }
        catch (Exception)
        {
            // Capture can already be torn down by the audio session.
        }

        lock (_gate)
        {
            _rms = 0;
        }
    }

    public void Dispose() => Stop();
}
