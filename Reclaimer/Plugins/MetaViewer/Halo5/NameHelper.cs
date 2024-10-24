namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public class NameHelper
    {
        //^*~!&#
        private const string specialChars = "^:#";

        public string Name { get; }
        public string ToolTip { get; }
        public string Description { get; }
        public bool IsBlockName { get; }

        public NameHelper(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            IsBlockName = value.StartsWith("^");
            Name = value.Split(specialChars.ToArray(), StringSplitOptions.RemoveEmptyEntries)[0];
            Description = GetSection(':', value);
            ToolTip = GetSection('#', value);
        }

        private static string GetSection(char separator, string value)
        {
            var start = value.IndexOf(separator);
            if (start < 0)
                return null;

            var end = value.IndexOfAny(specialChars.ToArray(), start + 1);
            if (end < 0)
                end = value.Length;

            return value[(start + 1)..end];
        }
    }
}
