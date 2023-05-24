using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;   //@11-1 방 접속 인원 수 파악 Text
using UnityEngine.SceneManagement;  //@11-1 방에서 나간 후, 씬 전환까지
public class LevelManager : MonoBehaviour
{
    //@3-4
    //스테이지
    public int stage;   //스테이지1 -> stage : 1
    private SoundManager _sMgr;

    //@3-5
    // public AudioClip soundClip; //효과음처럼 반복재생
    private float soundTime;
//@11-1 포톤
    // 포톤 추가////////////////////////////////////////////////
    //접속된 플레이어 수를 표시할 Text UI 항목 연결 레퍼런스 (Text 컴포넌트 연결 레퍼런스)
    [SerializeField]
    private Text txtConnect;
    //RPC 호출을 위한 PhotonView 연결 레퍼런스
    PhotonView pv;
    //플레어의 생성 위치 저장 레퍼런스
    private Transform[] playerPos;
    ////////////////////////////////////////////////////////////
//@11-1 Enemy 관련 포톤
    // public GameObject singleMainPlayer1;
    //스폰 장소 
    private Transform[] EnemySpawnPoints;
    // private Transform[] StarSpawnPoints;    //@21-3
    // private Transform[] BombSpawnPoints;    //@21-3
    // private Transform[] FoodSpawnPoints;    //@21-3

    // (네트워크 UI 버전에서 ...)   //#20-5 주석 처리
    //Enemy 프리팹을 위한 레퍼런스
    //public GameObject Enemy;

    //게임 끝
    private bool gameEnd;

    // 스테이지 Enemy들을 위한 레퍼런스
    private GameObject[] Enemys;

//@21-1 승패 관련 설정 
    [SerializeField]
    private int goalNum;    //동적 수행    
    [SerializeField]
    private int GoalArrived =0;    // 현재 골 지점 도착 수
    public bool gameProceeding;         // 딱 한 번만 타도록(게임 시작/ 게임 끝)    //확인용 public
    private PlayerLife playerLife;
    private ScoreManager scoreManager;


    void Awake()
    {
        //@11-1     //#20-5     // 포톤 추가////////////////////////////////////////////////
        txtConnect = GameObject.Find("txtConnect").GetComponent<Text>();
        //PhotonView 컴포넌트를 레퍼런스에 할당
        pv = GetComponent<PhotonView>();
        playerPos = GameObject.Find("PlayerSpawnPoint").GetComponentsInChildren<Transform>();
        //플레이어를 생성하는 함수 호출
        StartCoroutine(this.CreatePlayer());
        
        //포톤 클라우드로부터 네트워크 메시지 수신을 다시 연결
        PhotonNetwork.isMessageQueueRunning = true;

        //룸에 입장한 후 기존 접속자 정보를 출력
        GetConnectPlayerCount();
        ////////////////////////////////////////////////////////////

        //스폰 위치 얻기
        EnemySpawnPoints = GameObject.Find("EnemySpawnPoint").GetComponentsInChildren<Transform>();

        // StarSpawnPoints = GameObject.Find("StarSpawnPoint").GetComponentsInChildren<Transform>();   //@21-3 스타 스폰 위치 얻기
        // BombSpawnPoints = GameObject.Find("BombSpawnPoint").GetComponentsInChildren<Transform>();   //@21-3 폭탄 스폰 위치 얻기
        // FoodSpawnPoints = GameObject.Find("FoodSpawnPoint").GetComponentsInChildren<Transform>();   //@21-3 음식 스폰 위치 얻기

        //포톤 추가 - if문만
        if(PhotonNetwork.connected && PhotonNetwork.isMasterClient)
        {
            // 몬스터 스폰 코루틴 호출
            StartCoroutine(this.CreateEnemy());
            //Enemy3은 너무 난이도 높으니까 1마리만
            // PhotonNetwork.InstantiateSceneObject("MainEnemy3", Vector3.zero, Quaternion.identity, 0, null);

            //@21-3 별, 폭탄, 음식들 동적으로 생성
            // StartCoroutine(this.CreateBomb());

        }
        Invoke("ConnectPlayerLife", 2.0f);

        //@21-2 승패 관련
        scoreManager = GameObject.FindGameObjectWithTag("ScoreManager").GetComponent<ScoreManager>();
    }

    void ConnectPlayerLife()
    {
        playerLife = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerLife>();
    }
    
    void Start()
    {
        //SoundManger 스크립트에서 가져와서 쓰겠다~
        _sMgr = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        //원래는 따로 GetComponent 빼줘야 하는데, 가독성을 위해 이렇게 적은 것
        _sMgr.PlayBackground(stage);

//@21-1 골 지점 개수 동적 정의
        goalNum = GameObject.FindObjectsOfType<Goal>().Length;

        gameProceeding = true;
    }

