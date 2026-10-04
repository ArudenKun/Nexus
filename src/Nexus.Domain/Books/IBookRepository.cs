using System;
using Volo.Abp.Domain.Repositories;

namespace Nexus.Books;

public interface IBookRepository : IRepository<Book, Guid> { }
