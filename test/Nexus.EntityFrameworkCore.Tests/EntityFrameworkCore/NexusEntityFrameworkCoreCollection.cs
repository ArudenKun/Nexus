using Xunit;

namespace Nexus.EntityFrameworkCore;

[CollectionDefinition(NexusTestConsts.CollectionDefinitionName)]
public class NexusEntityFrameworkCoreCollection : ICollectionFixture<NexusEntityFrameworkCoreFixture>
{

}
