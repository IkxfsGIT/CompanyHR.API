using CompanyHR.API.Models;
using CompanyHR.API.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace CompanyHR.API.Data;

/// <summary>
/// Интерфейс Unit of Work для группировки операций с БД.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<Employee> Employees { get; }
    IRepository<User> Users { get; }
    IRepository<Position> Positions { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    // При необходимости добавьте другие репозитории

    Task<int> CompleteAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

/// <summary>
/// Реализация Unit of Work.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IRepository<Employee>? _employees;
    private IRepository<User>? _users;
    private IRepository<Position>? _positions;
    private IRepository<RefreshToken>? _refreshTokens;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<Employee> Employees => _employees ??= new Repository<Employee>(_context);
    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Position> Positions => _positions ??= new Repository<Position>(_context);
    public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
                await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
