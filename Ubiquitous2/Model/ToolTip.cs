using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class ToolTip
    {
        public ToolTip(string header, string text)
        {
            Header = header;
            Text = text;

        }
        public string Header { get; set; }
        public string Text { get; set; }
    }
}
