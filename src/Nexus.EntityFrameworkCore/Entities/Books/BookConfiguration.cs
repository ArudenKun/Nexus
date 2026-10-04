using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Authors;
using Nexus.Books;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Nexus.Entities.Books;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable(NexusConsts.DbTablePrefix + "Books", NexusConsts.DbSchema);
        builder.ConfigureByConvention(); //auto configure for the base class props
        builder.Property(x => x.Name).IsRequired().HasMaxLength(128);
        builder.HasOne<Author>().WithMany().HasForeignKey(x => x.AuthorId).IsRequired();
    }
}
