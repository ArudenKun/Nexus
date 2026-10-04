using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexus.Authors;
using Nexus.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Nexus.Entities.Authors;

public class AuthorRepository : EfCoreRepository<NexusDbContext, Author, Guid>, IAuthorRepository
{
    public AuthorRepository(IDbContextProvider<NexusDbContext> dbContextProvider)
        : base(dbContextProvider) { }

    public async Task<Author?> FindByNameAsync(string name)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.Name == name && !string.IsNullOrWhiteSpace(x.ShortBio))
            .FirstOrDefaultAsync();
    }
}
