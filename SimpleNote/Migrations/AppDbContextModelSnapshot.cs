﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SimpleNote.Data;

#nullable disable

namespace SimpleNote.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SimpleNote.Models.Measure", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("MeasureNumber")
                        .HasColumnType("int");

                    b.Property<int>("TrackId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TrackId");

                    b.ToTable("Measures");
                });

            modelBuilder.Entity("SimpleNote.Models.Note", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<double>("Duration")
                        .HasColumnType("float");

                    b.Property<int>("MeasureId")
                        .HasColumnType("int");

                    b.Property<int>("Pitch")
                        .HasColumnType("int");

                    b.Property<int>("Velocity")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("MeasureId");

                    b.ToTable("Notes");
                });

            modelBuilder.Entity("SimpleNote.Models.Project", b =>
                {
                    b.Property<int>("ProjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProjectId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProjectName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("ProjectId");

                    b.HasIndex("UserId");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("SimpleNote.Models.Sample", b =>
                {
                    b.Property<int>("SampleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SampleId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("SampleData")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SampleName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UploadedBy")
                        .HasColumnType("int");

                    b.HasKey("SampleId");

                    b.ToTable("Samples");
                });

            modelBuilder.Entity("SimpleNote.Models.Track", b =>
                {
                    b.Property<int>("TrackId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TrackId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("MidiData")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

                    b.Property<string>("TrackName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("TrackId");

                    b.HasIndex("ProjectId");

                    b.ToTable("Tracks");
                });

            modelBuilder.Entity("SimpleNote.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SimpleNote.Models.UserSample", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("SampleId")
                        .HasColumnType("int");

                    b.Property<int>("UsedInProject")
                        .HasColumnType("int");

                    b.HasKey("UserId", "SampleId", "UsedInProject");

                    b.HasIndex("SampleId");

                    b.HasIndex("UsedInProject");

                    b.ToTable("UserSamples");
                });

            modelBuilder.Entity("SimpleNote.Models.Measure", b =>
                {
                    b.HasOne("SimpleNote.Models.Track", "Track")
                        .WithMany("Measures")
                        .HasForeignKey("TrackId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Track");
                });

            modelBuilder.Entity("SimpleNote.Models.Note", b =>
                {
                    b.HasOne("SimpleNote.Models.Measure", "Measure")
                        .WithMany("Notes")
                        .HasForeignKey("MeasureId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Measure");
                });

            modelBuilder.Entity("SimpleNote.Models.Project", b =>
                {
                    b.HasOne("SimpleNote.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("SimpleNote.Models.Track", b =>
                {
                    b.HasOne("SimpleNote.Models.Project", "Project")
                        .WithMany("Tracks")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("SimpleNote.Models.UserSample", b =>
                {
                    b.HasOne("SimpleNote.Models.Sample", "Sample")
                        .WithMany("UserSamples")
                        .HasForeignKey("SampleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SimpleNote.Models.Project", "Project")
                        .WithMany("UserSamples")
                        .HasForeignKey("UsedInProject")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("SimpleNote.Models.User", "User")
                        .WithMany("UserSamples")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Project");

                    b.Navigation("Sample");

                    b.Navigation("User");
                });

            modelBuilder.Entity("SimpleNote.Models.Measure", b =>
                {
                    b.Navigation("Notes");
                });

            modelBuilder.Entity("SimpleNote.Models.Project", b =>
                {
                    b.Navigation("Tracks");

                    b.Navigation("UserSamples");
                });

            modelBuilder.Entity("SimpleNote.Models.Sample", b =>
                {
                    b.Navigation("UserSamples");
                });

            modelBuilder.Entity("SimpleNote.Models.Track", b =>
                {
                    b.Navigation("Measures");
                });

            modelBuilder.Entity("SimpleNote.Models.User", b =>
                {
                    b.Navigation("UserSamples");
                });
#pragma warning restore 612, 618
        }
    }
}
