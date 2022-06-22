using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace antistract.Core
{
    public class Plans
    {
        public Plans(string entryName, string title, string type, string duration)
        {
            this.entryName = entryName;
            this.title = title;
            this.type = type;
            this.duration = duration;
        }

        public string entryName { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string duration { get; set; }
    }
}
