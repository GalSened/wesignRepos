using System.Runtime.Serialization;

namespace Common.Enums.Reports
{
    public enum ManagementReportType
    {
        [EnumMember(Value = "Expiration Utilization")]
        ExpirationUtilization,
        [EnumMember(Value = "Program Utilization")]
        ProgramUtilization,
        [EnumMember(Value = "Use Percentage Utilization")]
        UsePercentageUtilization,
        [EnumMember(Value = "All Companies Utilization")]
        AllCompaniesUtilization,
        [EnumMember(Value = "Group Utilization")]
        GroupUtilization,
        [EnumMember(Value = "Program By Utilization")]
        ProgramByUtilization,
        [EnumMember(Value = "Programs By Usage")]
        ProgramsByUsage,
        [EnumMember(Value = "Group Document Statuses")]
        GroupDocumentStatuses,
        [EnumMember(Value = "Docs By Users")]
        DocsByUsers,
        [EnumMember(Value = "Docs By Signers")]
        DocsBySigners,
        [EnumMember(Value = "Company Users")]
        CompanyUsers,
        [EnumMember(Value = "Free Trial Users")]
        FreeTrialUsers,
        [EnumMember(Value = "Usage By Users")]
        UsageByUsers,
        [EnumMember(Value = "Usage By Companies")]
        UsageByCompanies,
        [EnumMember(Value = "Templates By Usage")]
        TemplatesByUsage,
        [EnumMember(Value = "Usage By Signature Type")]
        UsageBySignatureType
    }
}
