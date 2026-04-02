using System.Linq.Expressions;
using CompanyHR.API.Models;

namespace CompanyHR.API.Specifications;

/// <summary>
/// Базовый класс спецификации для сотрудников.
/// Позволяет динамически строить запросы с фильтрацией.
/// </summary>
public abstract class EmployeeSpecification
{
    public abstract Expression<Func<Employee, bool>> ToExpression();
    
    public IQueryable<Employee> Apply(IQueryable<Employee> query)
    {
        return query.Where(ToExpression());
    }
    
    // Комбинация спецификаций через AndAlso
    public EmployeeSpecification And(EmployeeSpecification other)
    {
        return new AndSpecification(this, other);
    }
    
    public EmployeeSpecification Or(EmployeeSpecification other)
    {
        return new OrSpecification(this, other);
    }
    
    public EmployeeSpecification Not()
    {
        return new NotSpecification(this);
    }
}

// Внутренние классы для комбинаций
internal class AndSpecification : EmployeeSpecification
{
    private readonly EmployeeSpecification _left;
    private readonly EmployeeSpecification _right;

    public AndSpecification(EmployeeSpecification left, EmployeeSpecification right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<Employee, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();
        var param = Expression.Parameter(typeof(Employee));
        var body = Expression.AndAlso(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param)
        );
        return Expression.Lambda<Func<Employee, bool>>(body, param);
    }
}

internal class OrSpecification : EmployeeSpecification
{
    private readonly EmployeeSpecification _left;
    private readonly EmployeeSpecification _right;

    public OrSpecification(EmployeeSpecification left, EmployeeSpecification right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<Employee, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();
        var param = Expression.Parameter(typeof(Employee));
        var body = Expression.OrElse(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param)
        );
        return Expression.Lambda<Func<Employee, bool>>(body, param);
    }
}

internal class NotSpecification : EmployeeSpecification
{
    private readonly EmployeeSpecification _spec;

    public NotSpecification(EmployeeSpecification spec)
    {
        _spec = spec;
    }

    public override Expression<Func<Employee, bool>> ToExpression()
    {
        var expr = _spec.ToExpression();
        var param = expr.Parameters[0];
        var body = Expression.Not(expr.Body);
        return Expression.Lambda<Func<Employee, bool>>(body, param);
    }
}

// Примеры конкретных спецификаций
public class EmployeeByPositionSpecification : EmployeeSpecification
{
    private readonly string _position;

    public EmployeeByPositionSpecification(string position)
    {
        _position = position;
    }

    public override Expression<Func<Employee, bool>> ToExpression()
    {
        return e => e.Position != null && e.Position.Contains(_position);
    }
}

public class EmployeeByHireDateRangeSpecification : EmployeeSpecification
{
    private readonly DateTime? _from;
    private readonly DateTime? _to;

    public EmployeeByHireDateRangeSpecification(DateTime? from, DateTime? to)
    {
        _from = from;
        _to = to;
    }

    public override Expression<Func<Employee, bool>> ToExpression()
    {
        return e =>
            (!_from.HasValue || e.HireDate >= _from.Value) &&
            (!_to.HasValue || e.HireDate <= _to.Value);
    }
}

public class EmployeeByDepartmentSpecification : EmployeeSpecification
{
    private readonly string _department;

    public EmployeeByDepartmentSpecification(string department)
    {
        _department = department;
    }

    public override Expression<Func<Employee, bool>> ToExpression()
    {
        return e => e.Department != null && e.Department.Contains(_department);
    }
}

public class ActiveEmployeeSpecification : EmployeeSpecification
{
    public override Expression<Func<Employee, bool>> ToExpression()
    {
        return e => e.IsActive;
    }
}
