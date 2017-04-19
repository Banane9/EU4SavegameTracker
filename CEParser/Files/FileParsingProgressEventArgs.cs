using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEParser.Files
{
    public sealed class FileParsingProgressEventArgs : EventArgs
    {
        public double Progress { get; }

        public FileParsingProgressEventArgs(double progress)
        {
            Progress = progress;
        }
    }
}