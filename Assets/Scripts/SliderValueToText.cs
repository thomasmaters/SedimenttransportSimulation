using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Simulation
{
    public class SliderValueToText : MonoBehaviour
    {
        /// <summary>
        /// Updates the text component this script is attached to.
        /// </summary>
        /// <param name="slider"></param>
        public void UpdateTextValue(Slider slider)
        {
            GetComponent<Text>().text = System.Math.Round(slider.value, 2).ToString();
        }
    }
}
