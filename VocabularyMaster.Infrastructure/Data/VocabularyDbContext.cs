using Microsoft.EntityFrameworkCore;
using VocabularyMaster.Core.Models;
using VocabularyMaster.Core.Enums; // YENİ

namespace VocabularyMaster.Infrastructure.Data
{
    public class VocabularyDbContext : DbContext
    {
        public DbSet<Word> Words { get; set; }
        public DbSet<ReviewHistory> ReviewHistories { get; set; }
        public DbSet<WordMeaning> WordMeanings { get; set; }

        public VocabularyDbContext(DbContextOptions<VocabularyDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Word>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.English).IsRequired().HasMaxLength(200);
                entity.Property(w => w.Category).HasMaxLength(100);
                entity.HasIndex(w => w.English);
                entity.HasIndex(w => w.Category);
                entity.HasIndex(w => w.DifficultyLevel);

                entity.HasMany(w => w.Meanings)
                    .WithOne(m => m.Word)
                    .HasForeignKey(m => m.WordId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WordMeaning>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Turkish).IsRequired().HasMaxLength(200);
                entity.Property(m => m.ExampleSentence).HasMaxLength(500);
                entity.HasIndex(m => m.WordId);
            });

            modelBuilder.Entity<ReviewHistory>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.ReviewDate).IsRequired();
                entity.HasIndex(r => r.WordId);
                entity.HasIndex(r => r.ReviewDate);

                entity.HasOne<Word>()
                    .WithMany()
                    .HasForeignKey(r => r.WordId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}