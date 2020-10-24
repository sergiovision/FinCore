using System;

namespace BusinessObjects
{
    public class ScheduledJobView : Idable
    {
        public int Id { get; set; }
        public DateTime PrevDate { get; set; }
        public DateTime NextDate { get; set; }

        public string Group { get; set; }

        public string Name { get; set; }

        public string Schedule { get; set; }

        public bool IsRunning { get; set; }

        public string Log { get; set; }
    }
}