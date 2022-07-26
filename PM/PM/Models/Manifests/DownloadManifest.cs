using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Models.Manifests
{
    public class DownloadManifest
    {
        public string? Uri { get; set; }
        public bool Remote { get; set; }
        public string Dest { get; set; } = "";
    }
}
