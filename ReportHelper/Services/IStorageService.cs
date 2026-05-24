namespace ReportHelper.Services
{
    public interface IStorageService
    {
        void SaveDraft(object draft); //called when officer completes a section
        object? LoadDraft(); //called on launch to check for EC-04 recovery
        void SaveReport(object report); //called when officer signs off on the report
        List<object> GetAllReports();
    }
}
