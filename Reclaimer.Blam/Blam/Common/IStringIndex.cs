﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Common
{
    public interface IStringIndex : IEnumerable<string>
    {
        int StringCount { get; }
        string this[int id] { get; }
        int GetStringId(string value);
    }
}