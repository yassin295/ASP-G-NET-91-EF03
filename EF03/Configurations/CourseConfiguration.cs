using EF03.Entities;
using EF03.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF03.Configurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.ToTable("Courses");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(c => c.Description)
                   .HasMaxLength(2000);

            builder.Property(c => c.Price)
                   .IsRequired()
                   .HasPrecision(10, 2);

            // Store the level as text (Beginner / Intermediate / Advanced)
            builder.Property(c => c.Level)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(c => c.IsPublished)
                   .IsRequired();

            // Null for unpublished courses
            builder.Property(c => c.PublishedDate)
                   .IsRequired(false);

            // Deleting a course deletes its enrollments (Cascade)
            builder.HasMany(c => c.Enrollments)
                   .WithOne(e => e.Course)
                   .HasForeignKey(e => e.CourseId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasData(
                new Course
                {
                    Id = 1,
                    Title = "C# Fundamentals",
                    Description = "Learn C# from scratch.",
                    Price = 49.99m,
                    Level = CourseLevel.Beginner,
                    IsPublished = true,
                    PublishedDate = new DateTime(2025, 1, 10),
                    InstructorId = 1,
                    CategoryId = 1
                },
                new Course
                {
                    Id = 2,
                    Title = "ASP.NET Core Web API",
                    Description = "Build REST APIs with ASP.NET Core.",
                    Price = 99.99m,
                    Level = CourseLevel.Intermediate,
                    IsPublished = true,
                    PublishedDate = new DateTime(2025, 3, 5),
                    InstructorId = 1,
                    CategoryId = 3
                },
                new Course
                {
                    Id = 3,
                    Title = "Advanced EF Core",
                    Description = null,
                    Price = 149.50m,
                    Level = CourseLevel.Advanced,
                    IsPublished = false,
                    PublishedDate = null,
                    InstructorId = 2,
                    CategoryId = 2
                },
                new Course
                {
                    Id = 4,
                    Title = "UI/UX Basics",
                    Description = "Introduction to interface design.",
                    Price = 29.00m,
                    Level = CourseLevel.Beginner,
                    IsPublished = true,
                    PublishedDate = new DateTime(2025, 6, 20),
                    InstructorId = 2,
                    CategoryId = 4
                }
            );
        }
    }
}
