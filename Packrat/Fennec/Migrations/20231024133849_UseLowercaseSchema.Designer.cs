﻿// <auto-generated />
using System;
using System.Net;
using Fennec.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fennec.Migrations
{
    [DbContext(typeof(PackratContext))]
    [Migration("20231024133849_UseLowercaseSchema")]
    partial class UseLowercaseSchema
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("fennec")
                .HasAnnotation("ProductVersion", "7.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Fennec.Database.Domain.Layout.CompressedGroup", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("GraphNodeId")
                        .HasColumnType("bigint");

                    b.Property<long>("NetworkHostId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("GraphNodeId");

                    b.HasIndex("NetworkHostId");

                    b.ToTable("CompressedGroups", "fennec");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.GraphNode", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsVisible")
                        .HasColumnType("boolean");

                    b.Property<long>("LayoutId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("LayoutId");

                    b.ToTable("GraphNodes", "fennec");

                    b.HasDiscriminator<string>("Discriminator").HasValue("GraphNode");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.Layout", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Layouts", "fennec");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Technical.NetworkHost", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<IPAddress>("IpAddress")
                        .IsRequired()
                        .HasColumnType("inet");

                    b.HasKey("Id");

                    b.ToTable("NetworkHosts", "fennec");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Technical.SingleTrace", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<decimal>("ByteCount")
                        .HasColumnType("numeric(20,0)");

                    b.Property<long>("DestinationHostId")
                        .HasColumnType("bigint");

                    b.Property<int>("DestinationPort")
                        .HasColumnType("integer");

                    b.Property<IPAddress>("ExporterIp")
                        .IsRequired()
                        .HasColumnType("inet");

                    b.Property<decimal>("PacketCount")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Protocol")
                        .HasColumnType("integer");

                    b.Property<long>("SourceHostId")
                        .HasColumnType("bigint");

                    b.Property<int>("SourcePort")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("DestinationHostId");

                    b.HasIndex("SourceHostId");

                    b.ToTable("SingleTraces", "fennec");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.HostNode", b =>
                {
                    b.HasBaseType("Fennec.Database.Domain.Layout.GraphNode");

                    b.Property<long?>("IslandGroup")
                        .HasColumnType("bigint");

                    b.Property<long>("NetworkHostId")
                        .HasColumnType("bigint");

                    b.HasIndex("NetworkHostId");

                    b.HasDiscriminator().HasValue("HostNode");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.CompressedGroup", b =>
                {
                    b.HasOne("Fennec.Database.Domain.Layout.GraphNode", "GraphNode")
                        .WithMany()
                        .HasForeignKey("GraphNodeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fennec.Database.Domain.Technical.NetworkHost", "NetworkHost")
                        .WithMany()
                        .HasForeignKey("NetworkHostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GraphNode");

                    b.Navigation("NetworkHost");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.GraphNode", b =>
                {
                    b.HasOne("Fennec.Database.Domain.Layout.Layout", "Layout")
                        .WithMany("GraphNodes")
                        .HasForeignKey("LayoutId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("Fennec.Database.Domain.Layout.PositionInfo", "Position", b1 =>
                        {
                            b1.Property<long>("GraphNodeId")
                                .HasColumnType("bigint");

                            b1.Property<int>("X")
                                .HasColumnType("integer");

                            b1.Property<int>("Y")
                                .HasColumnType("integer");

                            b1.HasKey("GraphNodeId");

                            b1.ToTable("GraphNodes", "fennec");

                            b1.WithOwner()
                                .HasForeignKey("GraphNodeId");
                        });

                    b.Navigation("Layout");

                    b.Navigation("Position");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Technical.NetworkHost", b =>
                {
                    b.OwnsOne("Fennec.Database.Domain.Technical.DnsInfo", "DnsInfo", b1 =>
                        {
                            b1.Property<long>("NetworkHostId")
                                .HasColumnType("bigint");

                            b1.Property<string>("DnsName")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<DateTimeOffset>("LastChecked")
                                .HasColumnType("timestamp with time zone");

                            b1.HasKey("NetworkHostId");

                            b1.ToTable("NetworkHosts", "fennec");

                            b1.WithOwner()
                                .HasForeignKey("NetworkHostId");
                        });

                    b.Navigation("DnsInfo");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Technical.SingleTrace", b =>
                {
                    b.HasOne("Fennec.Database.Domain.Technical.NetworkHost", "DestinationHost")
                        .WithMany()
                        .HasForeignKey("DestinationHostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fennec.Database.Domain.Technical.NetworkHost", "SourceHost")
                        .WithMany()
                        .HasForeignKey("SourceHostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DestinationHost");

                    b.Navigation("SourceHost");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.HostNode", b =>
                {
                    b.HasOne("Fennec.Database.Domain.Technical.NetworkHost", "NetworkHost")
                        .WithMany()
                        .HasForeignKey("NetworkHostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NetworkHost");
                });

            modelBuilder.Entity("Fennec.Database.Domain.Layout.Layout", b =>
                {
                    b.Navigation("GraphNodes");
                });
#pragma warning restore 612, 618
        }
    }
}