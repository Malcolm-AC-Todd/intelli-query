using System;
using System.Collections.Generic;
using System.Text;
using IntelliQuery.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliQuery.Infrastructure.Configurations
{
    public class ImportJobConfiguration : IEntityTypeConfiguration<ImportJob>
    {
        public void Configure(EntityTypeBuilder<ImportJob> builder)
        {
            builder.ToTable("ImportJobs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(2000);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.CompletedAtUtc);

            builder.HasOne<Dataset>()
                .WithMany()
                .HasForeignKey(x => x.DatasetId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
