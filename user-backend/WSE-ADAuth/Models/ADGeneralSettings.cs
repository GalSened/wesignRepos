namespace WSE_ADAuth.Models
{
    public class ADGeneralSettings
    {
        public string ADUserGroupName { get; set; }
        public string ADSignerGroupName { get; set; }        
        public string ADDomainName { get; set; }
        public string ADAdminUser { get; set; }
        public string ADAdminPass{ get; set; }      
        public string ConnectionString { get; set; }
        public string ADKeyForMoblie { get; set; }
        public int UserKeyValidationInSeconds { get; set; }
        public bool SupportSAML { get; set; }
        public bool CheckNestedGroups { get; set; }
        public bool SAMLWithIdUniqueHeader { get; set; }
        
    }
}
