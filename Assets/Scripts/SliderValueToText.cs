using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueToText : MonoBehaviour
{
    public void UpdateTextValue(Slider slider)
    {
        GetComponent<Text>().text = System.Math.Round(slider.value, 2).ToString();
    }
}
