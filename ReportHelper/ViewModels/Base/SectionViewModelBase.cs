using CommunityToolkit.Mvvm.ComponentModel;

namespace ReportHelper.ViewModels.Base
{
    public partial class SectionViewModelBase : ObservableObject
    {
        [ObservableProperty]
        private string _sectionTitle = string.Empty; //description of the section the officer is curerntly in e.g. "Incident Description", "Suspect Information", etc.
        [ObservableProperty]
        private bool _canAdvance = false; //whether the officer has completed the current section and can move on to the next one
        [ObservableProperty]
        private string? _errorMessage = string.Empty;
        public Dictionary<string, string> RequiredFields { get; } = new Dictionary<string, string>();

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
