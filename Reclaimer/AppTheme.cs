using System.Windows;

namespace Reclaimer
{
    public record AppTheme(string Id, string Path, string Name)
    {
        public ResourceDictionary Resources { get; } = new();
    }
}
