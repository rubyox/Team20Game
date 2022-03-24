using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TBOB
{
    [System.Serializable]
    public class SliderSettings
    {
        public Slider slider;
        public Text txtSlider;
        [Tooltip("the actual maximum value for your settings use will not see this")]
        public float maxSettingsValue;
        [Tooltip("the actual minimum value for your settings use will not see this")]
        public float minSettingsValue;
    }
}