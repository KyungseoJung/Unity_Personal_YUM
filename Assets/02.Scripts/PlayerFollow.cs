using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    //@6-5 ==================================
    //그냥 실행하면, 캐릭터의 배꼽 위치에 와 있음.
    //거리 약간 조정해서 머리 위로 올라오도록 거리 재서 > offset을 적어주자
    public Vector3 offset;  // 플레이어를 따라다니는 라이프 바의 offset

    private Transform player;   // player을 위한 레퍼런스
    //로컬  플레이어 연결 레퍼런스
    public PlayerCtrl localPlayer;

//@16-3 ===================================
    private bool catchPlayerMini = false;
    private Vector2 playerMiniPos;           //@16-3 일단 테스트용으로 public
    private RectTransform playerMiniRectT;   //@16-3 일단 테스트용으로 public


    void Awake()
    {
        // 레퍼런스(참조)을 셋팅.
        player = GameObject.FindGameObjectWithTag("Player").transform;  //@ 이전에 추가한 Player의 Tag 위치를 가져오는 것
        
        // 로컬 플레이어 연결
        localPlayer = transform.root.GetComponent<PlayerCtrl>();
        //플레이어와 분리       //#20 플레이어가 움직일 때, Base(터렛)도 같이 따라오면 안되잖아,, 가져오는 건 같이 가져오고 바로 떼어버려
        //transform.parent = null;  
        transform.SetParent(null);
    }

    void Start()
    {
        Invoke("CatchPlayerMini", 1.0f); //@16-3  // 먼저 호출돼서 null로 인식할까봐 0.5초 뒤에 잡기
    }

    void CatchPlayerMini()   //@16-3 플레이어 미니맵 오브젝트 잡기  
    {
        playerMiniRectT = GameObject.Find("PlayerMini").GetComponent<RectTransform>();
        catchPlayerMini = true;
    }
    
    void Update()
    {
        // offset셋과 함께 player의 position으로 현재 게임오브젝트의 position을 셋팅 
        transform.position = player.position+offset;    
    }

    void FixedUpdate()
    {
        if(!catchPlayerMini)    //아직 플레이어 미니맵 좌표 오브젝트 못 잡았다면 그냥 return;
            return;

        playerMiniPos = new Vector2(player.position.x / MiniMapConstants.MAP_WIDTH * MiniMapConstants.MINIMAP_WIDTH, 
                                    player.position.y / MiniMapConstants.MAP_HEIGHT * MiniMapConstants.MINIMAP_HEIGHT);

        //@16-3 
        playerMiniRectT.anchoredPosition = playerMiniPos;
    }
}
