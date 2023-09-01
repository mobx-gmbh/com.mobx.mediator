namespace MobX.Mediator.Pooling
{
    public interface IPoolAsset
    {
        public PoolState State { get; }

        void Load();

        void Clear();

        void Unload();
    }
}