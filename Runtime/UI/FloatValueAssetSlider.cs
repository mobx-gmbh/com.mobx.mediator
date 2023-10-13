using MobX.Mediator.Values;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobX.Mediator.UI
{
    public class FloatValueAssetSlider : MonoBehaviour
    {
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue = 1;
        [SerializeField] private ValueAsset<float> valueAsset;
        [SerializeField] private Slider slider;
        [SerializeField] private string nameText = "Missing";
        [SerializeField] private TMP_Text nameTextField;
        [SerializeField] private TMP_Text valueTextField;

        private void OnValidate()
        {
            if (nameTextField)
            {
                nameTextField.text = nameText;
            }
        }

        private void OnEnable()
        {
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = valueAsset.Value;
            slider.onValueChanged.AddListener(OnSliderValueChanged);
            valueTextField.SetText("{0}", valueAsset.Value);
            nameTextField.SetText(nameText);
        }

        private void OnDisable()
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float sliderValue)
        {
            valueAsset.Value = sliderValue;
            valueTextField.SetText("{0}", valueAsset.Value);
        }
    }
}