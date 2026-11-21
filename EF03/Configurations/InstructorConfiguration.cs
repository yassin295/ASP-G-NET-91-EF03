using EF03.Entities;
using EF03.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF03.Configurations
{
    public class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
    {
        public void Configure(EntityTypeBuilder<Instructor> builder)
        {
            builder.ToTable("Instructors");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.FullName)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(i => i.Email)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(i => i.Bio)
                   .HasMaxLength(1000);

            builder.Property(i => i.JoinedDate)
                   .IsRequired();

            // One-to-Many: Instructor -> Courses
            builder.HasMany(i => i.Courses)
                   .WithOne(c => c.Instructor)
                   .HasForeignKey(c => c.InstructorId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasData(
              new Instructor
              {
                  Id = 1,
                  FullName = "Ahmed Hassan",
                  Email = "ahmed.hassan@example.com",
                  Bio = "Senior .NET developer and trainer.",
                  JoinedDate = new DateTime(2023, 1, 15)
              },
              new Instructor
              {
                  Id = 2,
                  FullName = "Sara Ali",
                  Email = "sara.ali@example.com",
                  Bio = null,
                  JoinedDate = new DateTime(2024, 3, 1)
              }
          );
        }
    }
}

