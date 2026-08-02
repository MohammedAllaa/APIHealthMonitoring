using APIHealthMonitoring.Application.Interfaces;
using APIHealthMonitoring.Application.Interfaces.Repositories;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.UnitTests.Common.Fakes;

/// <summary>
/// Fake UnitOfWork backed by in-memory repositories.
/// </summary>
public class FakeUnitOfWork : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = new();

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);
        
        if (!_repositories.ContainsKey(type))
        {
            object repoInstance = type == typeof(ApiEndpoint)
                ? new FakeApiEndpointRepository()
                : new FakeRepository<T>();
            
            _repositories.Add(type, repoInstance);
        }
        
        return (IGenericRepository<T>)_repositories[type];
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Simulate successful database save
        return Task.FromResult(1);
    }

    public void Dispose()
    {
        // No-op for testing
    }
}
