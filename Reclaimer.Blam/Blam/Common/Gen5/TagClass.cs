namespace Reclaimer.Blam.Common.Gen5
{
    public class TagClass
    {
        public string ClassCode { get; }
        public string ClassName { get; }

        public TagClass(string code, string name)
        {
            ClassCode = code;
            ClassName = name;
        }
    }
}
