using Reclaimer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Windows
{
    public interface ITabContentHost
    {
        DockContainerModel DockContainer { get; }
        DocumentPanelModel DocumentPanel { get; }
    }
}