using Marada.Libs.Utils;

using Xunit.Sdk;

namespace Marada.Libs.Tests.UtilTests
{
    public class DateTimeSpanTests
    {
        [Fact]
        public void MethodEndIsAfterStart()
        {
            // Arrange:
            DateTime today = DateTime.Now;
            DateTime tomorrow = today.AddDays(1);
            bool actualCorrect = true, expectedCorrect = true;
            bool actualIncorrect = false, expectedIncorrect = false;
            
            // Act:
            actualCorrect = DateTimeSpan.EndIsAfterStart(today, tomorrow);
            actualIncorrect = DateTimeSpan.EndIsAfterStart(tomorrow, today);

            // Assert:
            Assert.Equal(expectedCorrect, actualCorrect);
            Assert.Equal(expectedIncorrect, actualIncorrect);
        }

        [Fact]
        public void Initialize()
        {
            // Arrange:
            DateTime today = DateTime.Now;
            DateTime tomorrow = today.AddDays(1);

            // Act
            DateTimeSpan correctDateTimeSpan = new(today, tomorrow);
            Assert.Throws<ArgumentOutOfRangeException>(() => new DateTimeSpan(tomorrow, today));
        }

        [Fact]
        public void Casts()
        {
            // Arrange:
            DateTime today = DateTime.Now;
            DateTime tomorrow = today.AddDays(1);
            DateTimeSpan dts = new(today, tomorrow);

            // Act:
            (DateTime start, DateTime end) d = dts;
            TimeSpan ts = dts;

            // Assert:
            Assert.True(d.end > d.start);
            Assert.True(ts.Equals(d.end - d.start));
        }

        [Theory]
        [InlineData("2024-02-10", "2024-02-15", "2024-02-12", "2024-02-14", true)] // Overlapping bookings
        [InlineData("2024-02-10", "2024-02-15", "2024-02-16", "2024-02-18", false)] // Non-overlapping bookings
        [InlineData("2024-02-10", "2024-02-15", "2024-02-15", "2024-02-16", false)] // back to back booking are OK
        public void Overlapping(string firstStart, string firstEnd, string secondStart, string secondEnd, bool expectedOverlap)
        {
            // Arrange:
            DateTime fS = DateTime.Parse(firstStart);
            DateTime fE = DateTime.Parse(firstEnd);
            DateTime sS = DateTime.Parse(secondStart);
            DateTime sE = DateTime.Parse(secondEnd);
            
            DateTimeSpan ds1 = new(fS, fE);
            DateTimeSpan ds2 = new(sS, sE);

            // Act:
            bool actualOverlap1 = DateTimeSpan.Overlaps(ds1, ds2);
            bool actualOverlap2 = DateTimeSpan.Overlaps(ds2, ds1);

            // Assert:
            Assert.Equal(expectedOverlap, actualOverlap1);
            Assert.Equal(expectedOverlap, actualOverlap2);
            
        }

    }
}