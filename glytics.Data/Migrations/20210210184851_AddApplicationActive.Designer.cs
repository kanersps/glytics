﻿// <auto-generated />

using System;
using glytics.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace glytics.Data.Migrations
{
    [DbContext(typeof(GlyticsDbContext))]
    [Migration("20210210184851_AddApplicationActive")]
    partial class AddApplicationActive
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.3");

            modelBuilder.Entity("glytics.Models.APIKey", b =>
                {
                    b.Property<Guid>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid?>("AccountId")
                        .HasColumnType("char(36)");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.HasKey("Key");

                    b.HasIndex("AccountId");

                    b.ToTable("ApiKey");
                });

            modelBuilder.Entity("glytics.Models.Account", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Password")
                        .HasColumnType("longtext");

                    b.Property<string>("Username")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Account");
                });

            modelBuilder.Entity("glytics.Models.Application", b =>
                {
                    b.Property<string>("TrackingCode")
                        .HasColumnType("varchar(255)");

                    b.Property<Guid?>("AccountId")
                        .HasColumnType("char(36)");

                    b.Property<bool?>("Active")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(true);

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Type")
                        .HasColumnType("longtext");

                    b.HasKey("TrackingCode");

                    b.HasIndex("AccountId");

                    b.ToTable("Application");
                });

            modelBuilder.Entity("glytics.Models.APIKey", b =>
                {
                    b.HasOne("glytics.Models.Account", "Account")
                        .WithMany("ApiKeys")
                        .HasForeignKey("AccountId");

                    b.Navigation("Account");
                });

            modelBuilder.Entity("glytics.Models.Application", b =>
                {
                    b.HasOne("glytics.Models.Account", "Account")
                        .WithMany("Applications")
                        .HasForeignKey("AccountId");

                    b.Navigation("Account");
                });

            modelBuilder.Entity("glytics.Models.Account", b =>
                {
                    b.Navigation("ApiKeys");

                    b.Navigation("Applications");
                });
#pragma warning restore 612, 618
        }
    }
}
