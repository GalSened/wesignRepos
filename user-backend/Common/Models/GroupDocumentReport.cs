using Common.Enums.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
    public class GroupDocumentReport
    {

        public string GroupName { get; set; }
        public int CreatedDocs { get; set; }
        public int SentDocs { get; set; }
        public int ViewedDocs { get; set; }
        public int SignedDocs { get; set; }
        public int DeclinedDocs { get; set; }
        public int DeletedDocs { get; set; }
        public int CanceledDocs { get; set; }
        public int ServerSignedDocs { get; set; }
        public GroupDocumentReport()
        {

        }
       
    }


}
