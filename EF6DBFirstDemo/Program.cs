using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Spatial;
using System.Linq;

namespace EF6DBFirstDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AddUpdateDeleteEntityInConnectedScenario();
            AddUpdateEntityInDisconnectedScenario();
            LinqToEntitiesQueries();
            FindEntity();
            LazyLoading();
            ExplicitLoading();
            ExecuteRawSQLusingSqlQuery();
            ExecuteSqlCommand();
            DynamicProxy();
            ReadDataUsingStoredProcedure();
            ChangeTracker();
            SpatialDataType();
            EntityEntry();
            OptimisticConcurrency();
            TransactionSupport();
            SetEntityState();

            Console.ReadLine();
        }

        public static void AddUpdateDeleteEntityInConnectedScenario()
        {
            Console.WriteLine("*** AddUpdateDeleteEntityInConnectedScenario Starts ***");

            using (var context = new SchoolDBEntities())
            {
                //Log DB commands to console
                context.Database.Log = Console.WriteLine;

                //Add a new student and address
                var newStudent = context.Students.Add(new Student() { StudentName = "Jonathan", StudentAddress = new StudentAddress() { Address1 = "1, Harbourside", City = "Jersey City", State = "NJ" } });
                context.SaveChanges(); // Executes Insert command

                //Edit student name
                newStudent.StudentName = "Alex";
                context.SaveChanges(); // Executes Update command

                //Remove student
                context.Students.Remove(newStudent);
                context.SaveChanges(); // Executes Delete command
            }

            Console.WriteLine("*** AddUpdateDeleteEntityInConnectedScenario Ends ***");
        }

        public static void AddUpdateEntityInDisconnectedScenario()
        {
            Console.WriteLine("*** AddUpdateEntityInDisconnectedScenario Starts ***");

            // disconnected entities
            var newStudent = new Student() { StudentName = "Bill" };
            var existingStudent = new Student() { StudentID = 10, StudentName = "Chris" };

            using (var context = new SchoolDBEntities())
            {
                //Log DB commands to console
                context.Database.Log = Console.WriteLine;

                context.Entry(newStudent).State = newStudent.StudentID == 0 ? EntityState.Added : EntityState.Modified;
                context.Entry(existingStudent).State = existingStudent.StudentID == 0 ? EntityState.Added : EntityState.Modified;

                context.SaveChanges(); // Executes Delete command
            }

            Console.WriteLine("*** AddUpdateEntityInDisconnectedScenario Ends ***");
        }

        public static void LinqToEntitiesQueries()
        {
            Console.WriteLine("*** LinqToEntitiesQueries Starts  ***");

            using (var context = new SchoolDBEntities())
            {
                //Log DB commands to console
                context.Database.Log = Console.WriteLine;

                //Retrieve students whose name is Bill - Linq-to-Entities Query Syntax
                var students = (from s in context.Students
                                where s.StudentName == "Bill"
                                select s).ToList();

                //Retrieve students with the same name - Linq-to-Entities Method Syntax
                var studentsWithSameName = context.Students
                    .GroupBy(s => s.StudentName)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                Console.WriteLine("Students with same name");
                foreach (var stud in studentsWithSameName)
                {
                    Console.WriteLine(stud);
                }

                //Retrieve students of standard 1
                var standard1Students = context.Students
                    .Where(s => s.StandardId == 1)
                    .ToList();
            }

            Console.WriteLine("*** LinqToEntitiesQueries Ends ***");
        }

        public static void FindEntity()
        {
            Console.WriteLine("*** FindEntity Starts  ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = Console.WriteLine;

                var stud = context.Students.Find(1);

                Console.WriteLine(stud.StudentName + " found");
            }

            Console.WriteLine("*** FindEntity Ends ***");
        }

        public static void LazyLoading()
        {
            Console.WriteLine("*** LazyLoading Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = Console.Write;

                Student student = context.Students.Where(s => s.StudentID == 1).FirstOrDefault<Student>();

                Console.WriteLine("*** Retrieve standard from the database ***");
                Standard std = student.Standard;
            }

            Console.WriteLine("*** LazyLoading Ends ***");
        }

        public static void ExplicitLoading()
        {
            Console.WriteLine("*** ExplicitLoading Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = Console.Write;

                Student std = context.Students
                    .Where(s => s.StudentID == 1)
                    .FirstOrDefault<Student>();

                //Loading Standard for Student (seperate SQL query)
                context.Entry(std).Reference(s => s.Standard).Load();

                //Loading Courses for Student (seperate SQL query)
                context.Entry(std).Collection(s => s.Courses).Load();
            }

            Console.WriteLine("*** ExplicitLoading Ends ***");
        }

        public static void ExecuteRawSQLusingSqlQuery()
        {
            Console.WriteLine("*** ExecuteRawSQLusingSqlQuery Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = Console.Write;

                var studentList = context.Students.SqlQuery("Select * from Student").ToList<Student>();

                var student = context.Students.SqlQuery("Select StudentId, StudentName, StandardId, RowVersion from Student where StudentId = 1").ToList();
            }

            Console.WriteLine("*** ExecuteRawSQLusingSqlQuery Ends ***");
        }

        public static void ExecuteSqlCommand()
        {
            Console.WriteLine("*** ExecuteSqlCommand Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = Console.Write;

                //Insert command
                int noOfRowInsert = context.Database.ExecuteSqlCommand("insert into student(studentname) values('Robert')");

                //Update command
                int noOfRowUpdate = context.Database.ExecuteSqlCommand("Update student set studentname ='Mark' where studentname = 'Robert'");

                //Delete command
                int noOfRowDeleted = context.Database.ExecuteSqlCommand("delete from student where studentname = 'Mark'");
            }

            Console.WriteLine("*** ExecuteSqlCommand Ends ***");
        }

        public static void DynamicProxy()
        {
            Console.WriteLine("*** DynamicProxy Starts ***");

            using (var context = new SchoolDBEntities())
            {
                var student = context.Students.Where(s => s.StudentName == "Bill")
                        .FirstOrDefault<Student>();

                Console.WriteLine("Proxy Type: {0}", student.GetType().Name);
                Console.WriteLine("Underlying Entity Type: {0}", student.GetType().BaseType);

                //Disable Proxy creation
                context.Configuration.ProxyCreationEnabled = false;

                Console.WriteLine("Proxy Creation Disabled");

                var student1 = context.Students.Where(s => s.StudentName == "Steve")
                        .FirstOrDefault<Student>();

                Console.WriteLine("Entity Type: {0}", student1.GetType().Name);
            }

            Console.WriteLine("*** DynamicProxy Ends ***");
        }

        public static void ReadDataUsingStoredProcedure()
        {
            Console.WriteLine("*** ReadDataUsingStoredProcedure Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = Console.Write;
                //get all the courses of student whose id is 1
                var courses = context.GetCoursesByStudentId(1);
                //Set Course entity as return type of GetCoursesByStudentId function
                //Open ModelBrowser -> Function Imports -> Right click on GetCoursesByStudentId and Edit
                //Change Returns a Collection of to Course Entity from Complex Type
                //uncomment following lines
                //foreach (Course cs in courses)
                //    Console.WriteLine(cs.CourseName);
            }

            Console.WriteLine("*** ReadDataUsingStoredProcedure Ends ***");
        }

        public static void ChangeTracker()
        {
            Console.WriteLine("*** ChangeTracker Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Configuration.ProxyCreationEnabled = false;

                var student = context.Students.Add(new Student() { StudentName = "Mili" });
                DisplayTrackedEntities(context);

                Console.WriteLine("Retrieve Student");
                var existingStudent = context.Students.Find(1);

                DisplayTrackedEntities(context);

                Console.WriteLine("Retrieve Standard");
                var standard = context.Standards.Find(1);

                DisplayTrackedEntities(context);

                Console.WriteLine("Editing Standard");
                standard.StandardName = "Grade 5";

                DisplayTrackedEntities(context);

                Console.WriteLine("Remove Student");
                context.Students.Remove(existingStudent);
                DisplayTrackedEntities(context);
            }

            Console.WriteLine("*** ChangeTracker Ends ***");
        }

        private static void DisplayTrackedEntities(SchoolDBEntities context)
        {
            Console.WriteLine("Context is tracking {0} entities.", context.ChangeTracker.Entries().Count());
            DbChangeTracker changeTracker = context.ChangeTracker;
            var entries = changeTracker.Entries();
            foreach (var entry in entries)
            {
                Console.WriteLine("Entity Name: {0}", entry.Entity.GetType().FullName);
                Console.WriteLine("Status: {0}", entry.State);
            }
        }

        public static void SpatialDataType()
        {
            Console.WriteLine("*** SpatialDataType Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = Console.Write;
                //Add Location using System.Data.Entity.Spatial.DbGeography
                context.Courses.Add(new Course() { CourseName = "New Course from SpatialDataTypeDemo", Location = DbGeography.FromText("POINT(-122.360 47.656)") });

                context.SaveChanges();
            }

            Console.WriteLine("*** SpatialDataTypeDemo Ends ***");
        }

        public static void EntityEntry()
        {
            Console.WriteLine("*** EntityEntry Starts ***");

            using (var context = new SchoolDBEntities())
            {
                //get student whose StudentId is 1
                var student = context.Students.Find(1);

                //edit student name
                student.StudentName = "Monica";

                //get DbEntityEntry object for student entity object
                var entry = context.Entry(student);

                //get entity information e.g. full name
                Console.WriteLine("Entity Name: {0}", entry.Entity.GetType().FullName);

                //get current EntityState
                Console.WriteLine("Entity State: {0}", entry.State);

                Console.WriteLine("********Property Values********");

                foreach (var propertyName in entry.CurrentValues.PropertyNames)
                {
                    Console.WriteLine("Property Name: {0}", propertyName);

                    //get original value
                    var orgVal = entry.OriginalValues[propertyName];
                    Console.WriteLine("     Original Value: {0}", orgVal);

                    //get current values
                    var curVal = entry.CurrentValues[propertyName];
                    Console.WriteLine("     Current Value: {0}", curVal);
                }
            }

            Console.WriteLine("*** EntityEntryDemo Ends ***");
        }

        public static void TransactionSupport()
        {
            Console.WriteLine("*** TransactionSupport Starts ***");

            using (var context = new SchoolDBEntities())
            {
                Console.WriteLine("Built-in Transaction");
                //Log DB commands to console
                context.Database.Log = Console.WriteLine;

                //Add a new student and address
                context.Students.Add(new Student() { StudentName = "Kapil" });

                var existingStudent = context.Students.Find(10);
                //Edit student name
                existingStudent.StudentName = "Alex";

                context.SaveChanges(); // Executes Insert & Update command under one transaction
            }

            Console.WriteLine("External Transaction");
            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = Console.Write;

                using (DbContextTransaction transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.Students.Add(new Student()
                        {
                            StudentName = "Arjun"
                        });
                        context.SaveChanges();

                        context.Courses.Add(new Course() { CourseName = "Entity Framework" });
                        context.SaveChanges();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Error occurred.");
                    }
                }
            }

            Console.WriteLine("*** TransactionSupport Ends ***");
        }

        public static void SetEntityState()
        {
            Console.WriteLine("*** SetEntityState Starts ***");

            var student = new Student()
            {
                StudentID = 1, // root entity with key
                StudentName = "Bill",
                StandardId = 1,
                Standard = new Standard()   //Child entity (with key value)
                {
                    StandardId = 1,
                    StandardName = "Grade 1"
                },
                Courses = new List<Course>() {
                    new Course(){  CourseName = "Machine Language" }, //Child entity (empty key)
                    new Course(){  CourseId = 2 } //Child entity (with key value)
                }
            };

            using (var context = new SchoolDBEntities())
            {
                context.Entry(student).State = EntityState.Modified;

                foreach (var entity in context.ChangeTracker.Entries())
                {
                    Console.WriteLine("{0}: {1}", entity.Entity.GetType().Name, entity.State);
                }
            }

            Console.WriteLine("*** SetEntityState Ends ***");
        }

        public static void OptimisticConcurrency()
        {
            Console.WriteLine("*** OptimisticConcurrency Starts ***");

            Student student = null;

            using (var context = new SchoolDBEntities())
            {
                student = context.Students.First();
            }

            //Edit student name
            student.StudentName = "Robin";

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = Console.Write;

                try
                {
                    context.Entry(student).State = EntityState.Modified;
                    context.SaveChanges();

                    Console.WriteLine("Student saved successfully.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine("Concurrency Exception Occurred.");
                }
            }

            Console.WriteLine("*** OptimisticConcurrency Ends ***");
        }
    }
}