﻿// <auto-generated />
using System;
using FlowWing.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FlowWing.DataAccess.Migrations
{
    [DbContext(typeof(FlowWingDbContext))]
    partial class FlowWingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.18")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("FlowWing.Entities.Attachment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("EmailLogId")
                        .HasColumnType("int");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("EmailLogId");

                    b.ToTable("Attachments");
                });

            modelBuilder.Entity("FlowWing.Entities.EmailLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("Answer")
                        .HasColumnType("int");

                    b.Property<string>("AttachmentIds")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("EmailSubject")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ForwardedFrom")
                        .HasColumnType("int");

                    b.Property<string>("HangfireJobId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsScheduled")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("RecieverDeletionDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("RecipientsEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("SenderDeletionDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("SenderEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("SentDateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("SentEmailBody")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int?>("repeatingLogId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("EmailLogs");
                });

            modelBuilder.Entity("FlowWing.Entities.Log", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("LogTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("FlowWing.Entities.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("FlowWing.Entities.ScheduledEmail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("EmailLogId")
                        .HasColumnType("int");

                    b.Property<bool>("IsRepeating")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastSendingDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("NextSendingDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("RecieverDeletionDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("RepeatEndDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("RepeatInterval")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("SenderDeletionDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("EmailLogId");

                    b.ToTable("ScheduledEmails");
                });

            modelBuilder.Entity("FlowWing.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletionDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsApplicationUser")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastLoginDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("FlowWing.Entities.Attachment", b =>
                {
                    b.HasOne("FlowWing.Entities.EmailLog", "EmailLog")
                        .WithMany()
                        .HasForeignKey("EmailLogId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EmailLog");
                });

            modelBuilder.Entity("FlowWing.Entities.EmailLog", b =>
                {
                    b.HasOne("FlowWing.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("FlowWing.Entities.ScheduledEmail", b =>
                {
                    b.HasOne("FlowWing.Entities.EmailLog", "EmailLog")
                        .WithMany()
                        .HasForeignKey("EmailLogId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EmailLog");
                });
#pragma warning restore 612, 618
        }
    }
}
