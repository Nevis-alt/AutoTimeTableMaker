using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace ConTimeTabler;


public enum DayOfWeek
{
    월 = 0,
    화 = 1,
    수 = 2,
    목 = 3,
    금 = 4,
    토 = 5,
    일 = 6,
    e러닝 = -1 // e러닝은 요일이 없음
}

public class Course
{
    public int Grade { get; init; }            // 학년
    public int Credit { get; init; }           // 학점
    public required string CourseID { get; init; }  // 학수번호
    public required string CourseNumber { get; init; }  // 교과번호
    public required string Division { get; init; }        // 이수구분
    public required string ClassNumber { get; init; }            // 과목번호
    public List<(DayOfWeek day, string Room, int start, int end)> Times { get; init; } = new(); // 시간 (요일, 강의실, 시작시간, 종료시간)
    public required string Name { get; init; }              // 교과목명
    public required string Professor { get; init; }         // 교수명
    public required string Time { get; init; }

    private string DayToString(DayOfWeek d) => d switch
    {
        DayOfWeek.월 => "월",
        DayOfWeek.화 => "화",
        DayOfWeek.수 => "수",
        DayOfWeek.목 => "목",
        DayOfWeek.금 => "금",
        DayOfWeek.토 => "토",
        DayOfWeek.일 => "일",
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
        /* //debuging
        for (int i = 0; i < courses.Count; i ++)
        {
            Console.WriteLine($"{i + 1}. {courses[i]}");
        }
        */
        

        var selectedCourses = new List<string>
        {
            courses[8], // 1번 과목 선택
            courses[3], // 2번 과목 선택
            courses[640],
            courses[883],
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
        // yield 기반 조합 생성, 유효성 검사 및 10개씩 출력
        int pageSize = 10;
        var buffer = new List<List<Course>>(pageSize);
        int page = 1;
        int idx = 1;
        foreach (var combination in GenerateSchedulesIterative(groupedCourses))
        {
            // 3. (필터를 적용한다) - 예시로 추가 필터 없음, 필요시 여기에 추가
            buffer.Add(combination);
            // 4. 10개가 되면 출력
            if (buffer.Count == pageSize)
            {
                Console.WriteLine($"=== 가능한 모든 시간표 조합 (충돌 없는 경우만) - {page}페이지 ===");
                foreach (var combi in buffer)
                {
                    Console.WriteLine($"[{idx++}]");
                    Console.WriteLine(string.Join(",\n ", combi));
                }
                buffer.Clear();
                page++;
                Console.WriteLine("--- 다음 페이지를 보려면 Enter를 누르세요 ---");
                Console.ReadLine();
            }
        }
        // 남은 조합 출력
        if (buffer.Count > 0)
        {
            Console.WriteLine($"=== 가능한 모든 시간표 조합 (충돌 없는 경우만) - {page}페이지 ===");
            foreach (var combi in buffer)
            {
                Console.WriteLine($"[{idx++}]");
                Console.WriteLine(string.Join(",\n ", combi));
            }
        }
    }
    // yield를 이용해 가능한 모든 조합을 하나씩 반환
    public static IEnumerable<List<Course>> GenerateSchedulesIterative(List<List<Course>> groups)
    {
        int n = groups.Count;
        if (n == 0) yield break;

        int[] idx = new int[n]; // 각 그룹에서 선택된 과목의 인덱스

        while (true)
        {
            var combination = new List<Course>(n);
            for (int i = 0; i < n; i++)
            {
                combination.Add(groups[i][idx[i]]);
            }

            // 시간 충돌 없는 경우만 반환
            if (IsValidSchedule(combination))
                yield return combination;

            // 다음 조합으로 이동 (odometer 방식)
            int k = n - 1;
            while (k >= 0)
            {
                idx[k]++;
                if (idx[k] < groups[k].Count) break;
                idx[k] = 0;
                k--;
            }
            if (k < 0) yield break; // 모든 조합 탐색 완료
        }
    }

    /// 시간표가 유효한지 검사 (과목 간 시간이 겹치지 않는지 확인)
    public static bool IsValidSchedule(List<Course> schedule)
    {
        var occupied = new HashSet<(DayOfWeek day, int hour)>();

        foreach (var course in schedule)
        {
            foreach (var t in course.Times)
            {
                for (int h = t.start; h <= t.end; h++)
                {
                    var slot = (t.day, h);
                    if (occupied.Contains(slot))
                    {
                        return false; // 이미 차지된 시간 → 충돌 발생
                    }
                    occupied.Add(slot);
                }
            }
        }
        return true; // 충돌 없음
    }

    static string DayToString(DayOfWeek d) => d switch
    {
        DayOfWeek.월 => "월",
        DayOfWeek.화 => "화",
        DayOfWeek.수 => "수",
        DayOfWeek.목 => "목",
        DayOfWeek.금 => "금",
        DayOfWeek.토 => "토",
        DayOfWeek.일 => "일",
        DayOfWeek.e러닝 => "e러닝",
        _ => "?"
    };
}

