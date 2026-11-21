using EF03.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF03.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.ToTable("Students");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.FullName)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(s => s.Email)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(s => s.DateOfBirth)
                   .IsRequired();

            // Deleting a student deletes their enrollments (Cascade)
            builder.HasMany(s => s.Enrollments)
                   .WithOne(e => e.Student)
                   .HasForeignKey(e => e.StudentId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasData(
              new Student { Id = 1, FullName = "Omar Khaled", Email = "omar.khaled@example.com", DateOfBirth = new DateTime(2000, 5, 12) },
              new Student { Id = 2, FullName = "Mona Samir", Email = "mona.samir@example.com", DateOfBirth = new DateTime(1999, 8, 30) },
              new Student { Id = 3, FullName = "Ali Mahmoud", Email = "ali.mahmoud@example.com", DateOfBirth = new DateTime(2002, 1, 20) }
          );
        }
    }
}
