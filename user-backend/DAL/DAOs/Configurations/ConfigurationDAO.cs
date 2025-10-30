namespace DAL.DAOs.Configurations
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Configuration")]
    public class ConfigurationDAO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Key { get; set; }
        public string Value { get; set; }

    }
}
