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

// 점심시간 필터 (12~13시에는 수업 금지)
public class LunchBreakFilter : IFilter
{
    public bool Apply(List<Course> schedule)
    {
        return !schedule.Any(c => c.Times.Any(t => t.start < 13 && t.end > 12));
    }
}

// 금요일 공강 필터
public class NoFridayFilter : IFilter
{
    public bool Apply(List<Course> schedule)
    {
        return !schedule.Any(c => c.Times.Any(t => t.day == 4));
    }
}