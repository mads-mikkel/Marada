namespace Marada.Utils
{
    /// <summary>
    /// Generates random sequences.
    /// </summary>
    public class Randomizer
    {
        protected Random generator;

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        public Randomizer()
        {
            generator = new Random();
        }

        /// <summary>
        /// Generates a randomized set of characters in the UTF-16 range [0x0021;0x007E] as a <see cref="string"/>.
        /// </summary>
        /// <param name="length">The length of the string</param>
        /// <returns>A <see cref="string"/> containing the sequence of random generated characters.</returns>
        /// <exception cref="ArgumentOutOfRangeException">When the <paramref name="length"/> is outside the range [1;2^32 - 1]</exception>
        public string RandomCharacters(int length)
        {
            if(length < 1 || length > Int32.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            string result = string.Empty;
            int startUTF16 = 0x0021;
            int endUTF16 = 0x007E;

            for(int i = 0; i < length; i++)
            {
                char utf16char = (char)generator.Next(startUTF16, endUTF16 + 1);
                result += utf16char;
            }

            return result;
        }
    }
}