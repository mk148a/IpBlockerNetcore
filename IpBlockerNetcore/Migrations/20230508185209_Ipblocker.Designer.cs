﻿// <auto-generated />
using System;
using IpBlockerNetcore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace IpBlockerNetcore.Migrations
{
    [DbContext(typeof(IpBlockerNetcoreContext))]
    [Migration("20230508185209_Ipblocker")]
    partial class Ipblocker
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("IpBlockerNetcore.Models.Domain.BanLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("BanLog");
                });

            modelBuilder.Entity("IpBlockerNetcore.Models.Domain.Entry", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<string>("answer")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("clientIpAddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("protocol")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("qclass")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("qname")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("qtype")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("rcode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("responseType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("rowNumber")
                        .HasColumnType("int");

                    b.Property<DateTime>("timestamp")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.ToTable("Entry");
                });
#pragma warning restore 612, 618
        }
    }
}
