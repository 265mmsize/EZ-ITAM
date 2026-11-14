-- Read-only preflight for OptimizeDataIntegrityAndQueryIndexes and
-- OptimizeDatabaseColumnTypesAndLengths.
-- Run against the target SQL Server database before applying either migration.
-- Results contain counts only; no business data is returned.

SELECT N'UX_AssetAssignments_AssetId_Open' AS CheckName, COUNT_BIG(*) AS ViolationCount
FROM (
    SELECT AssetId
    FROM dbo.AssetAssignments
    WHERE ReturnedAt IS NULL
    GROUP BY AssetId
    HAVING COUNT_BIG(*) > 1
) AS violations
UNION ALL
SELECT N'CK_LicenseAssignments_ExactlyOneAssignee', COUNT_BIG(*)
FROM dbo.LicenseAssignments
WHERE (AssetId IS NULL AND EmployeeId IS NULL)
   OR (AssetId IS NOT NULL AND EmployeeId IS NOT NULL)
UNION ALL
SELECT N'UX_LicenseAssignments_Open_Employee', COUNT_BIG(*)
FROM (
    SELECT SoftwareLicenseId, EmployeeId
    FROM dbo.LicenseAssignments
    WHERE ReleasedAt IS NULL AND EmployeeId IS NOT NULL
    GROUP BY SoftwareLicenseId, EmployeeId
    HAVING COUNT_BIG(*) > 1
) AS violations
UNION ALL
SELECT N'UX_LicenseAssignments_Open_Asset', COUNT_BIG(*)
FROM (
    SELECT SoftwareLicenseId, AssetId
    FROM dbo.LicenseAssignments
    WHERE ReleasedAt IS NULL AND AssetId IS NOT NULL
    GROUP BY SoftwareLicenseId, AssetId
    HAVING COUNT_BIG(*) > 1
) AS violations
UNION ALL
SELECT N'UX_Agents_TokenHash', COUNT_BIG(*)
FROM (
    SELECT TokenHash
    FROM dbo.Agents
    WHERE TokenHash IS NOT NULL
    GROUP BY TokenHash
    HAVING COUNT_BIG(*) > 1
) AS violations
UNION ALL
SELECT N'Agents_TokenHash_length_not_64', COUNT_BIG(*)
FROM dbo.Agents
WHERE TokenHash IS NOT NULL AND LEN(TokenHash) <> 64
UNION ALL
SELECT N'CK_Assets_Status', COUNT_BIG(*)
FROM dbo.Assets
WHERE Status NOT IN (0, 1, 2, 3, 4, 5, 6)
UNION ALL
SELECT N'CK_Assets_PurchasePrice_NonNegative', COUNT_BIG(*)
FROM dbo.Assets
WHERE PurchasePrice < 0 OR PurchasePrice > 999999999999
UNION ALL
SELECT N'CK_AssetCategories_DepreciationMonths', COUNT_BIG(*)
FROM dbo.AssetCategories
WHERE DepreciationMonths NOT BETWEEN 0 AND 600
UNION ALL
SELECT N'CK_MaintenanceRecords_Type', COUNT_BIG(*)
FROM dbo.MaintenanceRecords
WHERE Type NOT IN (0, 1, 2, 3)
UNION ALL
SELECT N'CK_MaintenanceRecords_Cost_NonNegative', COUNT_BIG(*)
FROM dbo.MaintenanceRecords
WHERE Cost < 0 OR Cost > 999999999999
UNION ALL
SELECT N'CK_SoftwareLicenses_LicenseType', COUNT_BIG(*)
FROM dbo.SoftwareLicenses
WHERE LicenseType NOT IN (0, 1, 2, 3, 4)
UNION ALL
SELECT N'CK_SoftwareLicenses_TotalSeats', COUNT_BIG(*)
FROM dbo.SoftwareLicenses
WHERE TotalSeats NOT BETWEEN 0 AND 1000000
UNION ALL
SELECT N'CK_SoftwareLicenses_Cost_NonNegative', COUNT_BIG(*)
FROM dbo.SoftwareLicenses
WHERE Cost < 0 OR Cost > 999999999999
UNION ALL
SELECT N'CK_Agents_Status', COUNT_BIG(*)
FROM dbo.Agents
WHERE Status NOT IN (0, 1, 2)
UNION ALL
SELECT N'CK_Agents_Ring', COUNT_BIG(*)
FROM dbo.Agents
WHERE Ring NOT IN (0, 1)
UNION ALL
SELECT N'CK_Agents_NonNegativeHardware', COUNT_BIG(*)
FROM dbo.Agents
WHERE CpuCores < 0 OR TotalMemoryMb < 0 OR DiskTotalGb < 0
UNION ALL
SELECT N'CK_AuditLogs_Action', COUNT_BIG(*)
FROM dbo.AuditLogs
WHERE Action NOT IN (0, 1, 2)
UNION ALL
SELECT N'CK_Inventories_Status', COUNT_BIG(*)
FROM dbo.Inventories
WHERE InventoryStatus NOT IN (1, 2, 3)
UNION ALL
SELECT N'CK_Inventories_DateRange', COUNT_BIG(*)
FROM dbo.Inventories
WHERE DateFrom > DateTo
UNION ALL
SELECT N'CK_InventoryDetails_Status', COUNT_BIG(*)
FROM dbo.InventoryDetails
WHERE Status NOT IN (0, 1, 2, 3, 4, 5, 6)
UNION ALL
SELECT N'CK_InventoryDetails_AssetStatusIV', COUNT_BIG(*)
FROM dbo.InventoryDetails
WHERE AssetStatusIV NOT IN (0, 1, 2, 3, 4, 5, 6)
UNION ALL
SELECT N'CK_InventoryDetails_ScanCount', COUNT_BIG(*)
FROM dbo.InventoryDetails
WHERE ScanCount < 0
UNION ALL
SELECT N'CK_EmployeeTemps_IsResigned_invalid', COUNT_BIG(*)
FROM dbo.EmployeeTemps
WHERE IsResigned IS NOT NULL AND IsResigned NOT IN ('Y', 'N')
ORDER BY CheckName;

