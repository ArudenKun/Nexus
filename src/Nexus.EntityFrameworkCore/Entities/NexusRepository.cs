using System.Threading.Tasks;
using Dapper.SimpleSqlBuilder.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Nexus.Entities;

public abstract class NexusRepository<TDbContext, TEntity, TKey>
    : EfCoreRepository<TDbContext, TEntity, TKey>
    where TDbContext : IEfCoreDbContext
    where TEntity : class, IEntity<TKey>
{
    protected NexusRepository(IDbContextProvider<TDbContext> dbContextProvider)
        : base(dbContextProvider) { }

    protected async Task<string> GetTableNameAsync()
    {
        var dbContext = await GetDbContextAsync();
        return dbContext.Set<TEntity>().EntityType.GetTableName() ?? string.Empty;
    }

    protected ISimpleBuilder SqlBuilder =>
        LazyServiceProvider.LazyGetRequiredService<ISimpleBuilder>();
}
