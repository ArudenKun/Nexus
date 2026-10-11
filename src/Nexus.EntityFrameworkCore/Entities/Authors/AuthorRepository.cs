using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Nexus.Authors;
using Nexus.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Nexus.Entities.Authors;

public class AuthorRepository : NexusRepository<NexusDbContext, Author, Guid>, IAuthorRepository
{
    public AuthorRepository(IDbContextProvider<NexusDbContext> dbContextProvider)
        : base(dbContextProvider) { }

    public virtual async Task<Author?> FindByNameAsync(string name)
    {
        var connection = await GetDbConnectionAsync();
        var tableName = await GetTableNameAsync();
        var sqlBuilder = SqlBuilder
            .CreateFluent()
            .Select($"*")
            .From($"{tableName}")
            .WhereFilter($"Name = {name}")
            .WithFilter($"{nameof(Author.ShortBio)} IS NOT NULL")
            .OrWhere($"{nameof(Author.ShortBio)} <> ''");
        return await connection.QueryFirstOrDefaultAsync<Author>(
            sqlBuilder.Sql,
            sqlBuilder.Parameters
        );
        // var dbSet = await GetDbSetAsync();
        // return await dbSet
        //     .Where(x => x.Name == name && !string.IsNullOrWhiteSpace(x.ShortBio))
        //     .FirstOrDefaultAsync();
    }
}
