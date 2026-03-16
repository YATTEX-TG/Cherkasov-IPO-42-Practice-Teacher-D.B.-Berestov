using Xunit;

namespace CherkasovTests
{
    public class SimpleTest
    {
        [Fact]
        public void Test1_ShouldPass()
        {
            // Arrange
            int a = 2;
            int b = 2;

            // Act
            int result = a + b;

            // Assert
            Assert.Equal(4, result);
        }

        [Fact]
        public void Test2_ShouldPass()
        {
            // Arrange
            string str = "hello";

            // Act
            string result = str.ToUpper();

            // Assert
            Assert.Equal("HELLO", result);
        }
    }
}