using CompanyManagement.Api.Data;
using CompanyManagement.Api.Repositories;
using CompanyManagement.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CompanyManagement.Tests;

internal static class TestSupport
{
    internal static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    internal static ICacheService CreateCache() => new MemoryCacheService();

    internal static IClienteService CreateClientes(AppDbContext db) =>
        new ClienteService(new ClienteRepository(db), CreateCache());

    internal static IOrdenService CreateOrdenes(AppDbContext db) =>
        new OrdenService(new OrdenRepository(db), new ClienteRepository(db), CreateCache());

    private sealed class MemoryCacheService : ICacheService
    {
        private readonly Dictionary<string, object> _values = [];

        public Task<T?> Get<T>(string key) =>
            Task.FromResult(_values.TryGetValue(key, out var value) ? (T?)value : default);

        public Task Set<T>(string key, T value)
        {
            _values[key] = value!;
            return Task.CompletedTask;
        }

        public Task Remove(params string[] keys)
        {
            foreach (var key in keys) _values.Remove(key);
            return Task.CompletedTask;
        }
    }
}
