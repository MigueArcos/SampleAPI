using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using ArchitectureTest.Infrastructure.ExpressionUtils;
using ArchitectureTest.TestUtils;
using FluentAssertions;
using Xunit;

namespace ArchitectureTest.Infrastructure.Tests.ExpressionUtils;

public partial class ExpressionTranslatorTests
{
    public sealed class AnEntity {
        public int NumericId { get; set; }
        public required string Id { get; set; }
        public DateTime SomeDate { get; set; }
        public bool Flag { get; set; }
    }

    public sealed class EntityWithoutProp {
        public required string Id { get; set; }
        public DateTime SomeDate { get; set; }
        public bool Flag { get; set; }
    }

    public sealed class MappedEntity {
        public required string VisualId { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsMine { get; set; }
    }

    [Theory]
    [ClassData(typeof(ExpressionsInputData))]
    public void ReplaceLambdaParameter_WithDifferentExpressions_ShouldTransformCorrectlyTheExpression(
        Expression<Func<EntityWithoutProp, bool>> expr
    ){
        // Act
        var result = expr.ReplaceLambdaParameter<EntityWithoutProp, AnEntity>();

        // Assert
        result.Should().NotBeNull();
        result.Parameters.Count.Should().Be(expr.Parameters.Count);
        Regex.Replace(result.Body.ToString(), "t2", "e").Should().Be(expr.Body.ToString());
    }

    [Theory]
    [ClassData(typeof(MappedExpressionsInputData))]
    public void ReplaceLambdaParameter_WithDifferentPropertiesName_ShouldTransformCorrectlyTheExpression(
        Expression<Func<MappedEntity, bool>> expr
    ){
        var equivalences = new Dictionary<string, string> {
            [nameof(MappedEntity.VisualId)] = nameof(AnEntity.Id),
            [nameof(MappedEntity.CreationDate)] = nameof(AnEntity.SomeDate),
            [nameof(MappedEntity.IsMine)] = nameof(AnEntity.Flag)
        };
        var rewriter = new ExpressionTranslator<MappedEntity, AnEntity>(equivalences);

        // Act
        var result = (Expression<Func<AnEntity, bool>>)rewriter.Visit(expr);

        // Assert
        result.Should().NotBeNull();
        result.Parameters.Count.Should().Be(expr.Parameters.Count);
        var outParam = "t2";
        var body = result.Body.ToString()
            .Replace($"{outParam}.{equivalences[nameof(MappedEntity.VisualId)]}", $"e.{nameof(MappedEntity.VisualId)}")
            .Replace($"{outParam}.{equivalences[nameof(MappedEntity.CreationDate)]}", $"e.{nameof(MappedEntity.CreationDate)}")
            .Replace($"{outParam}.{equivalences[nameof(MappedEntity.IsMine)]}", $"e.{nameof(MappedEntity.IsMine)}");
        body.Should().Be(expr.Body.ToString());
    }

    internal class ExpressionsInputData : TheoryData<Expression<Func<EntityWithoutProp, bool>>>
    {
        public ExpressionsInputData()
        {
            var userId = StubData.UserId;
            var date = DateTime.Today;
            var flagTrue = true;
            var flagFalse = true;

            // We will use different sources for the same variables, that's why we didn't use the TestCombinationsGenerator
            Add(e => e.Id == userId && e.SomeDate == date && e.Flag == flagTrue);
            Add(e => e.Id == userId && e.SomeDate == date && e.Flag == false);
            Add(e => e.Id == userId && e.SomeDate == DateTime.Today && e.Flag == flagTrue);
            Add(e => e.Id == StubData.UserId && e.SomeDate == date && e.Flag == flagFalse);
            Add(e => e.Id == StubData.UserId && e.SomeDate == date);
            Add(e => e.Id == userId && e.SomeDate == DateTime.Today);
            Add(e => e.Id == StubData.UserId && e.SomeDate == date);
            Add(e => e.Id == userId);
            Add(e => e.Id == StubData.UserId);
            Add(e => e.SomeDate == date);
            Add(e => e.SomeDate == DateTime.Today);
            Add(e => e.Flag == flagFalse);
            Add(e => e.Flag == true);
            Add(e => e.Id == userId && e.Flag == false);
            Add(e => e.Id == StubData.UserId && e.Flag == flagTrue);
            Add(e => e.SomeDate == DateTime.Today && e.Flag == flagTrue);
            Add(e => e.SomeDate == date && e.Flag == false);
        }
    }

    internal class MappedExpressionsInputData : TheoryData<Expression<Func<MappedEntity, bool>>>
    {
        public MappedExpressionsInputData()
        {
            var userId = StubData.UserId;
            var date = DateTime.Today;
            var flagTrue = true;
            var flagFalse = true;

            // We will use different sources for the same variables, that's why we didn't use the TestCombinationsGenerator
            Add(e => e.VisualId == userId && e.CreationDate == date && e.IsMine == flagTrue);
            Add(e => e.VisualId == userId && e.CreationDate == date && e.IsMine == false);
            Add(e => e.VisualId == userId && e.CreationDate == DateTime.Today && e.IsMine == flagTrue);
            Add(e => e.VisualId == StubData.UserId && e.CreationDate == date && e.IsMine == flagFalse);
            Add(e => e.VisualId == StubData.UserId && e.CreationDate == date);
            Add(e => e.VisualId == userId && e.CreationDate == DateTime.Today);
            Add(e => e.VisualId == StubData.UserId && e.CreationDate == date);
            Add(e => e.VisualId == userId);
            Add(e => e.VisualId == StubData.UserId);
            Add(e => e.CreationDate == date);
            Add(e => e.CreationDate == DateTime.Today);
            Add(e => e.IsMine == flagFalse);
            Add(e => e.IsMine == true);
            Add(e => e.VisualId == userId && e.IsMine == false);
            Add(e => e.VisualId == StubData.UserId && e.IsMine == flagTrue);
            Add(e => e.CreationDate == DateTime.Today && e.IsMine == flagTrue);
            Add(e => e.CreationDate == date && e.IsMine == false);
        }
    }
}
