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
    public List<Course> LoadCourses(List<string> selectedCourseNames)
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
                    Professor = ws.Cell(row, ExcelColumns.Professor).GetString(),
                    Time = ws.Cell(row, ExcelColumns.Time).GetString()
                };
                courseList.Add(course);
                row++;
            }
        }
        return courseList;
    }
    public List<(int day, string Room, int start, int end)> GetTimesValue(string timesString)
    {
        var result = new List<(int day, string room, int start, int end)>();
        if (timesString == "(e러닝)")
        {
            var day = -1; // e러닝은 요일이 없음
            var room = "e러닝";
            var start = 0;
            var end = 0;
            result.Add((day, room, start, end));
            return result;
        }
        var slots = timesString.Split(',');
        Regex regex = new Regex(@"([월화수목금])(\d+)-(\d+)\((.+?)\)");
        foreach (var slot in slots) {
            var match = regex.Match(slot.Trim());
            if (match.Success) {
                string day = match.Groups[1].Value;
                int start = int.Parse(match.Groups[2].Value);
                int end = int.Parse(match.Groups[3].Value);
                string room = match.Groups[4].Value;
                result.Add((DayStringToInt(day), room, start, end));
                //Console.WriteLine($"요일: {day}, 시작: {start}, 종료: {end}, 강의실: {room}");
            }
        }
        return result;
    }
    static public int DayStringToInt(string d) => d switch
    {
        "월" => 0,
        "화" => 1,
        "수" => 2,
        "목" => 3,
        "금" => 4,
        _ => -1
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