using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IP_Player : MonoBehaviour
{
    // Current amount of HP
    public int currentHP;

    // Maximum amount of HP
    public int maxHP;
    
    // IP_Player name
    public string unitName;
    
    // Flag if defending
    public bool isDefending = false;

    // Points accumulated
    public int points = 0;

    // Damage they will deal
    public int damage;

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

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
            currentHP = maxHP;
    }

    public void gainPoints(int amount)
    {
        points += amount;
    }
}
