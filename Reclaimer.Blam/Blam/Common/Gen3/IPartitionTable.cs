namespace Reclaimer.Blam.Common.Gen3
{
    public interface IPartitionTable : IReadOnlyList<IPartitionLayout>
    {

    }

    public interface IPartitionLayout
    {
        ulong Address { get; set; }
        ulong Size { get; set; }
    }
}
