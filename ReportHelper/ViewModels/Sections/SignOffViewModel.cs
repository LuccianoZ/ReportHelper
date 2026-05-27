using CommunityToolkit.Mvvm.ComponentModel;
using ReportHelper.Models;
using ReportHelper.Services;
using ReportHelper.ViewModels.Base;

namespace ReportHelper.ViewModels.Sections
{
    public partial class SignOffViewModel : SectionViewModelBase
    {
        private readonly IStorageService _storageService;
        private readonly ReportRecord _reportRecord;

        [ObservableProperty]
        private bool saveButtonEnabled;
        [ObservableProperty]
        private bool saveFailed;

        public SignOffViewModel(IStorageService storageService, ReportRecord reportRecord)
        {
            _storageService = storageService;
            _reportRecord = reportRecord;
            SaveButtonEnabled = true;
            SaveFailed = false;
            ErrorMessage = string.Empty;
        }

        public void TrySaveReport()
        {
            try
            {
                _storageService.SaveReport(_reportRecord);
                SaveFailed = false;
                ErrorMessage = string.Empty;
            }
            catch (Microsoft.Data.Sqlite.SqliteException)
            {
                SaveFailed = true;
                SaveButtonEnabled = true;
                ErrorMessage = "Report could not be saved. Please check available disk space and try again.";
            }
        }
    }
}
