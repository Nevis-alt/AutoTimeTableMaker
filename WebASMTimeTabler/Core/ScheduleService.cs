using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebASMTimeTabler.Core;



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

public class ScheduleService
{
    private readonly int _maxPages;

    public ScheduleService(int maxPages = 50)
    {
        _maxPages = maxPages; // 최대 반환 개수
    }
    public List<List<Course>> GenerateSchedules(List<Course> selectedCourses)
    {
        if (selectedCourses == null || selectedCourses.Count == 0)
                    return new List<List<Course>>();
        //var allCourses = selectedCourses;//reader.LoadSelectCourses(selectedCourses);
        /* 디리디리디디딕디리버기기깅깅
        foreach (var course in allCourses)
        {
            Console.WriteLine($"과목명: {course.Name}, 교수명: {course.Professor}");
            foreach (var time in course.Times)
            {
                Console.WriteLine(time);
            }
        }
        */
        var groupedCourses = selectedCourses.GroupBy(c => c.Name)
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
        var result = new List<List<Course>>(_maxPages);

        // yield 기반 조합 생성, 최종 필터 적용 및 _maxPages개씩 반환
        foreach (var schedule in generator.Generate())
        {
            if (finalFilters.Any(f => !f.Apply(schedule)))
                continue;

            result.Add(schedule);

            if (result.Count >= _maxPages)
                break;
        }
        return result;
    }
}

