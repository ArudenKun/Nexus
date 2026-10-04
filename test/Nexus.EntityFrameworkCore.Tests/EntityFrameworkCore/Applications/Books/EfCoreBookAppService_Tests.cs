using Nexus.Books;
using Xunit;

namespace Nexus.EntityFrameworkCore.Applications.Books;

[Collection(NexusTestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<NexusEntityFrameworkCoreTestModule>
{

}