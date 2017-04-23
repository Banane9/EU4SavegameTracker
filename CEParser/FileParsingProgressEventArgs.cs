using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEParser
{
    public sealed class SaveDecodingProgressEventArgs : EventArgs
    {
        public double Progress { get; }

        public SaveDecodingProgressEventArgs(double progress)
        {
            Progress = progress;
        }
    }
}