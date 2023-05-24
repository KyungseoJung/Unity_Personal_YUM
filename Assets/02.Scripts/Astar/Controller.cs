using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 마우스 왼쪽 버튼을 클릭 : 노드의 시점으로 설정
// 마우스 오른쪽 버튼을 클릭 : 노드의 종점으로 설정(그냥 바로가게 변경했다.)
// Space키 단계별로 실행
// 좀 수정했음 Space 빼고, W 장애물 설치, R 리셋 

// 재귀 함수의 문제 : 호출 횟수 제한이 존재한다. 각 언어마다 존재하는 제한이 다르다. 
public class Controller : MonoBehaviour
{
    // 플레이어와 타겟의 레퍼런스 설정(부모가 BaseChar 이기 때문에 다 참조 가능)
    public BaseChar m_player;   //테스트용 public
    public BaseChar m_target;   //위와 동일

    // Grid2D 레퍼런스 
    Grid2D m_grid2D;

    // PathFinding 레퍼런스
    PathFinding pathfinding;

//@18-1 
    // private Transform player;   // player을 위한 레퍼런스
//@18-2 에러 방지용 - 이전과 같은 노드이면, 추적 시작 안 해
    private Node prevControllNode;
    private Node currControllNode;
//@18-3 에러 방지용 - 그냥 Rotation 지점을 빠르게 다니는 Enemy를 만들자
    private Transform[] EnemyRotationPoints;
    private Vector3 targetPos;
    private int targetPosNum = 1;

    // 각 레퍼런스 연결
    private void Awake()
    {
        m_grid2D = GameObject.FindObjectOfType<Grid2D>();
        pathfinding = GameObject.FindObjectOfType<PathFinding>();

        m_player = GameObject.FindObjectOfType<AstarPlayer>();
        m_target = GameObject.FindObjectOfType<AstarTarget>();
    }

    void Start()
    {
//@18-2        // player = GameObject.FindGameObjectWithTag("Player").transform;  //@ 이전에 추가한 Player의 Tag 위치를 가져오는 것
            //혹시나 이게 먼저 탈까봐 -> Start에서 찾자
//@18-3
        EnemyRotationPoints = GameObject.Find("EnemyRotationPoint").GetComponentsInChildren<Transform>();


    // // 클릭된 그리드 안에 노드를 Node 레퍼런스로 반환   //@26 쓴대
    // Node RayCast()
    // {
    //     if(m_grid2D.ClickNode() == null)
    //         Debug.Log("//@18-1 1번째");

    //     return m_grid2D.ClickNode();
    // }

        InvokeRepeating("ReadyTrace", 1.0f, 10.0f);    //@18-1 10초마다 한번씩 경로 탐색. 플레이어 찾기



    }

    void ReadyTrace()
    {
//@18-3 ========================================
        if(targetPosNum >= EnemyRotationPoints.Length)
            targetPosNum = 1;

        targetPos = EnemyRotationPoints[targetPosNum].position;
        targetPosNum ++;

//@18-2 ========================================
        //currControllNode = m_grid2D.FindNode(player.transform.position);

        // pathfinding.ResetNode();                     //내가 추가한 거. 여기서 Reset하면, closedList 생기기 전부터 없애는 거여서 에러 발생함
        // if(currControllNode == prevControllNode)        // 이전값과 위치 같으면 에러 발생하므로 return  //근데 이게 문제가 아닌 것 같다.. 그냥 매번 Reset해주자
        // {
        //     Debug.Log("//@18-2 이전 타겟 노드와 같으므로 예외처리 리턴");
        //     return; 

        // }

        TraceAstarTarget();
    }

    void TraceAstarTarget()
    {
        //  // 마우스 왼쪽 클릭시
        // if( Input.GetMouseButtonUp(0) )
        // {
        //     // 플레이어의 위치를 선택된 노드의 위치로 셋팅
        //     m_player.transform.position = RayCast().transform.position;            
        // }

        // // 마우스 오른쪽 클릭시
        // if( Input.GetMouseButtonUp(1) )
        // {
            // 타깃의 위치를 선택된 노드의 위치로 셋팅
            // m_target.transform.position = RayCast().transform.position;
            m_target.transform.position = targetPos;     //@18-1 게임 플레이어 따라가기
            // 그리고 바로 아래 함수의 호출로 패스를 찾는 코루틴 실행
            pathfinding.FindPathCoroutine(m_player, m_target);
//@18-2 ========================================
           // prevControllNode = m_grid2D.FindNode(player.transform.position);
            
        // pathfinding.ResetNode(); //내가 추가했던 코드

        // }   

        // // R 키 클릭시
        // if( Input.GetKeyDown(KeyCode.R))
        // {
        //     // A* 길찾기 알고리즘을 리셋하는 함수
        //     pathfinding.ResetNode();
        // }

        // // W 키 클릭시
        // if( Input.GetKey( KeyCode.W ) )
        // {
        //     // 선택된 노드를 가져와서...
        //     Node node = RayCast();

        //     // 만약 노드를 가져왔다면...
        //     if( node != null )
        //     {
        //         // 노드의 타입을 Wall 즉 장애물로...
        //         node.SetNodeType(NodeType.Wall);
        //     }
        // }
    }
}
