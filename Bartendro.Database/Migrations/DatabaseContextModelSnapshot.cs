﻿// <auto-generated />
using System;
using Bartendro.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bartendro.Database.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.0.0");

            modelBuilder.Entity("Bartendro.Database.Entities.Ingredient", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("DateModified")
                        .HasColumnType("TEXT");

                    b.Property<string>("Ounces")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(128);

                    b.Property<Guid?>("RecipeId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(128);

                    b.Property<byte[]>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.HasKey("Id");

                    b.HasIndex("RecipeId");

                    b.ToTable("Ingredients");
                });

            modelBuilder.Entity("Bartendro.Database.Entities.Recipe", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("DateModified")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(128);

                    b.Property<byte[]>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.HasKey("Id");

                    b.ToTable("Recipes");
                });

            modelBuilder.Entity("Bartendro.Database.Entities.Ingredient", b =>
                {
                    b.HasOne("Bartendro.Database.Entities.Recipe", null)
                        .WithMany("Ingredients")
                        .HasForeignKey("RecipeId");
                });
#pragma warning restore 612, 618
        }
    }
}