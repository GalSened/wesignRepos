using System;

namespace Common.Models.Dashboard
{
    public class DashboardView
    {
        public Guid GroupId { get; set; }
        public int SignedDocsAmount { get; set; }
        public int PendingDocsAmount { get; set; }
        public int DeclinedDocsAmount { get; set; }
        public int CanceledDocsAmount { get; set; }
    }
}
