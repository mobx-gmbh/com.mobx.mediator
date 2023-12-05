namespace MobX.Mediator.Values
{
    public interface IPersistentDataAsset<TValue> : IValueAsset<TValue>
    {
        void SavePersistentData();

        void LoadPersistentData();

        void ResetPersistentData();
    }
}