using MobX.Mediator.Registry;

namespace MobX.Mediator.Enums
{
    public abstract class EnumAsset<T> : RegisteredAsset where T : EnumAsset<T>
    {
        public static T None => none ??= CreateInstance<T>();

        private static T none;
    }
}