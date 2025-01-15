using System;
using System.Linq.Expressions;

namespace ArchitectureTest.Infrastructure.ExpressionUtils;

public static class ExpressionExtensions
{
    // Note
    // This code is taken from here:
    // https://stackoverflow.com/questions/76102803/how-to-convert-type-expression-to-work-on-different-type-in-c-sharp#answer-76114155
    // TODO: When replacing the Expression types, not all queries work, we must check why, specifically
    // the Expression with variables (time in a variable) don't work
    // e.g time = new DateTime(2024, 12, 10); ...Where(n => n.UserId == CrudSettings.UserId && n.CreationDate > time))
    //
    // Edit on Jan/13/2025: Actually this works as expected, the problem with the date filter not working is something
    // related to EFCore, when we use dates for queries in Entity Framework some date functions are not translated properly
    // https://stackoverflow.com/questions/22039258/entity-framework-datetime-now-7-days-c-sharp
    public static Expression<Func<U, bool>> ReplaceLambdaParameter<T, U>(this Expression<Func<T, bool>> expr)
    {
        var rewriter = new ExpressionTranslator<T, U>();
        return (Expression<Func<U, bool>>)rewriter.Visit(expr);
    }
}
