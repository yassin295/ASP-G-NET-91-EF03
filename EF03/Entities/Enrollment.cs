using EF03.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF03.Entities
{
    public class Enrollment
    {
        // Composite key: StudentId + CourseId (configured with Fluent API)
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public DateTime EnrollmentDate { get; set; }
        public int ProgressPercentage { get; set; }
        public EnrollmentStatus Status { get; set; }

        // Only given when the course is completed
        public decimal? FinalGrade { get; set; }
    }
}
