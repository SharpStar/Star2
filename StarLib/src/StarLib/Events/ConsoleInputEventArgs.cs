using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Events
{
    public class ConsoleInputEventArgs : EventArgs
    {
        public string Input { get; set; }

        public ConsoleInputEventArgs(string input)
        {
            Input = input;
        }
    }
}
