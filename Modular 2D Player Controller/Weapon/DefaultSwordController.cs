using Player;
using UnityEngine;


    //THIS IS TEMPORARY LOGIC 
    //TODO: IMPLEMENT A PROPER STATE SYSTEM 
public class DefaultSwordController : MonoBehaviour, Iweapon
{
    public Animator anim;
    public GameObject parent;
    public Animator parentAnim;
    public Rigidbody2D parentRb;

    public float dmg;


    void Start()
    {
        anim = GetComponent<Animator>();
        parent = GetComponentInParent<Rigidbody2D>().gameObject;
        parentAnim = GetComponentInParent<Controller2D>().anim;
        parentRb = GetComponentInParent<Rigidbody2D>();

    }


    void Update()
    {
        //print(dmg);

        AnimatorStateInfo stateInfo = parentAnim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("PlayerMeleAttack1Anim"))
        {
            anim.Play("defaultSwordAttack1Anim");
            dmg = 2;
        }
        else if (stateInfo.IsName("PlayerMeleAttack2Anim"))
        {
            anim.Play("DefaultSwordAttack2Anim");
            dmg = 1;
        }
        else if (stateInfo.IsName("PlayerMeleAttack3Anim"))
        {
            anim.Play("defaultSwordAttack3Anim");
            dmg = 4;
        }
        else
        {
            anim.Play("defaultSwordIdlAnim");
        }

    }


    private bool imHit = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.transform.tag == "enemy")
        {
            imHit = true;
            Hit();
            collision.GetComponent<IDamagable>().Damage(this.gameObject , (uint)dmg);
        }

        if (collision.transform.tag == "static")
        {
            parentRb.velocity = Vector2.zero;

            if (parent.transform.localScale.x > 0)
            {
                parentRb.AddForce(new Vector2(-1, 0) * 5, ForceMode2D.Impulse);
            }
            else
            {
                parentRb.AddForce(new Vector2(1, 0) * 5, ForceMode2D.Impulse);
            }
        }

    }

    public void Hit()
    {
        GetComponentInParent<Controller2D>().attackCombo++;
    }

    public void AfterHit()
    {
        if (!imHit)
        {
            GetComponentInParent<Controller2D>().attackCombo = 0;
            dmg = 1;
        }

        imHit = false;
    }


    public void AttackOneDmg()
    {

    }

    public void AttackTwoDmg()
    {

    }
    public void AttackThreeDmg()
    {

    }
}