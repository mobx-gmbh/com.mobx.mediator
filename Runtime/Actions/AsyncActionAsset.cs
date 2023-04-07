using Cysharp.Threading.Tasks;
using MobX.Utilities.Inspector;
using UnityEngine;

namespace MobX.Mediator.Actions
{
    public abstract class AsyncActionAsset : ScriptableObject
    {
        [Button]
        public abstract UniTask PerformAsync();
    }

    public abstract class AsyncActionAsset<T> : ScriptableObject
    {
        [Button]
        public abstract UniTask PerformAsync(T value);
    }

    public abstract class AsyncActionAsset<T1, T2> : ScriptableObject
    {
        [Button]
        public abstract UniTask PerformAsync(T1 value1, T2 value2);
    }

    public abstract class AsyncActionAsset<T1, T2, T3> : ScriptableObject
    {
        [Button]
        public abstract UniTask PerformAsync(T1 value1, T2 value2, T3 value3);
    }

    public abstract class AsyncActionAsset<T1, T2, T3, T4> : ScriptableObject
    {
        [Button]
        public abstract UniTask PerformAsync(T1 value1, T2 value2, T3 value3, T4 value4);
    }
}