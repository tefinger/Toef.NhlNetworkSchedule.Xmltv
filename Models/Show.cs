using System;

namespace Toef.NhlNetworkSchedule.Xmltv.Models
{
    public class Show
    {
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public string Duration { get; set; }
        public string ShowTitle { get; set; }
        public string ShowDescription { get; set; }
        public string Live { get; set; }
        public string Icon { get; set; }
    }
}
