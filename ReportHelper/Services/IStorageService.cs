using ReportHelper.Models;

namespace ReportHelper.Services
{
    public interface IStorageService
    {
        void SaveDraft(ReportRecord draft); // called when officer completes a section
        ReportRecord? LoadDraft(); // called on launch to check for EC-04 recovery
        void SaveReport(ReportRecord report); // called when officer signs off on the report
        List<ReportRecord> GetAllReports();

        // S1.1 — generates the next report number for a given date, e.g. "20260623-0001".
        // Counts existing reports for that date and returns the next sequence number.
        // Called once when the officer starts a new report (BL-09).
        string GetNextReportNumber(DateTime date);
    }
}
