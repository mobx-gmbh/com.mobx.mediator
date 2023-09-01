using Cysharp.Threading.Tasks;
using MobX.Mediator.Pooling;
using MobX.Utilities.Extensions;
using MobX.Utilities.Types;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Pool;

namespace MobX.Mediator.Tasks
{
    public static class PoolAssetAsyncExtensions
    {
        /// <summary>
        ///     Release an element to the pool after a fixed amount of seconds
        /// </summary>
        public static async void ReleaseAfter<T>(this IObjectPool<T> pool, T element, Seconds seconds) where T : class
        {
            await UniTask.Delay(seconds.TimeSpan);
            if (pool != null && element != null)
            {
                pool.Release(element);
            }
        }

        /// <summary>
        ///     Get an element from the pool and release it after a fixed amount of seconds
        /// </summary>
        public static T Borrow<T>(this IObjectPool<T> pool, Seconds seconds) where T : class
        {
            var element = pool.Get();
            pool.ReleaseAfter(element, seconds);
            return element;
        }

        /// <summary>
        ///     Get an element from the pool and release after 3 Seconds
        /// </summary>
        public static T Borrow<T>(this IObjectPool<T> pool) where T : class
        {
            var element = pool.Get();
            pool.ReleaseAfter(element, 3.Seconds());
            return element;
        }

        /// <summary>
        ///     Use a line renderer pool to draw a line from one point to another.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        [Conditional("DEBUG")]
        public static void DrawLine(this PoolAsset<LineRenderer> pool, Vector3 from, Vector3 to, Seconds duration)
        {
            var line = pool.Borrow(duration);
            line.SetPosition(0, from);
            line.SetPosition(1, to);
        }
    }
}