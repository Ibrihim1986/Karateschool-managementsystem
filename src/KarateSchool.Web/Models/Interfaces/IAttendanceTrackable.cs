namespace KarateSchool.Web.Models.Interfaces;

public interface IAttendanceTrackable
{
    IReadOnlyCollection<Entities.Attendance> AttendanceRecords { get; }

    void RecordAttendance(Entities.Attendance attendance);

    double GetAttendanceRate();
}
