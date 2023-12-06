using Marada.Utils;

using Xunit.Abstractions;

namespace Marada.Tests.Utils
{
    public class RandomizerTests
    {
        private readonly ITestOutputHelper output;

        public RandomizerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void RandomString()
        {
            // Arrange
            int expectedLength = 512;
            int actualLength = 0;
            string result = string.Empty;
            Randomizer randomizer = new Randomizer();

            // Act
            result = randomizer.RandomCharacters(expectedLength);
            actualLength = result.Length;

            // Assert
            Assert.NotEmpty(result);
            Assert.NotInRange(actualLength, Int32.MinValue, 0);
            Assert.Equal(expectedLength, actualLength);            
            output.WriteLine(result);
        }
    }
}