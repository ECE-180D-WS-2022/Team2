using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Monster : MonoBehaviour
{
    // Current amount of HP
    public int currentHP;

    // Maximum amount of HP
    public int maxHP;

    // Monster name
    public string unitName;

    // Value of monster
    public int value;

    // Damage they will deal
    public int damage;

    public int index;

    public bool TakeDamage(int dmg)
    {
        currentHP -= dmg;
        
        if (currentHP <= 0) {
            currentHP = 0;
            return true;
        } else {
            return false;
        }
    }

    public void OnClick(BaseEventData data)
    {
        PointerEventData pData = (PointerEventData)data;
    }
}
