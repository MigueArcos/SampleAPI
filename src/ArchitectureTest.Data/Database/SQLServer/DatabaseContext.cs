using System;
using System.Collections.Generic;
using ArchitectureTest.Data.Database.SQLServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArchitectureTest.Data.Database.SQLServer;

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
        modelBuilder.Entity<Checklist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Checklis__3214EC07943C235B");

            entity.ToTable("Checklist");

            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Checklists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Checklist__UserI__23BE4960");
        });

        modelBuilder.Entity<ChecklistDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Checklis__3214EC078FA978B9");

            entity.ToTable("ChecklistDetail");

            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TaskName)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Checklist).WithMany(p => p.ChecklistDetails)
                .HasForeignKey(d => d.ChecklistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Checklist__Check__2882FE7D");
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Note__3214EC070E57ACC1");

            entity.ToTable("Note");

            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Notes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Note__UserId__1EF99443");
        });

        modelBuilder.Entity<TokenType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TokenTyp__3214EC079E845675");

            entity.ToTable("TokenType");

            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07A6DB2CC6");

            entity.ToTable("User");

            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(320)
                .IsUnicode(false);
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(256)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserToke__3214EC07EC97A79C");

            entity.ToTable("UserToken");

            entity.Property(e => e.ExpiryTime).HasColumnType("datetime");
            entity.Property(e => e.Token)
                .HasMaxLength(256)
                .IsUnicode(false);

            entity.HasOne(d => d.TokenType).WithMany(p => p.UserTokens)
                .HasForeignKey(d => d.TokenTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserToken__Token__1A34DF26");

            entity.HasOne(d => d.User).WithMany(p => p.UserTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserToken__UserI__1940BAED");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
