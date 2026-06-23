using NAudio.Wave;
using System.IO;
using System.Text;
using Whisper.net;

namespace ReportHelper.Services
{
    public class VoiceInputService : IVoiceInputService 
    {
        private WaveInEvent? _waveIn;
        private MemoryStream? _audioBuffer;
        private readonly WaveFormat _waveFormat = new WaveFormat(16000, 1); // 16 kHz, mono — required by Whisper
        private readonly string _modelPath;
        private readonly string _tempWavPath;

        public VoiceInputService(string modelPath)
        {
            _modelPath = modelPath;
            _tempWavPath = Path.GetTempFileName() + ".wav";
        }

        public bool IsMicrophoneAvailable()
        {
            return WaveInEvent.DeviceCount > 0;
        }

        public void StartRecording()
        {
            if (!IsMicrophoneAvailable())
                return;

            _audioBuffer = new MemoryStream();
            var buffer = _audioBuffer; // local copy so the lambda captures a non-nullable reference
            _waveIn = new WaveInEvent();
            _waveIn.WaveFormat = _waveFormat;

            // Each time NAudio fills an audio buffer chunk, append it to our MemoryStream.
            // e.Buffer contains the raw audio bytes; e.BytesRecorded is how many are valid
            // (the buffer may be larger than the actual recorded data).
            _waveIn.DataAvailable += (sender, e) =>
            {
                buffer.Write(e.Buffer, 0, e.BytesRecorded);
            };

            _waveIn.StartRecording();
        }

        public async Task<string> StopAndTranscribe()
        {
            if (_waveIn == null)
                return string.Empty;

            _waveIn.StopRecording();
            _waveIn.Dispose();
            _waveIn = null;

            // Write the buffered audio bytes to a WAV file.
            // WaveFileWriter handles the WAV header automatically — we just give it raw bytes.
            // The writer must be disposed (flushed) before Whisper reads the file.
            _audioBuffer!.Position = 0;
            using (var writer = new WaveFileWriter(_tempWavPath, _waveFormat))
            {
                writer.Write(_audioBuffer.ToArray(), 0, (int)_audioBuffer.Length);
            }

            // Transcribe the WAV file using Whisper.
            // ProcessAsync returns IAsyncEnumerable<SegmentData> — one segment per phrase.
            // We await each segment and concatenate the text.
            var result = new StringBuilder();

            using var whisperFactory = WhisperFactory.FromPath(_modelPath);
            using var processor = whisperFactory
                .CreateBuilder()
                .WithLanguage("en")
                .Build();

            using var fileStream = File.OpenRead(_tempWavPath);
            await foreach (var segment in processor.ProcessAsync(fileStream))
            {
                result.Append(segment.Text);
            }

            return result.ToString().Trim();
        }

        public void Dispose()
        {
            _waveIn?.Dispose();
            _audioBuffer?.Dispose();
            if (File.Exists(_tempWavPath))
                File.Delete(_tempWavPath);
        }
    }
}
