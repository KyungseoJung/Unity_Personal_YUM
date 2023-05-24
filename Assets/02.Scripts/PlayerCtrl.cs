using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FoodSpace;    // Food 스크립트 내 enum 변수

public class PlayerCtrl : MonoBehaviour //@1-1 플레이어 컨트롤(움직임)
{
    private Animator anim;
    private Rigidbody2D myRbody;
    private FixedJoint2D fixedJoint;         //@5-1 플레이어에 있는 fixedJoint
    private Rigidbody2D ropeRig;             //@5-1 밧줄의 Rigidbody

    public enum MODE_TYPE {PLAYER1 = 1, PLAYER2 };      //@7-1
    public enum MODE_STATE {IDLE = 1, EATFOOD, EATBOMB, EATMONSTER};
    public MODE_TYPE playerType;                //@7-1
    public MODE_STATE playerState = MODE_STATE.IDLE;
    private bool dirRight = true;           //플레이어가 바라보는 방향(오른쪽 : 1, 왼쪽 : -1)

    private float moveForce = 50f;          //이동 속도
    private float maxSpeed = 5f;            //달리기 가속도. 최고 속도
    private float normalSpeed = 5f;         //밧줄 매달리기 전 원래 속도    (기존 maxSpeed)
    private float ropeSpeed = 30f;          //밧줄에 매달려있을 때 좌우 최고 속도

    public bool hurt;                       //다친 상태인지 체크 - 다친 상태라면 점프 불가능
    private float jumpTimer;
    private float jumpTimeLimit = 0.3f;
    private bool jump;                      //점프 가능한지 체크
    public float jumpForce = 70f;           //점프 가속도. 누르는 동안 더해지는 높이
    public float minJump = 100f;            //최소 점프 높이
    public float ropeJump = 500f;           //밧줄에서 점프할 때

    public float wallFallSpeed = 0.6f;            //벽에 닿으면 벽에 붙어서 느리게 떨어지도록
    public float wallJumpSpeed = 10f;
    public float wallJumpTime = 0.5f;       // 벽점프 하고 있는 시간(동안은 방향키 안 먹히도록)
    private bool grounded;                  //땅 밟았는지 체크
    private bool touchWall;                 //벽에 닿았는지 체크
    private bool jumpWall;                  //벽점프하고있는지 체크

    private bool touchRope;                 //밧줄에 닿았는지 체크
    private bool holdingRope;               //밧줄에 매달려있는지 체크


    private bool holdingObj;                //무언가를 쥐고있는지(음식, 폭탄, 몬스터 중)
    private bool isPulling;                 //끌어당기고 있는지 체크
    // private float pullTimer;
    // private float pullTimeLimit = 0.2f;     //0.2초동안은 끌어당기기 유지
    private float pullForce = 5.0f;          //끌어당기는 힘
    private float dist;                     //오브젝트와의 거리
    private float eatDist = 2f;                  //먹을 수 있는 거리

    private int eatKind;                   //쥐고 있는 음식의 종류

    public float laySpeed = 3f;            //음식 놓는 속도
    public float throwSpeed = 5f;          //폭탄 던지는 속도
    public float shootSpeed = 10f;          //총알 나가는 속도
    public Transform groundCheck;           //땅 밟았는지 체크
    public Transform frontCheck;            //벽에 닿았는지 체크

    private Transform myTr;                 //@11-1

    private SpriteRenderer balloon;                  //플레이어의 풍선
    public Rigidbody2D[] foodsRbody;
    public Rigidbody2D bombWithTimer;
    public Rigidbody2D mummyMonster;

    public Rigidbody2D bullet;
    // private Bomb bomb;

//@ 오디오 ==================================
    public AudioClip jumpClip;
    public AudioClip inhaleClip;
    public AudioClip exhaleClip;
    public AudioClip bulletClip;        //총 쏠 때 소리
    public AudioClip eatingClip;        //@21-5 뭔가를 먹을 때 나는 소리

//@10-1 포톤 연결 ============================================
    //PhotonView 컴포넌트를 할당할 레퍼런스 
    PhotonView pv = null;

