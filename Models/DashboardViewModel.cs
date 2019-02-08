using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RMA_Docker.Models {

    public class DashboardViewModel {
        public int CountTotalWorkers { get; set; }
        public int CountTotalRoles { get; set; }
        public int CountTotalTemplates { get; set; }
        public int CountTotalDocuments { get; set; }
    }

}