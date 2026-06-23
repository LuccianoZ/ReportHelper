using ReportHelper.Services;
using ReportHelper.ViewModels.Base;

namespace ReportHelper.Tests.Unit
{
    // --- Test stubs ---

    // TC-EH-01: Simulates Whisper throwing a runtime exception.
    // IsMicrophoneAvailable returns true so the Dictate button starts enabled —
    // the point of this test is the transcription step failing, not the mic check.
    public class ThrowingVoiceService : IVoiceInputService
    {
        public bool IsMicrophoneAvailable() => true;
        public void StartRecording() { }
        public Task<string> StopAndTranscribe() => throw new Exception("Whisper model failed");
        public void Dispose() { }
    }

    // TC-EH-03: Simulates no microphone being available on the machine.
    public class NoMicVoiceService : IVoiceInputService
    {
        public bool IsMicrophoneAvailable() => false;
        public void StartRecording() { }
        public Task<string> StopAndTranscribe() => Task.FromResult(string.Empty);
        public void Dispose() { }
    }

    // --- Tests ---

    public class VoiceInputTests
    {
        // TC-EH-01
        // When Whisper throws, the dictated field must stay empty, an error message
        // must be shown, and the Dictate button must stay enabled so the officer can retry.
        [Fact]
        public async Task StopAndTranscribeAsync_WhenWhisperThrows_SetsErrorAndLeavesFieldEmpty()
        {
            // Arrange
            var viewModel = new SectionViewModelBase(new ThrowingVoiceService());

            // Act
            await viewModel.StopAndTranscribeAsync();

            // Assert
            Assert.Equal(string.Empty, viewModel.DictatedText);        
            Assert.NotEmpty(viewModel.DictateErrorMessage);             
            Assert.True(viewModel.IsDictateEnabled);                    
        }

        // TC-EH-03
        // When the microphone is unavailable, IsDictateEnabled must be false from the
        // moment the ViewModel is constructed — the Dictate button is disabled before
        // the officer even tries to use it.
        [Fact]
        public void Constructor_WhenMicUnavailable_IsDictateEnabledIsFalse()
        {
            // Arrange + Act
            var viewModel = new SectionViewModelBase(new NoMicVoiceService());

            // Assert
            Assert.False(viewModel.IsDictateEnabled);
            Assert.Equal(string.Empty, viewModel.DictatedText);         
        }
    }
}
