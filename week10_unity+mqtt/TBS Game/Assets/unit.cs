using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unit : MonoBehaviour
{
    
    public string name;
    public int damage;
    public int maxHP;
    public int currentHP;
    public bool Dead;

    public bool TakeDamage(int dmg)
    {
        currentHP-=dmg;

        if(currentHP<=0)
            return true;
        else
            return false;

    }

    public void FullyHeal()
    {
        currentHP=maxHP;
        return;
    }

    public void HealAmount(int Healing)
    {
        currentHP+=Healing;
        if(currentHP>=maxHP)
            currentHP=maxHP;

        return;
    }
    
}
