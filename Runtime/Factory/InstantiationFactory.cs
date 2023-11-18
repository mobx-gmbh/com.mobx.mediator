using UnityEngine;

namespace MobX.Mediator.Factory
{
    public class InstantiationFactory : FactoryAsset<GameObject>
    {
        [SerializeField] private GameObject prefab;

        public override GameObject Create()
        {
            return Instantiate(prefab);
        }
    }
}