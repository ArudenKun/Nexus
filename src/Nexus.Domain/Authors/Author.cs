using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Nexus.Authors;

public class Author : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    public string? ShortBio { get; set; }
}