    // void Update()
    // {
        // if(Time.time > soundTime)
        // {
        //     //3.5초마다 번개사운드 연출
        //     LightningSound();   
        //     //굳이 코루틴 함수로 호출하는 이유 : 생성과 파괴에 사용하면 좋아서

        //     soundTime = Time.time + 3.5f;
        // }
    // }

    // void LightningSound()
    // {
    //     StartCoroutine(this.PlayEffectSound(soundClip));
    // }

    IEnumerator PlayEffectSound(AudioClip _clip)
    {
        _sMgr.PlayEffect(transform.position, _clip);
        yield return null;
    }

//@21-1 승패 조건
    public void UpdateGoalState()
    {
        GoalArrived += 1;
    }
    public void CheckWinOrLose()
    {
        if(gameProceeding && (GoalArrived >= goalNum))  //승리했을 때
        {
            gameProceeding = false;
            // _sMgr.isWin = true;

            // SceneManager.LoadScene("scGameEnd");   
            // _sMgr.ConnetWinOrLoseText(); 
            
            // _sMgr.PlayGameEndSound(true);

            _sMgr.PlayBackground(2);    //@21-2

            scoreManager.ConnectWinOrLoseText(true);    
            scoreManager.SaveRecordData();  //@21-2 데이터 저장

            Invoke("GameEndLeaveRoom", 5.0f);
            
        }

        if(gameProceeding && (playerLife.HP <= 0))  //패배했을 때
        {
            gameProceeding = false;
            // _sMgr.isWin = false;

            // SceneManager.LoadScene("scGameEnd");
            // _sMgr.ConnetWinOrLoseText(); 

            // _sMgr.PlayGameEndSound(false);

            _sMgr.PlayBackground(3);    //@21-2

            scoreManager.ConnectWinOrLoseText(false);
            scoreManager.SaveRecordData();  //@21-2 데이터 저장

            Invoke("GameEndLeaveRoom", 5.0f);

        }
    }

    void GameEndLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();	//@21-1
        _sMgr.GoBackHome(); //@21-2
    }



//@11-1 여기부터 끝까지 쭉 포톤
//#12-7 추가 : Enemy Spawn 하기 ======================================
    IEnumerator CreateEnemy()
    {
        //게임중 일정 시간마다 계속 호출됨 
        while (!gameEnd)
        {
            //리스폰 타임 5초
            yield return new WaitForSeconds(5.0f);

            // 스테이지 총 몬스터 객수 제한을 위하여 찾자~
            Enemys = GameObject.FindGameObjectsWithTag("Enemy");

            //스테이지 총 몬스터 개수 제한
            if(Enemys.Length<3)    //#몬스터 6개만 나와있도록   //@22-1 6->3으로 줄임
            {
                //만든 SpawnPoint에서 동시에 한마리씩 나오도록
                //루트 생성위치는 필요하지 않다.그래서 1 번째 인덱스부터 돌리자
                for (int i = 1; i< EnemySpawnPoints.Length; i++)
                {
            //#20-5 // (포톤 추가)
                    // 네트워크 플레이어를 Scene 에 귀속하여 생성
                    PhotonNetwork.InstantiateSceneObject("MainEnemy1", EnemySpawnPoints[i].localPosition, EnemySpawnPoints[i].localRotation, 0, null);
                    PhotonNetwork.InstantiateSceneObject("MainEnemy2", EnemySpawnPoints[i].localPosition, EnemySpawnPoints[i].localRotation, 0, null);
                }
            }
        }
    }

//#20-5    // 포톤 추가
    // 플레이어를 생성하는 함수
    IEnumerator CreatePlayer()
    {
        // 지금은 테스트를 위하여 플레이어 스폰 포인트가 2개이다 따라서 차후 접속 인원수에 맞게 스폰 포인트와
        // 총 접속인원의 수를 제한

        //현재 입장한 룸 정보를 받아옴(레퍼런스 연결)
        Room currRoom = PhotonNetwork.room;

        //테스트를 위한 object 배열
        object[] ex = new object[3];
        ex[0] = 3;
        ex[1] = 4;
        ex[2] = 5;

        //float pos = Random.Range(-100.0f, 100.0f);
        //포톤네트워크를 이용한 동적 네트워크 객체는 다음과 같이 Resources 폴더 안에 애셋의 이름을 인자로 전달 해야한다. //#20 -> "MainPlayer"라는 이름을 가진 프리팹을 가져오는 거~
        //PhotonNetwork.Instantiate( "MainPlayer", new Vector3(pos, 20.0f, pos), Quaternion.identity, 0 );
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.Instantiate("MainPlayer1", playerPos[currRoom.PlayerCount].position, playerPos[currRoom.PlayerCount].rotation, 0, ex);
        }
        else
        {
            //아예 다른 씬에서 시작해보자
            // Instantiate(singleMainPlayer1, playerPos[0].position, playerPos[0].rotation);
        }

        // 기존 이름으로 변경해야 드럼통 폭파 가능(DestructionRay 스크립트 참조)
    //    player.name = "Player"; //#20 	오브젝트 이름을 다시 바꿔놔야 DestructionRay 내용이 적용이 된대

        //PhotonNetwork.InstantiateSceneObject(string prefabName, Vector3 position, Quaternion rotation, byte group, object[] data);
        //이 함수도 PhotonNetwork.Instantiate와 마찬가지로 네트워크 상에 프리팹을 동시에 생성시키지만, Master Client 만 생성 및 삭제 가능.
        //생성된 프리팹 오브젝트의 PhotonView 컴포넌트의 Owner는 Scene이 된다.

        yield return null;
    }
