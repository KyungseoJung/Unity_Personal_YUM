using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 위 스크립트에서 EnableAttack 함수가 실행되는 시점을 Enemy마다 랜덤하게 줘서 
// 불 뿜는 모습을 자연스럽게 연출하고 싶어
public class Enemy : MonoBehaviour
{
    public enum ENEMY_TYPE {ENEMY1 = 1, ENEMY2, ENEMY3 };      //@7-3
    public ENEMY_TYPE enemyType;                //@7-3  // 몬스터 종류
    public int HP = 100;                // 몬스터 체력
    public bool dead = false;          // 죽었나 살았나
    private bool deadCheck = false;    //if문 한번만 타도록
    private float deadTime = 3.0f;      //죽고나서 Destroy 되기까지의 시간
    public bool dirRight = true;        // 쳐다보는 방향(오른쪽붵)
    public float moveSpeed = 2f;        // 몬스터의 이동 속도
    private bool touchWall;             //벽에 닿았는지 체크
    private bool touchGround;           //땅에 닿았는지 체크

    private float damagePower;          //플레이어 공격 시,  데미지 입히는 양
    private float attackTimer;
    private float minAttackDelay = 3.0f;
    private float maxAttackDelay = 7.0f;
    private float attackDelay;
    private bool attack;                      //공격 가능한지 체크
    private float attackHoldTime = 2.0f;    //공격 유지 시간

    // public AudioClip[] deathClips;      // 몬스터가 죽었을때 플레이할 수 있는 오디오 클립 배열 
//    public GameObject PointsUI;         // 몬스터가 죽었을때 발생하는 100의 프리팹 
    private Animator anim;              //
    private Rigidbody2D myRbody;    //
    private Collider2D collider2d;      //

    private Transform frontCheck;
    private GameObject weapon;           //무기
    private SpriteRenderer lifeBar;      //라이프바 (남은 HP 표시)
    private Vector3 lifeBarScale;       //HP 줄어들수록 Scale 감소

    public PlayerLife playerLife;   //@22-1 이걸 못 찾아서 플레이어 수명이 줄어들지가 않는 것 같아
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
        if(enemyType != ENEMY_TYPE.ENEMY3)
        {
            // Transform firstChild = transform.GetChild(0);   //자식 오브젝트 위치 중 0번째 자식
            Transform secondChild = transform.GetChild(1);

            frontCheck = transform.Find("frontCheck").transform;

            if(enemyType == ENEMY_TYPE.ENEMY1)
                weapon = secondChild.gameObject;

        }
        anim = GetComponent<Animator>();
        myRbody = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<Collider2D>();

		lifeBar = transform.Find("LifeBar").GetComponent<SpriteRenderer>(); //GameObject로 찾으면 다른 플레이어/ Enemy의 라이프바와 혼동된다~
        lifeBarScale = lifeBar.transform.localScale;

        Invoke("SetPlayerLife", 2.0f);  //@20-1//지금은 일단 Astar Enemy는 씬에 기속되게 해놓아서 플레이어 생기기 전에 이것부터 찾음. 그래서 에러 발생
        scoreMgr = GameObject.FindGameObjectWithTag("ScoreManager").GetComponent<ScoreManager>();

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

    void SetPlayerLife()
    {
        playerLife = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerLife>();
    }
    void Start()
    {
//@11-1 
if(PhotonNetwork.isMasterClient)
{
        if(enemyType == ENEMY_TYPE.ENEMY1)
        {
            if(weapon.activeSelf)   //Enemy1에게만 해당
                weapon.SetActive(false);
        }
        attackDelay = Random.Range(minAttackDelay, maxAttackDelay); //각 몬스터당 랜덤한 공격 딜레이타임 가짐
    
        switch(enemyType)
        {
            case ENEMY_TYPE.ENEMY1 : 
            case ENEMY_TYPE.ENEMY2 : 
                damagePower = 15f;
                break;

            case ENEMY_TYPE.ENEMY3 : 
                damagePower = 30f;
                break;
        }

}
else
{
    // 없는데 (?)
}
    }

