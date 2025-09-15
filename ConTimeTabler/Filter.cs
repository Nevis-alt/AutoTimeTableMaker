using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConTimeTabler;

// ================== 실시간 필터 시스템 ==================
public interface IRealtimeFilter
{
    // nextCourse: 새로 추가할 과목, occupiedSlots: 현재까지 예약된 (요일, 시간) 쌍
    bool Apply(Course nextCourse, HashSet<(DayOfWeek day, int hour)> occupiedSlots);
}


// 점심시간 필터 <= 설정 시간 동안 수업이 없도록 강제
// 기본값은 12시부터 13시까지
// 필요시 생성자에 (시작시간, 종료시간) 튜플로 전달
// 예: new LunchBreakFilter((11, 12)) -> 11시부터 12시까지 점심시간
public class LunchBreakFilter : IRealtimeFilter
{
    private readonly (int start, int end) lunchTime;
    public LunchBreakFilter() : this((12, 13)){}
    public LunchBreakFilter((int start, int end) lunchTime)
    {
        if (lunchTime.start < 0 || lunchTime.start > 24 || lunchTime.end < 0 || lunchTime.end > 24 || lunchTime.start >= lunchTime.end)
            throw new ArgumentException("점심시간은 0부터 24 사이의 값이어야 하며, 시작 시간이 종료 시간보다 작아야 합니다.");
        this.lunchTime = lunchTime;
    }

    public bool Apply(Course nextCourse, HashSet<(DayOfWeek day, int hour)> occupiedSlots)
    {
        // nextCourse만으로도 점심시간 침범 여부 판단
        return !nextCourse.Times.Any(t => t.start < lunchTime.end && t.end > lunchTime.start);
    }
}
// 특정 요일, 특정 시간대에 수업이 없도록 강제하는 필터
public class BreakTimeFilter : IRealtimeFilter
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

    public bool Apply(Course nextCourse, HashSet<(DayOfWeek day, int hour)> occupiedSlots)
    {
        // nextCourse만으로도 해당 요일, 시간대 침범 여부 판단
        return !nextCourse.Times.Any(t => t.day == day && t.start < timeRange.end && t.end > timeRange.start);
    }
}

// 아침 수업 필터 <= 설정 시간 이전의 수업이 없도록 강제
// 기본값은 10시 이전 수업 금지
// 필요시 생성자에 시간(int)로 전달
// 예: new MorningFilter(9) -> 9시 이전 수업 금지
public class MorningFilter : IRealtimeFilter
{
    private readonly int earliestAllowed;

    public MorningFilter(int earliestAllowed = 10) // 기본: 10시 이후만 허용
    {
        this.earliestAllowed = earliestAllowed;
    }

    public bool Apply(Course nextCourse, HashSet<(DayOfWeek day, int hour)> occupiedSlots)
    {
        // nextCourse만으로도 아침 수업 여부 판단
        return !nextCourse.Times.Any(t => t.start < earliestAllowed);
    }
}

// 시간 충돌 필터
public class TimeConflictFilter : IRealtimeFilter
{
    public bool Apply(Course nextCourse, HashSet<(DayOfWeek day, int hour)> occupied)
    {
        foreach (var t in nextCourse.Times)
        {
            for (int h = t.start; h <= t.end; h++)
            {
                if (occupied.Contains((t.day, h)))
                    return false;
            }
        }
        return true;
    }
}

// 요일 공강 필터
public class NoDayFilter : IRealtimeFilter
{
    private DayOfWeek dayNumber;
    public NoDayFilter(DayOfWeek dayNumber)
    {
        if ((int)dayNumber < 0 || (int)dayNumber > 6)
            throw new ArgumentException("요일은 0(월)부터 6(일) 사이의 값이어야 합니다.");
        this.dayNumber = dayNumber;
    }
    public bool Apply(Course nextCourse, HashSet<(DayOfWeek day, int hour)> occupiedSlots)
    {
        // nextCourse만으로도 해당 요일 수업 여부 판단
        return !nextCourse.Times.Any(t => t.day == dayNumber);
    }
}

