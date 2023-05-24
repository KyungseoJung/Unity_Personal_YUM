using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class PlayerLife : MonoBehaviour
{
    private float initLife = 100.0f;    //@11-1 포톤
    public float HP = 100f;
    private float lastHitTime =0f;
    private float repeatDamagePeriod = 2.0f;
    private float hurtForce = 10f;        
    
    private Rigidbody2D rigidbody2d;
    public AudioClip[] hurtClips;

    private PlayerCtrl playerCtrl;
    private Image lifeBar;      //@11-1
    private Vector2 lifeScale;

    // private Animator playerHanim;
    private ScoreManager scoreMgr;
//@21-1 게임 승패
    private LevelManager levelManager;
    void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        lifeBar = GameObject.Find("PlayerLifeBar").GetComponent<Image>();   //@11-1 원래 이렇게 되어 있긴 했지만, 포톤에서도 이렇게 하는 게 필수래~
        lifeScale = lifeBar.transform.localScale;

        Invoke("SetScoreManager", 4.0f); //@마지막 //잘 못 찾길래
//@11-1 포톤
        HP = initLife;

//@21-1
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
    }

    void SetScoreManager()
    {
        scoreMgr = GameObject.FindGameObjectWithTag("ScoreManager").GetComponent<ScoreManager>();
    }

    // void SetPlayerHanim()
    // {
    //     playerHanim = GameObject.FindGameObjectWithTag("PlayerHMove").GetComponent<Animator>();
    // }
    // void Start()
    // {
    //     if(playerHanim != null)
    //     {
    //         // Debug.Log("애니 찾았는지 확인용 : " + playerHanim);
    //     }
    // }
    public void GetHurt(Transform weapon, float damageValue = 20f)
    {
        if(Time.time > lastHitTime + repeatDamagePeriod)
        {

            if(HP - damageValue> 0) //다침
            {   
                // playerHanim.SetTrigger("Hurt");     //@8-1 홀로그램에 애니 효과
                scoreMgr.SetPlayerHanim(1);
                TakeDamage(weapon.transform, damageValue);
                lastHitTime = Time.time;
            }
            else    //죽음
            {

                HP = 0;
                // playerHanim.SetTrigger("Die");   //@8-1 홀로그램에 애니 효과
                scoreMgr.SetPlayerHanim(2);
                //아예 죽는 조건 - 애니, 생명, 이미지
                levelManager.CheckWinOrLose();  //@21-1

            }
            UpdateLifeBar();
        }
    }

    void TakeDamage(Transform weapon, float damageValue = 20f)
    {
        // Debug.Log("다침");
//      다쳤을 때 애니(anim)
        playerCtrl.hurt = true;         //다치는 순간은 점프하지 못하도록
        Invoke("ReturnToNormal", 0.3f); //0.3초 후 다시 회복

        //타격 입어서 몸이 튕기는 효과
        Vector3 hurtVector = transform.position - weapon.position + Vector3.up  * 50f;
        rigidbody2d.AddForce(hurtVector * hurtForce);
        
        HP -= damageValue;

        int i = Random.Range(0, hurtClips.Length);
        AudioSource.PlayClipAtPoint(hurtClips[i], transform.position);
    }

    void ReturnToNormal()
    {   
        // Debug.Log("정상으로");
        playerCtrl.hurt = false;
    }

    void UpdateLifeBar()
    {
        //라이프 바 업데이트
        Color newColor = Color.Lerp(Color.green, Color.red, 1-HP * 0.01f);
        newColor.a = 0.5f;    //알파값 0.5로 설정
        lifeBar.color =newColor;

        // lifeBar.rectTransform.localScale = new Vector3(2, lifeScale.x * HP * 0.01f, 1);
        lifeBar.fillAmount = HP/initLife; //남아있는 HP만큼 차오르고, HP가 감소하면 라이프바도 함께 감소하도록
    }

    
}
