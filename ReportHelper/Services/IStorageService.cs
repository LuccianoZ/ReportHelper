using ReportHelper.Models;

namespace ReportHelper.Services
{
    public interface IStorageService
    {
        void SaveDraft(ReportRecord draft); // called when officer completes a section
        ReportRecord? LoadDraft(); // called on launch to check for EC-04 recovery
        void SaveReport(ReportRecord report); // called when officer signs off on the report
        List<ReportRecord> GetAllReports();
    }
}
