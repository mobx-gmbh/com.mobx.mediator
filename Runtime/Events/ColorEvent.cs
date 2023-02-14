using UnityEngine;

namespace MobX.Mediator.Events
{
    public class ColorEvent : EventAsset<Color>
    {
//         // Disable warning to hide base.Raise with custom parameter name.
// #pragma warning disable 109

        public new void Raise(Color color)
        {
            base.Raise(color);
        }
    }
}