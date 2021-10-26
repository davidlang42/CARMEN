﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Carmen.ShowModel.Migrations
{
    public partial class InitialRelease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlternativeCasts",
                columns: table => new
                {
                    AlternativeCastId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Initial = table.Column<char>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlternativeCasts", x => x.AlternativeCastId);
                });

            migrationBuilder.CreateTable(
                name: "CastGroups",
                columns: table => new
                {
                    CastGroupId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Abbreviation = table.Column<string>(type: "TEXT", nullable: false),
                    RequiredCount = table.Column<uint>(type: "INTEGER", nullable: true),
                    AlternateCasts = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CastGroups", x => x.CastGroupId);
                });

            migrationBuilder.CreateTable(
                name: "Criterias",
                columns: table => new
                {
                    CriteriaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Required = table.Column<bool>(type: "INTEGER", nullable: false),
                    Primary = table.Column<bool>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Weight = table.Column<double>(type: "REAL", nullable: false),
                    MaxMark = table.Column<uint>(type: "INTEGER", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", nullable: false),
                    Options = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Criterias", x => x.CriteriaId);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    ImageId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ImageData = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.ImageId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "SameCastSets",
                columns: table => new
                {
                    SameCastSetId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SameCastSets", x => x.SameCastSetId);
                });

            migrationBuilder.CreateTable(
                name: "SectionTypes",
                columns: table => new
                {
                    SectionTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    AllowMultipleRoles = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowNoRoles = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowConsecutiveItems = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionTypes", x => x.SectionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    TagId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IconImageId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.TagId);
                    table.ForeignKey(
                        name: "FK_Tags_Images_IconImageId",
                        column: x => x.IconImageId,
                        principalTable: "Images",
                        principalColumn: "ImageId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Roles_CountByGroups",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    CastGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles_CountByGroups", x => new { x.RoleId, x.CastGroupId });
                    table.ForeignKey(
                        name: "FK_Roles_CountByGroups_CastGroups_CastGroupId",
                        column: x => x.CastGroupId,
                        principalTable: "CastGroups",
                        principalColumn: "CastGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Roles_CountByGroups_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    NodeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentNodeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Discriminator = table.Column<string>(type: "TEXT", nullable: false),
                    SectionTypeId = table.Column<int>(type: "INTEGER", nullable: true),
                    ShowDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LogoImageId = table.Column<int>(type: "INTEGER", nullable: true),
                    CastNumberOrderByCriteriaId = table.Column<int>(type: "INTEGER", nullable: true),
                    CastNumberOrderDirection = table.Column<int>(type: "INTEGER", nullable: true),
                    AllowConsecutiveItems = table.Column<bool>(type: "INTEGER", nullable: true),
                    CommonOverallWeight = table.Column<double>(type: "REAL", nullable: true),
                    WeightExistingRoleCosts = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.NodeId);
                    table.ForeignKey(
                        name: "FK_Nodes_Criterias_CastNumberOrderByCriteriaId",
                        column: x => x.CastNumberOrderByCriteriaId,
                        principalTable: "Criterias",
                        principalColumn: "CriteriaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nodes_Images_LogoImageId",
                        column: x => x.LogoImageId,
                        principalTable: "Images",
                        principalColumn: "ImageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nodes_Nodes_ParentNodeId",
                        column: x => x.ParentNodeId,
                        principalTable: "Nodes",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nodes_SectionTypes_SectionTypeId",
                        column: x => x.SectionTypeId,
                        principalTable: "SectionTypes",
                        principalColumn: "SectionTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Requirements",
                columns: table => new
                {
                    RequirementId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    SuitabilityWeight = table.Column<double>(type: "REAL", nullable: false),
                    OverallWeight = table.Column<double>(type: "REAL", nullable: false),
                    Primary = table.Column<bool>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    Discriminator = table.Column<string>(type: "TEXT", nullable: false),
                    CriteriaId = table.Column<int>(type: "INTEGER", nullable: true),
                    AbilityExactRequirement_ExistingRoleCost = table.Column<double>(type: "REAL", nullable: true),
                    RequiredValue = table.Column<uint>(type: "INTEGER", nullable: true),
                    ScaleSuitability = table.Column<bool>(type: "INTEGER", nullable: true),
                    ExistingRoleCost = table.Column<double>(type: "REAL", nullable: true),
                    Minimum = table.Column<uint>(type: "INTEGER", nullable: true),
                    Maximum = table.Column<uint>(type: "INTEGER", nullable: true),
                    AverageSuitability = table.Column<bool>(type: "INTEGER", nullable: true),
                    SubRequirementId = table.Column<int>(type: "INTEGER", nullable: true),
                    RequiredTagId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requirements", x => x.RequirementId);
                    table.ForeignKey(
                        name: "FK_Requirements_Criterias_CriteriaId",
                        column: x => x.CriteriaId,
                        principalTable: "Criterias",
                        principalColumn: "CriteriaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Requirements_Requirements_SubRequirementId",
                        column: x => x.SubRequirementId,
                        principalTable: "Requirements",
                        principalColumn: "RequirementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Requirements_Tags_RequiredTagId",
                        column: x => x.RequiredTagId,
                        principalTable: "Tags",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags_CountByGroups",
                columns: table => new
                {
                    TagId = table.Column<int>(type: "INTEGER", nullable: false),
                    CastGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags_CountByGroups", x => new { x.TagId, x.CastGroupId });
                    table.ForeignKey(
                        name: "FK_Tags_CountByGroups_CastGroups_CastGroupId",
                        column: x => x.CastGroupId,
                        principalTable: "CastGroups",
                        principalColumn: "CastGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tags_CountByGroups_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Applicants",
                columns: table => new
                {
                    ApplicantId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShowRootNodeId = table.Column<int>(type: "INTEGER", nullable: true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExternalData = table.Column<string>(type: "TEXT", nullable: false),
                    PhotoImageId = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    CastGroupId = table.Column<int>(type: "INTEGER", nullable: true),
                    CastNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    AlternativeCastId = table.Column<int>(type: "INTEGER", nullable: true),
                    SameCastSetId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applicants", x => x.ApplicantId);
                    table.ForeignKey(
                        name: "FK_Applicants_AlternativeCasts_AlternativeCastId",
                        column: x => x.AlternativeCastId,
                        principalTable: "AlternativeCasts",
                        principalColumn: "AlternativeCastId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Applicants_CastGroups_CastGroupId",
                        column: x => x.CastGroupId,
                        principalTable: "CastGroups",
                        principalColumn: "CastGroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Applicants_Images_PhotoImageId",
                        column: x => x.PhotoImageId,
                        principalTable: "Images",
                        principalColumn: "ImageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Applicants_Nodes_ShowRootNodeId",
                        column: x => x.ShowRootNodeId,
                        principalTable: "Nodes",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Applicants_SameCastSets_SameCastSetId",
                        column: x => x.SameCastSetId,
                        principalTable: "SameCastSets",
                        principalColumn: "SameCastSetId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemRole",
                columns: table => new
                {
                    ItemsNodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    RolesRoleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemRole", x => new { x.ItemsNodeId, x.RolesRoleId });
                    table.ForeignKey(
                        name: "FK_ItemRole_Nodes_ItemsNodeId",
                        column: x => x.ItemsNodeId,
                        principalTable: "Nodes",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemRole_Roles_RolesRoleId",
                        column: x => x.RolesRoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Nodes_CountByGroups",
                columns: table => new
                {
                    NodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CastGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes_CountByGroups", x => new { x.NodeId, x.CastGroupId });
                    table.ForeignKey(
                        name: "FK_Nodes_CountByGroups_CastGroups_CastGroupId",
                        column: x => x.CastGroupId,
                        principalTable: "CastGroups",
                        principalColumn: "CastGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Nodes_CountByGroups_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CastGroupRequirement",
                columns: table => new
                {
                    RequirementsRequirementId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsedByCastGroupsCastGroupId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CastGroupRequirement", x => new { x.RequirementsRequirementId, x.UsedByCastGroupsCastGroupId });
                    table.ForeignKey(
                        name: "FK_CastGroupRequirement_CastGroups_UsedByCastGroupsCastGroupId",
                        column: x => x.UsedByCastGroupsCastGroupId,
                        principalTable: "CastGroups",
                        principalColumn: "CastGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CastGroupRequirement_Requirements_RequirementsRequirementId",
                        column: x => x.RequirementsRequirementId,
                        principalTable: "Requirements",
                        principalColumn: "RequirementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CombinedRequirementRequirement",
                columns: table => new
                {
                    SubRequirementsRequirementId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsedByCombinedRequirementsRequirementId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombinedRequirementRequirement", x => new { x.SubRequirementsRequirementId, x.UsedByCombinedRequirementsRequirementId });
                    table.ForeignKey(
                        name: "FK_CombinedRequirementRequirement_Requirements_SubRequirementsRequirementId",
                        column: x => x.SubRequirementsRequirementId,
                        principalTable: "Requirements",
                        principalColumn: "RequirementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CombinedRequirementRequirement_Requirements_UsedByCombinedRequirementsRequirementId",
                        column: x => x.UsedByCombinedRequirementsRequirementId,
                        principalTable: "Requirements",
                        principalColumn: "RequirementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequirementRole",
                columns: table => new
                {
                    RequirementsRequirementId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsedByRolesRoleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequirementRole", x => new { x.RequirementsRequirementId, x.UsedByRolesRoleId });
                    table.ForeignKey(
                        name: "FK_RequirementRole_Requirements_RequirementsRequirementId",
                        column: x => x.RequirementsRequirementId,
                        principalTable: "Requirements",
                        principalColumn: "RequirementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequirementRole_Roles_UsedByRolesRoleId",
                        column: x => x.UsedByRolesRoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequirementTag",
                columns: table => new
                {
                    RequirementsRequirementId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsedByTagsTagId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequirementTag", x => new { x.RequirementsRequirementId, x.UsedByTagsTagId });
                    table.ForeignKey(
                        name: "FK_RequirementTag_Requirements_RequirementsRequirementId",
                        column: x => x.RequirementsRequirementId,
                        principalTable: "Requirements",
                        principalColumn: "RequirementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequirementTag_Tags_UsedByTagsTagId",
                        column: x => x.UsedByTagsTagId,
                        principalTable: "Tags",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Abilities",
                columns: table => new
                {
                    ApplicantId = table.Column<int>(type: "INTEGER", nullable: false),
                    CriteriaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Mark = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abilities", x => new { x.ApplicantId, x.CriteriaId });
                    table.ForeignKey(
                        name: "FK_Abilities_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "ApplicantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Abilities_Criterias_CriteriaId",
                        column: x => x.CriteriaId,
                        principalTable: "Criterias",
                        principalColumn: "CriteriaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicantRole",
                columns: table => new
                {
                    CastApplicantId = table.Column<int>(type: "INTEGER", nullable: false),
                    RolesRoleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantRole", x => new { x.CastApplicantId, x.RolesRoleId });
                    table.ForeignKey(
                        name: "FK_ApplicantRole_Applicants_CastApplicantId",
                        column: x => x.CastApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "ApplicantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicantRole_Roles_RolesRoleId",
                        column: x => x.RolesRoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicantTag",
                columns: table => new
                {
                    MembersApplicantId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagsTagId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantTag", x => new { x.MembersApplicantId, x.TagsTagId });
                    table.ForeignKey(
                        name: "FK_ApplicantTag_Applicants_MembersApplicantId",
                        column: x => x.MembersApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "ApplicantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicantTag_Tags_TagsTagId",
                        column: x => x.TagsTagId,
                        principalTable: "Tags",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_CriteriaId",
                table: "Abilities",
                column: "CriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantRole_RolesRoleId",
                table: "ApplicantRole",
                column: "RolesRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_AlternativeCastId",
                table: "Applicants",
                column: "AlternativeCastId");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_CastGroupId",
                table: "Applicants",
                column: "CastGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_PhotoImageId",
                table: "Applicants",
                column: "PhotoImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_SameCastSetId",
                table: "Applicants",
                column: "SameCastSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_ShowRootNodeId",
                table: "Applicants",
                column: "ShowRootNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantTag_TagsTagId",
                table: "ApplicantTag",
                column: "TagsTagId");

            migrationBuilder.CreateIndex(
                name: "IX_CastGroupRequirement_UsedByCastGroupsCastGroupId",
                table: "CastGroupRequirement",
                column: "UsedByCastGroupsCastGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CombinedRequirementRequirement_UsedByCombinedRequirementsRequirementId",
                table: "CombinedRequirementRequirement",
                column: "UsedByCombinedRequirementsRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemRole_RolesRoleId",
                table: "ItemRole",
                column: "RolesRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_CastNumberOrderByCriteriaId",
                table: "Nodes",
                column: "CastNumberOrderByCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_LogoImageId",
                table: "Nodes",
                column: "LogoImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_ParentNodeId",
                table: "Nodes",
                column: "ParentNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_SectionTypeId",
                table: "Nodes",
                column: "SectionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_CountByGroups_CastGroupId",
                table: "Nodes_CountByGroups",
                column: "CastGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementRole_UsedByRolesRoleId",
                table: "RequirementRole",
                column: "UsedByRolesRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Requirements_CriteriaId",
                table: "Requirements",
                column: "CriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Requirements_RequiredTagId",
                table: "Requirements",
                column: "RequiredTagId");

            migrationBuilder.CreateIndex(
                name: "IX_Requirements_SubRequirementId",
                table: "Requirements",
                column: "SubRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementTag_UsedByTagsTagId",
                table: "RequirementTag",
                column: "UsedByTagsTagId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CountByGroups_CastGroupId",
                table: "Roles_CountByGroups",
                column: "CastGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_IconImageId",
                table: "Tags",
                column: "IconImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CountByGroups_CastGroupId",
                table: "Tags_CountByGroups",
                column: "CastGroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Abilities");

            migrationBuilder.DropTable(
                name: "ApplicantRole");

            migrationBuilder.DropTable(
                name: "ApplicantTag");

            migrationBuilder.DropTable(
                name: "CastGroupRequirement");

            migrationBuilder.DropTable(
                name: "CombinedRequirementRequirement");

            migrationBuilder.DropTable(
                name: "ItemRole");

            migrationBuilder.DropTable(
                name: "Nodes_CountByGroups");

            migrationBuilder.DropTable(
                name: "RequirementRole");

            migrationBuilder.DropTable(
                name: "RequirementTag");

            migrationBuilder.DropTable(
                name: "Roles_CountByGroups");

            migrationBuilder.DropTable(
                name: "Tags_CountByGroups");

            migrationBuilder.DropTable(
                name: "Applicants");

            migrationBuilder.DropTable(
                name: "Requirements");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "AlternativeCasts");

            migrationBuilder.DropTable(
                name: "CastGroups");

            migrationBuilder.DropTable(
                name: "Nodes");

            migrationBuilder.DropTable(
                name: "SameCastSets");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Criterias");

            migrationBuilder.DropTable(
                name: "SectionTypes");

            migrationBuilder.DropTable(
                name: "Images");
        }
    }
}
