using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizWebApp.Models;

namespace QuizWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=quizwebapp.db");

        public virtual DbSet<Contest> Contests { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<Answer> Answers { get; set; }
        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public virtual DbSet<ContestQuestion> ContestQuestions { get; set; }
        public virtual DbSet<ContestQuestionUser> ContestQuestionUsers { get; set; }
        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ContestQuestion>()
                    .HasKey(cq => cq.Id);
            builder.Entity<ContestQuestion>()
                    .HasOne(c => c.Contest)
                    .WithMany(cq => cq.ContestQuestions)
                    .HasForeignKey(c => c.ContestId);
            builder.Entity<ContestQuestion>()
                    .HasOne(q => q.Question)
                    .WithMany(cq => cq.ContestQuestions)
                    .HasForeignKey(q => q.QuestionId);

            builder.Entity<ContestQuestionUser>()
                    .HasKey(cqu => new { cqu.UserId, cqu.ContestQuestionId });
            builder.Entity<ContestQuestionUser>()
                    .HasOne(u => u.ApplicationUser)
                    .WithMany(cqu => cqu.ContestQuestionUsers)
                    .HasForeignKey(u => u.UserId);
            builder.Entity<ContestQuestionUser>()
                    .HasOne(cq => cq.ContestQuestion)
                    .WithMany(cqu => cqu.ContestQuestionUsers)
                    .HasForeignKey(cq => cq.ContestQuestionId);

        }
    }
}
