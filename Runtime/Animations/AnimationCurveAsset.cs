using UnityEngine;

namespace MobX.Mediator.Animations
{
    public class AnimationCurveAsset : ScriptableObject
    {
        [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve Curve => curve;
    }
}