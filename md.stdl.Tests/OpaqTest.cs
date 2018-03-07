using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Coding;
using Xunit;

namespace md.stdl.Tests
{
    public class TestContainer
    {
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, TestContainer> Children { get; set; } = new Dictionary<string, TestContainer>();

        public List<string> Opaq(string path, string separator = "/")
        {
            return this.Opaq(path, separator,
                container => container.Data.Keys,
                container => container.Children.Keys,
                (container, k) =>
                {
                    if (container.Data.ContainsKey(k))
                        return new[] { container.Data[k] };
                    return Enumerable.Empty<string>();
                },
                (container, k) =>
                {
                    if (container.Children.ContainsKey(k))
                        return new[] { container.Children[k] };
                    return Enumerable.Empty<TestContainer>();
                });
        }
    }

    public class OpaqTests
    {
        TestContainer _cont = new TestContainer
        {
            Data = new Dictionary<string, string>
            {
                {"jazz", "miles" },
                {"rock", "doors" },
                {"techno", "daft punk" },
            },
            Children = new Dictionary<string, TestContainer>
            {
                {
                    "healthy", new TestContainer
                    {
                        Data = new Dictionary<string, string>
                        {
                            {"juice", "orange" },
                            {"wholewheat", "bread" },
                            {"vitamin", "c" },
                            {"foo", "bar 0" }
                        },
                        Children = new Dictionary<string, TestContainer>
                        {
                            {
                                "veggies", new TestContainer
                                {
                                    Data = new Dictionary<string, string>
                                    {
                                        {"root", "carrot"},
                                        {"fruit", "apple" },
                                        {"?", "tomato" }
                                    }
                                }
                            },
                            {
                                "hamburger", new TestContainer()
                            }
                        }
                    }
                },
                {
                    "numbers", new TestContainer
                    {
                        Data = new Dictionary<string, string>
                        {
                            {"0", "zero" },
                            {"1", "one" },
                            {"2", "two" },
                            {"3", "three" },
                            {"foo", "bar 1" }
                        }
                    }
                }
            }
        };

        [Fact]
        public void OpaqTestSingle()
        {
            var result = _cont.Opaq("rock");
            Assert.Equal("doors", result[0]);
        }

        [Fact]
        public void OpaqTestPath()
        {
            var result = _cont.Opaq("healthy/juice");
            Assert.Equal("orange", result[0]);
        }

        [Fact]
        public void OpaqTestPathInvalid()
        {
            var result = _cont.Opaq("healthy/hamburger/fruit");
            Assert.Empty(result);
        }

        [Fact]
        public void OpaqTestRegex()
        {
            var result = _cont.Opaq("`.*`");
            Assert.All(result, s => Assert.True(_cont.Data.ContainsValue(s)));
            Assert.NotEmpty(result);
        }

        [Fact]
        public void OpaqTestRegexPath()
        {
            var result = _cont.Opaq("`.*`/foo");
            Assert.All(result, s => Assert.StartsWith("bar", s));
            Assert.NotEmpty(result);
        }
    }
}
