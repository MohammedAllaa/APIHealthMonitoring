using APIHealthMonitoring.Application.Interfaces.Repositories;
using APIHealthMonitoring.Application.Specifications;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.UnitTests.Common.Fakes;

/// <summary>
/// A list-backed generic repository fake for unit testing.
/// Avoids hitting a database or EF context entirely.
/// </summary>
public class FakeRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly List<T> _entities = new();

    public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = _entities.FirstOrDefault(e => e.Id == id);
        return Task.FromResult(entity);
    }

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<T> list = _entities.ToList();
        return Task.FromResult(list);
    }

    public void Add(T entity)
    {
        if (entity.Id == 0)
        {
            // Auto-increment Id fake
            entity.Id = _entities.Any() ? _entities.Max(e => e.Id) + 1 : 1;
        }
        _entities.Add(entity);
    }

    public void Update(T entity)
    {
        var existing = _entities.FirstOrDefault(e => e.Id == entity.Id);
        if (existing is not null)
        {
            _entities.Remove(existing);
            _entities.Add(entity);
        }
    }

    public void Delete(T entity)
    {
        var existing = _entities.FirstOrDefault(e => e.Id == entity.Id);
        if (existing is not null)
        {
            _entities.Remove(existing);
        }
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_entities.Any(e => e.Id == id));
    }

    public Task<T?> GetEntityWithSpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return Task.FromResult(query.FirstOrDefault());
    }

    public Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        IReadOnlyList<T> result = query.ToList();
        return Task.FromResult(result);
    }

    public Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        var query = _entities.AsQueryable();
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }
        return Task.FromResult(query.Count());
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> specification)
    {
        var query = _entities.AsQueryable();

        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // We ignore includes/include strings because all objects are already in-memory in the object graph.

        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }
}
