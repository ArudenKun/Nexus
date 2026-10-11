using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nexus.Authors;

public interface IAuthorDapperRepository
{
    Task<IReadOnlyList<Author>> GetAllDeadAuthorsAsync();
}
