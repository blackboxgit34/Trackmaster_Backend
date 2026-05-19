using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trackmaster_Model
{
    public class DataTableRequestModel
    {
        public int sEcho { get; set; } = 0;

        public int iDisplayStart { get; set; } = 0;

        public int iDisplayLength { get; set; } = 20;

        public string? sSearch { get; set; }

        public string sortColumn { get; set; } = "";

        public string sortDirection { get; set; } = "asc";

        public int CustId { get; set; }

        public string? Interval { get; set; }

        public string? beginDate { get; set; }

        public string? endDate { get; set; }
        public string? Status { get; set; }
    }
}
