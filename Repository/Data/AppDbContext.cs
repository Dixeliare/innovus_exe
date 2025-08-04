using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<_class> _classes { get; set; }

    public virtual DbSet<attendance> attendances { get; set; }

    public virtual DbSet<attendance_status> attendance_statuses { get; set; }

    public virtual DbSet<class_session> class_sessions { get; set; }

    public virtual DbSet<consultation_request> consultation_requests { get; set; }

    public virtual DbSet<consultation_topic> consultation_topics { get; set; }

    public virtual DbSet<day> days { get; set; }

    public virtual DbSet<day_of_week_lookup> day_of_week_lookups { get; set; }

    public virtual DbSet<document> documents { get; set; }

    public virtual DbSet<gender> genders { get; set; }

    public virtual DbSet<genre> genres { get; set; }

    public virtual DbSet<instrument> instruments { get; set; }

    public virtual DbSet<opening_schedule> opening_schedules { get; set; }

    public virtual DbSet<role> roles { get; set; }

    public virtual DbSet<room> rooms { get; set; }

    public virtual DbSet<schedule> schedules { get; set; }

    public virtual DbSet<sheet> sheets { get; set; }

    public virtual DbSet<sheet_music> sheet_musics { get; set; }

    public virtual DbSet<statistic> statistics { get; set; }

    public virtual DbSet<timeslot> timeslots { get; set; }

    public virtual DbSet<user> users { get; set; }

    public virtual DbSet<user_favorite_sheet> user_favorite_sheets { get; set; }

    public virtual DbSet<week> weeks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<_class>(entity =>
        {
            entity.HasKey(e => e.class_id).HasName("class_pkey");

            entity.ToTable("class");

            entity.HasIndex(e => e.class_code, "class_class_code_key").IsUnique();

            entity.HasIndex(e => e.instrument_id, "idx_class_instrument");

            entity.Property(e => e.class_code).HasMaxLength(255);
            entity.Property(e => e.current_students_count).HasDefaultValue(0);
            entity.Property(e => e.total_students).HasDefaultValue(0);

            entity.HasOne(d => d.instrument).WithMany(p => p._classes)
                .HasForeignKey(d => d.instrument_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_class_instrument");

            entity.HasMany(d => d.users).WithMany(p => p.classes)
                .UsingEntity<Dictionary<string, object>>(
                    "user_class",
                    r => r.HasOne<user>().WithMany()
                        .HasForeignKey("user_id")
                        .HasConstraintName("fk_user_class_user"),
                    l => l.HasOne<_class>().WithMany()
                        .HasForeignKey("class_id")
                        .HasConstraintName("fk_user_class_class"),
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

            entity.HasIndex(e => new { e.user_id, e.class_session_id }, "uq_user_class_session").IsUnique();

            entity.Property(e => e.check_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.status_id).HasDefaultValue(0);

            entity.HasOne(d => d.class_session).WithMany(p => p.attendances)
                .HasForeignKey(d => d.class_session_id)
                .HasConstraintName("fk_attendance_class_session");

            entity.HasOne(d => d.status).WithMany(p => p.attendances)
                .HasForeignKey(d => d.status_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_attendance_status_id");

            entity.HasOne(d => d.user).WithMany(p => p.attendances)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("fk_attendance_user");
        });

        modelBuilder.Entity<attendance_status>(entity =>
        {
            entity.HasKey(e => e.status_id).HasName("attendance_status_pkey");

            entity.ToTable("attendance_status");

            entity.HasIndex(e => e.status_name, "attendance_status_status_name_key").IsUnique();

            entity.Property(e => e.status_id).ValueGeneratedNever();
            entity.Property(e => e.status_name).HasMaxLength(50);
        });

        modelBuilder.Entity<class_session>(entity =>
        {
            entity.HasKey(e => e.class_session_id).HasName("class_session_pkey");

            entity.ToTable("class_session");

            entity.HasOne(d => d._class).WithMany(p => p.class_sessions)
                .HasForeignKey(d => d.class_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_class_session_class");

            entity.HasOne(d => d.day).WithMany(p => p.class_sessions)
                .HasForeignKey(d => d.day_id)
                .HasConstraintName("fk_class_session_days");

            entity.HasOne(d => d.room).WithMany(p => p.class_sessions)
                .HasForeignKey(d => d.room_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_class_session_room");

            entity.HasOne(d => d.time_slot).WithMany(p => p.class_sessions)
                .HasForeignKey(d => d.time_slot_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_class_session_timeslot");
        });

        modelBuilder.Entity<consultation_request>(entity =>
        {
            entity.HasKey(e => e.consultation_request_id).HasName("consultation_request_pkey");

            entity.ToTable("consultation_request");

            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.fullname).HasMaxLength(255);
            entity.Property(e => e.has_contact).HasDefaultValue(false);

            entity.HasOne(d => d.consultation_topic).WithMany(p => p.consultation_requests)
                .HasForeignKey(d => d.consultation_topic_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_consultation_request_topic");

            entity.HasOne(d => d.handled_byNavigation).WithMany(p => p.consultation_requests)
                .HasForeignKey(d => d.handled_by)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_consultation_request_handled_by");

            entity.HasOne(d => d.statistic).WithMany(p => p.consultation_requests)
                .HasForeignKey(d => d.statistic_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_consultation_request_statistic");
        });

        modelBuilder.Entity<consultation_topic>(entity =>
        {
            entity.HasKey(e => e.consultation_topic_id).HasName("consultation_topic_pkey");

            entity.ToTable("consultation_topic");

            entity.HasIndex(e => e.consultation_topic_name, "consultation_topic_consultation_topic_name_key").IsUnique();

            entity.Property(e => e.consultation_topic_name).HasMaxLength(255);
        });

        modelBuilder.Entity<day>(entity =>
        {
            entity.HasKey(e => e.day_id).HasName("days_pkey");

            entity.Property(e => e.day_of_week_name).HasMaxLength(10);
            entity.Property(e => e.is_active).HasDefaultValue(true);

            entity.HasOne(d => d.week).WithMany(p => p.days)
                .HasForeignKey(d => d.week_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_days_weeks");
        });

        modelBuilder.Entity<day_of_week_lookup>(entity =>
        {
            entity.HasKey(e => e.day_of_week_id).HasName("day_of_week_lookup_pkey");

            entity.ToTable("day_of_week_lookup");

            entity.HasIndex(e => e.day_name, "day_of_week_lookup_day_name_key").IsUnique();

            entity.HasIndex(e => e.day_number, "day_of_week_lookup_day_number_key").IsUnique();

            entity.Property(e => e.day_name).HasMaxLength(20);
        });

        modelBuilder.Entity<document>(entity =>
        {
            entity.HasKey(e => e.document_id).HasName("document_pkey");

            entity.ToTable("document");

            entity.HasOne(d => d.instrument).WithMany(p => p.documents)
                .HasForeignKey(d => d.instrument_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_document_instrument");
        });

        modelBuilder.Entity<gender>(entity =>
        {
            entity.HasKey(e => e.gender_id).HasName("gender_pkey");

            entity.ToTable("gender");

            entity.HasIndex(e => e.gender_name, "gender_gender_name_key").IsUnique();

            entity.Property(e => e.gender_name).HasMaxLength(50);
        });

        modelBuilder.Entity<genre>(entity =>
        {
            entity.HasKey(e => e.genre_id).HasName("genre_pkey");

            entity.ToTable("genre");

            entity.HasIndex(e => e.genre_name, "genre_genre_name_key").IsUnique();

            entity.Property(e => e.genre_name).HasMaxLength(255);
        });

        modelBuilder.Entity<instrument>(entity =>
        {
            entity.HasKey(e => e.instrument_id).HasName("instrument_pkey");

            entity.ToTable("instrument");

            entity.HasIndex(e => e.instrument_name, "instrument_instrument_name_key").IsUnique();

            entity.Property(e => e.instrument_name).HasMaxLength(255);
        });

        modelBuilder.Entity<opening_schedule>(entity =>
        {
            entity.HasKey(e => e.opening_schedule_id).HasName("opening_schedule_pkey");

            entity.ToTable("opening_schedule");

            entity.Property(e => e.class_code).HasMaxLength(255);
            entity.Property(e => e.instrument_id).HasDefaultValue(1);
            entity.Property(e => e.is_advanced_class).HasDefaultValue(false);
            entity.Property(e => e.student_quantity).HasDefaultValue(0);
            entity.Property(e => e.total_sessions).HasDefaultValue(0);

            entity.HasOne(d => d.class_codeNavigation).WithMany(p => p.opening_schedules)
                .HasPrincipalKey(p => p.class_code)
                .HasForeignKey(d => d.class_code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_opening_schedule_class");

            entity.HasOne(d => d.instrument).WithMany(p => p.opening_schedules)
                .HasForeignKey(d => d.instrument_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_opening_schedule_instrument");

            entity.HasOne(d => d.teacher_user).WithMany(p => p.opening_schedules)
                .HasForeignKey(d => d.teacher_user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_opening_schedule_teacher_user");

            entity.HasMany(d => d.day_of_weeks).WithMany(p => p.opening_schedules)
                .UsingEntity<Dictionary<string, object>>(
                    "opening_schedule_selected_day",
                    r => r.HasOne<day_of_week_lookup>().WithMany()
                        .HasForeignKey("day_of_week_id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("fk_os_selected_days_day_of_week"),
                    l => l.HasOne<opening_schedule>().WithMany()
                        .HasForeignKey("opening_schedule_id")
                        .HasConstraintName("fk_os_selected_days_opening_schedule"),
                    j =>
                    {
                        j.HasKey("opening_schedule_id", "day_of_week_id").HasName("opening_schedule_selected_days_pkey");
                        j.ToTable("opening_schedule_selected_days");
                    });
        });

        modelBuilder.Entity<role>(entity =>
        {
            entity.HasKey(e => e.role_id).HasName("role_pkey");

            entity.ToTable("role");

            entity.HasIndex(e => e.role_name, "role_role_name_key").IsUnique();

            entity.Property(e => e.role_name).HasMaxLength(255);
        });

        modelBuilder.Entity<room>(entity =>
        {
            entity.HasKey(e => e.room_id).HasName("room_pkey");

            entity.ToTable("room");

            entity.HasIndex(e => e.room_code, "room_room_code_key").IsUnique();

            entity.Property(e => e.room_code).HasMaxLength(50);
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

            entity.HasIndex(e => e.sheet_id, "sheet_music_sheet_id_key").IsUnique();

            entity.Property(e => e.composer).HasMaxLength(255);
            entity.Property(e => e.favorite_count).HasDefaultValue(0);
            entity.Property(e => e.music_name).HasMaxLength(255);

            entity.HasOne(d => d.sheet).WithOne(p => p.sheet_music)
                .HasForeignKey<sheet_music>(d => d.sheet_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_sheet_music_sheet");

            entity.HasMany(d => d.genres).WithMany(p => p.sheet_musics)
                .UsingEntity<Dictionary<string, object>>(
                    "sheet_music_genre",
                    r => r.HasOne<genre>().WithMany()
                        .HasForeignKey("genre_id")
                        .HasConstraintName("fk_sheet_music_genres_genre"),
                    l => l.HasOne<sheet_music>().WithMany()
                        .HasForeignKey("sheet_music_id")
                        .HasConstraintName("fk_sheet_music_genres_sheet_music"),
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

            entity.Property(e => e.consultation_count).HasDefaultValue(0);
            entity.Property(e => e.consultation_request_count).HasDefaultValue(0);
            entity.Property(e => e.monthly_revenue)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0.00");
            entity.Property(e => e.new_students)
                .HasDefaultValue(0)
                .HasComment("Số học sinh mới trong tháng hiện tại");
            entity.Property(e => e.total_guitar_class)
                .HasDefaultValue(0)
                .HasComment("Tổng số lớp guitar");
            entity.Property(e => e.total_piano_class)
                .HasDefaultValue(0)
                .HasComment("Tổng số lớp piano");
            entity.Property(e => e.total_students)
                .HasDefaultValue(0)
                .HasComment("Tổng số học sinh (role student, không bị disable)");
        });

        modelBuilder.Entity<timeslot>(entity =>
        {
            entity.HasKey(e => e.timeslot_id).HasName("timeslot_pkey");

            entity.ToTable("timeslot");

            entity.HasIndex(e => new { e.start_time, e.end_time }, "timeslot_start_time_end_time_key").IsUnique();
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("user_pkey");

            entity.ToTable("user");

            entity.HasIndex(e => e.create_at, "idx_user_create_at");

            entity.HasIndex(e => new { e.role_id, e.is_disabled }, "idx_user_role_disabled");

            entity.HasIndex(e => e.email, "user_email_key").IsUnique();

            entity.HasIndex(e => e.username, "user_username_key").IsUnique();

            entity.Property(e => e.create_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.gender_id).HasDefaultValue(3);
            entity.Property(e => e.is_disabled).HasDefaultValue(false);
            entity.Property(e => e.password).HasMaxLength(255);
            entity.Property(e => e.phone_number).HasMaxLength(255);
            entity.Property(e => e.username).HasMaxLength(255);

            entity.HasOne(d => d.gender).WithMany(p => p.users)
                .HasForeignKey(d => d.gender_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_user_gender");

            entity.HasOne(d => d.role).WithMany(p => p.users)
                .HasForeignKey(d => d.role_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_user_role");

            entity.HasOne(d => d.statistic).WithMany(p => p.users)
                .HasForeignKey(d => d.statistic_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user_statistic");

            entity.HasMany(d => d.documents).WithMany(p => p.users)
                .UsingEntity<Dictionary<string, object>>(
                    "user_doc",
                    r => r.HasOne<document>().WithMany()
                        .HasForeignKey("document_id")
                        .HasConstraintName("fk_user_doc_document"),
                    l => l.HasOne<user>().WithMany()
                        .HasForeignKey("user_id")
                        .HasConstraintName("fk_user_doc_user"),
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

            entity.Property(e => e.is_favorite).HasDefaultValue(true);

            entity.HasOne(d => d.sheet_music).WithMany(p => p.user_favorite_sheets)
                .HasForeignKey(d => d.sheet_music_id)
                .HasConstraintName("fk_user_favorite_sheet_sheet_music");

            entity.HasOne(d => d.user).WithMany(p => p.user_favorite_sheets)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("fk_user_favorite_sheet_user");
        });

        modelBuilder.Entity<week>(entity =>
        {
            entity.HasKey(e => e.week_id).HasName("weeks_pkey");

            entity.Property(e => e.num_active_days).HasDefaultValue(0);

            entity.HasOne(d => d.schedule).WithMany(p => p.weeks)
                .HasForeignKey(d => d.schedule_id)
                .HasConstraintName("fk_weeks_schedule");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
