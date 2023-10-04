using MobX.Mediator.Values;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobX.Mediator.UI
{
    public class BoolValueAssetToggle : MonoBehaviour
    {
        [SerializeField] private ValueAsset<bool> valueAsset;
        [SerializeField] private Toggle toggle;
        [SerializeField] private string nameText = "Missing";
        [SerializeField] private TMP_Text nameTextField;

        private void OnValidate()
        {
            if (nameTextField)
            {
                nameTextField.text = nameText;
            }
        }

        private void OnEnable()
        {
            toggle.isOn = valueAsset.Value;
            toggle.onValueChanged.AddListener(OnValueChanged);
            nameTextField.SetText(nameText);
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(bool value)
        {
            valueAsset.Value = value;
        }
    }
}