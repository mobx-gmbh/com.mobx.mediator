using UnityEngine;

namespace MobX.Mediator.Conditions
{
    public abstract class ConditionAsset : ScriptableObject
    {
        public abstract bool Check();

        public static implicit operator bool(ConditionAsset conditionAsset)
        {
            return conditionAsset.Check();
        }
    }
}
