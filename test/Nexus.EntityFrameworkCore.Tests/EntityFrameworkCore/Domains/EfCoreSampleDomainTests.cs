using Nexus.Samples;
using Xunit;

namespace Nexus.EntityFrameworkCore.Domains;

[Collection(NexusTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<NexusEntityFrameworkCoreTestModule>
{

}
