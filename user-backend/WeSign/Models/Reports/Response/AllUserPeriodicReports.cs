using Common.Models.Users;
using System.Collections;
using System.Collections.Generic;

namespace WeSign.Models.Reports.Response
{
    public class AllUserPeriodicReports
    {
        public IEnumerable<UserPeriodicReport> userPeriodicReports { get; set; }
    }
}
