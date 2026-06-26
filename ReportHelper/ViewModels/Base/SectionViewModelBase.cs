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

        // BL-10: which field (by property name, e.g. "OfficerName") the officer is
        // currently dictating into. Needed because a section can have MULTIPLE
        // voice-enabled fields sharing this one base class's single DictatedText
        // property (e.g. ReportHeaderView has 3: Officer Name, Badge Number,
        // Unit/Division) — without this, a derived ViewModel has no way to know
        // which property a finished transcription belongs in. Set by
        // StartRecording(fieldName); read by the derived class's own
        // OnDictatedTextChanged partial-method hook (added per-ViewModel, since
        // only the derived class knows which property name maps to which
        // property — this base class only remembers the name, it doesn't route it).
        [ObservableProperty]
        private string _activeDictationField = string.Empty;

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

        // BL-10 overload: records WHICH field is being dictated into, alongside
        // the original behavior. Existing callers/tests using the parameterless
        // StartRecording() are untouched — this is purely additive, not a
        // signature change to the original method.
        public void StartRecording(string fieldName)
        {
            ActiveDictationField = fieldName;
            StartRecording();
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

        // ── BL-10 dictation routing ─────────────────────────────────────────
        //
        // IMPORTANT: this partial method hook MUST live here, in the same class
        // that declares the [ObservableProperty] field it hooks (_dictatedText).
        // CommunityToolkit.Mvvm's source generator only emits the matching
        // defining declaration of On<Property>Changed in the class where the
        // [ObservableProperty] attribute is applied — it is NOT inherited or
        // re-emitted for derived classes. Putting this same partial method
        // directly on ReportHeaderViewModel (a subclass) was tried first and
        // produced CS0759 ("no defining declaration found"), because the
        // generator never wrote one there — only here, on SectionViewModelBase,
        // does a matching defining declaration exist for it to implement.
        //
        // To let DERIVED classes still react to a completed dictation without
        // needing the generator's partial-method machinery themselves, this
        // hook does the minimal generic work (skip empty values, identify which
        // field was targeted) and then delegates to a normal protected virtual
        // method, OnDictationCompleted, which subclasses CAN safely override
        // using ordinary C# inheritance.
        partial void OnDictatedTextChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                // An empty transcription (including the self-clearing assignment
                // a derived class's OnDictationCompleted override may perform
                // after routing) means "nothing to route" — no-op rather than
                // re-invoking the virtual method with nothing useful to do.
                return;
            }

            OnDictationCompleted(ActiveDictationField, value);
        }

        // Called once per completed dictation, after DictatedText has been set
        // to a non-empty value. Base implementation does nothing — this base
        // class has no named fields of its own to route into; only a derived
        // ViewModel (e.g. ReportHeaderViewModel) knows which of ITS properties
        // fieldName refers to. Override this in a derived class to implement
        // that routing; the override is responsible for clearing DictatedText
        // (e.g. via the base class's own DictatedText setter) once it has
        // copied the value where it belongs, so a stale value can't be
        // reapplied to the wrong field later.
        protected virtual void OnDictationCompleted(string fieldName, string value)
        {
        }
    }
}
