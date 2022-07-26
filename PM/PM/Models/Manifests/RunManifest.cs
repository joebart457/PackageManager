using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Models.Manifests
{
    public class RunManifest
    {
        public string Cmd { get; set; } = "";
        public bool IgnoreOnFail { get; set; }
        public List<int> ExitCodeSuccess { get; set; } = new List<int>();
        public int Stage { get; set; }
        public bool ShowLogs { get; set; } = false;
    }
}