-- NULL IsResigned values are backfilled to 'N' by the migration before making
-- the column NOT NULL. Review that mapping with the HRIS owner before rollout.
SELECT N'EmployeeTemps_IsResigned_NULL_will_be_backfilled' AS CheckName,
       COUNT_BIG(*) AS RowCount
FROM dbo.EmployeeTemps
WHERE IsResigned IS NULL;

-- SQL Server DATE conversion discards any time-of-day component. Review these
-- counts before changing the business-date columns from datetime2 to date.
SELECT N'Assets_PurchaseDate_time_component' AS CheckName, COUNT_BIG(*) AS RowCount
FROM dbo.Assets
WHERE PurchaseDate IS NOT NULL AND CONVERT(time(7), PurchaseDate) <> '00:00:00'
UNION ALL
SELECT N'Assets_WarrantyExpiry_time_component', COUNT_BIG(*)
FROM dbo.Assets
WHERE WarrantyExpiry IS NOT NULL AND CONVERT(time(7), WarrantyExpiry) <> '00:00:00'
UNION ALL
SELECT N'MaintenanceRecords_Date_time_component', COUNT_BIG(*)
FROM dbo.MaintenanceRecords
WHERE Date IS NOT NULL AND CONVERT(time(7), Date) <> '00:00:00'
UNION ALL
SELECT N'MaintenanceRecords_CompletedAt_time_component', COUNT_BIG(*)
FROM dbo.MaintenanceRecords
WHERE CompletedAt IS NOT NULL AND CONVERT(time(7), CompletedAt) <> '00:00:00'
UNION ALL
SELECT N'SoftwareLicenses_PurchaseDate_time_component', COUNT_BIG(*)
FROM dbo.SoftwareLicenses
WHERE PurchaseDate IS NOT NULL AND CONVERT(time(7), PurchaseDate) <> '00:00:00'
UNION ALL
SELECT N'SoftwareLicenses_ExpiryDate_time_component', COUNT_BIG(*)
FROM dbo.SoftwareLicenses
WHERE ExpiryDate IS NOT NULL AND CONVERT(time(7), ExpiryDate) <> '00:00:00'
UNION ALL
SELECT N'Assets_IpAddress_over_64', COUNT_BIG(*)
FROM dbo.Assets
WHERE IpAddress IS NOT NULL AND DATALENGTH(IpAddress) / 2 > 64
UNION ALL
SELECT N'Agents_IpAddress_over_64', COUNT_BIG(*)
FROM dbo.Agents
WHERE IpAddress IS NOT NULL AND DATALENGTH(IpAddress) / 2 > 64
ORDER BY CheckName;
