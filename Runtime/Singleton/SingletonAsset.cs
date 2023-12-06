using JetBrains.Annotations;
using MobX.Mediator.Callbacks;
using MobX.Mediator.Registry;
using MobX.Utilities.Reflection;

namespace MobX.Mediator.Singleton
{
    [AddressablesGroup("Singletons")]
    public abstract class SingletonAsset<T> : ScriptableAsset where T : SingletonAsset<T>
    {
        public static T Singleton => singleton ??= AssetRegistry.ResolveSingleton<T>();
        private static T singleton;

        [PublicAPI]
        public bool IsSingleton => AssetRegistry.ExistsSingleton<T>() && AssetRegistry.ResolveSingleton<T>() == this;

        protected override void OnEnable()
        {
            base.OnEnable();
            singleton = (T) this;

            if (AssetRegistry.ExistsSingleton<T>() is false)
            {
                AssetRegistry.RegisterSingleton(this);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (singleton == this)
            {
                singleton = null;
            }
        }
    }
}