using System;
using System.Collections.Generic;
using System.Text;
using IntelliQuery.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliQuery.Infrastructure.Configurations
{
    public class DatasetFileConfiguration : IEntityTypeConfiguration<DatasetFile>
    {
        public void Configure(EntityTypeBuilder<DatasetFile> builder)
        {
            builder.ToTable("DatasetFiles");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(260);

            builder.Property(x => x.StoragePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.UploadedAtUtc)
                .IsRequired();
        }
    }
}
