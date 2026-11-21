using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF03.Entities
{
    public class Instructor
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Bio { get; set; }
        public DateTime JoinedDate { get; set; }

        // One instructor publishes many courses
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
