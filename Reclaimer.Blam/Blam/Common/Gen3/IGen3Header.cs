using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common.Gen3
{
    public interface IGen3Header
    {
        long FileSize { get; set; }
        //Pointer64 IndexPointer { get; set; }
        int TagDataAddress { get; set; }
        int VirtualSize { get; set; }
        string BuildString { get; set; }
        int StringCount { get; set; }
        int StringTableSize { get; set; }
        Pointer StringTableIndexPointer { get; set; }
        Pointer StringTablePointer { get; set; }
        string ScenarioName { get; set; }
        int FileCount { get; set; }
        Pointer FileTablePointer { get; set; }
        int FileTableSize { get; set; }
        Pointer FileTableIndexPointer { get; set; }
        long VirtualBaseAddress { get; set; }
        IPartitionTable PartitionTable { get; }
        SectionOffsetTable SectionOffsetTable { get; }
        SectionTable SectionTable { get; }
    }
}
