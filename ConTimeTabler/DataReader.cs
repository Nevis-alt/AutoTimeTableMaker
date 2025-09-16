namespace ConTimeTabler;
using ClosedXML.Excel;
using System.Text.RegularExpressions;

class ExcelReader {
    string path;
    public ExcelReader(string path) {
        this.path = path;
    }
    public List<string> LoadDistinctCourseNames() {
        var names = new HashSet<string>();
        using (var workbook = new XLWorkbook(path)) {
            var ws = workbook.Worksheets.First();
            int row = 2; // 헤더는 1행
            while (!ws.Cell(row, ExcelColumns.Name).IsEmpty()) {
                names.Add(ws.Cell(row, ExcelColumns.Name).GetString());
                row++;
            }
        }
        return names.ToList();
    }
    public List<Course> LoadSelectCourses(List<string> selectedCourseNames)
    {
        var courseList = new List<Course>();
        using (var workbook = new XLWorkbook(path))
        {
            var ws = workbook.Worksheets.First(); // 첫 번째 시트 사용
            int row = 2; // 1행은 헤더라고 가정
            while (!ws.Cell(row, 1).IsEmpty())
            {
                if(!selectedCourseNames.Contains(ws.Cell(row, ExcelColumns.Name).GetString()))
                {
                    row++;
                    continue;
                }
                var course = new Course
                {
                    Grade = ws.Cell(row, ExcelColumns.Grade).GetValue<int>(),
                    Credit = ws.Cell(row, ExcelColumns.Credit).GetValue<int>(),
                    CourseID = ws.Cell(row, ExcelColumns.CourseID).GetString(),
                    CourseNumber = ws.Cell(row, ExcelColumns.CourseNumber).GetString(),
                    Division = ws.Cell(row, ExcelColumns.Division).GetString(),
                    ClassNumber = ws.Cell(row, ExcelColumns.ClassNumber).GetString(),
                    Times = GetTimesValue(ws.Cell(row, ExcelColumns.Times).GetString()),
                    Name = ws.Cell(row, ExcelColumns.Name).GetString(),
                    Professor = ws.Cell(row, ExcelColumns.Professor).GetString().TrimStart(),
                    Time = ws.Cell(row, ExcelColumns.Time).GetString()
                };
                courseList.Add(course);
                row++;
            }
        }
        return courseList;
    }
    public List<Course> LoadAllCourses(List<string> selectedCourseNames)
    {
        var courseList = new List<Course>();
        using (var workbook = new XLWorkbook(path))
        {
            var ws = workbook.Worksheets.First(); // 첫 번째 시트 사용
            int row = 2; // 1행은 헤더라고 가정
            while (!ws.Cell(row, 1).IsEmpty())
            {
                var course = new Course
                {
                    Grade = ws.Cell(row, ExcelColumns.Grade).GetValue<int>(),
                    Credit = ws.Cell(row, ExcelColumns.Credit).GetValue<int>(),
                    CourseID = ws.Cell(row, ExcelColumns.CourseID).GetString(),
                    CourseNumber = ws.Cell(row, ExcelColumns.CourseNumber).GetString(),
                    Division = ws.Cell(row, ExcelColumns.Division).GetString(),
                    ClassNumber = ws.Cell(row, ExcelColumns.ClassNumber).GetString(),
                    Times = GetTimesValue(ws.Cell(row, ExcelColumns.Times).GetString()),
                    Name = ws.Cell(row, ExcelColumns.Name).GetString(),
                    Professor = ws.Cell(row, ExcelColumns.Professor).GetString().TrimStart(),
                    Time = ws.Cell(row, ExcelColumns.Time).GetString()
                };
                courseList.Add(course);
                row++;
            }
        }
        return courseList;
    }
    public List<CourseTime> GetTimesValue(string timesString)
    {
        //Console.WriteLine($"디버깅 : {timesString}");
        var result = new List<CourseTime>();
        if (timesString == "(e-러닝)")
        {
            var courseTime = new CourseTime
            {
                day = DayOfWeek.e러닝,
                Room = "e러닝",
                start = 0,
                end = 0
            };
            //Console.WriteLine($"디버깅 : {courseTime}");
            result.Add(courseTime);
            return result;
        }
        var slots = timesString.Split(',');
        // 1. 월12-13(산학관) 또는 월12(산학관) 모두 처리
        Regex regex = new Regex(@"([월화수목금토일])([0-9]+)(?:-([0-9]+))?\((.+?)\)");
        foreach (var slot in slots)
        {
            var match = regex.Match(slot.Trim());
            if (match.Success)
            {
                int start = int.Parse(match.Groups[2].Value);
                int end = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : start;
                var courseTime = new CourseTime
                {
                    day = DayStringToEnum(match.Groups[1].Value),
                    Room = match.Groups[4].Value,
                    start = start,
                    end = end
                };
                result.Add(courseTime);
            }
        }
        return result;
    }
    static public DayOfWeek DayStringToEnum(string d) => d switch
    {
        "월" => DayOfWeek.월,
        "화" => DayOfWeek.화,
        "수" => DayOfWeek.수,
        "목" => DayOfWeek.목,
        "금" => DayOfWeek.금,
        "토" => DayOfWeek.토,
        "일" => DayOfWeek.일,
        "e러닝" => DayOfWeek.e러닝,
        _ => (DayOfWeek)(-1)
    };
}

static class ExcelColumns{
    public const int Grade = 2;
    public const int Credit = 12;// 학점
    public const int CourseID = 3;  // 학수번호
    public const int CourseNumber = 5;  // 교과번호
    public const int Division = 6;        // 이수구분
    public const int ClassNumber = 7;          // 과목번호
    public const int Times = 17; // 시간 (요일, 강의실, 시작시간, 종료시간)
    public const int Name = 8;           // 교과목명
    public const int Professor = 18;         // 교수명
    public const int Time = 13; // 강의 시간
}