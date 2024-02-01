using Marada.Libs.Utils;

using Xunit.Sdk;

namespace Marada.Libs.Tests.UtilTests
{
    public class DateTimeSpanTests
    {
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

    }
}