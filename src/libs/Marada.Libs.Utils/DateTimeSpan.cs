namespace Marada.Libs.Utils
{
    public struct DateTimeSpan
    {
        private readonly DateTime start, end;
        private readonly TimeSpan duration;

        public DateTimeSpan(DateTime start, DateTime end)
        {
            duration = CalulateDuration(start, end);            
            this.start = start;
            this.end = end;
        }

        public DateTime Start => start;
        public DateTime End => end;
        public TimeSpan Duration => duration;

        public static TimeSpan CalulateDuration(DateTime start, DateTime end)
        {
            if(!EndIsAfterStart(start, end)) { throw new ArgumentOutOfRangeException(nameof(end), end, "End is not after start."); }
            return end - start;
        }

        public static bool EndIsAfterStart(DateTime start, DateTime end)
            => end > start;

        public static bool Overlaps(DateTimeSpan first, DateTimeSpan second)
        {
            if(first.End == second.Start ^ second.End == first.start)
            {
                return false;
            }
            bool areOverlapping = first.End >= second.Start && second.End >= first.Start;
            return areOverlapping;
        }

        public static implicit operator TimeSpan(DateTimeSpan dateTimeSpan) => dateTimeSpan.duration;
        public static implicit operator (DateTime start, DateTime end)(DateTimeSpan dateTimeSpan) => (dateTimeSpan.start, dateTimeSpan.end);
    }
}