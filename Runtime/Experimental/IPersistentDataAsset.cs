namespace MobX.Mediator.Experimental
{
    public interface IPersistentDataAsset<TValue> : IValueAsset<TValue>
    {
        void SavePersistentData();

        void LoadPersistentData();

        void ResetPersistentData();
    }
}