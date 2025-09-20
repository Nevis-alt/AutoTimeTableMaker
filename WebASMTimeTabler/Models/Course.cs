namespace WebASMTimeTabler.Models;

public class CourseTime
{
    public DayOfWeek day { get; init; }
    public string Room { get; init; } = string.Empty;
    public int start { get; init; }
    public int end { get; init; }

    public override string ToString()
    {
        string[] koreanDays = { "월", "화", "수", "목", "금", "토", "일" };
        string dayStr;
        int intDay = (int)day;
        if (intDay == -1)
        {
            return "(e러닝)";
        }
        else if (intDay >= 0 && intDay <= 6)
        {
            // 0=월, 1=화, ..., 6=일로 출력
            dayStr = koreanDays[intDay];
        }
        else
        {
            dayStr = "알수없음";
        }
        return $"({dayStr}, {Room}, {start}-{end})";
    }
}
public class Course
{
    public int Grade { get; init; }            // 학년
    public int Credit { get; init; }           // 학점
    public required string CourseID { get; init; }  // 학수번호
    public required string CourseNumber { get; init; }  // 교과번호
    public required string Division { get; init; }        // 이수구분
    public required string ClassNumber { get; init; }            // 과목번호
    public IReadOnlyList<CourseTime> Times { get; init; } = new List<CourseTime>(); // 시간 (요일, 강의실, 시작시간, 종료시간)
    public required string Name { get; init; }              // 교과목명
    public required string Professor { get; init; }         // 교수명 (여러 명일 경우 ,로 구분)
    public required string Time { get; init; }
    public override string ToString()
    {
        return $"{CourseID} {Name} ({Credit}학점) - {Professor} {string.Join(", ", Times)}";
    }
}
// Course 비교용 클래스 (ClassNumber 기준)
public class CourseComparer : IEqualityComparer<Course>
{
    public bool Equals(Course? x, Course? y)
    {
        if (x is null || y is null) return false;
        return x.ClassNumber == y.ClassNumber;
    }

    public int GetHashCode(Course obj)
    {
        return obj.ClassNumber?.GetHashCode() ?? 0;
    }
}
