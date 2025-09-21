using WebASMTimeTabler.Core;

public class ScheduleStateService
{
    public List<Course> SelectedCourses { get; private set; } = new List<Course>();

    public void SetSelectedCourses(List<Course> courses)
    {
        SelectedCourses = courses;
    }
}
