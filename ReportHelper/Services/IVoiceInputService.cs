namespace ReportHelper.Services
{
    public interface IVoiceInputService : IDisposable
    {
        bool IsMicrophoneAvailable();
        void StartRecording();
        Task<string> StopAndTranscribe(); // async — Whisper.net 1.9.0 has no sync API
    }
}
