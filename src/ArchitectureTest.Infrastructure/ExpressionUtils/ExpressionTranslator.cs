using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace ArchitectureTest.Infrastructure.ExpressionUtils;

public class ExpressionTranslator<T1, T2> : ExpressionVisitor {
    private readonly Dictionary<string, string>? ParameterMap;
    private ParameterExpression? oldP;
    private readonly ParameterExpression newP = Expression.Parameter(typeof(T2), "t2");

    public ExpressionTranslator(Dictionary<string, string>? pMap = null) => ParameterMap = pMap;

    [return: NotNullIfNotNull("node")]
    public override Expression? Visit(Expression? node) {
        if (node is Expression<Func<T1, bool>> lambdaExpr)
            oldP = lambdaExpr.Parameters[0]; // save original parameter to replace later
        return base.Visit(node);
    }

    protected override Expression VisitLambda<T>(Expression<T> lambdaExpr) => Expression.Lambda(Visit(lambdaExpr.Body), newP);

    protected override Expression VisitParameter(ParameterExpression parmExpr) {
        if (parmExpr == oldP)
            return newP;
        else
            return base.VisitParameter(parmExpr);
    }

    protected override Expression VisitMember(MemberExpression memberExpr) {
        string propertyName = memberExpr.Member.Name;
        if (ParameterMap != null && ParameterMap.ContainsKey(propertyName))
            propertyName = ParameterMap[propertyName];

        if (memberExpr.Expression == oldP)
            return Expression.Property(newP, propertyName);
        
        return base.VisitMember(memberExpr);
    }
}
