using EF03.Data;
using EF03.Entities;
using EF03.Enums;
using Microsoft.EntityFrameworkCore;

namespace EF03
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // ===================== CREATE =====================
            Console.WriteLine("===== CREATE =====");

            Console.WriteLine("\n--- 1) Add an instructor ---");
            int newInstructorId = AddInstructor();

            Console.WriteLine("\n--- 2) Add a course ---");
            int newCourseId = AddCourse(newInstructorId, 3);   // category 3 = ASP.NET (seeded)
            AddCourse(999, 3);                                 // instructor doesn't exist
            AddCourse(newInstructorId, 999);                   // category doesn't exist

            Console.WriteLine("\n--- 3) Add several students ---");
            List<int> newStudentIds = AddStudents();

            Console.WriteLine("\n--- 4) Enroll a student ---");
            EnrollStudent(newStudentIds[0], newCourseId);      // should succeed
            EnrollStudent(newStudentIds[0], newCourseId);      // already enrolled
            EnrollStudent(newStudentIds[1], 3);                // course 3 is not published (first run)
            EnrollStudent(999, newCourseId);                   // student doesn't exist
            EnrollStudent(newStudentIds[1], 999);              // course doesn't exist

            // ===================== READ =====================
            Console.WriteLine("\n===== READ =====");

            Console.WriteLine("\n--- 1) Find a course ---");
            FindCourse(1, "ASP.NET Core Web API");   // both exist
            FindCourse(999, "Not A Real Course");    // both missing

            Console.WriteLine("\n--- 2) Filter courses by price ---");
            FilterCoursesByPrice(30m, 100m);
            FilterCoursesByPrice(500m, 1000m);       // no results

            Console.WriteLine("\n--- 3) Sub-categories ---");
            PrintSubCategories(1);                   // Programming -> Backend
            PrintSubCategories(3);                   // ASP.NET has no sub-categories
            PrintSubCategories(999);                 // doesn't exist
            PrintTopLevelCategories();

            Console.WriteLine("\n--- 4) Course roster ---");
            PrintCourseRoster(1);
            PrintCourseRoster(999);                  // doesn't exist

            // ===================== UPDATE =====================
            Console.WriteLine("\n===== UPDATE =====");

            Console.WriteLine("\n--- 1) Change a price ---");
            ChangePrice(1, 59.99m);        // success
            ChangePrice(1, -5m);           // invalid price
            ChangePrice(999, 10m);         // course doesn't exist

            Console.WriteLine("\n--- 2) Update progress ---");
            UpdateProgress(1, 1, 75);      // success (student 1, course 1)
            UpdateProgress(1, 1, 150);     // invalid progress
            UpdateProgress(2, 4, 50);      // enrollment doesn't exist
            UpdateProgress(1, 2, 100);     // reaches 100 -> Completed

            Console.WriteLine("\n--- 3) Publish a course ---");
            PublishCourse(3);              // seeded unpublished course
            PublishCourse(3);              // already published
            PublishCourse(999);            // doesn't exist

            // ===================== DELETE =====================
            Console.WriteLine("\n===== DELETE =====");

            Console.WriteLine("\n--- 1) Unenroll ---");
            Unenroll(3, 4);                // seeded enrollment (student 3, course 4)
            Unenroll(3, 4);                // already deleted -> friendly message
            Unenroll(999, 999);            // doesn't exist

            Console.WriteLine("\n--- 2) Delete a category ---");
            DeleteCategoryUnsafe(1);       // has sub-categories -> DbUpdateException
            DeleteCategorySafe(1);         // safe version refuses
            int tempCategoryId = AddTempCategory();
            DeleteCategorySafe(tempCategoryId);   // no sub-categories -> deleted
            DeleteCategorySafe(999);       // doesn't exist

            Console.WriteLine("\n--- 3) Delete a course ---");
            EnrollStudent(newStudentIds[2], newCourseId);   // so the course has 2 enrollments
            DeleteCourse(newCourseId);
            DeleteCourse(newCourseId);     // already deleted
            DeleteCourse(999);             // doesn't exist

            Console.WriteLine("\nDone.");
        }

        // =====================================================
        //                        CREATE
        // =====================================================

        // ---------- Create 1: Add an instructor ----------
        static int AddInstructor()
        {
            using var context = new LearningDbContext();

            var instructor = new Instructor
            {
                FullName = "Khaled Mostafa",
                Email = "khaled.mostafa@example.com",
                Bio = "Backend engineer who loves teaching.",
                JoinedDate = DateTime.Now
            };

            context.Instructors.Add(instructor);
            context.SaveChanges();

            Console.WriteLine($"Instructor added. Generated Id = {instructor.Id}");
            return instructor.Id;
        }

        // ---------- Create 2: Add a course ----------
        static int AddCourse(int instructorId, int categoryId)
        {
            using var context = new LearningDbContext();

            if (!context.Instructors.Any(i => i.Id == instructorId))
            {
                Console.WriteLine($"Cannot add course: instructor with Id {instructorId} does not exist.");
                return 0;
            }

            if (!context.Categories.Any(c => c.Id == categoryId))
            {
                Console.WriteLine($"Cannot add course: category with Id {categoryId} does not exist.");
                return 0;
            }

            var course = new Course
            {
                Title = "Clean Architecture in .NET",
                Description = "Design maintainable .NET applications.",
                Price = 79.99m,
                Level = CourseLevel.Intermediate,
                IsPublished = true,
                PublishedDate = DateTime.Now,
                InstructorId = instructorId,
                CategoryId = categoryId
            };

            context.Courses.Add(course);
            context.SaveChanges();

            Console.WriteLine($"Course added. Generated Id = {course.Id}");
            return course.Id;
        }

        // ---------- Create 3: Add several students (one AddRange, one SaveChanges) ----------
        static List<int> AddStudents()
        {
            using var context = new LearningDbContext();

            var students = new List<Student>
            {
                new Student { FullName = "Hana Adel",     Email = "hana.adel@example.com",     DateOfBirth = new DateTime(2001, 4, 18) },
                new Student { FullName = "Youssef Nabil", Email = "youssef.nabil@example.com", DateOfBirth = new DateTime(2000, 11, 2) },
                new Student { FullName = "Salma Tarek",   Email = "salma.tarek@example.com",   DateOfBirth = new DateTime(2003, 7, 9) }
            };

            context.Students.AddRange(students);
            context.SaveChanges();

            foreach (var s in students)
            {
                Console.WriteLine($"Student added: {s.FullName} | Id = {s.Id}");
            }

            return students.Select(s => s.Id).ToList();
        }

        // ---------- Create 4: Enroll a student ----------
        static void EnrollStudent(int studentId, int courseId)
        {
            using var context = new LearningDbContext();

            var student = context.Students.Find(studentId);
            if (student is null)
            {
                Console.WriteLine($"Enrollment refused: student with Id {studentId} does not exist.");
                return;
            }

            var course = context.Courses.Find(courseId);
            if (course is null)
            {
                Console.WriteLine($"Enrollment refused: course with Id {courseId} does not exist.");
                return;
            }

            if (!course.IsPublished)
            {
                Console.WriteLine($"Enrollment refused: course '{course.Title}' is not published yet.");
                return;
            }

            bool alreadyEnrolled = context.Enrollments
                .Any(e => e.StudentId == studentId && e.CourseId == courseId);

            if (alreadyEnrolled)
            {
                Console.WriteLine($"Enrollment refused: {student.FullName} is already enrolled in '{course.Title}'.");
                return;
            }

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollmentDate = DateTime.Now,
                ProgressPercentage = 0,
                Status = EnrollmentStatus.Active,
                FinalGrade = null
            };

            context.Enrollments.Add(enrollment);
            context.SaveChanges();

            Console.WriteLine($"{student.FullName} enrolled in '{course.Title}' successfully.");
        }

        // =====================================================
        //                         READ
        // =====================================================

        // ---------- Read 1: Find a course (Find, then FirstOrDefault) ----------
        static void FindCourse(int id, string title)
        {
            using var context = new LearningDbContext();

            // Find: search by primary key
            var byId = context.Courses.Find(id);
            if (byId is null)
                Console.WriteLine($"Find by Id {id}: not found.");
            else
                Console.WriteLine($"Find by Id {id}: {byId.Title} | {byId.Level} | ${byId.Price:F2}");

            // FirstOrDefault: search by any condition
            var byTitle = context.Courses.FirstOrDefault(c => c.Title == title);
            if (byTitle is null)
                Console.WriteLine($"FirstOrDefault by title '{title}': not found.");
            else
                Console.WriteLine($"FirstOrDefault by title '{title}': found (Id = {byTitle.Id}).");
        }

        // ---------- Read 2: Filter courses by price (title, level, price only) ----------
        static void FilterCoursesByPrice(decimal min, decimal max)
        {
            using var context = new LearningDbContext();

            if (min > max)
            {
                Console.WriteLine("Invalid range: the minimum price is greater than the maximum price.");
                return;
            }

            var courses = context.Courses
                .Where(c => c.Price >= min && c.Price <= max)
                .OrderByDescending(c => c.Price)
                .Select(c => new { c.Title, c.Level, c.Price })
                .ToList();

            Console.WriteLine($"Courses priced between {min} and {max}:");

            if (courses.Count == 0)
            {
                Console.WriteLine("  No courses found in this price range.");
                return;
            }

            foreach (var c in courses)
            {
                Console.WriteLine($"  {c.Title} | {c.Level} | ${c.Price:F2}");
            }
        }

        // ---------- Read 3a: Direct sub-categories of a category ----------
        static void PrintSubCategories(int categoryId)
        {
            using var context = new LearningDbContext();

            var category = context.Categories.Find(categoryId);
            if (category is null)
            {
                Console.WriteLine($"Category with Id {categoryId} does not exist.");
                return;
            }

            var subCategories = context.Categories
                .Where(c => c.ParentCategoryId == categoryId)
                .ToList();

            Console.WriteLine($"Direct sub-categories of '{category.Name}':");

            if (subCategories.Count == 0)
            {
                Console.WriteLine("  (none)");
                return;
            }

            foreach (var sub in subCategories)
            {
                Console.WriteLine($"  - {sub.Name} (Id = {sub.Id})");
            }
        }

        // ---------- Read 3b: Top-level categories ----------
        static void PrintTopLevelCategories()
        {
            using var context = new LearningDbContext();

            var topLevel = context.Categories
                .Where(c => c.ParentCategoryId == null)
                .ToList();

            Console.WriteLine("Top-level categories:");
            foreach (var c in topLevel)
            {
                Console.WriteLine($"  - {c.Name} (Id = {c.Id})");
            }
        }

        // ---------- Read 4: Course roster (no joins, no Include) ----------
        static void PrintCourseRoster(int courseId)
        {
            using var context = new LearningDbContext();

            var course = context.Courses.Find(courseId);
            if (course is null)
            {
                Console.WriteLine($"Course with Id {courseId} does not exist.");
                return;
            }

            // Step 1: get the student Ids from the enrollments
            var studentIds = context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Select(e => e.StudentId)
                .ToList();

            Console.WriteLine($"Course '{course.Title}' has {studentIds.Count} enrolled student(s).");

            if (studentIds.Count == 0)
                return;

            // Step 2: get the students with Contains
            var names = context.Students
                .Where(s => studentIds.Contains(s.Id))
                .Select(s => s.FullName)
                .ToList();

            foreach (var name in names)
            {
                Console.WriteLine($"  - {name}");
            }
        }

        // =====================================================
        //                        UPDATE
        // =====================================================

        // ---------- Update 1: Change a price ----------
        static void ChangePrice(int courseId, decimal newPrice)
        {
            using var context = new LearningDbContext();

            if (newPrice < 0)
            {
                Console.WriteLine($"Invalid price {newPrice}: the price cannot be negative.");
                return;
            }

            var course = context.Courses.Find(courseId);
            if (course is null)
            {
                Console.WriteLine($"Course with Id {courseId} does not exist.");
                return;
            }

            decimal oldPrice = course.Price;
            course.Price = newPrice;
            context.SaveChanges();

            Console.WriteLine($"Price of '{course.Title}' changed from ${oldPrice:F2} to ${newPrice:F2}.");
        }

        // ---------- Update 2: Update progress (composite key) ----------
        static void UpdateProgress(int studentId, int courseId, int newProgress)
        {
            using var context = new LearningDbContext();

            if (newProgress < 0 || newProgress > 100)
            {
                Console.WriteLine($"Invalid progress {newProgress}: it must be between 0 and 100.");
                return;
            }

            // Find with a composite key: pass the key values in the same order as the key definition
            var enrollment = context.Enrollments.Find(studentId, courseId);
            if (enrollment is null)
            {
                Console.WriteLine($"No enrollment found for student {studentId} in course {courseId}.");
                return;
            }

            enrollment.ProgressPercentage = newProgress;

            if (newProgress == 100)
                enrollment.Status = EnrollmentStatus.Completed;

            context.SaveChanges();

            Console.WriteLine($"Progress updated to {newProgress}% (Status = {enrollment.Status}) for student {studentId} in course {courseId}.");
        }

        // ---------- Update 3: Publish a course ----------
        static void PublishCourse(int courseId)
        {
            using var context = new LearningDbContext();

            var course = context.Courses.Find(courseId);
            if (course is null)
            {
                Console.WriteLine($"Course with Id {courseId} does not exist.");
                return;
            }

            if (course.IsPublished)
            {
                Console.WriteLine($"Course '{course.Title}' is already published.");
                return;
            }

            course.IsPublished = true;
            course.PublishedDate = DateTime.Now;
            context.SaveChanges();

            Console.WriteLine($"Course '{course.Title}' published on {course.PublishedDate:yyyy-MM-dd}.");
        }

        // =====================================================
        //                        DELETE
        // =====================================================

        // ---------- Delete 1: Unenroll (composite key) ----------
        static void Unenroll(int studentId, int courseId)
        {
            using var context = new LearningDbContext();

            var enrollment = context.Enrollments.Find(studentId, courseId);
            if (enrollment is null)
            {
                Console.WriteLine($"No enrollment found for student {studentId} in course {courseId}.");
                return;
            }

            context.Enrollments.Remove(enrollment);
            context.SaveChanges();

            Console.WriteLine($"Student {studentId} was unenrolled from course {courseId}.");
        }

        // ---------- Delete 2a: Unsafe delete (catches DbUpdateException) ----------
        static void DeleteCategoryUnsafe(int categoryId)
        {
            using var context = new LearningDbContext();

            var category = context.Categories.Find(categoryId);
            if (category is null)
            {
                Console.WriteLine($"Category with Id {categoryId} does not exist.");
                return;
            }

            try
            {
                context.Categories.Remove(category);
                context.SaveChanges();   // the database refuses because of DeleteBehavior.Restrict
                Console.WriteLine($"Category '{category.Name}' deleted.");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Delete failed (DbUpdateException): {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // ---------- Delete 2b: Safe delete (checks first) ----------
        static void DeleteCategorySafe(int categoryId)
        {
            using var context = new LearningDbContext();

            var category = context.Categories.Find(categoryId);
            if (category is null)
            {
                Console.WriteLine($"Category with Id {categoryId} does not exist.");
                return;
            }

            bool hasSubCategories = context.Categories.Any(c => c.ParentCategoryId == categoryId);
            if (hasSubCategories)
            {
                Console.WriteLine($"Cannot delete '{category.Name}': it still has sub-categories.");
                return;
            }

            // Courses also use Restrict, so check them too
            bool hasCourses = context.Courses.Any(c => c.CategoryId == categoryId);
            if (hasCourses)
            {
                Console.WriteLine($"Cannot delete '{category.Name}': it still has courses.");
                return;
            }

            context.Categories.Remove(category);
            context.SaveChanges();

            Console.WriteLine($"Category '{category.Name}' deleted safely.");
        }

        // Helper: creates a category with no children so we can demo a successful delete
        static int AddTempCategory()
        {
            using var context = new LearningDbContext();

            var category = new Category
            {
                Name = "Temp Category",
                Description = "Created only to demonstrate a safe delete.",
                ParentCategoryId = null
            };

            context.Categories.Add(category);
            context.SaveChanges();

            Console.WriteLine($"Temporary category added. Id = {category.Id}");
            return category.Id;
        }

        // ---------- Delete 3: Delete a course that has enrollments ----------
        static void DeleteCourse(int courseId)
        {
            using var context = new LearningDbContext();

            var course = context.Courses.Find(courseId);
            if (course is null)
            {
                Console.WriteLine($"Course with Id {courseId} does not exist.");
                return;
            }

            int before = context.Enrollments.Count(e => e.CourseId == courseId);
            Console.WriteLine($"Enrollments of '{course.Title}' before delete: {before}");

            context.Courses.Remove(course);
            context.SaveChanges();

            int after = context.Enrollments.Count(e => e.CourseId == courseId);
            Console.WriteLine($"Enrollments of '{course.Title}' after delete: {after}");

            // WHY were the enrollments removed?
            // The relationship Course -> Enrollments is configured with
            // OnDelete(DeleteBehavior.Cascade) in CourseConfiguration. The foreign key in the
            // Enrollments table is created with ON DELETE CASCADE, so when SQL Server deletes
            // the course row it automatically deletes every enrollment that points to it.
        }
    }
}
