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

    }
}