    void Update()
    {
//포톤 추가
// 마스터 클라이언트가 직접 Ai 및 애니메이션 로직 수행
// pv.isMine 해도 됨
if (pv.isMine)  //(PhotonNetwork.isMasterClient)
{
        if(HP <= 0)     //체력 다 닳면 죽음
            dead = true;

        if(dead && !deadCheck)
        {
            StartCoroutine(Die());
            deadCheck = true;
            anim.SetBool("Die", true);

            pv.RPC("ModeAction", PhotonTargets.Others, 0); //@11-1 포톤 아바타 애니
        }

        if(!dead && enemyType == ENEMY_TYPE.ENEMY1) //불 뿜기
        {
            if(attackTimer <= attackDelay)
            {
                attackTimer += Time.deltaTime;
            }
            else if((!attack) && (attackTimer >attackDelay))
            {
                attack = true;
                anim.SetBool("Attack", true);

                pv.RPC("ModeAction", PhotonTargets.Others, 1); //@11-1 포톤 아바타 애니

                EnableAttack();
            }


            if(attack)  //공격 상태일 때, 플레이어가 가까워지면 영향 주도록
            {
                Attack();
            }
        }
}
else
{
            //원격 플레이어의 아바타를 수신받은 위치까지 부드럽게 이동시키자
            myTr.position = Vector3.Lerp(myTr.position, currPos, Time.deltaTime * 3.0f);
            myTr.localScale = Vector3.Lerp(myTr.localScale, currScale, Time.deltaTime * 3.0f);
}
        
    }

    void FixedUpdate()
    {
//@11-1 //포톤 추가
// 마스터 클라이언트가 직접 Ai 및 애니메이션 로직 수행
// pv.isMine 해도 됨
if (PhotonNetwork.isMasterClient)
{
    if(enemyType != ENEMY_TYPE.ENEMY3)
    {

        //벽이나 땅에 닿으면 Flip
        touchWall = Physics2D.Linecast(transform.position, frontCheck.position, 1<<LayerMask.NameToLayer("Wall"));
        touchGround = Physics2D.Linecast(transform.position, frontCheck.position, 1<<LayerMask.NameToLayer("Ground"));

        if(touchWall || touchGround)
        {
            Flip();
            touchWall = false;  //여러번 if문 타는 거 방지
            touchGround = false; 
        }
            
        if(!dead && !attack && enemyType != ENEMY_TYPE.ENEMY3)    //죽은 상태 or 공격상태라면 이동 안 하도록    //@18-1 Enemy_3은 A*로 움직일 거야
            myRbody.velocity = new Vector2(transform.localScale.x * moveSpeed, myRbody.velocity.y); 

    } 
}
else
{
    // 아무 동작 없음 (?)
}

    }

    void OnTriggerEnter2D(Collider2D col) 
    {
        if((enemyType != ENEMY_TYPE.ENEMY3) && (col.tag == "endPoint") )    //@18-1 enemy3에는 A* 적용해봐야지~
        {
            Flip();
        }
    }

    void OnCollisionEnter2D(Collision2D other) 
    {
        if((other.gameObject.tag == "Player") )   //공격 상태일 땐, Flip하지 않도록.
        {
            if(((enemyType == ENEMY_TYPE.ENEMY1) && (!attack))
                || (enemyType == ENEMY_TYPE.ENEMY2))
            {
                Flip();
            }
            if(enemyType == ENEMY_TYPE.ENEMY2 || enemyType == ENEMY_TYPE.ENEMY3)  //Enemy2일 땐, (//@18-1 + Enemy3) 플레이어와 부딪히기만 해도 플레이어에게 데미지 줌
            {
                if(playerLife == null)
                {
                    playerLife = other.gameObject.GetComponent<PlayerLife>(); //@22-1 못 찾는 것 같으니까 예외 처리
                    playerLife.GetHurt(transform, damagePower);
                }
                if(playerLife!=null)    //@21-2 예외처리
                {
                    playerLife.GetHurt(transform, damagePower);
                }
            }
        }
        
        
    }

