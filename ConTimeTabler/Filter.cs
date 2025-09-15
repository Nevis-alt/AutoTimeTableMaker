using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConTimeTabler;

// ================== 사후 필터 시스템 ==================
public interface IFilter
{
    bool Apply(List<Course> schedule);
}

// 점심시간 필터 <= 설정 시간 동안 수업이 없도록 강제
// 기본값은 12시부터 13시까지
// 필요시 생성자에 (시작시간, 종료시간) 튜플로 전달
// 예: new LunchBreakFilter((11, 12)) -> 11시부터 12시까지 점심시간
public class LunchBreakFilter : IFilter
{
    private readonly (int start, int end) lunchTime;
    public LunchBreakFilter() : this((12, 13)){}
    public LunchBreakFilter((int start, int end) lunchTime)
    {
        if (lunchTime.start < 0 || lunchTime.start > 24 || lunchTime.end < 0 || lunchTime.end > 24 || lunchTime.start >= lunchTime.end)
            throw new ArgumentException("점심시간은 0부터 24 사이의 값이어야 하며, 시작 시간이 종료 시간보다 작아야 합니다.");
        this.lunchTime = lunchTime;
    }

    public bool Apply(List<Course> schedule)
    {
        return !schedule.Any(c => c.Times.Any(t => t.start < lunchTime.end && t.end > lunchTime.start));
    }
}
// 특정 요일, 특정 시간대에 수업이 없도록 강제하는 필터
public class BreakTimeFilter : IFilter
{
    private readonly DayOfWeek day;                 // 요일 (enum)
    private readonly (int start, int end) timeRange;

    public BreakTimeFilter(DayOfWeek day, (int start, int end) timeRange)
    {
        if ((int)day < 0 || (int)day > 6)
            throw new ArgumentException("요일은 0(월)부터 6(일) 사이의 값이어야 합니다.");
        this.day = day;
        if (timeRange.start < 0 || timeRange.start > 24 || timeRange.end < 0 || timeRange.end > 24 || timeRange.start >= timeRange.end)
            throw new ArgumentException("시간대는 0부터 24 사이의 값이어야 하며, 시작 시간이 종료 시간보다 작아야 합니다.");
        this.timeRange = timeRange;
    }

    public bool Apply(List<Course> schedule)
    {
        return !schedule.Any(c =>
            c.Times.Any(t =>
                t.day == day && t.start < timeRange.end && t.end > timeRange.start));
    }
}

// 아침 수업 필터 <= 설정 시간 이전의 수업이 없도록 강제
// 기본값은 10시 이전 수업 금지
// 필요시 생성자에 시간(int)로 전달
// 예: new MorningFilter(9) -> 9시 이전 수업 금지
public class MorningFilter : IFilter
{
    private readonly int earliestAllowed;

    public MorningFilter(int earliestAllowed = 10) // 기본: 10시 이후만 허용
    {
        this.earliestAllowed = earliestAllowed;
    }

    public bool Apply(List<Course> schedule)
    {
        return !schedule.Any(c =>
            c.Times.Any(t => t.start < earliestAllowed));
    }
}


// 요일 공강 필터
public class NoDayFilter : IFilter
{
    private DayOfWeek dayNumber;
    public NoDayFilter(DayOfWeek dayNumber)
    {
        if ((int)dayNumber < 0 || (int)dayNumber > 6)
            throw new ArgumentException("요일은 0(월)부터 6(일) 사이의 값이어야 합니다.");
        this.dayNumber = dayNumber;
    }
    public bool Apply(List<Course> schedule)
    {
        return !schedule.Any(c => c.Times.Any(t => t.day == dayNumber));
    }
}