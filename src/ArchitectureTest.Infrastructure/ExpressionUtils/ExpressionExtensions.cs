using System;
using System.Linq.Expressions;

namespace ArchitectureTest.Infrastructure.ExpressionUtils;

public static class ExpressionExtensions
{
    public static Expression<Func<U, bool>> ReplaceLambdaParameter<T, U>(this Expression<Func<T, bool>> expr)
    {
        var rewriter = new ExpressionTranslator<T, U>();
        return (Expression<Func<U, bool>>)rewriter.Visit(expr);
    }
}
