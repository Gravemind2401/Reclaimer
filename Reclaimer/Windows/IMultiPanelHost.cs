using Studio.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Windows
{
    public interface IMultiPanelHost
    {
        MultiPanel MultiPanel { get; }
        DocumentTabControl DocumentContainer { get; }
    }
}
