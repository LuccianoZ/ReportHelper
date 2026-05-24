
namespace ReportHelper.Services
{
    public interface IVoiceInputService : IDisposable
    {
        bool IsMicrophoneAvailable();
        void StartRecording();
        string StopAndTranscribe(); //called when officer finishes dictating a section, returns the transcribed text
    }
}
