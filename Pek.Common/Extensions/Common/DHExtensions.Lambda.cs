﻿using System.Linq.Expressions;
using System.Reflection;

using Pek.Expressions;
using Pek.Helpers;

namespace Pek;

/// <summary>
/// 系统扩展 - Lambda表达式扩展
/// </summary>
public static partial class DHExtensions
{
    #region Property(属性表达式)

    /// <summary>
    /// 创建属性表达式
    /// </summary>
    /// <param name="expression">表达式</param>
    /// <param name="propertyName">属性名，支持多级属性名，与句点分隔，范例：Customer.Name</param>
    /// <returns></returns>
    public static Expression Property(this Expression expression, string propertyName)
    {
        if (propertyName.All(t => t != '.'))
        {
            return Expression.Property(expression, propertyName);
        }
        var propertyNameList = propertyName.Split('.');
        Expression result = null;
        for (int i = 0; i < propertyName.Length; i++)
        {
            if (i == 0)
            {
                result = Expression.Property(expression, propertyNameList[0]);
                continue;
            }
            result = result.Property(propertyNameList[i]);
        }
        return result;
    }

    /// <summary>
    /// 创建属性表达式
    /// </summary>
    /// <param name="expression">表达式</param>
    /// <param name="member">属性</param>
    /// <returns></returns>
    public static Expression Property(this Expression expression, MemberInfo member)
    {
        return Expression.MakeMemberAccess(expression, member);
    }

    #endregion

    #region And(与表达式)

    /// <summary>
    /// 与操作表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns></returns>
    public static Expression And(this Expression left, Expression right)
    {
        if (left == null)
        {
            return right;
        }
        if (right == null)
        {
            return left;
        }
        return Expression.AndAlso(left, right);
    }

