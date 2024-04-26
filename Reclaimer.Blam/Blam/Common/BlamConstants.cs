using Reclaimer.Geometry;

namespace Reclaimer.Blam.Common
{
    internal static class BlamConstants
    {
        public const string SbspClustersGroupName = "<Clusters>";
        public const string ModelInstancesGroupName = "<Instances>";

        public const string ScenarioBspGroupName = "scenario_structure_bsps";
        public const string ScenarioSkyGroupName = "skies";
        public const string ScenarioSceneryGroupName = "scenery";
        public const string ScenarioMachineGroupName = "device_machines";
        public const string ScenarioControlGroupName = "device_controls";
        public const string ScenarioCrateGroupName = "crates";
        
        public const string SourceTagPropertyName = "tag_name";
        public const string GeometryInstancePropertyName = "geometry_instance";
        public const string InstanceNamePropertyName = "instance_name";
        public const string InstanceGroupPropertyName = "instance_group";
        public const string PermutationNamePropertyName = "permutation_name";

        //1 world unit = 10 feet
        public const float WorldUnitScale = 10 * StandardUnits.Feet;
    }
}
