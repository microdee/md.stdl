using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Coding;
using Xunit;

namespace md.stdl.Tests
{
    [AttributeUsage(AttributeTargets.All)]
    public class AssignToMeAttribute : Attribute
    {
        public string Name;

        public AssignToMeAttribute(string n)
        {
            Name = n;
        }
    }

    public abstract class ReflectionTestBase
    {
        [AssignToMe("Another Target")]
        public string AnotherAssignee { get; set; } = "nope";
    }

    public class ReflectionTests : ReflectionTestBase
    {
        [AssignToMe("Target")]
        public string Assignee { get; set; } = "nope";

        [AssignToMe("Yet Another Target")]
        public string FromInterface { get; set; } = "nope";

        [Fact]
        public void IsNumericTest()
        {
            object o = 7.0;
            Assert.True(o.IsNumeric());
            o = (int) 7;
            Assert.True(o.IsNumeric());
            o = new List<int>() {0, 1, 2};
            Assert.False(o.IsNumeric());
            Assert.False(typeof(ReflectionTests).IsNumeric());
        }

        [Fact]
        public void IsTest()
        {
            Assert.True(typeof(ReflectionTests).Is(typeof(ReflectionTestBase)));
        }

        [Fact]
        public void AttributeConditionalPropertyAssignmentTest()
        {
            var result = this.AttributeConditionalPropertyAssignment<AssignToMeAttribute>(
                "yarp", attr => attr.Name.Equals("Target"), inherit: true);
            Assert.True(result);
            Assert.Equal("yarp", Assignee);
            Assert.Equal("nope", AnotherAssignee);
            Assert.Equal("nope", FromInterface);

            result = this.AttributeConditionalPropertyAssignment<AssignToMeAttribute>(
                "yarp", attr => attr.Name.Equals("Another Target"), inherit: true);
            Assert.True(result);
            Assert.Equal("yarp", AnotherAssignee);
            Assert.Equal("nope", FromInterface);
        }
    }
}
