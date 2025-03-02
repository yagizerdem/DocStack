﻿// <auto-generated />
using System;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DataAccess.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250301221104_mig4")]
    partial class mig4
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.2");

            modelBuilder.Entity("Models.Entity.PaperEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Abstract")
                        .HasColumnType("TEXT");

                    b.Property<string>("Authors")
                        .HasColumnType("TEXT");

                    b.Property<string>("DOI")
                        .HasColumnType("TEXT");

                    b.Property<string>("FullTextLink")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Inserted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<DateTime>("LastUpdated")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Publisher")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("StarredEntityId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<string>("Year")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Papers");
                });

            modelBuilder.Entity("Models.Entity.StarredEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Inserted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<DateTime>("LastUpdated")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<Guid?>("PaperEntityId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PaperEntityId")
                        .IsUnique();

                    b.ToTable("Starred");
                });

            modelBuilder.Entity("Models.Entity.StarredEntity", b =>
                {
                    b.HasOne("Models.Entity.PaperEntity", "PaperEntity")
                        .WithOne("StarredEntity")
                        .HasForeignKey("Models.Entity.StarredEntity", "PaperEntityId");

                    b.Navigation("PaperEntity");
                });

            modelBuilder.Entity("Models.Entity.PaperEntity", b =>
                {
                    b.Navigation("StarredEntity");
                });
#pragma warning restore 612, 618
        }
    }
}
