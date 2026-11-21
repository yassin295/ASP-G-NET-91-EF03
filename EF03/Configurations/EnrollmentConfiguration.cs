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
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.ToTable("Enrollments");

            // Composite key: a student can enroll in the same course only once
            builder.HasKey(e => new { e.StudentId, e.CourseId });

            builder.Property(e => e.EnrollmentDate)
                   .IsRequired();

            builder.Property(e => e.ProgressPercentage)
                   .IsRequired();

            // Store the status as text (Active / Completed / Dropped)
            builder.Property(e => e.Status)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);

            // Optional: only given when the course is completed
            builder.Property(e => e.FinalGrade)
                   .HasPrecision(5, 2)
                   .IsRequired(false);

            // Database-level rule: progress must be between 0 and 100
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Enrollments_Progress",
                "[ProgressPercentage] >= 0 AND [ProgressPercentage] <= 100"));
            builder.HasData(
               new Enrollment
               {
                   StudentId = 1,
                   CourseId = 1,
                   EnrollmentDate = new DateTime(2025, 2, 1),
                   ProgressPercentage = 40,
                   Status = EnrollmentStatus.Active,
                   FinalGrade = null
               },
               new Enrollment
               {
                   StudentId = 2,
                   CourseId = 1,
                   EnrollmentDate = new DateTime(2025, 2, 3),
                   ProgressPercentage = 100,
                   Status = EnrollmentStatus.Completed,
                   FinalGrade = 92.50m
               },
               new Enrollment
               {
                   StudentId = 1,
                   CourseId = 2,
                   EnrollmentDate = new DateTime(2025, 4, 10),
                   ProgressPercentage = 10,
                   Status = EnrollmentStatus.Active,
                   FinalGrade = null
               },
               new Enrollment
               {
                   StudentId = 3,
                   CourseId = 4,
                   EnrollmentDate = new DateTime(2025, 7, 1),
                   ProgressPercentage = 20,
                   Status = EnrollmentStatus.Dropped,
                   FinalGrade = null
               }
           );
        }
    }
}
