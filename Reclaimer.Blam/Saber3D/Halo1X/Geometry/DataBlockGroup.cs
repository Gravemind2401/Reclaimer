namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    public class DataBlockGroup : List<DataBlock>
    {
        protected TBlock GetUniqueChild<TBlock>() => this.OfType<TBlock>().Single();
        protected TBlock GetOptionalChild<TBlock>() => this.OfType<TBlock>().SingleOrDefault();
    }
}
