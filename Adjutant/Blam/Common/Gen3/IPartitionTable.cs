using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common.Gen3
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
