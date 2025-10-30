using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class MergeTemplates
    {
        public List<Template> Templates { get; set;} = new List<Template>();
        public string Name { get; set; }    
        public bool IsOneTimeUseTemplate { get; set; }
    }
}
