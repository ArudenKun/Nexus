using Microsoft.EntityFrameworkCore;
using ServiceScan.SourceGenerator;

namespace Nexus.EntityFrameworkCore;

public static partial class ModelBuilderExtensions
{
    [ScanForTypes(
        AssignableTo = typeof(IEntityTypeConfiguration<>),
        Handler = nameof(ApplyConfiguration)
    )]
    public static partial ModelBuilder ApplyConfigurations(this ModelBuilder modelBuilder);

    private static void ApplyConfiguration<T, TEntity>(ModelBuilder modelBuilder)
        where T : IEntityTypeConfiguration<TEntity>, new()
        where TEntity : class
    {
        modelBuilder.ApplyConfiguration(new T());
    }
}
