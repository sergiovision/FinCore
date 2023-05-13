namespace BusinessObjects.BusinessObjects;

public class ScheduledJobInfo
{
    public bool IsRunning { get; set; }
    public string Group { get; set; }
    public string Name { get; set; }
    public string Log { get; set; }
    public string Schedule { get; set; }
    public long PrevTime { get; set; }
    public long NextTime { get; set; }
}