    //메인 카메라가 추적할 CamPivot(플레이어) 게임오브젝트  //@11-1 인스펙터 창에서 연결해줘야 돼
    public Transform camPivot;    
    //위치 정보를 송수신할 때 사용할 변수 선언 및 초기값 설정 
    Vector3 currPos = Vector3.zero;
    Vector3 currScale = Vector3.zero;
    // Quaternion currRot = Quaternion.identity;
    //플레이어 하위의 Canvas 객체를 연결할 레퍼런스->Canvas 컴포넌트를 연결 
    // public Canvas hudCanvas;
// 카메라 연결 ==========================
    FollowCamera followCamera;

// 여기까지 @10-1 ============================================
    private ScoreManager scoreMgr;  //@21-2 점수 계산
//@21-5
    private bool playInhaleSound;  //@ Z 누르고 있을 때만 사운드 출력하도록

    void Awake()
    {
        Transform firstChild = transform.GetChild(0);   //자식 오브젝트 위치 중 0번째 자식

        anim = firstChild.GetComponent<Animator>();
        myRbody = GetComponent<Rigidbody2D>();          //@11-1 아바타의 Rbody는 kinematic으로 할 거~
        fixedJoint = GetComponent<FixedJoint2D>();

        groundCheck = firstChild.Find("groundCheck");
        frontCheck = firstChild.Find("frontCheck");
        
        balloon = firstChild.Find("balloon").GetComponent<SpriteRenderer>();    //@9-4 풍선 스프라이트 이미지 연결
//카메라 =======================================
        followCamera = Camera.main.GetComponent<FollowCamera>();

//@10-1 ========================================
        
        //PhotonView 컴포넌트 할당
        pv = GetComponent<PhotonView>();    
        pv.ObservedComponents[0] = this;    //@10-1 //#20-1 컴포넌트에 지금 이 스크립트를 직접 연결할 수도 있구나~

// 여기까지 @10-1 ========================================
// 여기부터 @11-1 ========================================
        myTr = GetComponent<Transform>();

        //데이타 전송 타입을 설정
        pv.synchronization = ViewSynchronization.UnreliableOnChange;

        if(pv.isMine)
        {
            Camera.main.GetComponent<FollowCamera>().target = camPivot; 
        }
        else
        {
            myRbody.isKinematic = true; //아바타는 Rbody 필요 없으니까. 꼭두각시처럼 움직이기만~
        }

        currPos = myTr.position;
        currScale = myTr.localScale;
        // currRot = myTr.rotation;
// 여기까지 @11-1 ========================================

        scoreMgr = GameObject.FindGameObjectWithTag("ScoreManager").GetComponent<ScoreManager>();

    }
    
