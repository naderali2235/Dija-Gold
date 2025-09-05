using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DijaGoldPOS.API.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeConflictsWithRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPurchaseItems_KaratTypeLookups_KaratTypeId",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPurchases_PaymentMethodLookups_PaymentMethodId",
                table: "CustomerPurchases");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_BusinessEntityTypeLookups_BusinessEntityTypeId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_FinancialTransactionStatusLookups_StatusId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_FinancialTransactionTypeLookups_TransactionTypeId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_PaymentMethodLookups_PaymentMethodId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_GoldRates_KaratTypeLookups_KaratTypeId",
                table: "GoldRates");

            migrationBuilder.DropForeignKey(
                name: "FK_MakingCharges_ChargeTypeLookups_ChargeTypeId",
                table: "MakingCharges");

            migrationBuilder.DropForeignKey(
                name: "FK_MakingCharges_ProductCategoryTypeLookups_ProductCategoryId",
                table: "MakingCharges");

            migrationBuilder.DropForeignKey(
                name: "FK_MakingCharges_SubCategoryLookups_SubCategoryId",
                table: "MakingCharges");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_MakingCharges_MakingChargesId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderStatusLookups_StatusId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderTypeLookups_OrderTypeId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_KaratTypeLookups_KaratTypeId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductCategoryTypeLookups_CategoryTypeId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_SubCategoryLookups_SubCategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_RawGoldInventories_KaratTypeLookups_KaratTypeId",
                table: "RawGoldInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_RawGoldPurchaseOrderItems_KaratTypeLookups_KaratTypeId",
                table: "RawGoldPurchaseOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairJobs_RepairPriorityLookups_PriorityId",
                table: "RepairJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairJobs_RepairStatusLookups_StatusId",
                table: "RepairJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxConfigurations_ChargeTypeLookups_TaxTypeId",
                table: "TaxConfigurations");

            migrationBuilder.DropTable(
                name: "BusinessEntityTypeLookups");

            migrationBuilder.DropTable(
                name: "ChargeTypeLookups");

            migrationBuilder.DropTable(
                name: "FinancialTransactionStatusLookups");

            migrationBuilder.DropTable(
                name: "FinancialTransactionTypeLookups");

            migrationBuilder.DropTable(
                name: "KaratTypeLookups");

            migrationBuilder.DropTable(
                name: "OrderStatusLookups");

            migrationBuilder.DropTable(
                name: "OrderTypeLookups");

            migrationBuilder.DropTable(
                name: "PaymentMethodLookups");

            migrationBuilder.DropTable(
                name: "RepairPriorityLookups");

            migrationBuilder.DropTable(
                name: "RepairStatusLookups");

            migrationBuilder.DropTable(
                name: "SubCategoryLookups");

            migrationBuilder.DropTable(
                name: "TransactionStatusLookups");

            migrationBuilder.DropTable(
                name: "TransactionTypeLookups");

            migrationBuilder.DropTable(
                name: "ProductCategoryTypeLookups");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_MakingChargesId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "MakingChargesId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TotalOrders",
                table: "Customers");

            migrationBuilder.EnsureSchema(
                name: "Lookup");

            migrationBuilder.EnsureSchema(
                name: "Customer");

            migrationBuilder.EnsureSchema(
                name: "Product");

            migrationBuilder.EnsureSchema(
                name: "Supplier");

            migrationBuilder.RenameTable(
                name: "TaxConfigurations",
                newName: "TaxConfigurations",
                newSchema: "Product");

            migrationBuilder.RenameTable(
                name: "Suppliers",
                newName: "Suppliers",
                newSchema: "Supplier");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Products",
                newSchema: "Product");

            migrationBuilder.RenameTable(
                name: "MakingCharges",
                newName: "MakingCharges",
                newSchema: "Product");

            migrationBuilder.RenameTable(
                name: "GoldRates",
                newName: "GoldRates",
                newSchema: "Product");

            migrationBuilder.RenameTable(
                name: "Customers",
                newName: "Customers",
                newSchema: "Customer");

            migrationBuilder.RenameTable(
                name: "CustomerPurchases",
                newName: "CustomerPurchases",
                newSchema: "Customer");

            migrationBuilder.RenameTable(
                name: "CustomerPurchaseItems",
                newName: "CustomerPurchaseItems",
                newSchema: "Customer");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                schema: "Product",
                table: "Products",
                newName: "StandardCost");

            migrationBuilder.RenameColumn(
                name: "SubCategory",
                schema: "Product",
                table: "Products",
                newName: "Specifications");

            migrationBuilder.RenameColumn(
                name: "SubCategory",
                schema: "Product",
                table: "MakingCharges",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "LastPurchaseDate",
                schema: "Customer",
                table: "Customers",
                newName: "LastTransactionDate");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                schema: "Customer",
                table: "CustomerPurchases",
                newName: "ProcessedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerPurchases_CreatedByUserId",
                schema: "Customer",
                table: "CustomerPurchases",
                newName: "IX_CustomerPurchases_ProcessedByUserId");

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "RepairJobs",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastCountDate",
                table: "RawGoldInventories",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManufacturingRecordId",
                table: "ProductOwnerships",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                schema: "Product",
                table: "TaxConfigurations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Product",
                table: "TaxConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInclusive",
                schema: "Product",
                table: "TaxConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumTransactionAmount",
                schema: "Product",
                table: "TaxConfigurations",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumTransactionAmount",
                schema: "Product",
                table: "TaxConfigurations",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "Product",
                table: "TaxConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductCategoryId",
                schema: "Product",
                table: "TaxConfigurations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "Product",
                table: "TaxConfigurations",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxAuthority",
                schema: "Product",
                table: "TaxConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxRegistrationNumber",
                schema: "Product",
                table: "TaxConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentTerms",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternatePhone",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageOrderValue",
                schema: "Supplier",
                table: "Suppliers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountInfo",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPersonTitle",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                schema: "Supplier",
                table: "Suppliers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractStartDate",
                schema: "Supplier",
                table: "Suppliers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryTerms",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Documents",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EarlyPaymentDiscountDays",
                schema: "Supplier",
                table: "Suppliers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "EarlyPaymentDiscountPercentage",
                schema: "Supplier",
                table: "Suppliers",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContact",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceInfo",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApprovedForGold",
                schema: "Supplier",
                table: "Suppliers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPreferred",
                schema: "Supplier",
                table: "Suppliers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LanguagePreference",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LeadTimeDays",
                schema: "Supplier",
                table: "Suppliers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumOrderValue",
                schema: "Supplier",
                table: "Suppliers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumOrderValue",
                schema: "Supplier",
                table: "Suppliers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OnTimeDeliveryPercentage",
                schema: "Supplier",
                table: "Suppliers",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredCommunicationMethod",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QualityCertifications",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QualityRatingPercentage",
                schema: "Supplier",
                table: "Suppliers",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                schema: "Supplier",
                table: "Suppliers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "State",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupplierCode",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalOrdersCount",
                schema: "Supplier",
                table: "Suppliers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPurchaseAmount",
                schema: "Supplier",
                table: "Suppliers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TradeName",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatRegistrationNumber",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                schema: "Supplier",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "HasNumismaticValue",
                schema: "Product",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                schema: "Product",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentPrice",
                schema: "Product",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Images",
                schema: "Product",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableForSale",
                schema: "Product",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                schema: "Product",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ManufactureYear",
                schema: "Product",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumStockLevel",
                schema: "Product",
                table: "Products",
                type: "decimal(10,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumStockLevel",
                schema: "Product",
                table: "Products",
                type: "decimal(10,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "Product",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReorderPoint",
                schema: "Product",
                table: "Products",
                type: "decimal(10,3)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "Product",
                table: "Products",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanBeWaived",
                schema: "Product",
                table: "MakingCharges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Product",
                table: "MakingCharges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMandatory",
                schema: "Product",
                table: "MakingCharges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumCharge",
                schema: "Product",
                table: "MakingCharges",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumWeight",
                schema: "Product",
                table: "MakingCharges",
                type: "decimal(10,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumCharge",
                schema: "Product",
                table: "MakingCharges",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumWeight",
                schema: "Product",
                table: "MakingCharges",
                type: "decimal(10,3)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "Product",
                table: "MakingCharges",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                schema: "Product",
                table: "GoldRates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                schema: "Product",
                table: "GoldRates",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "Product",
                table: "GoldRates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "Product",
                table: "GoldRates",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SetByUserId",
                schema: "Product",
                table: "GoldRates",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                schema: "Product",
                table: "GoldRates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceReference",
                schema: "Product",
                table: "GoldRates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                schema: "Customer",
                table: "Customers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerCategory",
                schema: "Customer",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                schema: "Customer",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                schema: "Customer",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "KycCompletedDate",
                schema: "Customer",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KycDocuments",
                schema: "Customer",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KycStatus",
                schema: "Customer",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LanguagePreference",
                schema: "Customer",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "MarketingConsent",
                schema: "Customer",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "OutstandingBalance",
                schema: "Customer",
                table: "Customers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Preferences",
                schema: "Customer",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredContactMethod",
                schema: "Customer",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "Customer",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalGoldPurchased",
                schema: "Customer",
                table: "Customers",
                type: "decimal(10,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "AffectsInventory",
                schema: "Customer",
                table: "CustomerPurchases",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                schema: "Customer",
                table: "CustomerPurchases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                schema: "Customer",
                table: "CustomerPurchases",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AveragePurity",
                schema: "Customer",
                table: "CustomerPurchases",
                type: "decimal(5,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "FinancialTransactionId",
                schema: "Customer",
                table: "CustomerPurchases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InventoryAdjustmentReason",
                schema: "Customer",
                table: "CustomerPurchases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherDeductions",
                schema: "Customer",
                table: "CustomerPurchases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Photos",
                schema: "Customer",
                table: "CustomerPurchases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProcessingFee",
                schema: "Customer",
                table: "CustomerPurchases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNumber",
                schema: "Customer",
                table: "CustomerPurchases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "Customer",
                table: "CustomerPurchases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TestingFee",
                schema: "Customer",
                table: "CustomerPurchases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TestingMethod",
                schema: "Customer",
                table: "CustomerPurchases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWeight",
                schema: "Customer",
                table: "CustomerPurchases",
                type: "decimal(10,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualPurity",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "decimal(5,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdjustedWeight",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "decimal(10,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeductionReason",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Deductions",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalWeight",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "decimal(10,3)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemSequence",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "MeltingDate",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MeltingLoss",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "decimal(5,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Photos",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StonesRemoved",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "StonesValue",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StonesWeight",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "decimal(10,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestingNotes",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WasMelted",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BusinessEntityTypes",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CanPurchase = table.Column<bool>(type: "bit", nullable: false),
                    CanSell = table.Column<bool>(type: "bit", nullable: false),
                    HasCreditLimit = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessEntityTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChargeTypes",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPercentage = table.Column<bool>(type: "bit", nullable: false),
                    IsWeightBased = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactionStatuses",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    IsFailure = table.Column<bool>(type: "bit", nullable: false),
                    AllowsReversal = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactionStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactionTypes",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IncreasesCash = table.Column<bool>(type: "bit", nullable: false),
                    DecreasesCash = table.Column<bool>(type: "bit", nullable: false),
                    AffectsInventory = table.Column<bool>(type: "bit", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KaratTypes",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KaratValue = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    PurityPercentage = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCommon = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KaratTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatuses",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    IsCancellation = table.Column<bool>(type: "bit", nullable: false),
                    ColorClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderTypes",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IncreasesInventory = table.Column<bool>(type: "bit", nullable: false),
                    DecreasesInventory = table.Column<bool>(type: "bit", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    CanBeVoided = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiresImmediateSettlement = table.Column<bool>(type: "bit", nullable: false),
                    SupportsPartialPayments = table.Column<bool>(type: "bit", nullable: false),
                    MaxTransactionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProcessingFeePercentage = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    IsElectronic = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategoryTypes",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasMakingCharges = table.Column<bool>(type: "bit", nullable: false),
                    DefaultTaxRate = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    RequiresWeight = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategoryTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepairPriorities",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriorityLevel = table.Column<int>(type: "int", nullable: false),
                    ExpectedCompletionDays = table.Column<int>(type: "int", nullable: true),
                    AdditionalCostPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ColorClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairPriorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepairStatuses",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsInProgress = table.Column<bool>(type: "bit", nullable: false),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    RequiresCustomerNotification = table.Column<bool>(type: "bit", nullable: false),
                    ColorClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionStatuses",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionTypes",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RawGoldOwnerships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KaratTypeId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    RawGoldPurchaseOrderId = table.Column<int>(type: "int", nullable: true),
                    TotalWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OwnedWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OwnershipPercentage = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawGoldOwnerships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawGoldOwnerships_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawGoldOwnerships_KaratTypes_KaratTypeId",
                        column: x => x.KaratTypeId,
                        principalSchema: "Lookup",
                        principalTable: "KaratTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawGoldOwnerships_RawGoldPurchaseOrders_RawGoldPurchaseOrderId",
                        column: x => x.RawGoldPurchaseOrderId,
                        principalTable: "RawGoldPurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RawGoldOwnerships_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "Supplier",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RawGoldTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    FromSupplierId = table.Column<int>(type: "int", nullable: true),
                    ToSupplierId = table.Column<int>(type: "int", nullable: true),
                    FromKaratTypeId = table.Column<int>(type: "int", nullable: false),
                    ToKaratTypeId = table.Column<int>(type: "int", nullable: false),
                    FromWeight = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    ToWeight = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    FromGoldRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ToGoldRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConversionFactor = table.Column<decimal>(type: "decimal(8,6)", nullable: false),
                    TransferValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerPurchaseId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawGoldTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawGoldTransfers_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawGoldTransfers_CustomerPurchases_CustomerPurchaseId",
                        column: x => x.CustomerPurchaseId,
                        principalSchema: "Customer",
                        principalTable: "CustomerPurchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RawGoldTransfers_KaratTypes_FromKaratTypeId",
                        column: x => x.FromKaratTypeId,
                        principalSchema: "Lookup",
                        principalTable: "KaratTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawGoldTransfers_KaratTypes_ToKaratTypeId",
                        column: x => x.ToKaratTypeId,
                        principalSchema: "Lookup",
                        principalTable: "KaratTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawGoldTransfers_Suppliers_FromSupplierId",
                        column: x => x.FromSupplierId,
                        principalSchema: "Supplier",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawGoldTransfers_Suppliers_ToSupplierId",
                        column: x => x.ToSupplierId,
                        principalSchema: "Supplier",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierGoldBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    KaratTypeId = table.Column<int>(type: "int", nullable: false),
                    TotalWeightReceived = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    TotalWeightPaidFor = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    MerchantGoldBalance = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    OutstandingMonetaryValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageCostPerGram = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastTransactionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierGoldBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierGoldBalances_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierGoldBalances_KaratTypes_KaratTypeId",
                        column: x => x.KaratTypeId,
                        principalSchema: "Lookup",
                        principalTable: "KaratTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierGoldBalances_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "Supplier",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubCategories",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultMakingChargeRate = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    IsMens = table.Column<bool>(type: "bit", nullable: false),
                    IsWomens = table.Column<bool>(type: "bit", nullable: false),
                    IsUnisex = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategories_ProductCategoryTypes_CategoryTypeId",
                        column: x => x.CategoryTypeId,
                        principalSchema: "Lookup",
                        principalTable: "ProductCategoryTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepairJobs_CustomerId",
                table: "RepairJobs",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxConfigurations_ProductCategoryId",
                schema: "Product",
                table: "TaxConfigurations",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_GoldRates_ApprovedByUserId",
                schema: "Product",
                table: "GoldRates",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoldRates_SetByUserId",
                schema: "Product",
                table: "GoldRates",
                column: "SetByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchases_ApprovedByUserId",
                schema: "Customer",
                table: "CustomerPurchases",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchases_FinancialTransactionId",
                schema: "Customer",
                table: "CustomerPurchases",
                column: "FinancialTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEntityTypes_Name",
                schema: "Lookup",
                table: "BusinessEntityTypes",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ChargeTypes_Name",
                schema: "Lookup",
                table: "ChargeTypes",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactionStatuses_Name",
                schema: "Lookup",
                table: "FinancialTransactionStatuses",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactionTypes_Name",
                schema: "Lookup",
                table: "FinancialTransactionTypes",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_KaratTypes_Name",
                schema: "Lookup",
                table: "KaratTypes",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatuses_Name",
                schema: "Lookup",
                table: "OrderStatuses",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTypes_Name",
                schema: "Lookup",
                table: "OrderTypes",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_Name",
                schema: "Lookup",
                table: "PaymentMethods",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryTypes_Name",
                schema: "Lookup",
                table: "ProductCategoryTypes",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldOwnerships_BranchId",
                table: "RawGoldOwnerships",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldOwnerships_KaratTypeId_BranchId_SupplierId",
                table: "RawGoldOwnerships",
                columns: new[] { "KaratTypeId", "BranchId", "SupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldOwnerships_OutstandingAmount",
                table: "RawGoldOwnerships",
                column: "OutstandingAmount");

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldOwnerships_OwnershipPercentage",
                table: "RawGoldOwnerships",
                column: "OwnershipPercentage");

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldOwnerships_RawGoldPurchaseOrderId",
                table: "RawGoldOwnerships",
                column: "RawGoldPurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldOwnerships_SupplierId",
                table: "RawGoldOwnerships",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldTransfers_BranchId_TransferDate",
                table: "RawGoldTransfers",
                columns: new[] { "BranchId", "TransferDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldTransfers_CustomerPurchaseId",
                table: "RawGoldTransfers",
                column: "CustomerPurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldTransfers_FromKaratTypeId",
                table: "RawGoldTransfers",
                column: "FromKaratTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldTransfers_FromSupplierId",
                table: "RawGoldTransfers",
                column: "FromSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldTransfers_ToKaratTypeId",
                table: "RawGoldTransfers",
                column: "ToKaratTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldTransfers_ToSupplierId",
                table: "RawGoldTransfers",
                column: "ToSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldTransfers_TransferDate",
                table: "RawGoldTransfers",
                column: "TransferDate");

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldTransfers_TransferNumber",
                table: "RawGoldTransfers",
                column: "TransferNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawGoldTransfers_TransferType",
                table: "RawGoldTransfers",
                column: "TransferType");

            migrationBuilder.CreateIndex(
                name: "IX_RepairPriorities_Name",
                schema: "Lookup",
                table: "RepairPriorities",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RepairStatuses_Name",
                schema: "Lookup",
                table: "RepairStatuses",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_CategoryTypeId",
                schema: "Lookup",
                table: "SubCategories",
                column: "CategoryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_Name",
                schema: "Lookup",
                table: "SubCategories",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierGoldBalances_BranchId",
                table: "SupplierGoldBalances",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierGoldBalances_KaratTypeId",
                table: "SupplierGoldBalances",
                column: "KaratTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierGoldBalances_LastTransactionDate",
                table: "SupplierGoldBalances",
                column: "LastTransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierGoldBalances_SupplierId",
                table: "SupplierGoldBalances",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierGoldBalances_SupplierId_BranchId_KaratTypeId",
                table: "SupplierGoldBalances",
                columns: new[] { "SupplierId", "BranchId", "KaratTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionStatuses_Name",
                schema: "Lookup",
                table: "TransactionStatuses",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypes_Name",
                schema: "Lookup",
                table: "TransactionTypes",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPurchaseItems_KaratTypes_KaratTypeId",
                schema: "Customer",
                table: "CustomerPurchaseItems",
                column: "KaratTypeId",
                principalSchema: "Lookup",
                principalTable: "KaratTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPurchases_AspNetUsers_ApprovedByUserId",
                schema: "Customer",
                table: "CustomerPurchases",
                column: "ApprovedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPurchases_AspNetUsers_ProcessedByUserId",
                schema: "Customer",
                table: "CustomerPurchases",
                column: "ProcessedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPurchases_FinancialTransactions_FinancialTransactionId",
                schema: "Customer",
                table: "CustomerPurchases",
                column: "FinancialTransactionId",
                principalTable: "FinancialTransactions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPurchases_PaymentMethods_PaymentMethodId",
                schema: "Customer",
                table: "CustomerPurchases",
                column: "PaymentMethodId",
                principalSchema: "Lookup",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_BusinessEntityTypes_BusinessEntityTypeId",
                table: "FinancialTransactions",
                column: "BusinessEntityTypeId",
                principalSchema: "Lookup",
                principalTable: "BusinessEntityTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_FinancialTransactionStatuses_StatusId",
                table: "FinancialTransactions",
                column: "StatusId",
                principalSchema: "Lookup",
                principalTable: "FinancialTransactionStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_FinancialTransactionTypes_TransactionTypeId",
                table: "FinancialTransactions",
                column: "TransactionTypeId",
                principalSchema: "Lookup",
                principalTable: "FinancialTransactionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_PaymentMethods_PaymentMethodId",
                table: "FinancialTransactions",
                column: "PaymentMethodId",
                principalSchema: "Lookup",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoldRates_AspNetUsers_ApprovedByUserId",
                schema: "Product",
                table: "GoldRates",
                column: "ApprovedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GoldRates_AspNetUsers_SetByUserId",
                schema: "Product",
                table: "GoldRates",
                column: "SetByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GoldRates_KaratTypes_KaratTypeId",
                schema: "Product",
                table: "GoldRates",
                column: "KaratTypeId",
                principalSchema: "Lookup",
                principalTable: "KaratTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MakingCharges_ChargeTypes_ChargeTypeId",
                schema: "Product",
                table: "MakingCharges",
                column: "ChargeTypeId",
                principalSchema: "Lookup",
                principalTable: "ChargeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MakingCharges_ProductCategoryTypes_ProductCategoryId",
                schema: "Product",
                table: "MakingCharges",
                column: "ProductCategoryId",
                principalSchema: "Lookup",
                principalTable: "ProductCategoryTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MakingCharges_SubCategories_SubCategoryId",
                schema: "Product",
                table: "MakingCharges",
                column: "SubCategoryId",
                principalSchema: "Lookup",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderStatuses_StatusId",
                table: "Orders",
                column: "StatusId",
                principalSchema: "Lookup",
                principalTable: "OrderStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderTypes_OrderTypeId",
                table: "Orders",
                column: "OrderTypeId",
                principalSchema: "Lookup",
                principalTable: "OrderTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_KaratTypes_KaratTypeId",
                schema: "Product",
                table: "Products",
                column: "KaratTypeId",
                principalSchema: "Lookup",
                principalTable: "KaratTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductCategoryTypes_CategoryTypeId",
                schema: "Product",
                table: "Products",
                column: "CategoryTypeId",
                principalSchema: "Lookup",
                principalTable: "ProductCategoryTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_SubCategories_SubCategoryId",
                schema: "Product",
                table: "Products",
                column: "SubCategoryId",
                principalSchema: "Lookup",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RawGoldInventories_KaratTypes_KaratTypeId",
                table: "RawGoldInventories",
                column: "KaratTypeId",
                principalSchema: "Lookup",
                principalTable: "KaratTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RawGoldPurchaseOrderItems_KaratTypes_KaratTypeId",
                table: "RawGoldPurchaseOrderItems",
                column: "KaratTypeId",
                principalSchema: "Lookup",
                principalTable: "KaratTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairJobs_Customers_CustomerId",
                table: "RepairJobs",
                column: "CustomerId",
                principalSchema: "Customer",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairJobs_RepairPriorities_PriorityId",
                table: "RepairJobs",
                column: "PriorityId",
                principalSchema: "Lookup",
                principalTable: "RepairPriorities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairJobs_RepairStatuses_StatusId",
                table: "RepairJobs",
                column: "StatusId",
                principalSchema: "Lookup",
                principalTable: "RepairStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaxConfigurations_ProductCategoryTypes_ProductCategoryId",
                schema: "Product",
                table: "TaxConfigurations",
                column: "ProductCategoryId",
                principalSchema: "Lookup",
                principalTable: "ProductCategoryTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaxConfigurations_TransactionTypes_TaxTypeId",
                schema: "Product",
                table: "TaxConfigurations",
                column: "TaxTypeId",
                principalSchema: "Lookup",
                principalTable: "TransactionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPurchaseItems_KaratTypes_KaratTypeId",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPurchases_AspNetUsers_ApprovedByUserId",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPurchases_AspNetUsers_ProcessedByUserId",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPurchases_FinancialTransactions_FinancialTransactionId",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPurchases_PaymentMethods_PaymentMethodId",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_BusinessEntityTypes_BusinessEntityTypeId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_FinancialTransactionStatuses_StatusId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_FinancialTransactionTypes_TransactionTypeId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_PaymentMethods_PaymentMethodId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_GoldRates_AspNetUsers_ApprovedByUserId",
                schema: "Product",
                table: "GoldRates");

            migrationBuilder.DropForeignKey(
                name: "FK_GoldRates_AspNetUsers_SetByUserId",
                schema: "Product",
                table: "GoldRates");

            migrationBuilder.DropForeignKey(
                name: "FK_GoldRates_KaratTypes_KaratTypeId",
                schema: "Product",
                table: "GoldRates");

            migrationBuilder.DropForeignKey(
                name: "FK_MakingCharges_ChargeTypes_ChargeTypeId",
                schema: "Product",
                table: "MakingCharges");

            migrationBuilder.DropForeignKey(
                name: "FK_MakingCharges_ProductCategoryTypes_ProductCategoryId",
                schema: "Product",
                table: "MakingCharges");

            migrationBuilder.DropForeignKey(
                name: "FK_MakingCharges_SubCategories_SubCategoryId",
                schema: "Product",
                table: "MakingCharges");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderStatuses_StatusId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderTypes_OrderTypeId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_KaratTypes_KaratTypeId",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductCategoryTypes_CategoryTypeId",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_SubCategories_SubCategoryId",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_RawGoldInventories_KaratTypes_KaratTypeId",
                table: "RawGoldInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_RawGoldPurchaseOrderItems_KaratTypes_KaratTypeId",
                table: "RawGoldPurchaseOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairJobs_Customers_CustomerId",
                table: "RepairJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairJobs_RepairPriorities_PriorityId",
                table: "RepairJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairJobs_RepairStatuses_StatusId",
                table: "RepairJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxConfigurations_ProductCategoryTypes_ProductCategoryId",
                schema: "Product",
                table: "TaxConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxConfigurations_TransactionTypes_TaxTypeId",
                schema: "Product",
                table: "TaxConfigurations");

            migrationBuilder.DropTable(
                name: "BusinessEntityTypes",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "ChargeTypes",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "FinancialTransactionStatuses",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "FinancialTransactionTypes",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "OrderStatuses",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "OrderTypes",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "PaymentMethods",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "RawGoldOwnerships");

            migrationBuilder.DropTable(
                name: "RawGoldTransfers");

            migrationBuilder.DropTable(
                name: "RepairPriorities",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "RepairStatuses",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "SubCategories",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "SupplierGoldBalances");

            migrationBuilder.DropTable(
                name: "TransactionStatuses",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "TransactionTypes",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "ProductCategoryTypes",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "KaratTypes",
                schema: "Lookup");

            migrationBuilder.DropIndex(
                name: "IX_RepairJobs_CustomerId",
                table: "RepairJobs");

            migrationBuilder.DropIndex(
                name: "IX_TaxConfigurations_ProductCategoryId",
                schema: "Product",
                table: "TaxConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_GoldRates_ApprovedByUserId",
                schema: "Product",
                table: "GoldRates");

            migrationBuilder.DropIndex(
                name: "IX_GoldRates_SetByUserId",
                schema: "Product",
                table: "GoldRates");

            migrationBuilder.DropIndex(
                name: "IX_CustomerPurchases_ApprovedByUserId",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropIndex(
                name: "IX_CustomerPurchases_FinancialTransactionId",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "RepairJobs");

            migrationBuilder.DropColumn(
                name: "ManufacturingRecordId",
                table: "ProductOwnerships");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Product",
                table: "TaxConfigurations");

            migrationBuilder.DropColumn(
                name: "IsInclusive",
                schema: "Product",
                table: "TaxConfigurations");

            migrationBuilder.DropColumn(
                name: "MaximumTransactionAmount",
                schema: "Product",
                table: "TaxConfigurations");

            migrationBuilder.DropColumn(
                name: "MinimumTransactionAmount",
                schema: "Product",
                table: "TaxConfigurations");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "Product",
                table: "TaxConfigurations");

            migrationBuilder.DropColumn(
                name: "ProductCategoryId",
                schema: "Product",
                table: "TaxConfigurations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "Product",
                table: "TaxConfigurations");

            migrationBuilder.DropColumn(
                name: "TaxAuthority",
                schema: "Product",
                table: "TaxConfigurations");

            migrationBuilder.DropColumn(
                name: "TaxRegistrationNumber",
                schema: "Product",
                table: "TaxConfigurations");

            migrationBuilder.DropColumn(
                name: "AlternatePhone",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "AverageOrderValue",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "BankAccountInfo",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "ContactPersonTitle",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "ContractStartDate",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Country",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DeliveryTerms",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Documents",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "EarlyPaymentDiscountDays",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "EarlyPaymentDiscountPercentage",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "EmergencyContact",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "InsuranceInfo",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IsApprovedForGold",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IsPreferred",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "LanguagePreference",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "LeadTimeDays",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "MaximumOrderValue",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "MinimumOrderValue",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "OnTimeDeliveryPercentage",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "PreferredCommunicationMethod",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "QualityCertifications",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "QualityRatingPercentage",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Rating",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "State",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "SupplierCode",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TotalOrdersCount",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TotalPurchaseAmount",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TradeName",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "VatRegistrationNumber",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Website",
                schema: "Supplier",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Barcode",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CurrentPrice",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Images",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsAvailableForSale",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ManufactureYear",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MaximumStockLevel",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MinimumStockLevel",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReorderPoint",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CanBeWaived",
                schema: "Product",
                table: "MakingCharges");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Product",
                table: "MakingCharges");

            migrationBuilder.DropColumn(
                name: "IsMandatory",
                schema: "Product",
                table: "MakingCharges");

            migrationBuilder.DropColumn(
                name: "MaximumCharge",
                schema: "Product",
                table: "MakingCharges");

            migrationBuilder.DropColumn(
                name: "MaximumWeight",
                schema: "Product",
                table: "MakingCharges");

            migrationBuilder.DropColumn(
                name: "MinimumCharge",
                schema: "Product",
                table: "MakingCharges");

            migrationBuilder.DropColumn(
                name: "MinimumWeight",
                schema: "Product",
                table: "MakingCharges");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "Product",
                table: "MakingCharges");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                schema: "Product",
                table: "GoldRates");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                schema: "Product",
                table: "GoldRates");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "Product",
                table: "GoldRates");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "Product",
                table: "GoldRates");

            migrationBuilder.DropColumn(
                name: "SetByUserId",
                schema: "Product",
                table: "GoldRates");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "Product",
                table: "GoldRates");

            migrationBuilder.DropColumn(
                name: "SourceReference",
                schema: "Product",
                table: "GoldRates");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                schema: "Customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerCategory",
                schema: "Customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                schema: "Customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Gender",
                schema: "Customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "KycCompletedDate",
                schema: "Customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "KycDocuments",
                schema: "Customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "KycStatus",
                schema: "Customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LanguagePreference",
                schema: "Customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "MarketingConsent",
                schema: "Customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "OutstandingBalance",
                schema: "Customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Preferences",
                schema: "Customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PreferredContactMethod",
                schema: "Customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TotalGoldPurchased",
                schema: "Customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AffectsInventory",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "AveragePurity",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "FinancialTransactionId",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "InventoryAdjustmentReason",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "OtherDeductions",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "Photos",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "ProcessingFee",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "ReceiptNumber",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "TestingFee",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "TestingMethod",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "TotalWeight",
                schema: "Customer",
                table: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "ActualPurity",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "AdjustedWeight",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "Condition",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "DeductionReason",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "Deductions",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "FinalWeight",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "ItemSequence",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "ItemType",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "MeltingDate",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "MeltingLoss",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "Photos",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "StonesRemoved",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "StonesValue",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "StonesWeight",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "TestingNotes",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.DropColumn(
                name: "WasMelted",
                schema: "Customer",
                table: "CustomerPurchaseItems");

            migrationBuilder.RenameTable(
                name: "TaxConfigurations",
                schema: "Product",
                newName: "TaxConfigurations");

            migrationBuilder.RenameTable(
                name: "Suppliers",
                schema: "Supplier",
                newName: "Suppliers");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "Product",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "MakingCharges",
                schema: "Product",
                newName: "MakingCharges");

            migrationBuilder.RenameTable(
                name: "GoldRates",
                schema: "Product",
                newName: "GoldRates");

            migrationBuilder.RenameTable(
                name: "Customers",
                schema: "Customer",
                newName: "Customers");

            migrationBuilder.RenameTable(
                name: "CustomerPurchases",
                schema: "Customer",
                newName: "CustomerPurchases");

            migrationBuilder.RenameTable(
                name: "CustomerPurchaseItems",
                schema: "Customer",
                newName: "CustomerPurchaseItems");

            migrationBuilder.RenameColumn(
                name: "StandardCost",
                table: "Products",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "Specifications",
                table: "Products",
                newName: "SubCategory");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "MakingCharges",
                newName: "SubCategory");

            migrationBuilder.RenameColumn(
                name: "LastTransactionDate",
                table: "Customers",
                newName: "LastPurchaseDate");

            migrationBuilder.RenameColumn(
                name: "ProcessedByUserId",
                table: "CustomerPurchases",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerPurchases_ProcessedByUserId",
                table: "CustomerPurchases",
                newName: "IX_CustomerPurchases_CreatedByUserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastCountDate",
                table: "RawGoldInventories",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "MakingChargesId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "TaxConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentTerms",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "HasNumismaticValue",
                table: "Products",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalOrders",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BusinessEntityTypeLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessEntityTypeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChargeTypeLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargeTypeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactionStatusLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactionStatusLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactionTypeLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactionTypeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KaratTypeLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KaratTypeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderTypeLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTypeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethodLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethodLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategoryTypeLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategoryTypeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepairPriorityLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairPriorityLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepairStatusLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairStatusLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionStatusLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionStatusLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionTypeLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTypeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubCategoryLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryTypeId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategoryLookups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategoryLookups_ProductCategoryTypeLookups_CategoryTypeId",
                        column: x => x.CategoryTypeId,
                        principalTable: "ProductCategoryTypeLookups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MakingChargesId",
                table: "OrderItems",
                column: "MakingChargesId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEntityTypeLookups_Name",
                table: "BusinessEntityTypeLookups",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ChargeTypeLookups_Name",
                table: "ChargeTypeLookups",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactionStatusLookups_Name",
                table: "FinancialTransactionStatusLookups",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactionTypeLookups_Name",
                table: "FinancialTransactionTypeLookups",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_KaratTypeLookups_Name",
                table: "KaratTypeLookups",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusLookups_Name",
                table: "OrderStatusLookups",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTypeLookups_Name",
                table: "OrderTypeLookups",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodLookups_Name",
                table: "PaymentMethodLookups",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryTypeLookups_Name",
                table: "ProductCategoryTypeLookups",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RepairPriorityLookups_Name",
                table: "RepairPriorityLookups",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RepairStatusLookups_Name",
                table: "RepairStatusLookups",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryLookups_CategoryTypeId",
                table: "SubCategoryLookups",
                column: "CategoryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryLookups_Name",
                table: "SubCategoryLookups",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionStatusLookups_Name",
                table: "TransactionStatusLookups",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeLookups_Name",
                table: "TransactionTypeLookups",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPurchaseItems_KaratTypeLookups_KaratTypeId",
                table: "CustomerPurchaseItems",
                column: "KaratTypeId",
                principalTable: "KaratTypeLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPurchases_PaymentMethodLookups_PaymentMethodId",
                table: "CustomerPurchases",
                column: "PaymentMethodId",
                principalTable: "PaymentMethodLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_BusinessEntityTypeLookups_BusinessEntityTypeId",
                table: "FinancialTransactions",
                column: "BusinessEntityTypeId",
                principalTable: "BusinessEntityTypeLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_FinancialTransactionStatusLookups_StatusId",
                table: "FinancialTransactions",
                column: "StatusId",
                principalTable: "FinancialTransactionStatusLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_FinancialTransactionTypeLookups_TransactionTypeId",
                table: "FinancialTransactions",
                column: "TransactionTypeId",
                principalTable: "FinancialTransactionTypeLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_PaymentMethodLookups_PaymentMethodId",
                table: "FinancialTransactions",
                column: "PaymentMethodId",
                principalTable: "PaymentMethodLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoldRates_KaratTypeLookups_KaratTypeId",
                table: "GoldRates",
                column: "KaratTypeId",
                principalTable: "KaratTypeLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MakingCharges_ChargeTypeLookups_ChargeTypeId",
                table: "MakingCharges",
                column: "ChargeTypeId",
                principalTable: "ChargeTypeLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MakingCharges_ProductCategoryTypeLookups_ProductCategoryId",
                table: "MakingCharges",
                column: "ProductCategoryId",
                principalTable: "ProductCategoryTypeLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MakingCharges_SubCategoryLookups_SubCategoryId",
                table: "MakingCharges",
                column: "SubCategoryId",
                principalTable: "SubCategoryLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_MakingCharges_MakingChargesId",
                table: "OrderItems",
                column: "MakingChargesId",
                principalTable: "MakingCharges",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderStatusLookups_StatusId",
                table: "Orders",
                column: "StatusId",
                principalTable: "OrderStatusLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderTypeLookups_OrderTypeId",
                table: "Orders",
                column: "OrderTypeId",
                principalTable: "OrderTypeLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_KaratTypeLookups_KaratTypeId",
                table: "Products",
                column: "KaratTypeId",
                principalTable: "KaratTypeLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductCategoryTypeLookups_CategoryTypeId",
                table: "Products",
                column: "CategoryTypeId",
                principalTable: "ProductCategoryTypeLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_SubCategoryLookups_SubCategoryId",
                table: "Products",
                column: "SubCategoryId",
                principalTable: "SubCategoryLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RawGoldInventories_KaratTypeLookups_KaratTypeId",
                table: "RawGoldInventories",
                column: "KaratTypeId",
                principalTable: "KaratTypeLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RawGoldPurchaseOrderItems_KaratTypeLookups_KaratTypeId",
                table: "RawGoldPurchaseOrderItems",
                column: "KaratTypeId",
                principalTable: "KaratTypeLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairJobs_RepairPriorityLookups_PriorityId",
                table: "RepairJobs",
                column: "PriorityId",
                principalTable: "RepairPriorityLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairJobs_RepairStatusLookups_StatusId",
                table: "RepairJobs",
                column: "StatusId",
                principalTable: "RepairStatusLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaxConfigurations_ChargeTypeLookups_TaxTypeId",
                table: "TaxConfigurations",
                column: "TaxTypeId",
                principalTable: "ChargeTypeLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
