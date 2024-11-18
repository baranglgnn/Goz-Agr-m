using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;


public class PlayerControl : MonoBehaviour
{
    private float movementDirection;//hareket y�n�
    public float Speed;//hareket h�z� inspectorda g�r�nebilecek(public)
    public float JumpPower;//z�plama g�c�(public)
    public float GroundCheckRadius;//dairenin yar��ap�(public)
    public float AttackRate = 2f;//sald�r� h�z�
    float nextAttack = 0;//bir sonraki atak zaman�
    public float attackRadius;
    public float damage;
    
    private bool isFacingRight = true;//karakterin y�z� sa�a d�n�k m�?
    private bool isGrounded;//karakterin aya�� zeminde mi?

    public GameObject GroundCheck;//oyun objesine eri�im sa�lad�k
    public Transform AttackPoint;
    public GameObject NinjaStar;
    public Transform FirePoint;
    

    public LayerMask groundLayer;//y�zeyin layer�
    public LayerMask enemyLayer;

    
    

    Rigidbody2D rb;//Rigitbody2D componenetinden bir de�i�ken=rb

    Animator anim;
    void Start()//oyun ba�lat�ld��� an �al��acak kodlar.
    {
        //rb'ile Rigitbody2D componentini manip�le edeblice�iz.
        rb= GetComponent<Rigidbody2D >();
        anim= GetComponent<Animator>();//anim'ile animator� manip�le edece�iz
        
    }

    // Update is called once per frame
    void Update()//her frame i�in �al���r
    {
        CheckRotation();
        CheckSurface();
        CheckAnimations();
        Jump();
        AttackInput();
        Shoot();
       

    }

    private void FixedUpdate()//fiziksel i�lemler i�in daha do�ru bir kullan�m.
    {
        Movement();
    }

    void Movement()//Hareket metodu.
    {
        //GetAxixRaw=eksende bir hareket yapaca��m�z i�in.
        movementDirection = Input.GetAxisRaw("Horizontal");
        rb.velocity=new Vector2(movementDirection*Speed,rb.velocity.y);//rigitbody nin h�z� x y�n�nde hareket ederken,y ekseninde sabit kalacak.
        anim.SetFloat("runSpeed",math.abs(movementDirection*Speed));//runSpeed'e h�z�m�z� at�yoruz.
    }

    void CheckRotation()
    {
        if (isFacingRight && movementDirection < 0)//y�z�m�z sa�a d�n�kse ve sola do�ru gidiyor isek.
        {
            Flip();
        }
        else if(!isFacingRight && movementDirection > 0)//y�z�m�z sa�a d�n�k de�il ve sa�a do�ru gidiyor isek.
        {
            Flip();
        }
    }

    void Flip()//d�nd�rme fonksiyonu(swap mant���)
    {
        isFacingRight = !isFacingRight;//a� kapa mant��� true olunca flip bitminde false a d�necek.Bu sayede s�rekli true d�nmesi engellenecek.
        Vector3 theScale = transform.localScale;//theScale de�i�keni o anki localScale de�i�keninin x ine e�it olacak.
        theScale.x *= -1;//-1 ile �arparak de�eri bir pozitif bir negatif yaparak a� kapa mant��� uygulayaca��z.
        transform.localScale = theScale;//localScale de�i�keni i�lem sonundaki theScale de�i�kenine e�it olacak.
    }

    void Jump()//Z�plama fonksiyonu
    {
        if(isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space)){  //kullan�c� space ku�una basarsa(GetKeyDown)
                rb.velocity = new Vector2(rb.velocity.x, JumpPower);//rb nin velocity si yeni vectora e�it olur.(x y�n�nde de�erini korur).
            }
        }
        
    }

    void CheckSurface()//y�zeyalan� kontrol metodu
    {
        isGrounded = Physics2D.OverlapCircle(GroundCheck.transform.position, GroundCheckRadius, groundLayer);//isGrounded de�i�kenini kontrol i�in daire �rettik.(GroundCheck objesinin o anki konumu, yar��ap, ve ground un layer�)
    }

    private void OnDrawGizmos()//i�aret �retmemizi sa�l�yor
    {
        Gizmos.DrawWireSphere(GroundCheck.transform.position, GroundCheckRadius);//bir saire i�areti �izdik.
        Gizmos.DrawWireSphere(AttackPoint.position, attackRadius);
    }

  

    void CheckAnimations()//animasyonu kontrol edecek d����/y�kseli�
    {
        anim.SetBool("isGrounded", isGrounded);//anima ile beraber animasyonu manip�le ediyoruz
        anim.SetFloat("yVelocity", rb.velocity.y);
    }

    void Attack()//Atak metodu
    {
        float randomValue = UnityEngine.Random.Range(0, 2);//random say� �retmek i�in kulland�m(Gpt verdi)
        if(randomValue == 0)
        {
          anim.SetTrigger("Attack1");
          AudioManager.instance.PlayAudio(AudioManager.instance.swordAS);
        }else if(randomValue == 1)
        {
            anim.SetTrigger("Attack2");
            AudioManager.instance.PlayAudio (AudioManager.instance.swordAS);
        }

        Collider2D[] hitEnemys = Physics2D.OverlapCircleAll(AttackPoint.position, attackRadius, enemyLayer);

        foreach(Collider2D enemy in  hitEnemys)
        {
            enemy.GetComponent<EnemyStats>().takeDamage(damage);
        }
       
    }

   void AttackInput()
    {
        if(Time.time > nextAttack)
        {
         if (Input.GetKeyDown(KeyCode.LeftControl))
         {
            Attack();
                nextAttack = Time.time + 1f / AttackRate;
         }

        }
        
    }
    public void Shoot()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(StarsBank.instance.bankStar>0)
            {
             Instantiate(NinjaStar, FirePoint.position, FirePoint.rotation);
                StarsBank.instance.bankStar -=1;
            }
           

        }
    }

}
