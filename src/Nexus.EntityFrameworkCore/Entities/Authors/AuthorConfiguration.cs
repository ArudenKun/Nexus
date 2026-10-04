using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Authors;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Nexus.Entities.Authors;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable(NexusConsts.DbTablePrefix + "Authors", NexusConsts.DbSchema);
        builder.ConfigureByConvention(); //auto configure for the base class props
        builder.Property(x => x.Name).IsRequired().HasMaxLength(AuthorConsts.MaxNameLength);
        builder.Property(x => x.ShortBio).HasMaxLength(AuthorConsts.MaxShortBioLength);
    }
}
