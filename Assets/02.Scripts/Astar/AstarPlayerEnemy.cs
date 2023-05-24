using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 위 스크립트에서 EnableAttack 함수가 실행되는 시점을 Enemy마다 랜덤하게 줘서 
// 불 뿜는 모습을 자연스럽게 연출하고 싶어
public class  AstarPlayerEnemy : MonoBehaviour
{
    public int HP = 100;                // 몬스터 체력
    public bool dead = false;          // 죽었나 살았나
    private bool deadCheck = false;    //if문 한번만 타도록
    private float deadTime = 3.0f;      //죽고나서 Destroy 되기까지의 시간
    public bool dirRight = true;        // 쳐다보는 방향(오른쪽붵)

    private float damagePower;          //플레이어 공격 시,  데미지 입히는 양
    private float attackTimer;
    private float minAttackDelay = 3.0f;
    private float maxAttackDelay = 7.0f;
    private float attackDelay;
    private bool attack;                      //공격 가능한지 체크
    private float attackHoldTime = 2.0f;    //공격 유지 시간

    public AudioClip[] deathClips;      // 몬스터가 죽었을때 플레이할 수 있는 오디오 클립 배열 
//    public GameObject PointsUI;         // 몬스터가 죽었을때 발생하는 100의 프리팹 
    private Animator anim;              //
    private Rigidbody2D myRbody;    //
    private Collider2D collider2d;      //

    private SpriteRenderer lifeBar;      //라이프바 (남은 HP 표시)
    private Vector3 lifeBarScale;       //HP 줄어들수록 Scale 감소

    private PlayerLife playerLife;
    private ScoreManager scoreMgr;

//@11-1 포톤 ===========================================
    private Transform myTr;
    //PhotonView 컴포넌트를 할당할 레퍼런스 
    PhotonView pv = null;

    //위치 정보를 송수신할 때 사용할 변수 선언 및 초기값 설정 
    Vector3 currPos = Vector3.zero;
    Vector3 currScale = Vector3.zero;

//여기까지 @11-1 ===========================================

    void Awake()
    {
        // Transform firstChild = transform.GetChild(0);   //자식 오브젝트 위치 중 0번째 자식
        // Transform secondChild = transform.GetChild(1);

        anim = GetComponent<Animator>();
        myRbody = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<Collider2D>();

		lifeBar = transform.Find("LifeBar").GetComponent<SpriteRenderer>(); //GameObject로 찾으면 다른 플레이어/ Enemy의 라이프바와 혼동된다~
        lifeBarScale = lifeBar.transform.localScale;

        // playerLife = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerLife>();  //@18-3 Start에서 해보자
        scoreMgr = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();

//@11-1 포톤 =========================
        //자기 자신의 Transform 연결
        myTr = GetComponent<Transform>();
        pv = GetComponent<PhotonView>();
//PhotonView Observed Components 속성에 PlayerCtrl(현재) 스크립트 Component를 연결
        pv.ObservedComponents[0] = this;
//데이타 전송 타입을 설정
        pv.synchronization = ViewSynchronization.UnreliableOnChange;
//자신의 네트워크 객체가 아닐때...(마스터 클라이언트가 아닐때)
    if(!PhotonNetwork.isMasterClient)
    {
        //원격 네트워크 유저의 아바타는 물리력을 안받게 처리하고
        //또한, 물리엔진으로 이동 처리하지 않고(Rigidbody로 이동 처리시...)
        //실시간 위치값을 전송받아 처리 한다 그러므로 Rigidbody 컴포넌트의
        //isKinematic 옵션을 체크해주자. 한마디로 물리엔진의 영향에서 벗어나게 하여
        //불필요한 물리연산을 하지 않게 해주자...

        //원격 네트워크 플레이어의 아바타는 물리력을 이용하지 않음 (마스터 클라이언트가 아닐때)
        //(원래 게임이 이렇다는거다...우리건 안해도 체크 돼있음...)
        myRbody.isKinematic = true;
        //네비게이션도 중지
        //myTraceAgent.isStopped = true; 이걸로 하면 off Mesh Link 에서 에러 발생 그냥 비활성 하자
    }
        // 원격 플래이어의 위치 및 회전 값을 처리할 변수의 초기값 설정 
        // 잘 생각해보자 이런처리 안하면 순간이동 현상을 목격
        currPos = myTr.position;
        currScale = myTr.localScale;
/////////////////////////////////////////////////////////////////

    }

