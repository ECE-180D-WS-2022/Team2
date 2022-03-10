using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Text nameText;
    public Text HPText;
    public Slider hpSlider;

    public void SetHUD(Unit unit) 
    {
        nameText.text = unit.unitName;
        HPText.text = "HP: " + unit.currentHP.ToString() + "/" + unit.maxHP.ToString();
        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;
    }

    public void SetHP(Unit unit)
    {
        hpSlider.value = unit.currentHP;
        HPText.text = "HP: " + unit.currentHP.ToString() + "/" + unit.maxHP.ToString();
    }
}
