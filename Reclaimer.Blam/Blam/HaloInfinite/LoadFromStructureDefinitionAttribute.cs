namespace Reclaimer.Blam.HaloInfinite
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class LoadFromStructureDefinitionAttribute: Attribute
    {
        public Guid StructureGuid { get; }

        public LoadFromStructureDefinitionAttribute(string structureDefinitionGuid)
        {
            if (!Guid.TryParse(structureDefinitionGuid, out var guidValue))
                throw new FormatException("Not a valid GUID string");

            StructureGuid = guidValue;
        }
    }
}
