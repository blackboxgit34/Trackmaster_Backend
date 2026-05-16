using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trackmaster_Model
{
    public class PlaybackDataModel
    {
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        public string location { get; set; }
        public int speed { get; set; }
        public DateTime datadate { get; set; }
        public string acignition { get; set; }
        public decimal distance { get; set; }
    }
}
