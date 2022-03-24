using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Events;
using System;

namespace TBOB
{
    public class Settings : MonoBehaviour
    {
        public PostProcessProfile profile;
        

        [Header("Sliders")]
        public SliderSettings brightnessSlider;
        public SliderSettings renderDistanceSlider;

        

        void Brightness(float currentValue)
        {
            float finalValue;
            finalValue = ConvertValue(brightnessSlider.slider.minValue, brightnessSlider.slider.maxValue,
                brightnessSlider.minSettingsValue, brightnessSlider.maxSettingsValue, currentValue);
            profile.GetSetting<ColorGrading>().postExposure.Override(finalValue);
            
        }

        float ConvertValue(float virtualMin, float virtualMax, float actualMin, float actualMax, float currentValue)
        {
            float ratio = (actualMax - actualMin) / (virtualMax - virtualMin);
            float returnValue = ((currentValue * ratio) - (virtualMin * ratio)) + actualMin;
            return returnValue;

        }
        void Start()
        {
            brightnessSlider.slider.onValueChanged.AddListener(delegate { Brightness(brightnessSlider.slider.value); });
        }
    }
}