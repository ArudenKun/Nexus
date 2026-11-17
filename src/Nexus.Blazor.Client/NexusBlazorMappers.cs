using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Nexus.Authors;
using Nexus.Books;
namespace Nexus.Blazor.Client;
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class NexusBlazorMappers : MapperBase<BookDto, CreateUpdateBookDto>
{
    public override partial CreateUpdateBookDto Map(BookDto source);
    public override partial void Map(BookDto source, CreateUpdateBookDto destination);
}
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class NexusAuthorDtoToCreateUpdateAuthorDtoMapper : MapperBase<AuthorDto, CreateUpdateAuthorDto>
{
    public override partial CreateUpdateAuthorDto Map(AuthorDto source);
    public override partial void Map(AuthorDto source, CreateUpdateAuthorDto destination);
}
