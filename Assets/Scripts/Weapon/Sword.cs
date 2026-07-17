using UnityEngine;

public class Sword : Weapon
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
    private void Update()
    {

    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy")
        {

        }
    }
}
