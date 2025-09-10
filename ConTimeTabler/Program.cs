using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace ConTimeTabler;

class Course
{
    public required int Grade { get; set; }            // 학년
    public required int Credit { get; set; }           // 학점
    public required string CourseID { get; set; }  // 학수번호
    public required string CourseNumber { get; set; }  // 교과번호
    public required string Division { get; set; }        // 이수구분
    public required string ClassNumber { get; set; }            // 과목번호
    public required List<(int day, string Room, int start, int end)> Times { get; set; } = []; // 시간 (요일, 강의실, 시작시간, 종료시간)
    public required string Name { get; set; }              // 교과목명
    public required string Professor { get; set; }         // 교수명
    public required string Time { get; set; }
    
    private string DayToString(int d) => d switch
    {
        0 => "월",
        1 => "화",
        2 => "수",
        3 => "목",
        4 => "금",
        _ => "?"
    };
    public override string ToString()
    {
        return $"{Name}({CourseID} | {ClassNumber}) - {Professor} [{string.Join(", ", Times.Select(t => $"({DayToString(t.day)}, {t.Room}, {t.start}-{t.end})"))}]";
    }
}


class Program
{
    static void Main()
    {
        string filePath = "Data/DataBase.xlsx";

        var reader = new ExcelReader(filePath);
        var courses = reader.LoadDistinctCourseNames();
        /*
        for (int i = 0; i < courses.Count; i += 5)
        {
            Console.WriteLine($"{i + 1}. {courses[i]}");
        }
        */

        var selectedCourses = new List<string>
        {
            courses[8], // 1번 과목 선택
            courses[3], // 2번 과목 선택
            courses[10]
        };
        var allCourses = reader.LoadCourses(selectedCourses);
        foreach (var course in allCourses)
        {
            Console.WriteLine($"과목명: {course.Name}, 교수명: {course.Professor}");
            foreach (var time in course.Times)
            {
                Console.WriteLine($"  요일: {DayToString(time.day)}, 강의실: {time.Room}, 시작: {time.start}, 종료: {time.end}");
            }
        }
        var groupedCourses = allCourses.GroupBy(c => c.Name)
                      .Select(g => g.ToList())
                      .ToList();
        var allCombinations = GetAllCombinations(groupedCourses);

        // 출력
        Console.WriteLine("=== 가능한 모든 시간표 조합 ===");
        foreach (var combination in allCombinations)
        {
            Console.WriteLine(string.Join(",\n ", combination));
        }
    }
    // 교과목별 리스트에서 가능한 모든 조합 생성
    static List<List<T>> GetAllCombinations<T>(List<List<T>> lists)
    {
        List<List<T>> result = [new List<T>()];

        foreach (var list in lists)
        {
            var temp = new List<List<T>>();
            foreach (var prefix in result)
            {
                foreach (var item in list)
                {
                    var newCombination = new List<T>(prefix) { item };
                    temp.Add(newCombination);
                }
            }
            result = temp;
        }

        return result;
    }
    static string DayToString(int d) => d switch
    {
        0 => "월",
        1 => "화",
        2 => "수",
        3 => "목",
        4 => "금",
        _ => "?"
    };
    
}

