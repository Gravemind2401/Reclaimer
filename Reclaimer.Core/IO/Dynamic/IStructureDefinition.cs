namespace Reclaimer.IO.Dynamic
{
    internal interface IStructureDefinition
    {
        Type TargetType { get; }
        IEnumerable<IVersionDefinition> Versions { get; }

        public static string GetTypeDisplayName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var genericTypes = string.Join(", ", type.GetGenericArguments().Select(GetTypeDisplayName));
            return $"{type.Name.Split('`')[0]}<{genericTypes}>";
        }
    }
}