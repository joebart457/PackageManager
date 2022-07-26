using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Models.Manifests
{
    public class PackageManifest
    {
        public string Name { get; set; } = "";
        public string Tag { get; set; } = "";
        public string Description { get; set; } = "No description.";
        public DownloadManifest? DownloadManifest { get; set; }
        public List<UnzipManifest>? UnzipManifests { get; set; }
        public List<RunManifest>? RunManifests { get; set; }
    }
}
