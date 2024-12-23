using System;
using System.Linq.Expressions;

namespace ArchitectureTest.Infrastructure.ExpressionUtils;

public static class ExpressionExtensions
{
    // TODO: When replacing the Expression types, not all queries work, we must check why, specifically
    // the Expression with variables (time in a variable) don't work
    // e.g time = new DateTime(2024, 12, 10); ...Where(n => n.UserId == CrudSettings.UserId && n.CreationDate > time))
    public static Expression<Func<U, bool>> ReplaceLambdaParameter<T, U>(this Expression<Func<T, bool>> expr)
    {
        var rewriter = new ExpressionTranslator<T, U>();
        return (Expression<Func<U, bool>>)rewriter.Visit(expr);
    }
}
