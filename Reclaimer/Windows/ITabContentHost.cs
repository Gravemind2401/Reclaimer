using Reclaimer.Models;

namespace Reclaimer.Windows
{
    public interface ITabContentHost
    {
        DockContainerModel DockContainer { get; }
        DocumentPanelModel DocumentPanel { get; }
    }
}