// 연강 제한 필터
// 최대 연강 시간을 설정하여 그 이상인 스케줄을 제거
// 기본값은 4시간
public class MaxConsecutiveFilter : IRealtimeFilter
{
    private readonly int maxConsecutive; // 최대 연강 시간
    public MaxConsecutiveFilter(int maxConsecutive = 4)
    {
        if (maxConsecutive <= 0) throw new ArgumentException("연강 제한은 1시간 이상이어야 합니다.");
        this.maxConsecutive = maxConsecutive;
    }
    public bool Apply(Course nextCourse, HashSet<(DayOfWeek day, int hour)> occupied)
    {
        // nextCourse의 각 시간 슬롯 확인
        foreach (var t in nextCourse.Times)
        {
            int start = t.start;
            int end = t.end;

            int consecutiveBefore = 0;
            int consecutiveAfter = 0;

            // 이전 시간 확인
            for (int h = start - 1; h >= start - maxConsecutive; h--)
            {
                if (occupied.Contains((t.day, h)))
                    consecutiveBefore++;
                else
                    break;
            }

            // 이후 시간 확인
            for (int h = end + 1; h <= end + maxConsecutive; h++)
            {
                if (occupied.Contains((t.day, h)))
                    consecutiveAfter++;
                else
                    break;
            }

            int totalConsecutive = consecutiveBefore + end - start + 1 + consecutiveAfter;

            if (totalConsecutive > maxConsecutive)
                return false; // 연강 제한 초과
        }

        return true;
    }
}

// ================== 조합 완료 후 최종 필터 인터페이스 ==================
public interface IFinalFilter
{
    bool Apply(List<Course> schedule);
}

// 유휴시간 최소화 필터
// 최대 허용 유휴 시간을 설정하여 그 이상인 스케줄을 제거
// 기본값은 2시간
// 필요시 생성자에 최대 허용 유휴 시간(int)로 전달
// 예: new IdleTimeFilter(1) -> 최대 1시간 유휴 시간 허용
public class IdleTimeFilter : IFinalFilter
{
    private readonly int maxIdle; // 최대 허용 유휴 시간 (시간 단위)
    public IdleTimeFilter(int maxIdle = 2)
    {
        this.maxIdle = maxIdle;
    }

    public bool Apply(List<Course> schedule)
    {
        // 요일별 그룹핑
        var byDay = schedule
            .SelectMany(c => c.Times.Select(t => (c, t.day, t.start, t.end)))
            .GroupBy(x => x.day);

        foreach (var dayGroup in byDay)
        {
            var times = dayGroup.OrderBy(t => t.start).ToList();
            for (int i = 1; i < times.Count; i++)
            {
                int idle = times[i].start - times[i - 1].end;
                if (idle > maxIdle)
                    return false; // 유휴 시간이 너무 길면 제거
            }
        }
        return true;
    }
}

// 하루 최대 수업 필터
public class MaxDailyCoursesFilter : IFinalFilter
{
    private readonly int maxCoursesPerDay;
    public MaxDailyCoursesFilter(int maxCoursesPerDay = 3)
    {
        this.maxCoursesPerDay = maxCoursesPerDay;
    }

    public bool Apply(List<Course> schedule)
    {
        var byDay = schedule
            .SelectMany(c => c.Times.Select(t => (t.day, c)))
            .GroupBy(x => x.day);

        foreach (var dayGroup in byDay)
        {
            if (dayGroup.Count() > maxCoursesPerDay)
                return false;
        }
        return true;
    }
}

// 총 학점 필터
// 최소 및 최대 학점을 설정하여 그 범위를 벗어나는 스케줄을 제거
// 기본값은 최대 18학점
public class TotalCreditFilter : IFinalFilter
{
    private readonly int minCredit;
    private readonly int maxCredit;
    public TotalCreditFilter(int minCredit, int maxCredit = 18)
    {
        this.minCredit = minCredit;
        this.maxCredit = maxCredit;
    }

    public bool Apply(List<Course> schedule)
    {
        int total = schedule.Sum(c => c.Credit);
        return total >= minCredit && total <= maxCredit;
    }
}