using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Context;

public partial class Palaro2026Context : DbContext
{
    public Palaro2026Context(DbContextOptions<Palaro2026Context> options)
        : base(options)
    {
    }

    public virtual DbSet<EventStages> EventStages { get; set; }

    public virtual DbSet<EventStreamServices> EventStreamServices { get; set; }

    public virtual DbSet<EventStreams> EventStreams { get; set; }

    public virtual DbSet<EventVenues> EventVenues { get; set; }

    public virtual DbSet<EventVersusTeamPlayers> EventVersusTeamPlayers { get; set; }

    public virtual DbSet<EventVersusTeams> EventVersusTeams { get; set; }

    public virtual DbSet<Events> Events { get; set; }

    public virtual DbSet<News> News { get; set; }

    public virtual DbSet<NewsCategories> NewsCategories { get; set; }

    public virtual DbSet<ProfileCoaches> ProfileCoaches { get; set; }

    public virtual DbSet<ProfilePlayerSportCoaches> ProfilePlayerSportCoaches { get; set; }

    public virtual DbSet<ProfilePlayerSports> ProfilePlayerSports { get; set; }

    public virtual DbSet<ProfilePlayers> ProfilePlayers { get; set; }

    public virtual DbSet<SchoolBilletingQuarters> SchoolBilletingQuarters { get; set; }

    public virtual DbSet<SchoolDivisions> SchoolDivisions { get; set; }

    public virtual DbSet<SchoolLevels> SchoolLevels { get; set; }

    public virtual DbSet<SchoolRegions> SchoolRegions { get; set; }

    public virtual DbSet<Schools> Schools { get; set; }

    public virtual DbSet<Sponsors> Sponsors { get; set; }

    public virtual DbSet<SportCategories> SportCategories { get; set; }

    public virtual DbSet<SportGenderCategories> SportGenderCategories { get; set; }

    public virtual DbSet<SportSubcategories> SportSubcategories { get; set; }

    public virtual DbSet<Sports> Sports { get; set; }

    public virtual DbSet<UserRoles> UserRoles { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventStages>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_EventStage");

            entity.Property(e => e.Stage)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<EventStreamServices>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_EventStreams");

            entity.Property(e => e.StreamService)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<EventStreams>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_EventStreamServiceStreams");

            entity.Property(e => e.StreamDate).HasColumnType("datetime");
            entity.Property(e => e.StreamTitle)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.StreamURL).IsUnicode(false);

