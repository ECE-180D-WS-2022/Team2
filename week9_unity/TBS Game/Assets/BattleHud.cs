using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    public Text nameText;
    public Slider HPslider;

    public void SetHUD(unit unit)
    {
        nameText.text=unit.name;
        HPslider.maxValue=unit.maxHP;
        HPslider.value=unit.currentHP;
        
    }

    public void SetHP(int hp){
        HPslider.value=hp;
    }


}
