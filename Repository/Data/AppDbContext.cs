using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Models;

namespace Repository.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<_class> _classes { get; set; }

    public virtual DbSet<attendance> attendances { get; set; }

    public virtual DbSet<class_session> class_sessions { get; set; }

    public virtual DbSet<consultation_request> consultation_requests { get; set; }

    public virtual DbSet<consultation_topic> consultation_topics { get; set; }

    public virtual DbSet<document> documents { get; set; }

    public virtual DbSet<genre> genres { get; set; }

    public virtual DbSet<instrument> instruments { get; set; }

    public virtual DbSet<opening_schedule> opening_schedules { get; set; }

    public virtual DbSet<role> roles { get; set; }

    public virtual DbSet<schedule> schedules { get; set; }

    public virtual DbSet<sheet> sheets { get; set; }

    public virtual DbSet<sheet_music> sheet_musics { get; set; }

    public virtual DbSet<statistic> statistics { get; set; }

    public virtual DbSet<timeslot> timeslots { get; set; }

    public virtual DbSet<user> users { get; set; }

    public virtual DbSet<user_favorite_sheet> user_favorite_sheets { get; set; }

    public virtual DbSet<week> weeks { get; set; }

//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//         => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=innovus_db;Username=postgres;Password=12345");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // hoặc AppDomain.CurrentDomain.BaseDirectory
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");
            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<_class>(entity =>
        {
            entity.HasKey(e => e.class_id).HasName("class_pkey");

            entity.ToTable("class");

            entity.Property(e => e.class_code).HasMaxLength(20);
            entity.Property(e => e.instrument).HasMaxLength(50);

            entity.HasMany(d => d.users).WithMany(p => p.classes)
                .UsingEntity<Dictionary<string, object>>(
                    "user_class",
                    r => r.HasOne<user>().WithMany()
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("user_class_user_id_fkey"),
                    l => l.HasOne<_class>().WithMany()
                        .HasForeignKey("class_id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("user_class_class_id_fkey"),
                    j =>
                    {
                        j.HasKey("class_id", "user_id").HasName("user_class_pkey");
                        j.ToTable("user_class");
                    });
        });

        modelBuilder.Entity<attendance>(entity =>
        {
            entity.HasKey(e => e.attendance_id).HasName("attendance_pkey");

            entity.ToTable("attendance");

            entity.Property(e => e.check_at).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.class_session).WithMany(p => p.attendances)
                .HasForeignKey(d => d.class_session_id)
                .HasConstraintName("attendance_class_session_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.attendances)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("attendance_user_id_fkey");
        });

        modelBuilder.Entity<class_session>(entity =>
        {
            entity.HasKey(e => e.class_session_id).HasName("class_session_pkey");

            entity.ToTable("class_session");

            entity.Property(e => e.room_code).HasMaxLength(20);

            entity.HasOne(d => d._class).WithMany(p => p.class_sessions)
                .HasForeignKey(d => d.class_id)
                .HasConstraintName("class_session_class_id_fkey");

            entity.HasOne(d => d.time_slot).WithMany(p => p.class_sessions)
                .HasForeignKey(d => d.time_slot_id)
                .HasConstraintName("class_session_time_slot_id_fkey");

            entity.HasOne(d => d.week).WithMany(p => p.class_sessions)
                .HasForeignKey(d => d.week_id)
                .HasConstraintName("class_session_week_id_fkey");
        });

        modelBuilder.Entity<consultation_request>(entity =>
        {
            entity.HasKey(e => e.consultation_request_id).HasName("consultation_request_pkey");

            entity.ToTable("consultation_request");

            entity.Property(e => e.email).HasMaxLength(100);
            entity.Property(e => e.fullname).HasMaxLength(100);

            entity.HasOne(d => d.consultation_topic).WithMany(p => p.consultation_requests)
                .HasForeignKey(d => d.consultation_topic_id)
                .HasConstraintName("consultation_request_consultation_topic_id_fkey");

            entity.HasOne(d => d.statistic).WithMany(p => p.consultation_requests)
                .HasForeignKey(d => d.statistic_id)
                .HasConstraintName("consultation_request_statistic_id_fkey");
        });

        modelBuilder.Entity<consultation_topic>(entity =>
        {
            entity.HasKey(e => e.consultation_topic_id).HasName("consultation_topic_pkey");

            entity.ToTable("consultation_topic");

            entity.Property(e => e.consultation_topic_name).HasMaxLength(100);
        });

        modelBuilder.Entity<document>(entity =>
        {
            entity.HasKey(e => e.document_id).HasName("document_pkey");

            entity.ToTable("document");

            entity.HasOne(d => d.instrument).WithMany(p => p.documents)
                .HasForeignKey(d => d.instrument_id)
                .HasConstraintName("document_instrument_id_fkey");
        });

        modelBuilder.Entity<genre>(entity =>
        {
            entity.HasKey(e => e.genre_id).HasName("genre_pkey");

            entity.ToTable("genre");

            entity.Property(e => e.genre_name).HasMaxLength(100);
        });

        modelBuilder.Entity<instrument>(entity =>
        {
            entity.HasKey(e => e.instrument_id).HasName("instrument_pkey");

            entity.ToTable("instrument");

            entity.Property(e => e.instrument_name).HasMaxLength(50);
        });

        modelBuilder.Entity<opening_schedule>(entity =>
        {
            entity.HasKey(e => e.opening_schedule_id).HasName("opening_schedule_pkey");

            entity.ToTable("opening_schedule");

            entity.Property(e => e.class_code).HasMaxLength(20);
            entity.Property(e => e.schedule).HasMaxLength(100);
            entity.Property(e => e.subject).HasMaxLength(100);
        });

        modelBuilder.Entity<role>(entity =>
        {
            entity.HasKey(e => e.role_id).HasName("role_pkey");

            entity.ToTable("role");

            entity.Property(e => e.role_name).HasMaxLength(50);
        });

        modelBuilder.Entity<schedule>(entity =>
        {
            entity.HasKey(e => e.schedule_id).HasName("schedule_pkey");

            entity.ToTable("schedule");
        });

        modelBuilder.Entity<sheet>(entity =>
        {
            entity.HasKey(e => e.sheet_id).HasName("sheet_pkey");

            entity.ToTable("sheet");
        });

        modelBuilder.Entity<sheet_music>(entity =>
        {
            entity.HasKey(e => e.sheet_music_id).HasName("sheet_music_pkey");

            entity.ToTable("sheet_music");

            entity.Property(e => e.composer).HasMaxLength(100);
            entity.Property(e => e.music_name).HasMaxLength(100);

            entity.HasOne(d => d.sheet).WithMany(p => p.sheet_musics)
                .HasForeignKey(d => d.sheet_id)
                .HasConstraintName("sheet_music_sheet_id_fkey");

            entity.HasMany(d => d.genres).WithMany(p => p.sheet_musics)
                .UsingEntity<Dictionary<string, object>>(
                    "sheet_music_genre",
                    r => r.HasOne<genre>().WithMany()
                        .HasForeignKey("genre_id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("sheet_music_genres_genre_id_fkey"),
                    l => l.HasOne<sheet_music>().WithMany()
                        .HasForeignKey("sheet_music_id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("sheet_music_genres_sheet_music_id_fkey"),
                    j =>
                    {
                        j.HasKey("sheet_music_id", "genre_id").HasName("sheet_music_genres_pkey");
                        j.ToTable("sheet_music_genres");
                    });
        });

        modelBuilder.Entity<statistic>(entity =>
        {
            entity.HasKey(e => e.statistic_id).HasName("statistic_pkey");

            entity.ToTable("statistic");

            entity.Property(e => e.date).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<timeslot>(entity =>
        {
            entity.HasKey(e => e.timeslot_id).HasName("timeslot_pkey");

            entity.ToTable("timeslot");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("user_pkey");

            entity.ToTable("user");

            entity.Property(e => e.create_at).HasColumnType("timestamp without time zone");
            entity.Property(e => e.password).HasMaxLength(100);
            entity.Property(e => e.phone_number).HasMaxLength(15);
            entity.Property(e => e.username).HasMaxLength(50);

            entity.HasOne(d => d.opening_schedule).WithMany(p => p.users)
                .HasForeignKey(d => d.opening_schedule_id)
                .HasConstraintName("user_opening_schedule_id_fkey");

            entity.HasOne(d => d.role).WithMany(p => p.users)
                .HasForeignKey(d => d.role_id)
                .HasConstraintName("user_role_id_fkey");

            entity.HasOne(d => d.statistic).WithMany(p => p.users)
                .HasForeignKey(d => d.statistic_id)
                .HasConstraintName("user_statistic_id_fkey");

            entity.HasMany(d => d.documents).WithMany(p => p.users)
                .UsingEntity<Dictionary<string, object>>(
                    "user_doc",
                    r => r.HasOne<document>().WithMany()
                        .HasForeignKey("document_id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("user_doc_document_id_fkey"),
                    l => l.HasOne<user>().WithMany()
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("user_doc_user_id_fkey"),
                    j =>
                    {
                        j.HasKey("user_id", "document_id").HasName("user_doc_pkey");
                        j.ToTable("user_doc");
                    });
        });

        modelBuilder.Entity<user_favorite_sheet>(entity =>
        {
            entity.HasKey(e => new { e.user_id, e.sheet_music_id }).HasName("user_favorite_sheet_pkey");

            entity.ToTable("user_favorite_sheet");

            entity.HasOne(d => d.sheet_music).WithMany(p => p.user_favorite_sheets)
                .HasForeignKey(d => d.sheet_music_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_favorite_sheet_sheet_music_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.user_favorite_sheets)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_favorite_sheet_user_id_fkey");
        });

        modelBuilder.Entity<week>(entity =>
        {
            entity.HasKey(e => e.week_id).HasName("week_pkey");

            entity.ToTable("week");

            entity.HasOne(d => d.schedule).WithMany(p => p.weeks)
                .HasForeignKey(d => d.schedule_id)
                .HasConstraintName("week_schedule_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
