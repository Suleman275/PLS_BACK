using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "V2");

            migrationBuilder.CreateTable(
                name: "ClientSources",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseTypes",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncomeTypes",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nationalities",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TwoLetterCode = table.Column<string>(type: "text", nullable: true),
                    ThreeLetterCode = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nationalities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProgramTypes",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UniversityTypes",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversityTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisaApplicationTypes",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisaApplicationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ExpenseTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalSchema: "V2",
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Expenses_ExpenseTypes_ExpenseTypeId",
                        column: x => x.ExpenseTypeId,
                        principalSchema: "V2",
                        principalTable: "ExpenseTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incomes",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IncomeStatus = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IncomeTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incomes_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalSchema: "V2",
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Incomes_IncomeTypes_IncomeTypeId",
                        column: x => x.IncomeTypeId,
                        principalSchema: "V2",
                        principalTable: "IncomeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResetPasswordToken = table.Column<string>(type: "text", nullable: true),
                    ResetPasswordTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmailVerificationToken = table.Column<string>(type: "text", nullable: true),
                    EmailVerificationTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PhoneVerificationToken = table.Column<string>(type: "text", nullable: true),
                    PhoneVerificationTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsPhoneVerified = table.Column<bool>(type: "boolean", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    ImmigrationClientUser_LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImmigrationClientUser_NationalityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImmigrationClientUser_AdmissionAssociateId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImmigrationClientUser_CounselorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImmigrationClientUser_SopWriterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImmigrationClientUser_RegisteredById = table.Column<Guid>(type: "uuid", nullable: true),
                    ImmigrationClientUser_RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ImmigrationClientUser_ClientSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImmigrationClientUser_StorageLimit = table.Column<long>(type: "bigint", nullable: true),
                    ImmigrationClientUser_StorageUsage = table.Column<long>(type: "bigint", nullable: true),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    NationalityId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdmissionAssociateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CounselorId = table.Column<Guid>(type: "uuid", nullable: true),
                    SopWriterId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegisteredById = table.Column<Guid>(type: "uuid", nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClientSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    StorageLimit = table.Column<long>(type: "bigint", nullable: true),
                    StorageUsage = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_ClientSources_ClientSourceId",
                        column: x => x.ClientSourceId,
                        principalSchema: "V2",
                        principalTable: "ClientSources",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_ClientSources_ImmigrationClientUser_ClientSourceId",
                        column: x => x.ImmigrationClientUser_ClientSourceId,
                        principalSchema: "V2",
                        principalTable: "ClientSources",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Locations_ImmigrationClientUser_LocationId",
                        column: x => x.ImmigrationClientUser_LocationId,
                        principalSchema: "V2",
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Locations_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "V2",
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Nationalities_ImmigrationClientUser_NationalityId",
                        column: x => x.ImmigrationClientUser_NationalityId,
                        principalSchema: "V2",
                        principalTable: "Nationalities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Nationalities_NationalityId",
                        column: x => x.NationalityId,
                        principalSchema: "V2",
                        principalTable: "Nationalities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Users_AdmissionAssociateId",
                        column: x => x.AdmissionAssociateId,
                        principalSchema: "V2",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Users_CounselorId",
                        column: x => x.CounselorId,
                        principalSchema: "V2",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Users_ImmigrationClientUser_AdmissionAssociateId",
                        column: x => x.ImmigrationClientUser_AdmissionAssociateId,
                        principalSchema: "V2",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Users_ImmigrationClientUser_CounselorId",
                        column: x => x.ImmigrationClientUser_CounselorId,
                        principalSchema: "V2",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Users_ImmigrationClientUser_RegisteredById",
                        column: x => x.ImmigrationClientUser_RegisteredById,
                        principalSchema: "V2",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Users_ImmigrationClientUser_SopWriterId",
                        column: x => x.ImmigrationClientUser_SopWriterId,
                        principalSchema: "V2",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Users_RegisteredById",
                        column: x => x.RegisteredById,
                        principalSchema: "V2",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Users_SopWriterId",
                        column: x => x.SopWriterId,
                        principalSchema: "V2",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Universities",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NumOfCampuses = table.Column<int>(type: "integer", nullable: true),
                    TotalStudents = table.Column<int>(type: "integer", nullable: true),
                    YearFounded = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UniversityTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Universities_Locations_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "V2",
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Universities_UniversityTypes_UniversityTypeId",
                        column: x => x.UniversityTypeId,
                        principalSchema: "V2",
                        principalTable: "UniversityTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    S3Key = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    DocumentStatus = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalSchema: "V2",
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documents_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "V2",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionAssignments",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<string>(type: "text", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "V2",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisaApplications",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisaApplicationTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationStatus = table.Column<int>(type: "integer", nullable: false),
                    ApplyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewSuccessDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResultDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisaApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisaApplications_Users_ApplicantId",
                        column: x => x.ApplicantId,
                        principalSchema: "V2",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisaApplications_VisaApplicationTypes_VisaApplicationTypeId",
                        column: x => x.VisaApplicationTypeId,
                        principalSchema: "V2",
                        principalTable: "VisaApplicationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UniversityPrograms",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DurationYears = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UniversityId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversityPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniversityPrograms_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalSchema: "V2",
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UniversityApplications",
                schema: "V2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UniversityProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationStatus = table.Column<int>(type: "integer", nullable: false),
                    ApplyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewSuccessDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResultDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversityApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniversityApplications_UniversityPrograms_UniversityProgram~",
                        column: x => x.UniversityProgramId,
                        principalSchema: "V2",
                        principalTable: "UniversityPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UniversityApplications_Users_ApplicantId",
                        column: x => x.ApplicantId,
                        principalSchema: "V2",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentTypeId",
                schema: "V2",
                table: "Documents",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_OwnerId",
                schema: "V2",
                table: "Documents",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CurrencyId",
                schema: "V2",
                table: "Expenses",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ExpenseTypeId",
                schema: "V2",
                table: "Expenses",
                column: "ExpenseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_CurrencyId",
                schema: "V2",
                table: "Incomes",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_IncomeTypeId",
                schema: "V2",
                table: "Incomes",
                column: "IncomeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionAssignments_UserId",
                schema: "V2",
                table: "PermissionAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_LocationId",
                schema: "V2",
                table: "Universities",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_UniversityTypeId",
                schema: "V2",
                table: "Universities",
                column: "UniversityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityApplications_ApplicantId",
                schema: "V2",
                table: "UniversityApplications",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityApplications_UniversityProgramId",
                schema: "V2",
                table: "UniversityApplications",
                column: "UniversityProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityPrograms_UniversityId",
                schema: "V2",
                table: "UniversityPrograms",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AdmissionAssociateId",
                schema: "V2",
                table: "Users",
                column: "AdmissionAssociateId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClientSourceId",
                schema: "V2",
                table: "Users",
                column: "ClientSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CounselorId",
                schema: "V2",
                table: "Users",
                column: "CounselorId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ImmigrationClientUser_AdmissionAssociateId",
                schema: "V2",
                table: "Users",
                column: "ImmigrationClientUser_AdmissionAssociateId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ImmigrationClientUser_ClientSourceId",
                schema: "V2",
                table: "Users",
                column: "ImmigrationClientUser_ClientSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ImmigrationClientUser_CounselorId",
                schema: "V2",
                table: "Users",
                column: "ImmigrationClientUser_CounselorId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ImmigrationClientUser_LocationId",
                schema: "V2",
                table: "Users",
                column: "ImmigrationClientUser_LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ImmigrationClientUser_NationalityId",
                schema: "V2",
                table: "Users",
                column: "ImmigrationClientUser_NationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ImmigrationClientUser_RegisteredById",
                schema: "V2",
                table: "Users",
                column: "ImmigrationClientUser_RegisteredById");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ImmigrationClientUser_SopWriterId",
                schema: "V2",
                table: "Users",
                column: "ImmigrationClientUser_SopWriterId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LocationId",
                schema: "V2",
                table: "Users",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NationalityId",
                schema: "V2",
                table: "Users",
                column: "NationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RegisteredById",
                schema: "V2",
                table: "Users",
                column: "RegisteredById");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SopWriterId",
                schema: "V2",
                table: "Users",
                column: "SopWriterId");

            migrationBuilder.CreateIndex(
                name: "IX_VisaApplications_ApplicantId",
                schema: "V2",
                table: "VisaApplications",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_VisaApplications_VisaApplicationTypeId",
                schema: "V2",
                table: "VisaApplications",
                column: "VisaApplicationTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documents",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "Expenses",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "Incomes",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "PermissionAssignments",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "ProgramTypes",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "UniversityApplications",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "VisaApplications",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "DocumentTypes",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "ExpenseTypes",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "Currencies",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "IncomeTypes",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "UniversityPrograms",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "VisaApplicationTypes",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "Universities",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "ClientSources",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "Nationalities",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "Locations",
                schema: "V2");

            migrationBuilder.DropTable(
                name: "UniversityTypes",
                schema: "V2");
        }
    }
}
