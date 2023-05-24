using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombWithTimer : MonoBehaviour
{
    public float bombTime = 1.5f;       //몇 초 뒤에 폭탄 터지나
    private float bombRadius = 4.0f;
    public float bombForce = 100.0f;
    public float blowForce = 300.0f;
    public float explodeTime = 1.5f;    //몇 초 뒤에 폭발이 적용되냐  (몇 초 뒤에 explosion 나타나냐)
    public int bombPower = 50;          //폭탄으로 Enemy가 입는 데미지 크기
    
    private Animator anim;              //
    public GameObject explosion;        //폭발 원

    void Awake()
    {
        anim = GetComponent<Animator>();

    }

    void Update()
    {
        StartCoroutine(LayBomb());
    }
    
    IEnumerator LayBomb()   //폭탄 놓고 n초 흐른 뒤에 폭발
    {
        yield return new WaitForSeconds(bombTime);  

        Invoke("ExplodeBomb", explodeTime);                              
    }

    void ExplodeBomb()
    {
//        anim.SetTrigger("Explode");               //폭탄 폭발 애니
        Instantiate(explosion, transform.position, Quaternion.identity);    //폭발 효과 원 이미지

        //bombRadius 내에 있는 모든 enemy 죽이기 & 폭탄 여파로 플레이어와 음식 들썩이기
        Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position, bombRadius);    //3번째 인자 없앰 : , 1<<LayerMask.NameToLayer("Enemies")
        foreach(Collider2D coll in colls)
        {   
            Rigidbody2D rbody = coll.GetComponent<Rigidbody2D>();
            if(rbody != null)
            {   
                if(rbody.gameObject.tag == "Enemy")
                {
                    Debug.Log("//@21 애너미 오버랩 범위 안에 들어옴");
                    rbody.gameObject.GetComponent<Enemy>().GetHurt(bombPower);

                    //폭탄 맞고 튕기는 효과
                    Vector3 bombPos = rbody.transform.position - transform.position;
                    Vector3 force = bombPos.normalized * bombForce;
                    rbody.AddForce(force);                   
                }
                else if(rbody.gameObject.tag == "Player" || rbody.gameObject.tag == "Food")
                {
                    rbody.gameObject.SendMessage("ExplosionBlow", blowForce
                                                , SendMessageOptions.DontRequireReceiver);   //해당 함수가 없어도 에러 발생하지 않도록 옵션 설정
                }

            }
        }
        Destroy(gameObject);    //폭탄 사라짐
    }

}
