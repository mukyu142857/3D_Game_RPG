using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Weapon weapon;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (weapon != null && Input.GetMouseButtonDown(0))
        {
            weapon.Attack();
        }
    }
    public void LoadWeapon(Weapon weapon)
    {
        this.weapon = weapon;
    }
    public void UnloadWeapon()
    {
        weapon = null;
    }
}
