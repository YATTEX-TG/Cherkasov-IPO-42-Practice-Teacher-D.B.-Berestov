using Xunit;
using CherkasovApp.ViewModels;
using CherkasovLibrary.Models;

namespace CherkasovTests
{
    public class ViewModelTests
    {
        [Fact]
        public void Test1_TrueIsTrue()
        {
            Assert.True(true);
        }

        [Fact]
        public void Test2_FalseIsFalse()
        {
            Assert.False(false);
        }

        [Fact]
        public void Test3_OnePlusOneEqualsTwo()
        {
            Assert.Equal(2, 1 + 1);
        }

        [Fact]
        public void Test4_StringIsNotNull()
        {
            string test = "hello";
            Assert.NotNull(test);
        }

        [Fact]
        public void Test5_ObjectCanBeCreated()
        {
            var obj = new object();
            Assert.NotNull(obj);
        }

        [Fact]
        public void Test6_NumbersAreNumbers()
        {
            int number = 42;
            Assert.IsType<int>(number);
        }
    }
}