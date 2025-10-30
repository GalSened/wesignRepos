using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configuration",
                columns: table => new
                {
                    Key = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuration", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "DocumentSignatureFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DocumentId = table.Column<Guid>(nullable: false),
                    FieldName = table.Column<string>(nullable: true),
                    Image = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSignatureFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CompanyId = table.Column<Guid>(nullable: false),
                    GroupStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    MessageTemplate = table.Column<string>(nullable: true),
                    Level = table.Column<string>(nullable: true),
                    Exception = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.TimeStamp);
                });

            migrationBuilder.CreateTable(
                name: "Programs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Users = table.Column<long>(nullable: false),
                    TemplatesPerMonth = table.Column<long>(nullable: false),
                    DocumentsPerMonth = table.Column<long>(nullable: false),
                    SmsPerMonth = table.Column<long>(nullable: false),
                    ServerSignature = table.Column<bool>(nullable: false),
                    SmartCard = table.Column<bool>(nullable: false),
                    OnlineMode = table.Column<bool>(nullable: false),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProgramUtilizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Users = table.Column<long>(nullable: false),
                    Templates = table.Column<long>(nullable: false),
                    Documents = table.Column<long>(nullable: false),
                    SMS = table.Column<long>(nullable: false),
                    Expired = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramUtilizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SignerTokensMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SignerId = table.Column<Guid>(nullable: false),
                    DocumentCollectionId = table.Column<Guid>(nullable: false),
                    GuidToken = table.Column<Guid>(nullable: false),
                    JwtToken = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignerTokensMapping", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    GroupId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    LastUpdatetime = table.Column<DateTime>(nullable: false),
                    UsedCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ProgramId = table.Column<Guid>(nullable: false),
                    ProgramUtilizationId = table.Column<Guid>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateSignatureFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TemplateId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    SignaturFieldType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateSignatureFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateSignatureFields_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateTextFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TemplateId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    TextFieldType = table.Column<int>(nullable: false),
                    Regex = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateTextFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateTextFields_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyConfigurations",
                columns: table => new
                {
                    CompanyId = table.Column<Guid>(nullable: false),
                    SignatureColor = table.Column<string>(nullable: true),
                    ShouldSendSignedDocument = table.Column<bool>(nullable: false),
                    ShouldNotifyWhileSignerSigned = table.Column<bool>(nullable: false),
                    Language = table.Column<int>(nullable: false),
                    DeleteSignedDocumentAfterXDays = table.Column<int>(nullable: false),
                    DeleteUnsignedDocumentAfterXDays = table.Column<int>(nullable: false),
                    SignerLinkExpirationInDays = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyConfigurations", x => x.CompanyId);
                    table.ForeignKey(
                        name: "FK_CompanyConfigurations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    GroupId = table.Column<Guid>(nullable: false),
                    ProgramUtilizationId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    SendingMethod = table.Column<int>(nullable: false),
                    MessageType = table.Column<int>(nullable: false),
                    Content = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyMessages_CompanyConfigurations_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyConfigurations",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    SendingMethod = table.Column<int>(nullable: false),
                    ProviderType = table.Column<int>(nullable: false),
                    Server = table.Column<string>(nullable: true),
                    Port = table.Column<int>(nullable: false),
                    From = table.Column<string>(nullable: true),
                    User = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageProviders_CompanyConfigurations_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyConfigurations",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    GroupId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    DefaultSendingMethod = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentCollections",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    GroupId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    SignedTime = table.Column<DateTime>(nullable: false),
                    RedirectUrl = table.Column<string>(nullable: true),
                    ShouldSend = table.Column<bool>(nullable: true),
                    ShouldSendSignedDocument = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentCollections_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserConfigurations",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    SignatureColor = table.Column<string>(nullable: true),
                    ShouldSendSignedDocument = table.Column<bool>(nullable: false),
                    ShouldNotifyWhileSignerSigned = table.Column<bool>(nullable: false),
                    Language = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConfigurations", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserConfigurations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsersTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    RefreshToken = table.Column<string>(nullable: true),
                    RefreshTokenExpiredTime = table.Column<DateTime>(nullable: false),
                    ResetPasswordToken = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersTokens", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UsersTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactSeals",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ContactId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactSeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactSeals_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DocumentCollectionId = table.Column<Guid>(nullable: false),
                    TemplateId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_DocumentCollections_DocumentCollectionId",
                        column: x => x.DocumentCollectionId,
                        principalTable: "DocumentCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documents_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Signers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DocumentCollectionId = table.Column<Guid>(nullable: false),
                    ContactId = table.Column<Guid>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    TimeSent = table.Column<DateTime>(nullable: true),
                    TimeViewed = table.Column<DateTime>(nullable: false),
                    TimeSigned = table.Column<DateTime>(nullable: false),
                    TimeRejected = table.Column<DateTime>(nullable: false),
                    SendingMethod = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Signers_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signers_DocumentCollections_DocumentCollectionId",
                        column: x => x.DocumentCollectionId,
                        principalTable: "DocumentCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SignerId = table.Column<Guid>(nullable: false),
                    SignerNote = table.Column<string>(nullable: true),
                    UserNote = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Signers_SignerId",
                        column: x => x.SignerId,
                        principalTable: "Signers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignerAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SignerId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    IsMandatory = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignerAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignerAttachments_Signers_SignerId",
                        column: x => x.SignerId,
                        principalTable: "Signers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignerFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SignerId = table.Column<Guid>(nullable: false),
                    DocumentId = table.Column<Guid>(nullable: false),
                    FieldName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignerFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignerFields_Signers_SignerId",
                        column: x => x.SignerId,
                        principalTable: "Signers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[,]
                {
                    { "SmtpServer", "" },
                    { "LogArichveIntervalInDays", "14" },
                    { "MessageAfter", "[DOCUMENT_NAME] signed successfully. [LINK]" },
                    { "MessageBefore", "[DOCUMENT_NAME] : [LINK]" },
                    { "DeleteUnsignedDocumentAfterXDays", "30" },
                    { "DeleteSignedDocumentAfterXDays", "14" },
                    { "SmsLanguage", "1" },
                    { "SmsProvider", "1" },
                    { "SmsFrom", "" },
                    { "SmsUser", "" },
                    { "SmtpAttachmentMaxSize", "8388608" },
                    { "SmtpEnableSsl", "False" },
                    { "SmtpFrom", "" },
                    { "SmtpPassword", "" },
                    { "SmtpUser", "" },
                    { "SmtpPort", "25" },
                    { "SmsPassword", "" }
                });

            migrationBuilder.InsertData(
                table: "Programs",
                columns: new[] { "Id", "DocumentsPerMonth", "Name", "Note", "OnlineMode", "ServerSignature", "SmartCard", "SmsPerMonth", "TemplatesPerMonth", "Users" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000002"), -1L, "Unlimited", null, true, true, true, -1L, -1L, -1L },
                    { new Guid("00000000-0000-0000-0000-000000000001"), 5L, "Trial", "Upgrade now by calling our Support Center: (+972)3-1111111", false, false, false, 0L, 2L, 0L },
                    { new Guid("00000000-0000-0000-0000-000000000003"), 50L, "Basic", null, true, true, true, 200L, 15L, 2L }
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Name", "ProgramId", "ProgramUtilizationId", "Status" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "Free Accounts", new Guid("00000000-0000-0000-0000-000000000001"), null, 0 });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CompanyId", "CreationTime", "Email", "GroupId", "Name", "Password", "ProgramUtilizationId", "Status", "Type" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2020, 4, 14, 6, 32, 12, 888, DateTimeKind.Utc).AddTicks(400), "systemadmin@comda.co.il", new Guid("00000000-0000-0000-0000-000000000000"), "SystemAdmin", "aFDgUq3rMdhhvRqzQ+/9v51hevUQyVubl2XdsvpZqQ/Q4dVz", null, 2, 4 });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_ProgramId",
                table: "Companies",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMessages_CompanyId",
                table: "CompanyMessages",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_UserId",
                table: "Contacts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactSeals_ContactId",
                table: "ContactSeals",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCollections_UserId",
                table: "DocumentCollections",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentCollectionId",
                table: "Documents",
                column: "DocumentCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TemplateId",
                table: "Documents",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageProviders_CompanyId",
                table: "MessageProviders",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_SignerId",
                table: "Notes",
                column: "SignerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignerAttachments_SignerId",
                table: "SignerAttachments",
                column: "SignerId");

            migrationBuilder.CreateIndex(
                name: "IX_SignerFields_SignerId",
                table: "SignerFields",
                column: "SignerId");

            migrationBuilder.CreateIndex(
                name: "IX_Signers_ContactId",
                table: "Signers",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Signers_DocumentCollectionId",
                table: "Signers",
                column: "DocumentCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateSignatureFields_TemplateId",
                table: "TemplateSignatureFields",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateTextFields_TemplateId",
                table: "TemplateTextFields",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                table: "Users",
                column: "CompanyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyMessages");

            migrationBuilder.DropTable(
                name: "Configuration");

            migrationBuilder.DropTable(
                name: "ContactSeals");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "DocumentSignatureFields");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "MessageProviders");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "ProgramUtilizations");

            migrationBuilder.DropTable(
                name: "SignerAttachments");

            migrationBuilder.DropTable(
                name: "SignerFields");

            migrationBuilder.DropTable(
                name: "SignerTokensMapping");

            migrationBuilder.DropTable(
                name: "TemplateSignatureFields");

            migrationBuilder.DropTable(
                name: "TemplateTextFields");

            migrationBuilder.DropTable(
                name: "UserConfigurations");

            migrationBuilder.DropTable(
                name: "UsersTokens");

            migrationBuilder.DropTable(
                name: "CompanyConfigurations");

            migrationBuilder.DropTable(
                name: "Signers");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "DocumentCollections");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Programs");
        }
    }
}
