using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttributeBehaviour : MonoBehaviour
{
    public Slider slider;
    public TMP_Text text;

    void Start()
    {
        ValueChanged();
        slider.onValueChanged.AddListener(delegate { ValueChanged(); });
    }

    public void ValueChanged()
    {
        float val = Mathf.Round(slider.value * 100) / 100;

        text.text = val.ToString();
    }
}
