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
        public int Id = 0;
        public List<(string key, string data)> Data { get; set; } = new List<(string, string)>();
        public List<(string key, TestContainer child)> Children { get; set; } = new List<(string, TestContainer)>();

        public List<string> Opaq(string path, string separator = "/", bool compareData = true)
        {
            return this.Opaq(path, separator,
                container => container.Data.Select(d => d.key),
                container => container.Children.Select(d => d.key),
                (container, k) => from c in container.Data where c.key == k select c.data,
                (container, k) => from c in container.Children where c.key == k select c.child,
                dataEqualityComparer: (a, b) => compareData && a == b,
                childEqualityComparer: (a, b) => a.Id == b.Id);
        }
    }

    public class OpaqTests
    {
        TestContainer _cont = new TestContainer
        {
            Id = 0,
            Data = new List<(string, string)>
            {
                ("jazz", "miles"),
                ("jazz", "dexter"),
                ("more jazz", "miles"),
                ("rock", "doors"),
                ("techno", "daft punk"),
            },
            Children = new List<(string, TestContainer)>
            {
                (
                    "healthy", new TestContainer
                    {
                        Id = 1,
                        Data = new List<(string, string)>
                        {
                            ("juice", "orange"),
                            ("wholewheat", "bread"),
                            ("vitamin", "c"),
                            ("foo", "bar 0")
                        },
                        Children = new List<(string, TestContainer)>
                        {
                            (
                                "veggies", new TestContainer
                                {
                                    Data = new List<(string, string)>
                                    {
                                        ("root", "carrot"),
                                        ("fruit", "apple"),
                                        ("?", "tomato")
                                    }
                                }
                            ),
                            ("hamburger", new TestContainer())
                        }
                    }
                ),
                (
                "healthy", new TestContainer
                {
                    Id = 1,
                    Data = new List<(string, string)>
                    {
                        ("juice", "orange"),
                        ("wholewheat", "bread"),
                        ("vitamin", "c"),
                        ("foo", "bar 1")
                    },
                    Children = new List<(string, TestContainer)>
                    {
                        (
                            "veggies", new TestContainer
                            {
                                Data = new List<(string, string)>
                                {
                                    ("root", "carrot"),
                                    ("fruit", "apple"),
                                    ("?", "tomato")
                                }
                            }
                        ),
                        ("hamburger", new TestContainer())
                    }
                }
                ),
                (
                "healthy", new TestContainer
                {
                    Id = 2,
                    Data = new List<(string, string)>
                    {
                        ("juice", "orange"),
                        ("wholewheat", "bread"),
                        ("vitamin", "c"),
                        ("foo", "bar 2")
                    },
                    Children = new List<(string, TestContainer)>
                    {
                        (
                        "veggies", new TestContainer
                        {
                            Data = new List<(string, string)>
                            {
                                ("root", "carrot"),
                                ("fruit", "apple"),
                                ("?", "tomato")
                            }
                        }
                        ),
                        ("hamburger", new TestContainer())
                    }
                }
                ),
                (
                    "numbers", new TestContainer
                    {
                        Id = 3,
                        Data = new List<(string, string)>
                        {
                            ("0", "zero"),
                            ("1", "one"),
                            ("2", "two"),
                            ("3", "three"),
                            ("foo", "bar 3")
                        }
                    }
                )
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
            Assert.All(result, s => Assert.Contains(_cont.Data, tuple => tuple.data == s));
            Assert.True(result.Count == _cont.Data.Count - 1);
        }

        [Fact]
        public void OpaqTestRegexNoComparison()
        {
            var result = _cont.Opaq("`.*`", compareData: false);
            Assert.All(result, s => Assert.Contains(_cont.Data, tuple => tuple.data == s));
            Assert.True(result.Count == 7);
        }

        [Fact]
        public void OpaqTestRegexPath()
        {
            var result = _cont.Opaq("`.*`/foo");
            Assert.All(result, s => Assert.StartsWith("bar", s));
            Assert.True(result.Count == 3);
        }
    }
}
