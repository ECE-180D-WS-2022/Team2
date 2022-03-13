using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public Text nameText;
    public Text HPText;
    public Text pointsText;
    public Slider hpSlider;

    public void SetHUD(IP_Player unit) 
    {
        nameText.text = unit.unitName;
        HPText.text = "HP: " + unit.currentHP.ToString() + "/" + unit.maxHP.ToString();
        pointsText.text = "Points: " + unit.points;
        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;
    }

    public void SetHP(IP_Player unit)
    {
        HPText.text = "HP: " + unit.currentHP.ToString() + "/" + unit.maxHP.ToString();
        hpSlider.value = unit.currentHP;
    }

    public void SetPoints(IP_Player unit)
    {
        pointsText.text = "Points: " + unit.points;
    }
}
