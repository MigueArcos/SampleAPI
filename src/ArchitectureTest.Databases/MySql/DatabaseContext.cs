using System;
using System.Collections.Generic;
using ArchitectureTest.Databases.MySql.Entities;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace ArchitectureTest.Databases.MySql;

public partial class DatabaseContext : DbContext
{
    public DatabaseContext()
    {
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Checklist> Checklists { get; set; }

    public virtual DbSet<ChecklistDetail> ChecklistDetails { get; set; }

    public virtual DbSet<Note> Notes { get; set; }

    public virtual DbSet<TokenType> TokenTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Checklist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Checklist");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.Property(e => e.Id).HasMaxLength(32);
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.ModificationDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(32);

            entity.HasOne(d => d.User).WithMany(p => p.Checklists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Checklist_ibfk_1");
        });

        modelBuilder.Entity<ChecklistDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ChecklistDetail");

            entity.HasIndex(e => e.ChecklistId, "ChecklistId");

            entity.Property(e => e.Id).HasMaxLength(32);
            entity.Property(e => e.ChecklistId)
                .IsRequired()
                .HasMaxLength(32);
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.ModificationDate).HasColumnType("datetime");
            entity.Property(e => e.ParentDetailId).HasMaxLength(32);
            entity.Property(e => e.TaskName)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(d => d.Checklist).WithMany(p => p.ChecklistDetails)
                .HasForeignKey(d => d.ChecklistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ChecklistDetail_ibfk_1");
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Note");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.Property(e => e.Id).HasMaxLength(32);
            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.ModificationDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(32);

            entity.HasOne(d => d.User).WithMany(p => p.Notes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Note_ibfk_1");
        });

        modelBuilder.Entity<TokenType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("TokenType");

            entity.Property(e => e.Id).HasMaxLength(32);
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(20);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("User");

            entity.Property(e => e.Id).HasMaxLength(32);
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.Property(e => e.ModificationDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(256);
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("UserToken");

            entity.HasIndex(e => e.TokenTypeId, "TokenTypeId");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.Property(e => e.Id).HasMaxLength(32);
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiryTime).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(256);
            entity.Property(e => e.TokenTypeId)
                .IsRequired()
                .HasMaxLength(32);
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(32);

            entity.HasOne(d => d.TokenType).WithMany(p => p.UserTokens)
                .HasForeignKey(d => d.TokenTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserToken_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.UserTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserToken_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
