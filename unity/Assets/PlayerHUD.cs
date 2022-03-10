using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public Text nameText;
    public Text HPText;
    public Slider hpSlider;

    public void SetHUD(IP_Player unit) 
    {
        nameText.text = unit.unitName;
        HPText.text = "HP: " + unit.currentHP.ToString() + "/" + unit.maxHP.ToString();
        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;
    }

    public void SetHP(IP_Player unit)
    {
        hpSlider.value = unit.currentHP;
        HPText.text = "HP: " + unit.currentHP.ToString() + "/" + unit.maxHP.ToString();
    }
}
