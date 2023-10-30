using UnityEngine;

namespace MobX.Mediator.Components
{
    public class RotationComponent : MonoBehaviour
    {
        [SerializeField] private Vector3 rotation;

        private void Update()
        {
            transform.Rotate(rotation * Time.deltaTime);
        }
    }
}