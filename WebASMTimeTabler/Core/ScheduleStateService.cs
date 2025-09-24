using WebASMTimeTabler.Core;

public class ScheduleStateService
{
    public HashSet<Course> SelectedCourses { get; } = new HashSet<Course>(new CourseComparer());

    public void ToggleCourse(Course course)
    {
        if (SelectedCourses.Contains(course))
            SelectedCourses.Remove(course);
        else
            SelectedCourses.Add(course);
    }

    public bool IsSelected(Course course) => SelectedCourses.Contains(course);

    public void Clear() => SelectedCourses.Clear();
}
