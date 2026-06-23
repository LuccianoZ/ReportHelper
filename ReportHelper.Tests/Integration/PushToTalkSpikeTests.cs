using ReportHelper.Services;
using ReportHelper.ViewModels.Base;

namespace ReportHelper.Tests.Integration
{
    // A controlled fake that returns a predictable transcription result.
    // Unlike ThrowingVoiceService (which tests failure), this tests the happy path.
    public class FakeVoiceService : IVoiceInputService
    {
        public bool WasStartRecordingCalled { get; private set; } = false;

        public bool IsMicrophoneAvailable() => true;

        public void StartRecording() => WasStartRecordingCalled = true;

        // Returns a fixed string so the test can assert an exact value without
        // needing real audio hardware or a running Whisper model.
        public Task<string> StopAndTranscribe() =>
            Task.FromResult("On patrol I observed a vehicle.");

        public void Dispose() { }
    }

    public class PushToTalkSpikeTests
    {
        // SC-17: When the officer holds the Dictate button, the microphone activates
        // and IsRecording becomes true.
        [Fact]
        public void StartRecording_WhenMicAvailable_IsRecordingIsTrue()
        {
            var fake = new FakeVoiceService();
            var vm = new SectionViewModelBase(fake);

            vm.StartRecording();

            Assert.True(vm.IsRecording);
            Assert.True(fake.WasStartRecordingCalled); // confirms the service was actually called
        }

        // SC-18: When the officer releases the Dictate button, transcription appears
        // in the field and IsRecording returns to false.
        [Fact]
        public async Task StopAndTranscribeAsync_AfterRecording_PopulatesDictatedTextAndStopsRecording()
        {
            var vm = new SectionViewModelBase(new FakeVoiceService());
            vm.StartRecording();

            await vm.StopAndTranscribeAsync();

            Assert.Equal("On patrol I observed a vehicle.", vm.DictatedText);
            Assert.False(vm.IsRecording);
            Assert.Empty(vm.DictateErrorMessage);
        }
    }
}