    void Flip()
    {
        dirRight = !dirRight;           //바라보는 방향 변경
        Vector3 enemyScale = transform.localScale;
        enemyScale.x *= -1;
        transform.localScale = enemyScale;
    }
    IEnumerator Die()
    {
        //Enemy 죽인 후 점수 처리
        switch(enemyType)
        {
            case ENEMY_TYPE.ENEMY1 : 
                scoreMgr.GetScore(100);
                break;
            case ENEMY_TYPE.ENEMY2 : 
                scoreMgr.GetScore(200);
                break;
            case ENEMY_TYPE.ENEMY3 :    //@18-1 ENEMY3
                scoreMgr.GetScore(300);
                break;
        }
        // if(enemyType == ENEMY_TYPE.ENEMY1)
            
        // else if(enemyType == ENEMY_TYPE.ENEMY2)
            
        // else if(enemyType == ENEMY_TYPE.ENEMY3)     
            

        collider2d.enabled = false;                 //죽으면 시체와 콜라이더 충돌 일어나지 않도록
        if(enemyType == ENEMY_TYPE.ENEMY3)          //@21-3 애초에 무기 없어. 그냥 부딪히면 플레이어 HP 감소
             weapon.SetActive(false);
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
        if(enemyType != ENEMY_TYPE.ENEMY3)  //ENEMY3는 무적
        {
            HP -= hurtPower;        //폭탄은 50, 플레이어가 쏘는 총알은 10씩 데미지
            UpdateLifeBar();    
        }
    }

    void Attack()
    {
        if(playerLife ==null)    //@21-2 예외처리
        {
            playerLife = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerLife>(); //@22-1 
        }

        float distX = (playerLife.transform.position.x - transform.position.x)
                        * (playerLife.transform.position.x - transform.position.x);
        float distY = (playerLife.transform.position.y - transform.position.y)
                        * (playerLife.transform.position.y - transform.position.y); 
        bool playerPosRight = playerLife.transform.position.x > transform.position.x;   //플레이어가 오른쪽에 있으면 true
        if(dirRight)   
        {
            if(playerPosRight && (distX < 7) && (distY < 1))
            {
                playerLife.GetHurt(transform, damagePower);
            }

        }
        else
        {
            if(!playerPosRight && (distX < 7) && (distY < 1))
            {
                playerLife.GetHurt(transform, damagePower);
            }
        }
    }
    void EnableAttack()
    {
        if(!weapon.activeSelf)
        {
            weapon.SetActive(true);
            //enemy : 불 뿜는 애니
            //weapon : 불 움직이는 애니


            Invoke("DisableAttack", attackHoldTime);
        }
    }

    void DisableAttack()
    {
        if(weapon.activeSelf)
            weapon.SetActive(false);
        
        attack = false;
        anim.SetBool("Attack", false);

        pv.RPC("ModeAction", PhotonTargets.Others, 2); //@11-1 포톤 아바타 애니

        attackTimer = 0f;                           // 공격 타이머 다시 시작하도록
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
            // if(weapon.activeSelf)            //@21-6
            //     weapon.SetActive(false);
            attackDelay = Random.Range(minAttackDelay, maxAttackDelay); //각 몬스터당 랜덤한 공격 딜레이타임 가짐
        
            switch(enemyType)
            {
                case ENEMY_TYPE.ENEMY1 : 
                case ENEMY_TYPE.ENEMY2 : 
                    damagePower = 15f;
                    break;
                case ENEMY_TYPE.ENEMY3 : 
                    damagePower = 30f;
                    break;
            }

            // if(enemyType == ENEMY_TYPE.ENEMY1)
            //     damagePower = 15f;
            // if(enemyType == ENEMY_TYPE.ENEMY2)
            //     damagePower = 90f;
        }
    }

    /*

    // 다음은 마스터 클라이언트의 명시적 변경을 예를 든거다

    // 접속 플레이어 정보 추가
    List<PhotonPlayer> setPlayer;

    //네트워크 플레이어가 룸으로 접속했을 때 호출되는 콜백 함수
    //PhotonPlayer 클래스 타입의 파라미터가 전달(서버에서...)
    //PhotonPlayer 파라미터는 해당 네트워크 플레이어의 정보를 담고 있다.
    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        // 플레이어 ID (접속 순번), 이름, 커스텀 속성
        Debug.Log(newPlayer.ToStringFull());

        if(PhotonNetwork.isMasterClient)
        {
            setPlayer.Add(newPlayer);
        }

    }

    // 마스터 클라이언트만 아래 함수 호출
    // 마스터 클라이언트 명시적 변경
    void SetMasterClient()
    {
        PhotonNetwork.SetMasterClient(setPlayer[0]);
    }

    */


}