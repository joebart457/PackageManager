using PM.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Models
{
    public class RunResult
    {
        public RunStatus Status { get; set; }
        public int ExitCode { get; set; }
        public bool ProducedUsableExitCode { get; set; }
        public bool HadError { get; set; }
        public string? ErrorTrace { get; set; }
    }
}