//@21-3 별, 폭탄, 음식 - 씬에 동적 생성
/*
    IEnumerator CreateStar()
    {
        for(int i=1; i<StarSpawnPoints.Length; i++)
        {
            PhotonNetwork.InstantiateSceneObject("Star", StarSpawnPoints[i].localPosition, StarSpawnPoints[i].localRotation, 0, null);
        }

        yield return null;
    }
    IEnumerator CreateBomb()
    {
        for(int i=1; i<BombSpawnPoints.Length; i++)
        {
            PhotonNetwork.InstantiateSceneObject("Bomb", BombSpawnPoints[i].localPosition, BombSpawnPoints[i].localRotation, 0, null);
        }

        yield return null;
    }
    IEnumerator CreateFood()
    {
        PhotonNetwork.InstantiateSceneObject("Food1", FoodSpawnPoints[1].localPosition, FoodSpawnPoints[1].localRotation, 0, null);
        PhotonNetwork.InstantiateSceneObject("Food2", FoodSpawnPoints[2].localPosition, FoodSpawnPoints[2].localRotation, 0, null);
        PhotonNetwork.InstantiateSceneObject("Food3", FoodSpawnPoints[3].localPosition, FoodSpawnPoints[3].localRotation, 0, null);
        PhotonNetwork.InstantiateSceneObject("Food4", FoodSpawnPoints[4].localPosition, FoodSpawnPoints[4].localRotation, 0, null);
        
        yield return null;
    }
*/

    //#20-5    //포톤 추가
    //룸 접속자 정보를 조회하는 함수
    void GetConnectPlayerCount()
    {
        if (!PhotonNetwork.connected)
            return;
        //현재 입장한 룸 정보를 받아옴(레퍼런스 연결)
        Room currRoom = PhotonNetwork.room;
        //현재 룸의 접속자 수와 최대 접속 가능한 수를 문자열로 구성한 다음 Text UI 항목에 출력
        txtConnect.text = currRoom.PlayerCount.ToString()
                            + "/"
                            + currRoom.MaxPlayers.ToString();
    }

//#20-5    //포톤 추가
    //네트워크 플레이어가 룸으로 접속했을 때 호출되는 콜백 함수
    //PhotonPlayer 클래스 타입의 파라미터가 전달(서버에서...)
    //PhotonPlayer 파라미터는 해당 네트워크 플레이어의 정보를 담고 있다.
    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        // 플레이어 ID (접속 순번), 이름, 커스텀 속성
        Debug.Log(newPlayer.ToStringFull());
        // 룸에 현재 접속자 정보를 display
        GetConnectPlayerCount();

        // _sMgr.GoBackHome(); //@21-5 방 튕겨서 나가져도 소리는 바뀌도록  //@21-6 이게 여기있으니까 다른 플레이어가 참가하면, 기존 플레이어가 나가지지..
    }

//#20-5    // 포톤 추가
    //네트워크 플레이어가 룸을 나가거나 접속이 끊어졌을 경우 호출되는 콜백 함수
    void OnPhotonPlayerDisconnected(PhotonPlayer outPlayer)
    {
        // 룸에 현재 접속자 정보를 display
        GetConnectPlayerCount();

         _sMgr.GoBackHome();
    }

//#20-5 // 포톤 추가    //@21-1 (포톤 연결된 상태일 때) SoundManager에서 나감 버튼 누르면 - 이게 자동으로 호출됨
    //룸에서 접속 종료됐을 때 호출되는 콜백 함수 ( (!) 과정 후 포톤이 호출 )
    void OnLeftRoom()
    {
        // StopAllCoroutines();    //@21-1
        // CancelInvoke();
        //로비로 이동
        SceneManager.LoadScene("scHome");
    }


}
