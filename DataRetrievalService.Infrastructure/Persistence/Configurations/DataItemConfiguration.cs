using DataRetrievalService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataRetrievalService.Infrastructure.Persistence.Configurations
{
    public class DataItemConfiguration : IEntityTypeConfiguration<DataItem>
    {
        public void Configure(EntityTypeBuilder<DataItem> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
             .ValueGeneratedOnAdd()
             .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.CreatedAt)
             .ValueGeneratedOnAdd()
             .HasDefaultValueSql("SYSUTCDATETIME()")
             .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

            builder.Property(x => x.Value)
             .HasMaxLength(1000);

            builder.ToTable("DataItems", "dbo");
        }
    }
}
