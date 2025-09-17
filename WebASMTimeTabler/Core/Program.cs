using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace ConTimeTabler;


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
        return $"({day}, {Room}, {start}-{end})";
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
        DayOfWeek.e러닝 => "e러닝",
        _ => "?"
    };
    public override string ToString()
    {
        return $"{Name}({CourseID} | {ClassNumber}) - {Professor} [{string.Join(", ", Times.Select(t => $"({DayToString(t.day)}, {t.Room}, {t.start}-{t.end})"))}]";
    }
}

public class ScheduleGenerator
{
    private readonly List<List<Course>> _groups;
    private readonly List<IRealtimeFilter> _filters;
    public ScheduleGenerator(List<List<Course>> groups, List<IRealtimeFilter> filters)
    {
        _groups = groups;
        _filters = filters ?? new List<IRealtimeFilter>();
    }

    public IEnumerable<List<Course>> Generate()
    {
        int n = _groups.Count;
        if (n == 0) yield break;

        int[] idx = new int[n];
        List<Course> schedule = new();

        while (true)
        {
            schedule.Clear();
            var occupiedTimeSlots = new HashSet<(DayOfWeek day, int hour)>();
            bool valid = true;

            // 현재 idx 조합에 대한 schedule 생성
            for (int i = 0; i < n; i++)
            {
                var course = _groups[i][idx[i]];

                // 실시간 필터 적용
                foreach (var filter in _filters)
                {
                    if (!filter.Apply(course, occupiedTimeSlots))
                    {
                        valid = false;
                        break;
                    }
                }

                if (!valid) break;

                schedule.Add(course);
                foreach (var t in course.Times)
                    for (int h = t.start; h <= t.end; h++)
                        occupiedTimeSlots.Add((t.day, h));
            }

            if (valid)
                yield return new List<Course>(schedule);

            // 다음 조합 (odometer 방식) + 백트래킹
            int k = n - 1;
            while (k >= 0)
            {
                idx[k]++;
                if (idx[k] < _groups[k].Count) break;

                // 이전 과목의 시간 제거
                var prevCourse = _groups[k][idx[k] - 1];
                foreach (var t in prevCourse.Times)
                    for (int h = t.start; h <= t.end; h++)
                        occupiedTimeSlots.Remove((t.day, h));

                idx[k] = 0;
                k--;
            }

            if (k < 0) yield break;

            // 새 과목 추가
            var nextCourse = _groups[k][idx[k]];
            foreach (var t in nextCourse.Times)
                for (int h = t.start; h <= t.end; h++)
                    occupiedTimeSlots.Add((t.day, h));
        }
    }
}

class Program
{
    static void Run()
    {
        string filePath = "Data/DataBase.xlsx";

        var reader = new ExcelReader(filePath);
        var courses = reader.LoadDistinctCourseNames();
        //debuging
        for (int i = 0; i < courses.Count; i ++)
        {
            Console.WriteLine($"{i + 1}. {courses[i]}");
        }
        JsonLoader.Load(reader.LoadAllCourses(courses));


        var selectedCourses = new List<string>
        {
            courses[8], // 1번 과목 선택
            //courses[3], // 2번 과목 선택
            courses[640],
            //courses[883],
        };
        var allCourses = reader.LoadSelectCourses(selectedCourses);
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

        var realtimeFilters = new List<IRealtimeFilter>
        {
            new TimeConflictFilter(),
            // new LunchBreakFilter((DayOfWeek.수, 12, 13)), 등등 추가 가능
        };
        var finalFilters = new List<IFinalFilter>
        {
            new IdleTimeFilter(2), // 최대 2시간 유휴 시간 허용
            // 필요시 추가 가능
        };

        var generator = new ScheduleGenerator(groupedCourses, realtimeFilters);

        int pageSize = 10, page = 1, idx = 1;
        var buffer = new List<List<Course>>(pageSize);

        // yield 기반 조합 생성, 최종 필터 적용 및 10개씩 출력
        foreach (var schedule in generator.Generate())
        {
            if (finalFilters.Any(f => !f.Apply(schedule)))
                continue;
            buffer.Add(schedule);

            if (buffer.Count == pageSize)
            {
                Console.WriteLine($"=== {page} 페이지 ===");
                foreach (var combi in buffer)
                {
                    Console.WriteLine($"[{idx++}]");
                    Console.WriteLine(string.Join(",\n ", combi));
                }
                buffer.Clear();
                page++;
                Console.WriteLine("--- 다음 페이지를 보려면 Enter ---");
                Console.ReadLine();
            }
        }
        if (buffer.Count > 0)
        {
            Console.WriteLine($"=== {page} 페이지 ===");
            foreach (var combi in buffer)
            {
                Console.WriteLine($"[{idx++}]");
                Console.WriteLine(string.Join(",\n ", combi));
            }
        }
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

