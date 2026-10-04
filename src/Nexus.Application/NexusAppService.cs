using Nexus.Localization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Nexus;

/* Inherit your application services from this class.
 */
public abstract class NexusAppService : ApplicationService
{
    protected NexusAppService()
    {
        LocalizationResource = typeof(NexusResource);
    }
}

public abstract class NexusAppService<TRepository> : ApplicationService
    where TRepository : IRepository
{
    protected NexusAppService()
    {
        LocalizationResource = typeof(NexusResource);
    }

    protected TRepository Repository => LazyServiceProvider.LazyGetRequiredService<TRepository>();
}

public abstract class NexusAppService<TEntity, TKey> : ApplicationService
    where TEntity : class, IEntity<TKey>
{
    protected NexusAppService()
    {
        LocalizationResource = typeof(NexusResource);
    }

    protected IRepository<TEntity, TKey> Repository =>
        LazyServiceProvider.LazyGetRequiredService<IRepository<TEntity, TKey>>();
}
