using System;

namespace BusinessObjects.BusinessObjects
{
    public class ScheduledJobView : Idable
    {
        public DateTime PrevDate { get; set; }
        public DateTime NextDate { get; set; }

        public string Group { get; set; }

        public string Name { get; set; }

        public string Schedule { get; set; }

        public bool IsRunning { get; set; }

        public string Log { get; set; }
        public int Id { get; set; }
        
        public bool Retired { get; set; }
    }
}