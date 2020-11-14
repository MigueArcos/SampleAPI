using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ArchitectureTest.Data.Database.MySQL.Entities
{
    public partial class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Checklist> Checklist { get; set; }
        public virtual DbSet<ChecklistDetail> ChecklistDetail { get; set; }
        public virtual DbSet<Note> Note { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Checklist>(entity =>
            {
                entity.HasIndex(e => e.UserId)
                    .HasName("UserId");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreationDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.ModificationDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.Title).HasColumnType("varchar(100)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Checklist)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("checklist_ibfk_1");
            });

            modelBuilder.Entity<ChecklistDetail>(entity =>
            {
                entity.HasIndex(e => e.ChecklistId)
                    .HasName("ChecklistId");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.ChecklistId).HasColumnType("bigint(20)");

                entity.Property(e => e.CreationDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.ModificationDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.ParentDetailId).HasColumnType("bigint(20)");

                entity.Property(e => e.Status).HasColumnType("bit(1)");

                entity.Property(e => e.TaskName)
                    .IsRequired()
                    .HasColumnType("varchar(100)");

                entity.HasOne(d => d.Checklist)
                    .WithMany(p => p.ChecklistDetail)
                    .HasForeignKey(d => d.ChecklistId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("checklistdetail_ibfk_1");
            });

            modelBuilder.Entity<Note>(entity =>
            {
                entity.HasIndex(e => e.UserId)
                    .HasName("UserId");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Content).HasColumnType("text");

                entity.Property(e => e.CreationDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.ModificationDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.Title).HasColumnType("varchar(100)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Note)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("note_ibfk_1");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreationDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.Email).HasColumnType("varchar(320)");

                entity.Property(e => e.ModificationDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.UserName).HasColumnType("varchar(50)");
            });
        }
    }
}