            entity.HasOne(d => d.EventStreamService).WithMany(p => p.EventStreams)
                .HasForeignKey(d => d.EventStreamServiceID)
                .HasConstraintName("FK_EventStreamServiceStreams_EventStreamServices");
        });

        modelBuilder.Entity<EventVenues>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__Venues__3214EC2738289954");

            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Latitude).HasColumnType("decimal(8, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Venue)
                .HasMaxLength(250)
                .IsUnicode(false);
        });

        modelBuilder.Entity<EventVersusTeamPlayers>(entity =>
        {
            entity.Property(e => e.ProfilePlayerID)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.EventVersus).WithMany(p => p.EventVersusTeamPlayers)
                .HasForeignKey(d => d.EventVersusID)
                .HasConstraintName("FK_EventVersusTeamPlayers_EventVersus");

            entity.HasOne(d => d.ProfilePlayer).WithMany(p => p.EventVersusTeamPlayers)
                .HasForeignKey(d => d.ProfilePlayerID)
                .HasConstraintName("FK_EventVersusTeamPlayers_ProfilePlayers");
        });

        modelBuilder.Entity<EventVersusTeams>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_EventVersus");

            entity.Property(e => e.EventID)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Rank)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RecentUpdateAt).HasColumnType("datetime");
            entity.Property(e => e.Score)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Event).WithMany(p => p.EventVersusTeams)
                .HasForeignKey(d => d.EventID)
                .HasConstraintName("FK_EventVersuses_Events");

            entity.HasOne(d => d.SchoolRegion).WithMany(p => p.EventVersusTeams)
                .HasForeignKey(d => d.SchoolRegionID)
                .HasConstraintName("FK_EventVersus_SchoolRegions");
        });

        modelBuilder.Entity<Events>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__events__3213E83F7F467A0E");

            entity.Property(e => e.ID)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Time).HasColumnType("time(2)");
            entity.Property(e => e.UserID)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.EventStage).WithMany(p => p.Events)
                .HasForeignKey(d => d.EventStageID)
                .HasConstraintName("FK_Events_EventStages");

            entity.HasOne(d => d.EventStream).WithMany(p => p.Events)
                .HasForeignKey(d => d.EventStreamID)
                .HasConstraintName("FK_Events_EventStreams");

            entity.HasOne(d => d.EventVenues).WithMany(p => p.Events)
                .HasForeignKey(d => d.EventVenuesID)
                .HasConstraintName("FK__Events_EventVenues");

            entity.HasOne(d => d.SportSubcategory).WithMany(p => p.Events)
                .HasForeignKey(d => d.SportSubcategoryID)
                .HasConstraintName("FK_Events_SportSubcategories");

            entity.HasOne(d => d.User).WithMany(p => p.Events)
                .HasForeignKey(d => d.UserID)
                .HasConstraintName("FK_Events_Users");
        });

        modelBuilder.Entity<News>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__News__3214EC27B5C04219");

            entity.Property(e => e.AuthorID)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Content).IsUnicode(false);
            entity.Property(e => e.DatePosted).HasColumnType("datetime");

            entity.HasOne(d => d.Author).WithMany(p => p.News)
                .HasForeignKey(d => d.AuthorID)
                .HasConstraintName("FK_News_Users");

            entity.HasOne(d => d.NewsCategory).WithMany(p => p.News)
                .HasForeignKey(d => d.NewsCategoryID)
                .HasConstraintName("FK_News_NewsCategories");
        });

        modelBuilder.Entity<NewsCategories>(entity =>
        {
            entity.Property(e => e.ID).ValueGeneratedNever();
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ProfileCoaches>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__TeamCoac__3214EC27C880573E");

            entity.Property(e => e.ID)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.SchoolRegion).WithMany(p => p.ProfileCoaches)
                .HasForeignKey(d => d.SchoolRegionID)
                .HasConstraintName("FK__PlayerTeamCoach_SchoolRegions");
        });

        modelBuilder.Entity<ProfilePlayerSportCoaches>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_ProfilePlayerCoaches");

            entity.Property(e => e.ProfileCoachID)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.ProfileCoach).WithMany(p => p.ProfilePlayerSportCoaches)
                .HasForeignKey(d => d.ProfileCoachID)
                .HasConstraintName("FK_ProfilePlayerCoaches_ProfileCoaches");

            entity.HasOne(d => d.ProfilePlayerSport).WithMany(p => p.ProfilePlayerSportCoaches)
                .HasForeignKey(d => d.ProfilePlayerSportID)
                .HasConstraintName("FK_ProfilePlayerSportCoaches_ProfilePlayerSports");
        });

        modelBuilder.Entity<ProfilePlayerSports>(entity =>
        {
            entity.Property(e => e.ID).ValueGeneratedNever();
            entity.Property(e => e.ProfilePlayerID)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.ProfilePlayer).WithMany(p => p.ProfilePlayerSports)
                .HasForeignKey(d => d.ProfilePlayerID)
                .HasConstraintName("FK_ProfilePlayerSports_ProfilePlayers");

            entity.HasOne(d => d.SportSubcategory).WithMany(p => p.ProfilePlayerSports)
                .HasForeignKey(d => d.SportSubcategoryID)
                .HasConstraintName("FK_ProfilePlayerSports_SportSubcategories");
        });

        modelBuilder.Entity<ProfilePlayers>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__PlayerPr__3214EC2796F36333");

            entity.Property(e => e.ID)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.School).WithMany(p => p.ProfilePlayers)
                .HasForeignKey(d => d.SchoolID)
                .HasConstraintName("FK__PlayerProfiles_Schools");

            entity.HasOne(d => d.Sport).WithMany(p => p.ProfilePlayers)
                .HasForeignKey(d => d.SportID)
                .HasConstraintName("FK_ProfilePlayers_Sports");
        });

        modelBuilder.Entity<SchoolBilletingQuarters>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__billetin__3213E83F62A22D2C");

            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.BilletingQuarter)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ContactPersonNumber)
                .HasMaxLength(11)
                .IsUnicode(false);
            entity.Property(e => e.Latitude).HasColumnType("decimal(8, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");

            entity.HasOne(d => d.SchoolRegion).WithMany(p => p.SchoolBilletingQuarters)
                .HasForeignKey(d => d.SchoolRegionID)
                .HasConstraintName("FK__SchoolRegions_SchoolBilletingQuarters");
        });

        modelBuilder.Entity<SchoolDivisions>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__Division__3214EC27AA22701B");

            entity.Property(e => e.Division)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.SchoolRegion).WithMany(p => p.SchoolDivisions)
                .HasForeignKey(d => d.SchoolRegionID)
                .HasConstraintName("FK__SchoolRegions_SchoolDivisions");
        });

        modelBuilder.Entity<SchoolLevels>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__Levels__3214EC27EAD37397");

            entity.Property(e => e.Level)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SchoolRegions>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__Regional__3214EC27FFF642E1");

            entity.Property(e => e.Abbreviation)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Region)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Schools>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__Schools__3214EC27215C1750");

            entity.Property(e => e.School)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.SchoolDivision).WithMany(p => p.Schools)
                .HasForeignKey(d => d.SchoolDivisionID)
                .HasConstraintName("FK__Schools__SchoolDivisions");

            entity.HasOne(d => d.SchoolLevels).WithMany(p => p.Schools)
                .HasForeignKey(d => d.SchoolLevelsID)
                .HasConstraintName("FK_Schools_SchoolLevels");
        });

        modelBuilder.Entity<Sponsors>(entity =>
        {
            entity.Property(e => e.Logo).IsUnicode(false);
            entity.Property(e => e.Sponsor)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SportCategories>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__SportCat__3214EC274517CC87");

            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SportGenderCategories>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__GenderCa__3214EC273BE4306C");

            entity.Property(e => e.Gender)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SportSubcategories>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__sport_su__3213E83F92479A7A");

            entity.Property(e => e.Subcategory)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.SchoolLevel).WithMany(p => p.SportSubcategories)
                .HasForeignKey(d => d.SchoolLevelID)
                .HasConstraintName("FK_SportSubcategories_SportLevels");

            entity.HasOne(d => d.SportGenderCategory).WithMany(p => p.SportSubcategories)
                .HasForeignKey(d => d.SportGenderCategoryID)
                .HasConstraintName("FK_SportSubcategories_SportGenderCategories");

            entity.HasOne(d => d.Sport).WithMany(p => p.SportSubcategories)
                .HasForeignKey(d => d.SportID)
                .HasConstraintName("FK_SportSubcategories_Sports");
        });

        modelBuilder.Entity<Sports>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__sports__3213E83FE4966238");

            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Sport)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.SportCategory).WithMany(p => p.Sports)
                .HasForeignKey(d => d.SportCategoryID)
                .HasConstraintName("FK__Sports_SportCategories");
        });

        modelBuilder.Entity<UserRoles>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_roles");

            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.Property(e => e.ID)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Affiliation)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.EmailAddress)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash).IsUnicode(false);
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleID)
                .HasConstraintName("FK_Users_UserRoles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