    /// <summary>
    /// 与操作表达式
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        if (left == null)
        {
            return right;
        }
        if (right == null)
        {
            return left;
        }
        return left.Compose(right, Expression.AndAlso);
    }

    #endregion

    #region Or(或表达式)

    /// <summary>
    /// 或操作表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns></returns>
    public static Expression Or(this Expression left, Expression right)
    {
        if (left == null)
        {
            return right;
        }
        if (right == null)
        {
            return left;
        }
        return Expression.OrElse(left, right);
    }

    /// <summary>
    /// 或操作表达式
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        if (left == null)
        {
            return right;
        }
        if (right == null)
        {
            return left;
        }
        return left.Compose(right, Expression.OrElse);
    }

    #endregion

    #region Value(获取Lambda表达式的值)

    /// <summary>
    /// 获取Lambda表达式的值
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="expression">表达式</param>
    /// <returns></returns>
    public static object Value<T>(this Expression<Func<T, bool>> expression)
    {
        return Lambda.GetValue(expression);
    }

    #endregion

    #region Equal(等于表达式)

    /// <summary>
    /// 创建等于运算表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns></returns>
    public static Expression Equal(this Expression left, Expression right)
    {
        return Expression.Equal(left, right);
    }

    /// <summary>
    /// 创建等于运算表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static Expression Equal(this Expression left, object value)
    {
        return left.Equal(Lambda.Constant(value, left));
    }

    #endregion

    #region NotEqual(不等于表达式)

    /// <summary>
    /// 创建不等于运算表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns></returns>
    public static Expression NotEqual(this Expression left, Expression right)
    {
        return Expression.NotEqual(left, right);
    }

    /// <summary>
    /// 创建不等于运算表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static Expression NotEqual(this Expression left, object value)
    {
        return left.NotEqual(Lambda.Constant(value, left));
    }

    #endregion

    #region Greater(大于表达式)

    /// <summary>
    /// 创建大于运算表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns></returns>
    public static Expression Greater(this Expression left, Expression right)
    {
        return Expression.GreaterThan(left, right);
    }

    /// <summary>
    /// 创建大于运算表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static Expression Greater(this Expression left, object value)
    {
        return left.Greater(Lambda.Constant(value, left));
    }

    #endregion

    #region GreaterEqual(大于等于表达式)

    /// <summary>
    /// 创建大于等于运算表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns></returns>
    public static Expression GreaterEqual(this Expression left, Expression right)
    {
        return Expression.GreaterThanOrEqual(left, right);
    }

    /// <summary>
    /// 创建大于等于运算表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static Expression GreaterEqual(this Expression left, object value)
    {
        return left.GreaterEqual(Lambda.Constant(value, left));
    }

    #endregion

    #region Less(小于表达式)

    /// <summary>
    /// 创建小于运算表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns></returns>
    public static Expression Less(this Expression left, Expression right)
    {
        return Expression.LessThan(left, right);
    }

    /// <summary>
    /// 创建小于运算表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static Expression Less(this Expression left, object value)
    {
        return left.Less(Lambda.Constant(value, left));
    }

    #endregion

    #region LessEqual(小于等于表达式)

    /// <summary>
    /// 创建小于等于运算表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns></returns>
    public static Expression LessEqual(this Expression left, Expression right)
    {
        return Expression.LessThanOrEqual(left, right);
    }

    /// <summary>
    /// 创建小于等于运算表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static Expression LessEqual(this Expression left, object value)
    {
        return left.LessEqual(Lambda.Constant(value, left));
    }

    #endregion

    #region StartsWith(头匹配)

    /// <summary>
    /// 创建大于运算表达式
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static Expression StartsWith(this Expression left, object value)
    {
        return left.Call("StartsWith", new[] { typeof(string) }, value);
    }

    #endregion

    #region EndsWith(尾匹配)

    /// <summary>
    /// 尾匹配
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static Expression EndsWith(this Expression left, object value)
    {
        return left.Call("EndsWith", new[] { typeof(string) }, value);
    }

    #endregion

    #region Contains(模糊匹配)

    /// <summary>
    /// 模糊匹配
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static Expression Contains(this Expression left, object value)
    {
        return left.Call("Contains", new[] { typeof(string) }, value);
    }

    #endregion

    #region Call(调用方法表达式)

    /// <summary>
    /// 创建调用方法表达式
    /// </summary>
    /// <param name="instance">调用的实例</param>
    /// <param name="methodName">方法名</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public static Expression Call(this Expression instance, string methodName, params Expression[] values)
    {
        return Expression.Call(instance, instance.Type.GetTypeInfo().GetMethod(methodName), values);
    }

    /// <summary>
    /// 创建调用方法表达式
    /// </summary>
    /// <param name="instance">调用的实例</param>
    /// <param name="methodName">方法名</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public static Expression Call(this Expression instance, string methodName, params object[] values)
    {
        if (values == null || values.Length == 0)
        {
            return Expression.Call(instance, instance.Type.GetTypeInfo().GetMethod(methodName));
        }
        return Expression.Call(instance, instance.Type.GetTypeInfo().GetMethod(methodName),
            values.Select(Expression.Constant));
    }

    /// <summary>
    /// 创建调用方法表达式
    /// </summary>
    /// <param name="instance">调用的实例</param>
    /// <param name="methodName">方法名</param>
    /// <param name="paramTypes">参数类型列表</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public static Expression Call(this Expression instance, string methodName, Type[] paramTypes,
        params object[] values)
    {
        if (values == null || values.Length == 0)
        {
            return Expression.Call(instance, instance.Type.GetTypeInfo().GetMethod(methodName, paramTypes));
        }
        return Expression.Call(instance, instance.Type.GetTypeInfo().GetMethod(methodName, paramTypes),
            values.Select(Expression.Constant));
    }

    #endregion

    #region Compose(组合表达式)

    /// <summary>
    /// 组合表达式
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="first">左操作数</param>
    /// <param name="second">右操作数</param>
    /// <param name="merge">合并操作</param>
    /// <returns></returns>
    internal static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second,
        Func<Expression, Expression, Expression> merge)
    {
        var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] })
            .ToDictionary(p => p.s, p => p.f);
        var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);
        return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
    }

    #endregion

    #region ToLambda(创建Lambda表达式)

    /// <summary>
    /// 创建Lambda表达式
    /// </summary>
    /// <typeparam name="TDelegate">委托类型</typeparam>
    /// <param name="body">表达式</param>
    /// <param name="parameters">参数列表</param>
    /// <returns></returns>
    public static Expression<TDelegate> ToLambda<TDelegate>(this Expression body,
        params ParameterExpression[] parameters)
    {
        if (body == null)
        {
            return null;
        }
        return Expression.Lambda<TDelegate>(body, parameters);
    }

    #endregion

    #region ToPredicate(创建谓词表达式)

    /// <summary>
    /// 创建谓词表达式
    /// </summary>
    /// <typeparam name="T">委托类型</typeparam>
    /// <param name="body">表达式</param>
    /// <param name="parameters">参数列表</param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> ToPredicate<T>(this Expression body,
        params ParameterExpression[] parameters)
    {
        return ToLambda<Func<T, bool>>(body, parameters);
    }

    #endregion
}