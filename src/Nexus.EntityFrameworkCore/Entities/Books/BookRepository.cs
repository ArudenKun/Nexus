using System;
using Nexus.Books;
using Nexus.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Nexus.Entities.Books;

public class BookRepository : EfCoreRepository<NexusDbContext, Book, Guid>
{
    public BookRepository(IDbContextProvider<NexusDbContext> dbContextProvider)
        : base(dbContextProvider) { }
}
