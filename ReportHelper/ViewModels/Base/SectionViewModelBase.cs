using CommunityToolkit.Mvvm.ComponentModel;
using ReportHelper.Services;

namespace ReportHelper.ViewModels.Base
{
    public partial class SectionViewModelBase : ObservableObject
    {
        [ObservableProperty]
        private string _sectionTitle = string.Empty;

        [ObservableProperty]
        private bool _canAdvance = false;

        [ObservableProperty]
        private string? _errorMessage = string.Empty;

        // The transcribed text returned by the voice engine for the current field.
        [ObservableProperty]
        private string _dictatedText = string.Empty;

        // Shown to the officer when transcription fails. Separate from ErrorMessage
        [ObservableProperty]
        private string _dictateErrorMessage = string.Empty;

        // False when no microphone is detected — disables the Dictate button in the UI.
        [ObservableProperty]
        private bool _isDictateEnabled = false;

        // True while the mic is actively recording — drives the recording indicator in the UI.
        [ObservableProperty]
        private bool _isRecording = false;

        public Dictionary<string, string> RequiredFields { get; } = new Dictionary<string, string>();

        private readonly IVoiceInputService? _voiceInputService;

        // voiceInputService is optional (default null) so that existing tests that use
        // new SectionViewModelBase() without a voice service continue to compile and pass.
        public SectionViewModelBase(IVoiceInputService? voiceInputService = null)
        {
            _voiceInputService = voiceInputService;
            IsDictateEnabled = voiceInputService?.IsMicrophoneAvailable() ?? false;
        }

        public void StartRecording()
        {
            _voiceInputService?.StartRecording();
            IsRecording = true;
        }

        public async Task StopAndTranscribeAsync()
        {
            IsRecording = false;
            const string voiceErrorMessage = "Transcription failed. Please try again or type manually.";

            try
            {
                var result = await _voiceInputService!.StopAndTranscribe();

                if (string.IsNullOrWhiteSpace(result))
                {
                    // Whisper returned nothing — treat as a failure
                    DictateErrorMessage = voiceErrorMessage;
                }
                else
                {
                    DictatedText = result;
                    DictateErrorMessage = string.Empty;
                }
            }
            catch
            {
                // Whisper threw a runtime exception (model error, OOM, etc.).
                // DictatedText is intentionally left as-is so prior content is preserved.
                // IsDictateEnabled stays true — the officer can retry.
                DictateErrorMessage = voiceErrorMessage;
            }
        }

        protected virtual void ValidateRequiredFields()
        {
            foreach (var field in RequiredFields)
            {
                if (string.IsNullOrWhiteSpace(field.Value))
                {
                    ErrorMessage = $"Please fill out the required field: {field.Key}";
                    CanAdvance = false;
                    return;
                }
            }

            ErrorMessage = string.Empty;
            CanAdvance = true;
        }

        public event EventHandler? SectionAdvanced;

        public void OnConfirm()
        {
            ValidateRequiredFields();

            if (CanAdvance)
            {
                SectionAdvanced?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