    void Start()
    {
//@11-1 
if(PhotonNetwork.isMasterClient)
{
        attackDelay = Random.Range(minAttackDelay, maxAttackDelay); //각 몬스터당 랜덤한 공격 딜레이타임 가짐
        
        damagePower = 30f;
}
        playerLife = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerLife>();

    }

    void Update()
    {
//포톤 추가
// 마스터 클라이언트가 직접 Ai 및 애니메이션 로직 수행
// pv.isMine 해도 됨
if (pv.isMine)  //(PhotonNetwork.isMasterClient)
{
        if(HP == 0)     //체력 다 닳면 죽음
            dead = true;

        if(dead && !deadCheck)
        {
            StartCoroutine(Die());
            deadCheck = true;
            anim.SetBool("Die", true);

            pv.RPC("ModeAction", PhotonTargets.Others, 0); //@11-1 포톤 아바타 애니
        }
}
else
{
            //원격 플레이어의 아바타를 수신받은 위치까지 부드럽게 이동시키자
            myTr.position = Vector3.Lerp(myTr.position, currPos, Time.deltaTime * 3.0f);
            myTr.localScale = Vector3.Lerp(myTr.localScale, currScale, Time.deltaTime * 3.0f);
}
        
    }

    void OnCollisionEnter2D(Collision2D other) 
    {
        if((other.gameObject.tag == "Player") )   //공격 상태일 땐, Flip하지 않도록.
        {
            playerLife.GetHurt(transform, damagePower);
        }
    }

    IEnumerator Die()
    {
    scoreMgr.GetScore(300);
    
    
        // if(enemyType == ENEMY_TYPE.ENEMY1)
            
        // else if(enemyType == ENEMY_TYPE.ENEMY2)
            
        // else if(enemyType == ENEMY_TYPE.ENEMY3)     
            

        collider2d.enabled = false;                 //죽으면 시체와 콜라이더 충돌 일어나지 않도록
        yield return new WaitForSeconds(deadTime);  //deadTime 후에 사라져서 죽도록
//        Destroy(gameObject);
        //@11-1 그냥 Destroy 대신 
        PhotonNetwork.Destroy(gameObject);
    }

    void UpdateLifeBar()
    {
        //HP가 줄어들수록 초록 -> 빨강
        lifeBar.material.color = Color.Lerp(Color.green, Color.red, 1 - HP * 0.01f);    
        //HP가 줄어들수록 HP 스케일도 감소
        lifeBar.transform.localScale = new Vector3(lifeBarScale.x * HP * 0.01f, 1, 1);
    }

    public void GetHurt(int hurtPower)
    {
        HP -= hurtPower;        //폭탄은 50, 플레이어가 쏘는 총알은 10씩 데미지
        UpdateLifeBar();
    }


    [PunRPC]
    void ModeAction(int num)
    {
        while(!dead)
        {
            switch(num)
            {
                case 0 : 
                    anim.SetBool("Die", true);
                    break;
                case 1 : 
                    anim.SetBool("Attack", true);
                    break;
                case 2 : 
                    anim.SetBool("Attack", false);
                    break;
            }
        }
    }

    /*
     * PhotonView 컴포넌트의 Observe 속성이 스크립트 컴포넌트로 지정되면 PhotonView
     * 컴포넌트는 데이터를 송수신할 때, 해당 스크립트의 OnPhotonSerializeView 콜백 함수를 호출한다. 
     */
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //로컬 플레이어의 위치 정보를 송신
        if (stream.isWriting)
        {
            //박싱
            stream.SendNext(myTr.position);
            stream.SendNext(myTr.localScale);
            // stream.SendNext(myTr.rotation); 
            // stream.SendNext(net_Anim);
        }
        //원격 플레이어의 위치 정보를 수신
        else
        {
            //언박싱
            currPos = (Vector3)stream.ReceiveNext();
            currScale = (Vector3)stream.ReceiveNext();
            // currRot = (Quaternion)stream.ReceiveNext();
            // net_Anim = (int)stream.ReceiveNext();
        }

    }

    // 마스터 클라이언트가 변경되면 호출
    void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        if (PhotonNetwork.isMasterClient)
        {

            attackDelay = Random.Range(minAttackDelay, maxAttackDelay); //각 몬스터당 랜덤한 공격 딜레이타임 가짐
        
            damagePower = 30f;

            // if(enemyType == ENEMY_TYPE.ENEMY1)
            //     damagePower = 15f;
            // if(enemyType == ENEMY_TYPE.ENEMY2)
            //     damagePower = 90f;
        }
    }



}