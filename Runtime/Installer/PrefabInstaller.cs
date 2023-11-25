using MobX.Mediator.Callbacks;
using MobX.Mediator.Singleton;
using MobX.Utilities;
using MobX.Utilities.Types;
using UnityEngine;

namespace MobX.Mediator.Installer
{
    public class PrefabInstaller : SingletonAsset<PrefabInstaller>
    {
        [SerializeField] private bool autoInstall;
        [SerializeField] private Optional<GameObject>[] systems;

        private static bool installed;

        [CallbackOnInitialization]
        private void Install()
        {
            if (installed || autoInstall is false)
            {
                return;
            }
            installed = true;
            foreach (var system in systems)
            {
                if (system.TryGetValue(out var prefab))
                {
                    var instance = Instantiate(prefab);
                    instance.DontDestroyOnLoad();
                    instance.name = $"[{prefab.name}]";
                }
            }
        }

        [CallbackOnApplicationQuit]
        private void OnQuit()
        {
            installed = false;
        }
    }
}