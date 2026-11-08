using Nexus.Samples;
using Xunit;

namespace Nexus.EntityFrameworkCore.Applications;

[Collection(NexusTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<NexusEntityFrameworkCoreTestModule>
{

}
