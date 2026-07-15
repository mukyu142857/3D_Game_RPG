using UnityEngine;

public class Weapon1 : Weapon
{
    public const string ANIM_PARM_ISATTACK = "IsAttack";
    private Animator anim;
    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public override void Attack()
    {
        anim.SetTrigger(ANIM_PARM_ISATTACK);
    }
}
