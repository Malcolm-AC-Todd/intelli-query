using System;
using System.Collections.Generic;
using System.Text;
using IntelliQuery.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliQuery.Infrastructure.Configurations
{
    public class DatasetColumnConfiguration : IEntityTypeConfiguration<DatasetColumn>
    {
        public void Configure(EntityTypeBuilder<DatasetColumn> builder)
        {
            builder.ToTable("DatasetColumns");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.DataType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.IsNullable)
                .IsRequired();

            builder.Property(x => x.MinValue);
            builder.Property(x => x.MaxValue);
        }
    }
}
