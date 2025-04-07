using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlock.Shared.Models
{
    public class LeaveEvent
    {
        public string FromUser { get; set; } = string.Empty;
        public string ToUser { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
