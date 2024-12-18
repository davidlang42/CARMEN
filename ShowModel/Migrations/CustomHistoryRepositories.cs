﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Pomelo.EntityFrameworkCore.MySql.Migrations.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel
{
    // |-----------------------------------------------------------------------------|
    // | IMPORTANT: Any changes must be made to all Custom*HistoryRepository classes |
    // |-----------------------------------------------------------------------------|
#pragma warning disable EF1001 // Internal EF Core API usage.
    internal class CustomMySqlHistoryRepository : MySqlHistoryRepository
    {
        #region Extra values for history table
        public string ApplicationVersion = CustomSqliteHistoryRepository.VersionAsString(Assembly.GetEntryAssembly(), true);
        public string CarmenVersion = CustomSqliteHistoryRepository.VersionAsString(Assembly.GetExecutingAssembly());
        public DateTime DateApplied = DateTime.Now;
        #endregion

        public CustomMySqlHistoryRepository(HistoryRepositoryDependencies dependencies)
            : base(dependencies)
        { }

        protected override void ConfigureTable(EntityTypeBuilder<HistoryRow> history)
        {
            base.ConfigureTable(history);
            history.Property<string>(nameof(ApplicationVersion));
            history.Property<string>(nameof(CarmenVersion));
            history.Property<DateTime>(nameof(DateApplied));
        }

        public override string GetInsertScript(HistoryRow row)
        {
            // based on: https://github.com/dotnet/efcore/blob/main/src/EFCore.Relational/Migrations/HistoryRepository.cs
            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
            var dateTimeTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(DateTime));
            return new StringBuilder().Append("INSERT INTO ")
                .Append(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
                .Append(" (")
                .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
                .Append(", ")
                .Append(SqlGenerationHelper.DelimitIdentifier(ProductVersionColumnName))
                .Append(", ")
                .Append(SqlGenerationHelper.DelimitIdentifier(nameof(ApplicationVersion)))
                .Append(", ")
                .Append(SqlGenerationHelper.DelimitIdentifier(nameof(CarmenVersion)))
                .Append(", ")
                .Append(SqlGenerationHelper.DelimitIdentifier(nameof(DateApplied)))
                .AppendLine(")")
                .Append("VALUES (")
                .Append(stringTypeMapping.GenerateSqlLiteral(row.MigrationId))
                .Append(", ")
                .Append(stringTypeMapping.GenerateSqlLiteral(row.ProductVersion))
                .Append(", ")
                .Append(stringTypeMapping.GenerateSqlLiteral(ApplicationVersion))
                .Append(", ")
                .Append(stringTypeMapping.GenerateSqlLiteral(CarmenVersion))
                .Append(", ")
                .Append(dateTimeTypeMapping.GenerateSqlLiteral(DateApplied))
                .Append(')')
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .ToString();
        }
    }
#pragma warning restore EF1001 // Internal EF Core API usage.
}
