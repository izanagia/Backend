using Microsoft.EntityFrameworkCore;
using DiplomBackend.Models;
using System.Collections.Generic;

namespace DiplomBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<ExamSchedule> ExamSchedules { get; set; }
        public DbSet<GradeSheet> GradeSheets { get; set; }
        public DbSet<GradeEntry> GradeEntries { get; set; }


        public DbSet<SubjectTeacher> SubjectTeachers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SubjectTeacher>()
                .HasKey(st => new { st.SubjectId, st.TeacherId });

            modelBuilder.Entity<SubjectTeacher>()
                .HasOne(st => st.Subject)
                .WithMany(s => s.SubjectTeachers)
                .HasForeignKey(st => st.SubjectId);

            modelBuilder.Entity<SubjectTeacher>()
                .HasOne(st => st.Teacher)
                .WithMany(u => u.SubjectTeachers)
                .HasForeignKey(st => st.TeacherId);

            modelBuilder.Entity<GradeEntry>()
                .HasOne(g => g.Student)
                .WithMany()
                .HasForeignKey(g => g.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }


    }
}
