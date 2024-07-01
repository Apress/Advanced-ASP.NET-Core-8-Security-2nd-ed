using Humanizer;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using System.Linq.Expressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;

namespace JuiceShopDotNet.Safe.Data.ExpressionFilters;

public static class IQueryableExtensionMethods
{
    public static IQueryable<TSource> ForUser<TSource>(this IQueryable<TSource> source,
        ClaimsPrincipal user) where TSource : class
    {
        try
        {
            Expression<Func<TSource, bool>> userPredicate = GetUserFilterExpression<TSource>(user);
            return source.Where(userPredicate);
        }
        catch (Exception ex)
        {
            //Add logging later
            throw;
        }
    }

    public static TSource SingleForUser<TSource>(this IQueryable<TSource> source, 
        ClaimsPrincipal user, Expression<Func<TSource, bool>> predicate) where TSource : class
    {
        try
        {
            Expression<Func<TSource, bool>> userPredicate = GetUserFilterExpression<TSource>(user);

            try
            {
                return source.Where(userPredicate).Single(predicate);
            }
            catch
            {
                var preUserCount = source.Count(predicate);
                var postUserCount = source.Where(userPredicate).Count(predicate);

                if (preUserCount == 1 && postUserCount == 0)
                    throw new InvalidOperationException("Item not in user context");
                else
                    throw;
            }
        }
        catch (Exception ex)
        {
            //Add logging later
            throw;
        }
    }

    public static IQueryable<TSource> WhereForUser<TSource>(this IQueryable<TSource> source,
        ClaimsPrincipal user, Expression<Func<TSource, bool>> predicate) where TSource : class
    {
        try
        {
            Expression<Func<TSource, bool>> userPredicate = GetUserFilterExpression<TSource>(user);
            return source.Where(userPredicate).Where(predicate);
        }
        catch (Exception ex)
        {
            //Add logging later
            throw;
        }
    }

    private static Expression<Func<TSource, bool>> GetUserFilterExpression<TSource>(ClaimsPrincipal user)
        where TSource : class
    {
        Expression<Func<TSource, bool>> finalExpression = null;

        var properties = typeof(TSource).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(UserIdentifierAttribute)));

        if (properties.Count() == 0)
        {
            throw new MissingMemberException($"{typeof(TSource).Name} must have a UserIdentifierAttribute in order to use one of the UserContext search methods");
        }

        if (properties.Count() > 1)
        {
            throw new InvalidOperationException($"Found multiple UserIdentifierAttributes on type {typeof(TSource).Name}");
        }

        var userClaim = user.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userClaim == null)
            throw new NullReferenceException("There is no user logged in to provide context");

        var attrInfo = properties.Single();
        var parameter = Expression.Parameter(typeof(TSource));

        Expression property = Expression.Property(parameter, attrInfo.Name);

        object castUserID = Convert.ChangeType(userClaim.Value, attrInfo.PropertyType);

        var constant = Expression.Constant(castUserID);
        var equalClause = Expression.Equal(property, constant);
        finalExpression = Expression.Lambda<Func<TSource, bool>>(equalClause, parameter);

        return finalExpression;
    }
}
