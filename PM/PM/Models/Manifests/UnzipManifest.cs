using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Models.Manifests
{
    public class UnzipManifest
    {
        public string Src { get; set; } = "";
        public string Dest { get; set; } = "";
        public bool CreateDestIfNotExists { get; set; }
    }
}
