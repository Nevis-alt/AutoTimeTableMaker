namespace WebASMTimeTabler.Core;

public enum DayOfWeek
{
    월 = 1,
    화 = 2,
    수 = 3,
    목 = 4,
    금 = 5,
    토 = 6,
    일 = 0,
    e러닝 = -1 // e러닝은 요일이 없음
}
public class CourseTime
{
    public DayOfWeek day { get; init; }
    public string Room { get; init; } = string.Empty;
    public int start { get; init; }
    public int end { get; init; }

    public override string ToString()
    {
        string dayStr = day switch
        {
            DayOfWeek.월 => "월",
            DayOfWeek.화 => "화",
            DayOfWeek.수 => "수",
            DayOfWeek.목 => "목",
            DayOfWeek.금 => "금",
            DayOfWeek.토 => "토",
            DayOfWeek.일 => "일",
            DayOfWeek.e러닝 => "e러닝",
            _ => "알수없음"
        };
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