    void Start()
    {
        //@9-4 플레이 타입 불러와서 저장하기
        if(InfoManager.Info.playType == 1)
        {
            // Debug.Log("플레이 타입 1 시작");
            playerType = MODE_TYPE.PLAYER1;
        }
        else
        {
            // Debug.Log("플레이 타입 2 시작");
            playerType = MODE_TYPE.PLAYER2;
        }

        fixedJoint.enabled = false;     //비활성화 하고 시작
        balloon.color = InfoManager.Info.ballonColor;   //@9-4 색 연결

        followCamera.SetTarget();   //플레이어를 위치로 잡기
    }
    void Update()
    {
if(pv.isMine)   //@11-1 포톤
{        
        //땅 밟았는지 체크
        grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1<<LayerMask.NameToLayer("Ground"))
                    || Physics2D.Linecast(transform.position, groundCheck.position, 1<<LayerMask.NameToLayer("Wall"));
        touchWall = Physics2D.Linecast(transform.position, frontCheck.position, 1<<LayerMask.NameToLayer("Wall"));
        touchRope = Physics2D.Linecast(transform.position, frontCheck.position, 1<<LayerMask.NameToLayer("Rope"));
        //점프 가속도   //한번 '스페이스바" 누르면 최소 minJump만큼은 점프하도록
        if(Input.GetButtonDown("Jump") && grounded && !hurt && !jumpWall)     
        {
            jump = true;
            myRbody.AddForce(Vector2.up * minJump);
            anim.SetTrigger("Jump");
            AudioSource.PlayClipAtPoint(jumpClip, transform.position);
        }
//@5-1 밧줄 매달리기 ===============================
        //땅에 있으면 밧줄 매달릴 수 있도록
        if(grounded && holdingRope) //땅에 떨어졌는데 && 밧줄을 잡았다고 판단되고 있다면 - 밧줄 안 잡은 걸로 상태 업데이트
            holdingRope = false;

        if(touchRope && Input.GetButtonDown("Jump")) //밧줄에서 "점프"누르면 밧줄에서 떨어지도록
        {
            fixedJoint.connectedBody = null;
            fixedJoint.enabled = false;
            myRbody.AddForce(Vector2.up * ropeJump);   //밧줄에서 떨어질 때에도 약간 점프하면서 밧줄 놓기
            // Debug.Log("밧줄점프");
        }
        
        if(holdingObj)         //음식을 쥐고 있을 때
        {
            isPulling = false;      //끌어당기지 못하도록
        }
        else if(!holdingObj)   //음식을 쥐고 있지 않을 때
        {
            if(Input.GetKey(KeyCode.Z))    //누르면, 특정 시간동안 끌어당기기   //GetKeyDown :  1번 눌렀을 때
            {   
                if(!playInhaleSound)    //@21-5
                {
                    AudioSource.PlayClipAtPoint(inhaleClip, transform.position);    //@21-5 흡입하는 소리 - 연속해서 나지 않도록
                    playInhaleSound = true;
                }

                anim.SetBool("ObjIn", true);    //빨아들이는 애니 시작 (ObjectIn의 약자)
                isPulling = true;                
            }
            else
            {
                anim.SetBool("ObjIn", false);   //빨아들이는 애니 멈추도록
                isPulling = false;

                playInhaleSound = false;    //@21-5
            }
                
        }     

        // if(isPulling)
        // {
        //     if(pullTimer < pullTimeLimit)
        //         pullTimer += Time.deltaTime;
        //     else    
        //         {
        //             isPulling = false;
        //             pullTimer = 0f;
        //         }
        // }  
//폭탄 ===============================
        if(holdingObj)         //음식을 머금고 있을 때
        {
            if(Input.GetKeyDown(KeyCode.X))   //뱉기 or 공격하면
            {
                anim.SetTrigger("ObjOut"); //anim 물건 던지는 애니 
                AudioSource.PlayClipAtPoint(exhaleClip, transform.position);

                switch(playerState)
                {
                    case MODE_STATE.EATFOOD : 
                        LayFood();
                        pv.RPC("LayFood", PhotonTargets.Others, null);      //@11-1 포톤 아바타
                        break;
                    case MODE_STATE.EATBOMB : 
                        ShootBomb();
                        pv.RPC("ShootBomb", PhotonTargets.Others, null);    //@11-1
                        break;
                    case MODE_STATE.EATMONSTER :    //if(playerType == MODE_TYPE.PLAYER2) 일 때에만 적용
                        ShootMonster();
                        pv.RPC("ShootMonster", PhotonTargets.Others, null); //@11-1 
                        break;
                }
                holdingObj = false;
                playerState = MODE_STATE.IDLE;
                
            }
        }
//@4-1 벽점프 ===============================
        if(touchWall)
        {
            anim.SetBool("WallJump", true); //벽점프하는 애니
            myRbody.velocity = new Vector2(myRbody.velocity.x, myRbody.velocity.y * wallFallSpeed); //벽에 붙어있는 효과

            if(Input.GetButtonDown("Jump"))
            {
                // Debug.Log("벽점프");
                if(dirRight)
                {
                    myRbody.velocity = new Vector2(-wallJumpSpeed, wallJumpSpeed);
                    Flip();
                    jumpWall = true;
                }
                else
                {
                    myRbody.velocity = new Vector2(wallJumpSpeed, wallJumpSpeed);
                    Flip();
                    jumpWall = true;
                }
                Invoke("WallJumpEnd", wallJumpTime);
            }
        }

        if(!touchWall && !jumpWall)
            anim.SetBool("WallJump", false);
//@6-1 레이저 쏘기 ===============================
        if(playerType == MODE_TYPE.PLAYER1)
        {
            if(Input.GetKeyDown(KeyCode.C))
            {
                ShootBullet();
                pv.RPC("ShootBullet", PhotonTargets.Others, null);
            } 
        }
}
else    //@11-1 포톤 - 아바타의 경우
{
    myTr.position = Vector3.Lerp(myTr.position, currPos, Time.deltaTime * 3.0f);
    myTr.localScale = Vector3.Lerp(myTr.localScale, currScale, Time.deltaTime * 3.0f);
}        

    }

    void WallJumpEnd()
    {
        jumpWall = false;
    }


    void FixedUpdate()
    {  
if(pv.isMine)   //@11-1 포톤 - 로컬의 경우
{      
        if(jumpWall)
            return;
        // if(holdingRope && (maxSpeed == normalSpeed)) //줄에 매달려있을 땐, 더 큰 힘을 내도록
        //     maxSpeed = ropeSpeed;
        // else if(!holdingRope && (maxSpeed == ropeSpeed))    //줄 매달려있지 않다면, 원래 속도로 돌아오도록
        //     maxSpeed = normalSpeed;

    //달리기 가속도 ===============================
        float h = Input.GetAxis("Horizontal");
        anim.SetFloat("Speed", Mathf.Abs(h));

        if(h*myRbody.velocity.x < maxSpeed) //최고 속도 도달하기 전이면, 속도 계속 증가
            myRbody.AddForce(Vector2.right * h * moveForce);

        if(Mathf.Abs(myRbody.velocity.x) > maxSpeed)
            myRbody.velocity = new Vector2(Mathf.Sign(myRbody.velocity.x) * maxSpeed, myRbody.velocity.y);
    // 바라보는 방향 ===============================
        if(!touchWall)  //벽점프하고있으면 방향키 안되도록
        {
            if(h>0 && !dirRight)
            {
                Flip(); Debug.Log(1);
            }
            if(h<0 && dirRight)
            {
                Flip(); Debug.Log(2);
            }
        }
    //점프 가속도 ===============================
        if(jump)
        {
            myRbody.AddForce(Vector2.up * jumpForce);

            jumpTimer += Time.deltaTime;

            //점프 가속도 최대값
            if(!Input.GetButton("Jump") || jumpTimer > jumpTimeLimit)
            {
                jump = false;
                jumpTimer = 0f;
            }

        }
}
else    //@11-1 포톤 - 아바타의 경우
{
    myTr.position = Vector3.Lerp(myTr.position, currPos, Time.deltaTime * 3.0f);
    myTr.localScale = Vector3.Lerp(myTr.localScale, currScale, Time.deltaTime * 3.0f);
}
    }

    //OnTrigerStay2D 함수는 충돌 지속 중에 어떤 동작을 수행해야 하는 경우에 사용하고, 
    //OnTriggerEnter2D 함수는 충돌이 시작될 때 한 번만 실행되어야 하는 동작을 수행하는 경우에 사용합니다.
    void OnTriggerStay2D(Collider2D col)  //@1-2 박스콜라이더 범위 내에 물체가 있는지 확인(폭탄, 몬스터, 음식)
    {
// if(!pv.isMine)  //@21-4
//     return;

        if(isPulling)    //Ctrl키(->Z키) 계속 누르고 있어
        {
            if(playerType == MODE_TYPE.PLAYER1)
            {
                if((col.gameObject.layer == 10))    //주위에 "EatObject"가 있다면 끌어당기기
                {
                    // Debug.Log("끌어당긴다");
                    PullObject(col);
                }
            }
            
            if(playerType == MODE_TYPE.PLAYER2)
            {
                if((col.gameObject.layer == 10) ||(col.gameObject.layer == 13))    //주위에 "EatObject" 또는 Enemy가 있다면 끌어당기기
                {
                    // Debug.Log("끌어당긴다");
                    PullObject(col);
                }
            }


        }
        //
    }

    private void OnTriggerEnter2D(Collider2D col) 
    {
// if(!pv.isMine)  //@21-4
//     return;

        //커다란 박스 콜라이더와 충돌처리하는 것을 막기 위해 touchRope변수 체크
        if(touchRope && !holdingRope)   
        {
            AudioSource.PlayClipAtPoint(jumpClip, transform.position);
            ropeRig = col.gameObject.GetComponent<Rigidbody2D>();
            fixedJoint.enabled = true;
            fixedJoint.connectedBody = ropeRig;
            holdingRope = true;
        }
    }

    void Flip() // 플레이어 바라보는 방향 
    {
        // Debug.Log("뒤집어");
        dirRight = !dirRight;   //바라보는 방향 변경

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

//@11-1 #20-1    //포톤 추가 ////////////////////////////////// //포톤 클라우드를 위한 어트리뷰트로 함수 선언
    [PunRPC]
    void LayFood()
    {
        Vector3 layPos;
        layPos = transform.position;
        // 바라보는 방향으로 폭탄 쏘기
        if(dirRight)
        {
            layPos.x += 1f;
            //음식 생성, 속도 주기 동시에
            Rigidbody2D foodInstance = Instantiate(foodsRbody[eatKind], layPos , Quaternion.Euler(new Vector3(0,0,0))) as Rigidbody2D;
            foodInstance.velocity = new Vector2(laySpeed,0);
        }
        else
        {
            layPos.x -= 1f;
            Rigidbody2D foodInstance = Instantiate(foodsRbody[eatKind], layPos, Quaternion.Euler(new Vector3(0,0,0))) as Rigidbody2D;
            foodInstance.velocity = new Vector2(-laySpeed,0);
        }
    }

//@11-1 #20-1    //포톤 추가 ////////////////////////////////// //포톤 클라우드를 위한 어트리뷰트로 함수 선언
    [PunRPC]
    void ShootBomb()
    {
        Vector3 layPos;
        layPos = transform.position;
        // 바라보는 방향으로 폭탄 쏘기
        if(dirRight)
        {
            layPos.x += 1f;
            //폭탄 생성, 속도 주기 동시에
            Rigidbody2D bombInstance = Instantiate(bombWithTimer, layPos, Quaternion.Euler(new Vector3(0,0,0))) as Rigidbody2D;
            bombInstance.velocity = new Vector2(throwSpeed,0);
        }
        else
        {
            layPos.x -= 1f;
            Rigidbody2D bombInstance = Instantiate(bombWithTimer, layPos, Quaternion.Euler(new Vector3(0,0,0))) as Rigidbody2D;
            bombInstance.velocity = new Vector2(-throwSpeed,0);
        }
    }

//@11-1 #20-1    //포톤 추가 ////////////////////////////////// //포톤 클라우드를 위한 어트리뷰트로 함수 선언
    [PunRPC]
    void ShootMonster()
    {
        Vector3 layPos;
        layPos = transform.position;
        // 바라보는 방향으로 폭탄 쏘기
        if(dirRight)
        {
            layPos.x += 1f;
            //폭탄 생성, 속도 주기 동시에
            Rigidbody2D monInstance = Instantiate(mummyMonster, layPos, Quaternion.Euler(new Vector3(0,0,0))) as Rigidbody2D;
            monInstance.velocity = new Vector2(shootSpeed,0);
        }
        else
        {
            layPos.x -= 1f;
            Rigidbody2D monInstance = Instantiate(mummyMonster, layPos, Quaternion.Euler(new Vector3(0,0,0))) as Rigidbody2D;
            monInstance.velocity = new Vector2(-shootSpeed,0);
        }
    }
    
//@11-1 #20-1    //포톤 추가 ////////////////////////////////// //포톤 클라우드를 위한 어트리뷰트로 함수 선언
    [PunRPC]
    void ShootBullet()
    {
        AudioSource.PlayClipAtPoint(bulletClip, transform.position);
        //@6-1 레이저 쏘기 ===============================
        Vector3 layPos;
        layPos = transform.position;
        // 바라보는 방향으로 폭탄 쏘기
        if(dirRight)
        {
            layPos.x += 1f;
            //폭탄 생성, 속도 주기 동시에
            Rigidbody2D bulletInstance = Instantiate(bullet, layPos, Quaternion.Euler(new Vector3(0,0,0))) as Rigidbody2D;
            bulletInstance.velocity = new Vector2(shootSpeed,0);
        }
        else
        {
            layPos.x -= 1f;
            Rigidbody2D bulletInstance = Instantiate(bullet, layPos, Quaternion.Euler(new Vector3(0,0,0))) as Rigidbody2D;
            bulletInstance.velocity = new Vector2(-shootSpeed,0);
        }

    }

    //바라보는 방향쪽으로만 인식해야지 - 콜라이더도 뒤집을 수 있나? - 자동으로 뒤집혀
    void PullObject(Collider2D col)   //@1-2 물체 끌어당기기
    {   
        if(isPulling)
        {
            Vector2 dir = transform.position - col.transform.position;
            col.attachedRigidbody.AddForce(dir * pullForce);    //dir.normalized 안하고 하면 더 자연스러울듯
            // Debug.Log("dir : " + dir) ;

            //거리 가까워지면 먹어
            dist = (col.transform.position - transform.position).sqrMagnitude;

            if(dist < eatDist)
                EatObject(col);
        }
        
    }

    void EatObject(Collider2D col)
    {
        AudioSource.PlayClipAtPoint(eatingClip, transform.position); 

        anim.SetBool("ObjIn", false);
        anim.SetTrigger("Eat");   //빨아들이는 애니 멈추고, 먹은 상태의 애니로 넘어가(New StateMachine)

        if((!holdingObj) && (col.gameObject.tag == "Food"))         //음식을 쥐고 있는 게 없을 때 부딪히면
        {
            switch(col.gameObject.GetComponent<Food>().foodKind)    //어떤 음식을 쥐게 되었는지 확인
            {
                case FOOD_KIND.STRAWBERRY : 
                    eatKind = 0;
                    break;
                case FOOD_KIND.BREAD : 
                    eatKind = 1;
                    break;
                case FOOD_KIND.DONUT : 
                    eatKind = 2;
                    break;
                case FOOD_KIND.COOKIE : 
                    eatKind = 3;
                    break;
                default : 
                    break;
            }
            holdingObj = true;
            // Debug.Log("먹었다! 음식 종류는 : " + eatKind);
            //먹기
            Destroy(col.gameObject);

            //상태 변화
            playerState = MODE_STATE.EATFOOD;
        }

        if(!holdingObj && col.gameObject.tag == "Bomb")
        {
            // Debug.Log("먹었다");
            //먹기
            Destroy(col.gameObject);

            holdingObj = true;
            //상태 변화
            playerState = MODE_STATE.EATBOMB;
        }

        if(!holdingObj && col.gameObject.tag == "Enemy")    //if(playerType == MODE_TYPE.PLAYER2) 일 때에만 적용됨
        {
            // Debug.Log("먹었다");
            //먹기
            Destroy(col.gameObject);

            holdingObj = true;
            //상태 변화
            playerState = MODE_STATE.EATMONSTER;
            
            scoreMgr.GetScore(100);  //@21-2 점수 획득
        }
    }

    
    public void ExplosionBlow(float bombForce)
    {
        // Debug.Log("플레이어 스크립트");
        myRbody.AddForce(Vector2.up * bombForce);
        
    }

//@11-1 여기부터 끝까지
    //#20-1 // 포톤 추가    //# 메시지 주고 받을 수 있는 듯
    // 네트워크 객체 생성 완료시 자동 호출되는 함수
    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        //info.sender.TagObject = this.GameObject;
        // 네트워크 플레이어 생성시 전달 인자 확인
        object[] data = pv.instantiationData;
        // Debug.Log((int) data[0]);
    }

    //포톤 추가/////////////////////////////////////////////////////////
    /*
     * 게임을 실행하여 자신의 아바타를 이동시키고 있는 상태에서
     * 빌드한 후 실행한 게임 화면으로 보면 아바타 움직임이 끊기는 현상이
     * 나타남.  이유는 PhotonView 컴포넌트의 데이터 전송주기에 맞춰
     * 짧은 거리이지만 순간 이동하기 때문...
     * 이러한 현상을 보정하기 위해 포톤 클라우드도 유니티 빌트인 네트워크의
     * OnSerializeNetworkView 와 동일한 기능을 하는 OnPhotonSerializeView 콜백 함수를 제공!!!
     * 
     * OnPhotonSerializeView 콜백 함수의 호출 간격은 PhotonNetwork.sendRateOnSerialize 속성으로 설정 및 조회 
     * Sendrate 는 초당 패킷 전송 횟수로 기본값은 초당 10회로 설정돼 있다. 게임의 장르 또는 스피드를 고려해 
     * Sendrate 를 설정해야 하며, 네트워크 대역폭(Network Bandwidh)을 고려해 신중히 결정하자
     * 
     * // Debug.Log( PhotonNetwork.sendRateOnSerialize );
     * 
     */

    /*
     * PhotonView 컴포넌트의 Observe 속성이 스크립트 컴포넌트로 지정되면 PhotonView
     * 컴포넌트는 데이터를 송수신할 때, 해당 스크립트의 OnPhotonSerializeView 콜백 함수를 호출한다. 
     */

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) //#20-1
    {
        //로컬 플레이어의 위치 정보를 송신
        if(stream.isWriting)    //#로컬이 받는 
        {
            //박싱
            stream.SendNext(myTr.position);
            stream.SendNext(myTr.localScale);
        }
        else        //원격 플레이어의 위치 정보를 수신
        {
            //언박싱
            currPos = (Vector3)stream.ReceiveNext();
            currScale = (Vector3) stream.ReceiveNext();
            // currRot = (Quaternion)stream.ReceiveNext();
        }
    }

}
