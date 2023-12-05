using MobX.Mediator;
using MobX.Mediator.Generation;
using MobX.Utilities.Types;
using UnityEngine;

[assembly: GenerateMediatorFor(typeof(bool), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(string), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(string), typeof(string), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(byte), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(int), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(int), typeof(int), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(short), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(long), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(double), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(float), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Color), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Color32), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Vector2), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Vector3), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Vector4), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Quaternion), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Vector2Int), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Vector3Int), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(LayerMask), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Timer), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Seconds), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(RuntimeGUID), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Optional<int>), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Optional<bool>), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Optional<float>), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Optional<string>), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(ScriptableObject), NameSpace = NameSpace.Value)]
[assembly: GenerateMediatorFor(typeof(Object), NameSpace = NameSpace.Value)]

namespace MobX.Mediator
{
    internal static class NameSpace
    {
        public const string Value = "MobX.Mediator";
    }
}