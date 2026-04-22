using System;
using System.Collections.Generic;
using System.Text;
using IntelliQuery.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliQuery.Infrastructure.Configurations
{
    public class DatasetRowConfiguration : IEntityTypeConfiguration<DatasetRow>
    {
        public void Configure(EntityTypeBuilder<DatasetRow> builder)
        {
            builder.ToTable("DatasetRows");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RowNumber)
                .IsRequired();

            builder.Property(x => x.JsonData)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasOne(x => x.Dataset)
                .WithMany(x => x.Rows)
                .HasForeignKey(x => x.DatasetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.DatasetId, x.RowNumber });
        }
    }
}
