using Microsoft.EntityFrameworkCore;
using SimpleNote.Models;

namespace SimpleNote.Data
{
    public class AppDbContext : DbContext
    {
        // DbSet для каждой модели
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Sample> Samples { get; set; }
        public DbSet<UserSample> UserSamples { get; set; }
        public DbSet<Measure> Measures { get; set; }
        public DbSet<Note> Notes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Настройка подключения к базе данных
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SimpleNote;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Определяем составной первичный ключ для UserSample
            modelBuilder.Entity<UserSample>()
                .HasKey(us => new { us.UserId, us.SampleId, us.UsedInProject });

            // Устанавливаем связи между таблицами с NO ACTION для ON DELETE
            modelBuilder.Entity<UserSample>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSamples)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Изменяем поведение на NO ACTION

            modelBuilder.Entity<UserSample>()
                .HasOne(us => us.Sample)
                .WithMany(s => s.UserSamples)
                .HasForeignKey(us => us.SampleId)
                .OnDelete(DeleteBehavior.Cascade); // Оставляем каскадное удаление

            modelBuilder.Entity<UserSample>()
                .HasOne(us => us.Project)
                .WithMany(p => p.UserSamples)
                .HasForeignKey(us => us.UsedInProject)
                .OnDelete(DeleteBehavior.NoAction); // Изменяем поведение на NO ACTION

            // Устанавливаем связи между дорожками и тактами
            modelBuilder.Entity<Track>()
                .HasMany(t => t.Measures) // Track имеет много Measures
                .WithOne(m => m.Track)   // Measure принадлежит одному Track
                .HasForeignKey(m => m.TrackId); // Внешний ключ в Measure

            // Устанавливаем связи между тактами и нотами
            modelBuilder.Entity<Measure>()
                .HasMany(m => m.Notes)   // Measure имеет много Notes
                .WithOne(n => n.Measure) // Note принадлежит одному Measure
                .HasForeignKey(n => n.MeasureId); // Внешний ключ в Note
        }
    }
}