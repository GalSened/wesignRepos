export enum ReportMode {
    ExpirationUtilization,
    ProgramUtilization,
    UsePercentageUtilization,
    AllCompaniesUtilization,
    GroupUtilization,
    ProgramByUtilization,
    ProgramsByUsage,
    GroupDocumentStatuses,
    DocsByUsers,
    DocsBySigners,
    CompanyUsers,
    FreeTrialUsers,
    UsageByUsers,
    UsageByCompanies,
    TemplatesByUsage,
    UsageBySignatureType
};
export const ReportModeLabels = {
    [ReportMode.ExpirationUtilization]: "Expiration Utilization",
    [ReportMode.ProgramUtilization]: "Program Utilization",
    [ReportMode.UsePercentageUtilization]: "Use Percentage Utilization",
    [ReportMode.AllCompaniesUtilization]: "All Companies Utilization",
    [ReportMode.GroupUtilization]: "Group Utilization",
    [ReportMode.ProgramByUtilization]: "Program By Utilization",
    [ReportMode.ProgramsByUsage]: "Programs By Usage",
    [ReportMode.GroupDocumentStatuses]: "Group Document Statuses",
    [ReportMode.DocsByUsers]: "Docs By Users",
    [ReportMode.DocsBySigners]: "Docs By Signers",
    [ReportMode.CompanyUsers]: "Company Users",
    [ReportMode.FreeTrialUsers]: "Free Trial Users",
    [ReportMode.UsageByUsers]: "Usage By Users",
    [ReportMode.UsageByCompanies]: "Usage By Companies",
    [ReportMode.TemplatesByUsage]: "Templates By Usage",
    [ReportMode.UsageBySignatureType]: "Usage By Signature Type"
};
export const ReportPropeties: { mode: ReportMode[], headers: string[] }[] = [
    {
        mode: [ReportMode.ExpirationUtilization, ReportMode.ProgramUtilization, ReportMode.UsePercentageUtilization, ReportMode.AllCompaniesUtilization],
        headers: ["Company Name", "Program Start Date", "Expired", "Documents Usage", "SMS Usage", "Last _TIME_FOR_AVG Months Documents Average", "Last _TIME_FOR_AVG Months SMS Average", /*"Documents Usage %"*/]
    },
    {
        mode: [ReportMode.GroupUtilization],

        headers: ["Group Name", "Last _TIME_FOR_AVG Months Documents Average", "LAST _TIME_FOR_AVG Months SMS Average"]
    },
    {
        mode: [ReportMode.ProgramByUtilization, ReportMode.ProgramsByUsage],

        headers: ["Program Name", "Users", "Templates", "Documents Per Month", "SMS Per Month", "Server Signature", "Smart Card"]
    },
    {
        mode: [ReportMode.GroupDocumentStatuses],
        headers: ["Group Name", "Created Documents", "Sent Documents", "Viewed Documents", "Signed Documents", "Declined Documents", "Deleted Documents", "Canceled Documents", "Server Signed Documents"]
    },

    {
        mode: [ReportMode.DocsByUsers],
        headers: ["Contact Name", "Document Amount"]
    },

    {
        mode: [ReportMode.DocsBySigners],
        headers: ["Contact Name", "Document Amount"]
    },

    {
        mode: [ReportMode.CompanyUsers],
        headers: ["User Name", "User Email", "Documents Amount", "Group Name"]
    },
    {
        mode: [ReportMode.FreeTrialUsers],
        headers: ["Name", "Email", "UserName", "Documents Usage", "SMS Usage", "Templates Usage", "Creation Date", "Expiration Date"]
    },
    {
        mode: [ReportMode.UsageByUsers],
        headers: ["Company Name", "Group Name", "Email", "Sent", "Signed", "Declined", "Canceled", "Deleted"]
    },
    {
        mode: [ReportMode.UsageByCompanies],
        headers: ["Company Name", "Group Name", "Sent", "Signed", "Declined", "Canceled"]
    },
    {
        mode: [ReportMode.TemplatesByUsage],
        headers: ["Template Name", "Company Name", "Group Name", "UsageCount"]
    },
    {
        mode: [ReportMode.UsageBySignatureType],
        headers: ["Company Name", "Graphic", "SmartCard", "Server"]
    },
]

