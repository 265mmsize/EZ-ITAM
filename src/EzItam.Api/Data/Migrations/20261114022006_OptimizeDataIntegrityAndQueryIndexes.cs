using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzItam.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeDataIntegrityAndQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityName_EntityKey",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_Agents_TokenHash",
                table: "Agents");

            migrationBuilder.AlterColumn<int>(
                name: "TotalSeats",
                table: "SoftwareLicenses",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "LicenseType",
                table: "SoftwareLicenses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "InventoryStatus",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "IsResigned",
                table: "EmployeeTemps",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "N",
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Departments",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Assets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DepreciationMonths",
                table: "AssetCategories",
                type: "int",
                nullable: false,
                defaultValue: 48,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Agents",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Ring",
                table: "Agents",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SoftwareLicenses_Cost_NonNegative",
                table: "SoftwareLicenses",
                sql: "[Cost] IS NULL OR [Cost] BETWEEN 0 AND 999999999999");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SoftwareLicenses_LicenseType",
                table: "SoftwareLicenses",
                sql: "[LicenseType] IN (0, 1, 2, 3, 4)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SoftwareLicenses_TotalSeats",
                table: "SoftwareLicenses",
                sql: "[TotalSeats] BETWEEN 0 AND 1000000");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_Open_Date_Id",
                table: "MaintenanceRecords",
                columns: new[] { "Date", "Id" },
                filter: "[CompletedAt] IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MaintenanceRecords_Cost_NonNegative",
                table: "MaintenanceRecords",
                sql: "[Cost] IS NULL OR [Cost] BETWEEN 0 AND 999999999999");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MaintenanceRecords_Type",
                table: "MaintenanceRecords",
                sql: "[Type] IN (0, 1, 2, 3)");

            migrationBuilder.CreateIndex(
                name: "UX_LicenseAssignments_Open_Asset",
                table: "LicenseAssignments",
                columns: new[] { "SoftwareLicenseId", "AssetId" },
                unique: true,
                filter: "[ReleasedAt] IS NULL AND [AssetId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_LicenseAssignments_Open_Employee",
                table: "LicenseAssignments",
                columns: new[] { "SoftwareLicenseId", "EmployeeId" },
                unique: true,
                filter: "[ReleasedAt] IS NULL AND [EmployeeId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LicenseAssignments_ExactlyOneAssignee",
                table: "LicenseAssignments",
                sql: "([AssetId] IS NOT NULL AND [EmployeeId] IS NULL) OR ([AssetId] IS NULL AND [EmployeeId] IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InventoryDetails_AssetStatusIV",
                table: "InventoryDetails",
                sql: "[AssetStatusIV] IN (0, 1, 2, 3, 4, 5, 6)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InventoryDetails_ScanCount",
                table: "InventoryDetails",
                sql: "[ScanCount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InventoryDetails_Status",
                table: "InventoryDetails",
                sql: "[Status] IN (0, 1, 2, 3, 4, 5, 6)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Inventories_DateRange",
                table: "Inventories",
                sql: "[DateFrom] <= [DateTo]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Inventories_Status",
                table: "Inventories",
                sql: "[InventoryStatus] IN (1, 2, 3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EmployeeTemps_IsResigned",
                table: "EmployeeTemps",
                sql: "[IsResigned] IN ('Y', 'N')");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityKey_Timestamp",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityKey", "Timestamp" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_AuditLogs_Action",
                table: "AuditLogs",
                sql: "[Action] IN (0, 1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Assets_PurchasePrice_NonNegative",
                table: "Assets",
                sql: "[PurchasePrice] IS NULL OR [PurchasePrice] BETWEEN 0 AND 999999999999");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Assets_Status",
                table: "Assets",
                sql: "[Status] IN (0, 1, 2, 3, 4, 5, 6)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AssetCategories_DepreciationMonths",
                table: "AssetCategories",
                sql: "[DepreciationMonths] BETWEEN 0 AND 600");

            migrationBuilder.CreateIndex(
                name: "UX_AssetAssignments_AssetId_Open",
                table: "AssetAssignments",
                column: "AssetId",
                unique: true,
                filter: "[ReturnedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_LastSeenAt",
                table: "Agents",
                column: "LastSeenAt");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_TokenHash",
                table: "Agents",
                column: "TokenHash",
                unique: true,
                filter: "[TokenHash] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Agents_NonNegativeHardware",
                table: "Agents",
                sql: "([CpuCores] IS NULL OR [CpuCores] >= 0) AND ([TotalMemoryMb] IS NULL OR [TotalMemoryMb] >= 0) AND ([DiskTotalGb] IS NULL OR [DiskTotalGb] >= 0)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Agents_Ring",
                table: "Agents",
                sql: "[Ring] IN (0, 1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Agents_Status",
                table: "Agents",
                sql: "[Status] IN (0, 1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SoftwareLicenses_Cost_NonNegative",
                table: "SoftwareLicenses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SoftwareLicenses_LicenseType",
                table: "SoftwareLicenses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SoftwareLicenses_TotalSeats",
                table: "SoftwareLicenses");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRecords_Open_Date_Id",
                table: "MaintenanceRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MaintenanceRecords_Cost_NonNegative",
                table: "MaintenanceRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MaintenanceRecords_Type",
                table: "MaintenanceRecords");

            migrationBuilder.DropIndex(
                name: "UX_LicenseAssignments_Open_Asset",
                table: "LicenseAssignments");

            migrationBuilder.DropIndex(
                name: "UX_LicenseAssignments_Open_Employee",
                table: "LicenseAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LicenseAssignments_ExactlyOneAssignee",
                table: "LicenseAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InventoryDetails_AssetStatusIV",
                table: "InventoryDetails");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InventoryDetails_ScanCount",
                table: "InventoryDetails");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InventoryDetails_Status",
                table: "InventoryDetails");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Inventories_DateRange",
                table: "Inventories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Inventories_Status",
                table: "Inventories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EmployeeTemps_IsResigned",
                table: "EmployeeTemps");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityName_EntityKey_Timestamp",
                table: "AuditLogs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AuditLogs_Action",
                table: "AuditLogs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Assets_PurchasePrice_NonNegative",
                table: "Assets");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Assets_Status",
                table: "Assets");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AssetCategories_DepreciationMonths",
                table: "AssetCategories");

            migrationBuilder.DropIndex(
                name: "UX_AssetAssignments_AssetId_Open",
                table: "AssetAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Agents_LastSeenAt",
                table: "Agents");

            migrationBuilder.DropIndex(
                name: "IX_Agents_TokenHash",
                table: "Agents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Agents_NonNegativeHardware",
                table: "Agents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Agents_Ring",
                table: "Agents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Agents_Status",
                table: "Agents");

            migrationBuilder.AlterColumn<int>(
                name: "TotalSeats",
                table: "SoftwareLicenses",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "LicenseType",
                table: "SoftwareLicenses",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Locations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "InventoryStatus",
                table: "Inventories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "IsResigned",
                table: "EmployeeTemps",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1,
                oldDefaultValue: "N");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Employees",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Departments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Assets",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DepreciationMonths",
                table: "AssetCategories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 48);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Agents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Ring",
                table: "Agents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityKey",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityKey" });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_TokenHash",
                table: "Agents",
                column: "TokenHash");
        }
    }
}
