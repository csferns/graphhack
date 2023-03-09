using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph.Console.Enums;

[Flags]
internal enum OutputFormat
{
    None = 1 << 0,
    Txt = 1 << 1,
    Csv = 1 << 2,
    Logger = 1 << 3,
}
