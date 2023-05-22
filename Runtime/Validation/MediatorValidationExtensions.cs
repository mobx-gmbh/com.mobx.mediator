using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MobX.Mediator
{
    public static class MediatorValidationExtensions
    {
        [Conditional("UNITY_ASSERTIONS")]
        public static void AssertMediatorIsAssigned(this ScriptableObject mediator, string memberName = null, [CallerMemberName] string member = "")
        {
            if (mediator == null)
            {
                Debug.LogError("Mediator", $"{memberName ?? "Mediator"} is not assigned! in {member}");
            }
        }
    }
}
