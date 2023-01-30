﻿using System.Linq.Expressions;
using Autofilter.Processors;
using Autofilter.Rules;
using Autofilter.Tests.Common;
using AutoFixture;
using FluentAssertions;

namespace Autofilter.Tests.ProcessorTests;

public class FilterProcessorTests
{
    [Fact]
    public void ShouldFilter()
    {
        var query = new Fixture().CreateMany<TestClass>().AsQueryable();

        Condition condition = new(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals);

        FilterRule filter = new(new[] { condition });

        FluentActions.Invoking(() => FilterProcessor.ApplyFilter(query, filter)).Should().NotThrow();
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void ShouldGenerateValidExpression(string expectedExpression, FilterRule filter)
    {
        var lambda = (Expression<Func<TestClass, bool>>)
            FilterProcessor.BuildPredicate(typeof(TestClass), filter.Conditions, filter.Groups);

        string resultExpression = ExpressionPrettifier.PrettifyLambda(lambda);

        resultExpression.Should().Be(expectedExpression);
    }

    public static IEnumerable<object[]> TestCases
    {
        get
        {
            // 2 operands

            yield return new object[]
            {
                "1 and 2",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                    }
                )
            }; // 1 and 2

            yield return new object[]
            {
                "1 or 2",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                    }
                )
            }; // 1 or 2

            // 3 operands

            yield return new object[]
            {
                "(1 and 2) and 3",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                    }
                )
            }; // 1 and 2 and 3

            yield return new object[]
            {
                "(1 or 2) or 3",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                    }
                )
            }; // 1 or 2 or 3

            yield return new object[]
            {
                "(1 and 2) or 3",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                    }
                ),
            }; // 1 and 2 or 3

            yield return new object[]
            {
                "1 or (2 and 3)",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                    }
                )
            }; // 1 or 2 and 3

            // 4 operands

            yield return new object[]
            {
                "((1 and 2) and 3) and 4",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                    }
                )
            }; // 1 and 2 and 3 and 4

            yield return new object[]
            {
                "((1 or 2) or 3) or 4",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                    }
                )
            }; // 1 or 2 or 3 or 4

            yield return new object[]
            {
                "((1 and 2) or 3) and 4",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                    },

                    new[]
                    {
                        new Group
                        (
                            Start: 1,
                            End: 3,
                            Level: 1
                        )
                    }
                )
            }; // (1 and 2 or 3) and 4

            yield return new object[]
            {
                "1 and ((2 and 3) or 4)",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                    },

                    new []
                    {
                        new Group
                        (
                            Start: 2,
                            End: 4,
                            Level: 1
                        )
                    }
                )
            }; // 1 and (2 and 3 or 4)

            // 5 operands

            yield return new object[]
            {
                "(1 and ((2 or 3) or 4)) and 5",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                    },

                    new[]
                    {
                        new Group
                        (
                            Start: 2,
                            End: 4,
                            Level: 1
                        )
                    }
                )
            }; // 1 and (2 or 3 or 4) and 5

            yield return new object[]
            {
                "((1 and 2) or (3 and 4)) or 5",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                    },

                    new []
                    {
                        new Group
                        (
                            Start: 1,
                            End: 2,
                            Level: 1
                        ),
                        new Group
                        (
                            Start: 3,
                            End: 4,
                            Level: 1
                        ),
                        new Group
                        (
                            Start: 1,
                            End: 4,
                            Level: 2
                        )
                    }
                )
            }; // ((1 and 2) or (3 and 4)) or 5

            yield return new object[]
            {
                "((1 or 2) and (3 or 4)) and 5",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                    },

                    new []
                    {
                        new Group
                        (
                            Start: 1,
                            End: 2,
                            Level: 1
                        ),
                        new Group
                        (
                            Start: 3,
                            End: 4,
                            Level: 1
                        ),
                        new Group
                        (
                            Start: 1,
                            End: 4,
                            Level: 2
                        )
                    }
                )
            }; // ((1 or 2) and (3 or 4)) and 5

            // 6 operands

            yield return new object[]
            {
                "(1 or (2 and 3)) or ((4 and 5) or 6)",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                    },

                    new[]
                    {
                        new Group
                        (
                            Start: 1,
                            End: 3,
                            Level: 1
                        ),
                        new Group
                        (
                            Start: 4,
                            End: 6,
                            Level: 1
                        ),
                    }
                )
            }; // (1 or 2 and 3) or (4 and 5 or 6)

            yield return new object[]
            {
                "(1 and ((2 or 3) and (4 or 5))) and 6",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                    },

                    new[]
                    {
                        new Group
                        (
                            Start: 2,
                            End: 3,
                            Level: 1
                        ),
                        new Group
                        (
                            Start: 4,
                            End: 5,
                            Level: 1
                        ),
                        new Group
                        (
                            Start: 2,
                            End: 5,
                            Level: 2
                        ),
                    }
                )
            }; // 1 and ((2 or 3) and (4 or 5)) and 6

            yield return new object[]
            {
                "(1 or ((2 and 3) or (4 and 5))) or 6",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                    },

                    new[]
                    {
                        new Group
                        (
                            Start: 2,
                            End: 3,
                            Level: 1
                        ),
                        new Group
                        (
                            Start: 4,
                            End: 5,
                            Level: 1
                        ),
                        new Group
                        (
                            Start: 2,
                            End: 5,
                            Level: 2
                        ),
                    }
                )
            }; // 1 or ((2 and 3) or (4 and 5)) or 6

            // 8 operands

            yield return new object[]
            {
                "(1 and (2 or (3 and 4))) or (((5 and 6) or 7) and 8)",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                    },

                    new[]
                    {
                        new Group
                        (
                            Start: 2,
                            End: 4,
                            Level: 1
                        ),
                        new Group
                        (
                            Start: 5,
                            End: 7,
                            Level: 1
                        ),
                    }
                )
            }; // 1 and (2 or 3 and 4) or (5 and 6 or 7) and 8

            yield return new object[]
            {
                "((1 and (2 or 3)) and 4) or ((5 and (6 or 7)) and 8)",
                new FilterRule
                (
                    new[]
                    {
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.Or),
                        new Condition(nameof(TestClass.Prop), new[] { "true" }, SearchOperator.Equals, LogicOperator.And),
                    },

                    new[]
                    {
                        new Group
                        (
                            Start: 2,
                            End: 3,
                            Level: 1
                        ),
                        new Group
                        (
                            Start: 6,
                            End: 7,
                            Level: 1
                        ),
                        new Group
                        (
                            Start: 1,
                            End: 4,
                            Level: 2
                        ),
                        new Group
                        (
                            Start: 5,
                            End: 8,
                            Level: 2
                        ),
                    }
                )
            }; // (1 and (2 or 3) and 4) or (5 and (6 or 7) and 8)
        }
    }

    private class TestClass
    {
        public bool Prop => true;
    }
}