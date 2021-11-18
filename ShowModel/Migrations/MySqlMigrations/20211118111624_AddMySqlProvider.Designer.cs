﻿// <auto-generated />
using System;
using Carmen.ShowModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Carmen.ShowModel.Migrations.MySqlMigrations
{
    [DbContext(typeof(MySqlShowContext))]
    [Migration("20211118111624_AddMySqlProvider")]
    partial class AddMySqlProvider
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.10");

            modelBuilder.Entity("ApplicantRole", b =>
                {
                    b.Property<int>("CastApplicantId")
                        .HasColumnType("int");

                    b.Property<int>("RolesRoleId")
                        .HasColumnType("int");

                    b.HasKey("CastApplicantId", "RolesRoleId");

                    b.HasIndex("RolesRoleId");

                    b.ToTable("ApplicantRole");
                });

            modelBuilder.Entity("ApplicantTag", b =>
                {
                    b.Property<int>("MembersApplicantId")
                        .HasColumnType("int");

                    b.Property<int>("TagsTagId")
                        .HasColumnType("int");

                    b.HasKey("MembersApplicantId", "TagsTagId");

                    b.HasIndex("TagsTagId");

                    b.ToTable("ApplicantTag");
                });

            modelBuilder.Entity("Carmen.ShowModel.Applicants.Ability", b =>
                {
                    b.Property<int>("ApplicantId")
                        .HasColumnType("int");

                    b.Property<int>("CriteriaId")
                        .HasColumnType("int");

                    b.Property<uint>("Mark")
                        .HasColumnType("int unsigned");

                    b.HasKey("ApplicantId", "CriteriaId");

                    b.HasIndex("CriteriaId");

                    b.ToTable("Abilities");
                });

            modelBuilder.Entity("Carmen.ShowModel.Applicants.AlternativeCast", b =>
                {
                    b.Property<int>("AlternativeCastId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Initial")
                        .IsRequired()
                        .HasColumnType("varchar(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("AlternativeCastId");

                    b.ToTable("AlternativeCasts");
                });

            modelBuilder.Entity("Carmen.ShowModel.Applicants.Applicant", b =>
                {
                    b.Property<int>("ApplicantId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int?>("AlternativeCastId")
                        .HasColumnType("int");

                    b.Property<int?>("CastGroupId")
                        .HasColumnType("int");

                    b.Property<int?>("CastNumber")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DateOfBirth")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ExternalData")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("Gender")
                        .HasColumnType("int");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("PhotoImageId")
                        .HasColumnType("int");

                    b.Property<int?>("SameCastSetId")
                        .HasColumnType("int");

                    b.Property<int?>("ShowRootNodeId")
                        .HasColumnType("int");

                    b.HasKey("ApplicantId");

                    b.HasIndex("AlternativeCastId");

                    b.HasIndex("CastGroupId");

                    b.HasIndex("PhotoImageId");

                    b.HasIndex("SameCastSetId");

                    b.HasIndex("ShowRootNodeId");

                    b.ToTable("Applicants");
                });

            modelBuilder.Entity("Carmen.ShowModel.Applicants.CastGroup", b =>
                {
                    b.Property<int>("CastGroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Abbreviation")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("AlternateCasts")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<uint?>("RequiredCount")
                        .HasColumnType("int unsigned");

                    b.HasKey("CastGroupId");

                    b.ToTable("CastGroups");
                });

            modelBuilder.Entity("Carmen.ShowModel.Applicants.SameCastSet", b =>
                {
                    b.Property<int>("SameCastSetId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.HasKey("SameCastSetId");

                    b.ToTable("SameCastSets");
                });

            modelBuilder.Entity("Carmen.ShowModel.Applicants.Tag", b =>
                {
                    b.Property<int>("TagId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<int?>("IconImageId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("TagId");

                    b.HasIndex("IconImageId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("Carmen.ShowModel.Criterias.Criteria", b =>
                {
                    b.Property<int>("CriteriaId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<uint>("MaxMark")
                        .HasColumnType("int unsigned");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<bool>("Primary")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("Required")
                        .HasColumnType("tinyint(1)");

                    b.Property<double>("Weight")
                        .HasColumnType("double");

                    b.HasKey("CriteriaId");

                    b.ToTable("Criterias");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Criteria");
                });

            modelBuilder.Entity("Carmen.ShowModel.Image", b =>
                {
                    b.Property<int>("ImageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<byte[]>("ImageData")
                        .IsRequired()
                        .HasColumnType("longblob");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("ImageId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.Requirement", b =>
                {
                    b.Property<int>("RequirementId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<double>("OverallWeight")
                        .HasColumnType("double");

                    b.Property<bool>("Primary")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Reason")
                        .HasColumnType("longtext");

                    b.Property<double>("SuitabilityWeight")
                        .HasColumnType("double");

                    b.HasKey("RequirementId");

                    b.ToTable("Requirements");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Requirement");
                });

            modelBuilder.Entity("Carmen.ShowModel.Structure.Node", b =>
                {
                    b.Property<int>("NodeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<int?>("ParentNodeId")
                        .HasColumnType("int");

                    b.HasKey("NodeId");

                    b.HasIndex("ParentNodeId");

                    b.ToTable("Nodes");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Node");
                });

            modelBuilder.Entity("Carmen.ShowModel.Structure.Role", b =>
                {
                    b.Property<int>("RoleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("RoleId");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Carmen.ShowModel.Structure.SectionType", b =>
                {
                    b.Property<int>("SectionTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<bool>("AllowConsecutiveItems")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("AllowMultipleRoles")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("AllowNoRoles")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("SectionTypeId");

                    b.ToTable("SectionTypes");
                });

            modelBuilder.Entity("CastGroupRequirement", b =>
                {
                    b.Property<int>("RequirementsRequirementId")
                        .HasColumnType("int");

                    b.Property<int>("UsedByCastGroupsCastGroupId")
                        .HasColumnType("int");

                    b.HasKey("RequirementsRequirementId", "UsedByCastGroupsCastGroupId");

                    b.HasIndex("UsedByCastGroupsCastGroupId");

                    b.ToTable("CastGroupRequirement");
                });

            modelBuilder.Entity("CombinedRequirementRequirement", b =>
                {
                    b.Property<int>("SubRequirementsRequirementId")
                        .HasColumnType("int");

                    b.Property<int>("UsedByCombinedRequirementsRequirementId")
                        .HasColumnType("int");

                    b.HasKey("SubRequirementsRequirementId", "UsedByCombinedRequirementsRequirementId");

                    b.HasIndex("UsedByCombinedRequirementsRequirementId");

                    b.ToTable("CombinedRequirementRequirement");
                });

            modelBuilder.Entity("ItemRole", b =>
                {
                    b.Property<int>("ItemsNodeId")
                        .HasColumnType("int");

                    b.Property<int>("RolesRoleId")
                        .HasColumnType("int");

                    b.HasKey("ItemsNodeId", "RolesRoleId");

                    b.HasIndex("RolesRoleId");

                    b.ToTable("ItemRole");
                });

            modelBuilder.Entity("RequirementRole", b =>
                {
                    b.Property<int>("RequirementsRequirementId")
                        .HasColumnType("int");

                    b.Property<int>("UsedByRolesRoleId")
                        .HasColumnType("int");

                    b.HasKey("RequirementsRequirementId", "UsedByRolesRoleId");

                    b.HasIndex("UsedByRolesRoleId");

                    b.ToTable("RequirementRole");
                });

            modelBuilder.Entity("RequirementTag", b =>
                {
                    b.Property<int>("RequirementsRequirementId")
                        .HasColumnType("int");

                    b.Property<int>("UsedByTagsTagId")
                        .HasColumnType("int");

                    b.HasKey("RequirementsRequirementId", "UsedByTagsTagId");

                    b.HasIndex("UsedByTagsTagId");

                    b.ToTable("RequirementTag");
                });

            modelBuilder.Entity("Carmen.ShowModel.Criterias.BooleanCriteria", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Criterias.Criteria");

                    b.HasDiscriminator().HasValue("BooleanCriteria");
                });

            modelBuilder.Entity("Carmen.ShowModel.Criterias.NumericCriteria", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Criterias.Criteria");

                    b.HasDiscriminator().HasValue("NumericCriteria");
                });

            modelBuilder.Entity("Carmen.ShowModel.Criterias.SelectCriteria", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Criterias.Criteria");

                    b.Property<string>("Options")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasDiscriminator().HasValue("SelectCriteria");
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.AbilityExactRequirement", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Requirements.Requirement");

                    b.Property<int>("CriteriaId")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("int")
                        .HasColumnName("CriteriaId");

                    b.Property<double>("ExistingRoleCost")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("double")
                        .HasColumnName("ExistingRoleCost");

                    b.Property<uint>("RequiredValue")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("int unsigned");

                    b.HasIndex("CriteriaId");

                    b.HasDiscriminator().HasValue("AbilityExactRequirement");
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.AbilityRangeRequirement", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Requirements.Requirement");

                    b.Property<int>("CriteriaId")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("int")
                        .HasColumnName("CriteriaId");

                    b.Property<double>("ExistingRoleCost")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("double")
                        .HasColumnName("ExistingRoleCost");

                    b.Property<uint?>("Maximum")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("int unsigned");

                    b.Property<uint?>("Minimum")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("int unsigned");

                    b.Property<bool>("ScaleSuitability")
                        .HasColumnType("tinyint(1)");

                    b.HasIndex("CriteriaId");

                    b.HasDiscriminator().HasValue("AbilityRangeRequirement");
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.AgeRequirement", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Requirements.Requirement");

                    b.Property<uint?>("Maximum")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("int unsigned");

                    b.Property<uint?>("Minimum")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("int unsigned");

                    b.HasDiscriminator().HasValue("AgeRequirement");
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.CombinedRequirement", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Requirements.Requirement");

                    b.HasDiscriminator().HasValue("CombinedRequirement");
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.GenderRequirement", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Requirements.Requirement");

                    b.Property<uint>("RequiredValue")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("int unsigned");

                    b.HasDiscriminator().HasValue("GenderRequirement");
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.NotRequirement", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Requirements.Requirement");

                    b.Property<int>("SubRequirementId")
                        .HasColumnType("int");

                    b.HasIndex("SubRequirementId");

                    b.HasDiscriminator().HasValue("NotRequirement");
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.TagRequirement", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Requirements.Requirement");

                    b.Property<int>("RequiredTagId")
                        .HasColumnType("int");

                    b.HasIndex("RequiredTagId");

                    b.HasDiscriminator().HasValue("TagRequirement");
                });

            modelBuilder.Entity("Carmen.ShowModel.Structure.InnerNode", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Structure.Node");

                    b.HasDiscriminator().HasValue("InnerNode");
                });

            modelBuilder.Entity("Carmen.ShowModel.Structure.Item", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Structure.Node");

                    b.HasDiscriminator().HasValue("Item");
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.AndRequirement", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Requirements.CombinedRequirement");

                    b.Property<bool>("AverageSuitability")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("AverageSuitability");

                    b.HasDiscriminator().HasValue("AndRequirement");
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.OrRequirement", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Requirements.CombinedRequirement");

                    b.Property<bool>("AverageSuitability")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("AverageSuitability");

                    b.HasDiscriminator().HasValue("OrRequirement");
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.XorRequirement", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Requirements.CombinedRequirement");

                    b.HasDiscriminator().HasValue("XorRequirement");
                });

            modelBuilder.Entity("Carmen.ShowModel.Structure.Section", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Structure.InnerNode");

                    b.Property<int>("SectionTypeId")
                        .HasColumnType("int");

                    b.HasIndex("SectionTypeId");

                    b.HasDiscriminator().HasValue("Section");
                });

            modelBuilder.Entity("Carmen.ShowModel.Structure.ShowRoot", b =>
                {
                    b.HasBaseType("Carmen.ShowModel.Structure.InnerNode");

                    b.Property<bool>("AllowConsecutiveItems")
                        .HasColumnType("tinyint(1)");

                    b.Property<int?>("CastNumberOrderByCriteriaId")
                        .HasColumnType("int");

                    b.Property<int>("CastNumberOrderDirection")
                        .HasColumnType("int");

                    b.Property<double?>("CommonOverallWeight")
                        .HasColumnType("double");

                    b.Property<int?>("LogoImageId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ShowDate")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("WeightExistingRoleCosts")
                        .HasColumnType("tinyint(1)");

                    b.HasIndex("CastNumberOrderByCriteriaId");

                    b.HasIndex("LogoImageId");

                    b.HasDiscriminator().HasValue("ShowRoot");
                });

            modelBuilder.Entity("ApplicantRole", b =>
                {
                    b.HasOne("Carmen.ShowModel.Applicants.Applicant", null)
                        .WithMany()
                        .HasForeignKey("CastApplicantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Carmen.ShowModel.Structure.Role", null)
                        .WithMany()
                        .HasForeignKey("RolesRoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ApplicantTag", b =>
                {
                    b.HasOne("Carmen.ShowModel.Applicants.Applicant", null)
                        .WithMany()
                        .HasForeignKey("MembersApplicantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Carmen.ShowModel.Applicants.Tag", null)
                        .WithMany()
                        .HasForeignKey("TagsTagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Carmen.ShowModel.Applicants.Ability", b =>
                {
                    b.HasOne("Carmen.ShowModel.Applicants.Applicant", "Applicant")
                        .WithMany("Abilities")
                        .HasForeignKey("ApplicantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Carmen.ShowModel.Criterias.Criteria", "Criteria")
                        .WithMany("Abilities")
                        .HasForeignKey("CriteriaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Applicant");

                    b.Navigation("Criteria");
                });

            modelBuilder.Entity("Carmen.ShowModel.Applicants.Applicant", b =>
                {
                    b.HasOne("Carmen.ShowModel.Applicants.AlternativeCast", "AlternativeCast")
                        .WithMany("Members")
                        .HasForeignKey("AlternativeCastId");

                    b.HasOne("Carmen.ShowModel.Applicants.CastGroup", "CastGroup")
                        .WithMany("Members")
                        .HasForeignKey("CastGroupId");

                    b.HasOne("Carmen.ShowModel.Image", "Photo")
                        .WithMany()
                        .HasForeignKey("PhotoImageId");

                    b.HasOne("Carmen.ShowModel.Applicants.SameCastSet", "SameCastSet")
                        .WithMany("Applicants")
                        .HasForeignKey("SameCastSetId");

                    b.HasOne("Carmen.ShowModel.Structure.ShowRoot", "ShowRoot")
                        .WithMany()
                        .HasForeignKey("ShowRootNodeId");

                    b.Navigation("AlternativeCast");

                    b.Navigation("CastGroup");

                    b.Navigation("Photo");

                    b.Navigation("SameCastSet");

                    b.Navigation("ShowRoot");
                });

            modelBuilder.Entity("Carmen.ShowModel.Applicants.Tag", b =>
                {
                    b.HasOne("Carmen.ShowModel.Image", "Icon")
                        .WithMany()
                        .HasForeignKey("IconImageId");

                    b.OwnsMany("Carmen.ShowModel.Structure.CountByGroup", "CountByGroups", b1 =>
                        {
                            b1.Property<int>("TagId")
                                .HasColumnType("int");

                            b1.Property<int>("CastGroupId")
                                .HasColumnType("int");

                            b1.Property<uint>("Count")
                                .HasColumnType("int unsigned");

                            b1.HasKey("TagId", "CastGroupId");

                            b1.HasIndex("CastGroupId");

                            b1.ToTable("Tags_CountByGroups");

                            b1.HasOne("Carmen.ShowModel.Applicants.CastGroup", "CastGroup")
                                .WithMany()
                                .HasForeignKey("CastGroupId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.WithOwner()
                                .HasForeignKey("TagId");

                            b1.Navigation("CastGroup");
                        });

                    b.Navigation("CountByGroups");

                    b.Navigation("Icon");
                });

            modelBuilder.Entity("Carmen.ShowModel.Structure.Node", b =>
                {
                    b.HasOne("Carmen.ShowModel.Structure.InnerNode", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentNodeId");

                    b.OwnsMany("Carmen.ShowModel.Structure.CountByGroup", "CountByGroups", b1 =>
                        {
                            b1.Property<int>("NodeId")
                                .HasColumnType("int");

                            b1.Property<int>("CastGroupId")
                                .HasColumnType("int");

                            b1.Property<uint>("Count")
                                .HasColumnType("int unsigned");

                            b1.HasKey("NodeId", "CastGroupId");

                            b1.HasIndex("CastGroupId");

                            b1.ToTable("Nodes_CountByGroups");

                            b1.HasOne("Carmen.ShowModel.Applicants.CastGroup", "CastGroup")
                                .WithMany()
                                .HasForeignKey("CastGroupId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.WithOwner()
                                .HasForeignKey("NodeId");

                            b1.Navigation("CastGroup");
                        });

                    b.Navigation("CountByGroups");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("Carmen.ShowModel.Structure.Role", b =>
                {
                    b.OwnsMany("Carmen.ShowModel.Structure.CountByGroup", "CountByGroups", b1 =>
                        {
                            b1.Property<int>("RoleId")
                                .HasColumnType("int");

                            b1.Property<int>("CastGroupId")
                                .HasColumnType("int");

                            b1.Property<uint>("Count")
                                .HasColumnType("int unsigned");

                            b1.HasKey("RoleId", "CastGroupId");

                            b1.HasIndex("CastGroupId");

                            b1.ToTable("Roles_CountByGroups");

                            b1.HasOne("Carmen.ShowModel.Applicants.CastGroup", "CastGroup")
                                .WithMany()
                                .HasForeignKey("CastGroupId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.WithOwner()
                                .HasForeignKey("RoleId");

                            b1.Navigation("CastGroup");
                        });

                    b.Navigation("CountByGroups");
                });

            modelBuilder.Entity("CastGroupRequirement", b =>
                {
                    b.HasOne("Carmen.ShowModel.Requirements.Requirement", null)
                        .WithMany()
                        .HasForeignKey("RequirementsRequirementId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Carmen.ShowModel.Applicants.CastGroup", null)
                        .WithMany()
                        .HasForeignKey("UsedByCastGroupsCastGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CombinedRequirementRequirement", b =>
                {
                    b.HasOne("Carmen.ShowModel.Requirements.Requirement", null)
                        .WithMany()
                        .HasForeignKey("SubRequirementsRequirementId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Carmen.ShowModel.Requirements.CombinedRequirement", null)
                        .WithMany()
                        .HasForeignKey("UsedByCombinedRequirementsRequirementId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ItemRole", b =>
                {
                    b.HasOne("Carmen.ShowModel.Structure.Item", null)
                        .WithMany()
                        .HasForeignKey("ItemsNodeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Carmen.ShowModel.Structure.Role", null)
                        .WithMany()
                        .HasForeignKey("RolesRoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RequirementRole", b =>
                {
                    b.HasOne("Carmen.ShowModel.Requirements.Requirement", null)
                        .WithMany()
                        .HasForeignKey("RequirementsRequirementId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Carmen.ShowModel.Structure.Role", null)
                        .WithMany()
                        .HasForeignKey("UsedByRolesRoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RequirementTag", b =>
                {
                    b.HasOne("Carmen.ShowModel.Requirements.Requirement", null)
                        .WithMany()
                        .HasForeignKey("RequirementsRequirementId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Carmen.ShowModel.Applicants.Tag", null)
                        .WithMany()
                        .HasForeignKey("UsedByTagsTagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.AbilityExactRequirement", b =>
                {
                    b.HasOne("Carmen.ShowModel.Criterias.Criteria", "Criteria")
                        .WithMany()
                        .HasForeignKey("CriteriaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Criteria");
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.AbilityRangeRequirement", b =>
                {
                    b.HasOne("Carmen.ShowModel.Criterias.Criteria", "Criteria")
                        .WithMany()
                        .HasForeignKey("CriteriaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Criteria");
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.NotRequirement", b =>
                {
                    b.HasOne("Carmen.ShowModel.Requirements.Requirement", "SubRequirement")
                        .WithMany()
                        .HasForeignKey("SubRequirementId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SubRequirement");
                });

            modelBuilder.Entity("Carmen.ShowModel.Requirements.TagRequirement", b =>
                {
                    b.HasOne("Carmen.ShowModel.Applicants.Tag", "RequiredTag")
                        .WithMany()
                        .HasForeignKey("RequiredTagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RequiredTag");
                });

            modelBuilder.Entity("Carmen.ShowModel.Structure.Section", b =>
                {
                    b.HasOne("Carmen.ShowModel.Structure.SectionType", "SectionType")
                        .WithMany("Sections")
                        .HasForeignKey("SectionTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SectionType");
                });

            modelBuilder.Entity("Carmen.ShowModel.Structure.ShowRoot", b =>
                {
                    b.HasOne("Carmen.ShowModel.Criterias.Criteria", "CastNumberOrderBy")
                        .WithMany()
                        .HasForeignKey("CastNumberOrderByCriteriaId");

                    b.HasOne("Carmen.ShowModel.Image", "Logo")
                        .WithMany()
                        .HasForeignKey("LogoImageId");

                    b.Navigation("CastNumberOrderBy");

                    b.Navigation("Logo");
                });

            modelBuilder.Entity("Carmen.ShowModel.Applicants.AlternativeCast", b =>
                {
                    b.Navigation("Members");
                });

            modelBuilder.Entity("Carmen.ShowModel.Applicants.Applicant", b =>
                {
                    b.Navigation("Abilities");
                });

            modelBuilder.Entity("Carmen.ShowModel.Applicants.CastGroup", b =>
                {
                    b.Navigation("Members");
                });

            modelBuilder.Entity("Carmen.ShowModel.Applicants.SameCastSet", b =>
                {
                    b.Navigation("Applicants");
                });

            modelBuilder.Entity("Carmen.ShowModel.Criterias.Criteria", b =>
                {
                    b.Navigation("Abilities");
                });

            modelBuilder.Entity("Carmen.ShowModel.Structure.SectionType", b =>
                {
                    b.Navigation("Sections");
                });

            modelBuilder.Entity("Carmen.ShowModel.Structure.InnerNode", b =>
                {
                    b.Navigation("Children");
                });
#pragma warning restore 612, 618
        }
    }
}
