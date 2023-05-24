using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 재귀 함수를 사용한 길찾기의 경우 맵이 커지고 장애물이 배치되어 있는 경우 
// 재귀 함수 호출의 제한으로 인해 프로그램이 강제 종료 될 수 있다.

//IEquatable 인터페이스를 상속받은 클래스를  List 컬렉션에서 사용한다면
//List 컬렉션의 Contains함수를 호출하였을 때 IEquatable의 Equals함수가 호출된다.
//여기선 안씀 참고만~
public class PairNode : IEquatable<PairNode>
{
    // 플레이어와 타깃의 BaseChar 레퍼런스
    public BaseChar m_player;
    public BaseChar m_target;

    //public override int GetHashCode()
    //{
    //    return base.GetHashCode();
    //}


    //public override bool Equals(object obj)
    //{
    //    return base.Equals(obj);
    //}

    // 전달된 PairNode를 참조하여 현재 객체(m_orderlist)와 내용적으로 같은지를 반환. 
    // 여기선 안씀 참고만~
    public bool Equals(PairNode other)
    {
        // 만약, 전달된 객체의 m_player 또는 m_target 가 null 이거나 현재 객체(m_orderlist)의 m_player 가 null 이면 true를 반환 하고 함수 종료
        // 즉, 전달된 PairNode 객체의 멤버 m_player, m_target 가 없다면 예외 처리
        if (other.m_player == null || other.m_target == null)
            return true;    //@262 아니라고 따로 처리 해줘야 돼
        // 즉, 현재 게임오브젝트(m_orderlist)의 m_player, m_target의 셋팅 값이 없다면 예외 처리. (m_target 필요???)
        // (주의) 현재 로직에서는 이 멤버를(m_player, m_target) 초기화 하는 부분이 없어서 여기서 항상 true
        else if (m_player == null || m_target == null)
            return true;

        // 그렇지 않다면 전달된 객체의 플레이어 인스턴스ID(int형)와 현재의 플레이어 인스턴스ID(int형)를 비교하여 같다면 true를 리턴
        // 즉, 서로 달르다면 false 가 리턴되어 같지 않다의 해석이 됨.
        bool state = other.m_player.GetInstanceID() == m_player.GetInstanceID();

        return state;
    }
}

// 길 찾는 역할을 수행하는 컴포넌트.
public class PathFinding : MonoBehaviour
{
    // Grid2D 레퍼런스
    private Grid2D m_grid2D;
    // 클로즈 노드 리스트
    private List<Node> m_closedList = new List<Node>();
    // 오픈 노드 리스트
    private List<Node> m_openList = new List<Node>();

    // 노드의 소비 비용을 셋팅하기 위한 객체 생성
    private NodeComparer m_nodeComparer = new NodeComparer();

    // 아래의 변수들은 단계별로 실행하기 위해서 설정해 둔 변수.

    // 아래 내용 public 으로 빼보기   //@26 public으로 빼서 인스펙터 창으로 공부
    // 현재 노드와 타겟 노드는 같은 값일 거다. 가는 목적지가 같다
    // 현재 노드
    public Node m_currNode;
    // 시작 노드 (첨에 스타트 했던 노드)
    public Node m_startNode;
    // 타겟 노드
    public Node m_targetNode;
    // 이전 노드 (노드 패스상에 바로 전 노드)
    public Node m_prevNode;

    // 패스 노드 리스트
    private List<Node> m_pathNode;
    // 근처 노드 리스트
    private List<Node> m_currNeighbours = new List<Node>();
    // 위에서 정의한 PairNode 리스트 (실행 리스트)(여기선 안씀 X 참고만~)(알아서 만들어보기)(미션)
    List<PairNode> m_orderlist = new List<PairNode>();

    // 실행 플래그
    private bool m_execute = false;

    // 레퍼런스 연결
    private void Awake()
    {
        m_grid2D = GameObject.FindObjectOfType<Grid2D>();
    }

    // 두 노드의 거리를 측정(핵심 알고리즘 1)
    public int GetDistance( Node a, Node b )
    {
        // 절대값을 활용하여 두 노드의 열 거리
        int x = Mathf.Abs(a.Col - b.Col);
        // 절대값을 활용하여 두 노드의 행 거리
        int y = Mathf.Abs(a.Row - b.Row);

        // 비지니스 로직(최소값과 절대값을 활용) 실제로 운영해보면 대각선 -14 값 소모 좌 우로 10 소모
        // (주의)소비값은 알아서 셋팅 해도 됨, 그러나 14 < 10 이 크면 안됨
        return 14 * Mathf.Min(x, y) + 10 * Mathf.Abs(x - y);
    }

    // (현재 노드를 레퍼런스로 전달 받아)다시 패스를 추적하는 함수
    public List<Node> RetracePath( Node currNode )
    {
        // 임시 Node 리스트를 생성하고...(임시 추적 패스)
        List<Node> nodes = new List<Node>();

        // 전달된 현재 노드(현재 노드의 레퍼런스)가 null 이 아닐때까지...즉 전달된 현재 노드 부터 순차적으로 추적 패스를 생성
        while( currNode != null )
        {
            // 임시 노드에 전달된(현재 노드가 가리키는) 현재 노드를 추가
            nodes.Add(currNode);
            // 현재 레퍼런스 노드에 현재 레퍼런스가 가리키는 노드의 부모를 셋팅
            currNode = currNode.Parent;
        }

        // 임시 노드 반전 (Add 한것은 현재 노드 부터니깐 반전 시켜야 함)
        nodes.Reverse();

        // 임시 노드 전달
        return nodes;
    }

    // 0. 처음 색상 :  흰색 - 
    // 1. 현재 시점의 이웃 노드 : 파란색 - 
    // 2. 클로즈 노드 : 회색 -
    // 3. 이전 오픈 노드 리스트들( 이웃노드들 ) : 노란색 -
    // 4. 길을 찾았을 때의 경로 : 빨간색
    // 5. 현재 노드의 색상 : 녹색 -
    // 6. 타겟 노드의 색상 : 보라색

    // 각 참조를 null, 각 리스트는 clear, Grid2D 는 리셋
    public void ResetNode()
    {
        m_currNode = null;
        m_startNode = null;
        m_targetNode = null;
        m_prevNode = null;

        m_pathNode.Clear() ;
        m_currNeighbours.Clear();
        m_openList.Clear();
        m_closedList.Clear();

        m_grid2D.ResetNode();
    }

    // 노드의 초기 색상을 셋팅
    public void ResetColor()
    {
        // 전체 이웃 노드 리스트 ( 오픈 노드 리스트 ) 색상을 변경.
        foreach( var n in m_openList )
        {
            n.SetColor(Color.yellow);
        }

        // 현재 시점의 이웃 노드 색상을 변경.
        foreach( var n in m_currNeighbours )
        {
            n.SetColor(Color.blue);
        }

        // 닫힌 노드를 회색으로 설정
        foreach (var n in m_closedList)
        {
            n.SetColor(Color.grey);
        }

        // 현재 노드의 색상값을 그린색으로 변경합니다.
        if (m_prevNode != null)
            m_prevNode.SetColor(Color.green);

        // 타겟 노드의 색상을 보라색
        m_targetNode.SetColor(Color.magenta);

        // 만약에 패스 노드가 null 이 아니라면 패스를 따라서 길을 찾아야 함. 따라서 패스 노드에 빨간색 길을 만들어 주자.
        if( m_pathNode != null )
        foreach( var n in m_pathNode )
        {
            n.SetColor(Color.red);
        }
    }

    // 준비 함수
    public void Ready(Vector3 player, Vector3 target)
    {
        // 실행을 true
        m_execute = true;

        // 오픈,클로즈 노드 리스트 초기화(클리어)
        m_openList.Clear();
        m_closedList.Clear();

        // 플레이어가 위치하고 있는 노드와 
        // 타겟이 위치하고 있는 노드를 찾는다.
        m_startNode = m_grid2D.FindNode(player);
        m_targetNode = m_grid2D.FindNode(target);

        // 각 노드를 독립적인 노드로 설정
        m_targetNode.SetParent(null);
        m_startNode.SetParent(null);

        // 현재 노드를 스타트 노드로
        m_currNode = m_startNode;

        // 시작점의 노드의 초기비용을(현재 위치에서 시작위치까지의 소비된 비용) 0으로 초기화한다.
        m_startNode.SetGCost(0);
        // 시작점의 노드의 초기비용을(현재 위치에서 목적지까지의 비용) 시작 노드와 타겟 노드 사이의 거리로 설정 (알고리즘 활용)
        m_startNode.SetHCost(GetDistance(m_startNode, m_targetNode));
        
        // 색상을 리셋 하고
        ResetColor();
        // 시작 노드의 섹상을 그린으로
        m_startNode.SetColor(Color.green);
    }

    // 재귀함수와 코루틴을 사용하여 비동기로 처리해본 함수.
    public void FindPathCoroutine( BaseChar Player, BaseChar Target )
    {
        // 실행중이지 않은 경우라면 실행될 수 있도록 한다.
        if( ! m_execute )
        {
            // 준비시키고...
            Ready(Player.transform.position, Target.transform.position );
            // 실행하고...
            StartCoroutine(IEStep(Player));
        }
        // 누군가에 의해 길찾기 알고리즘이 실행되고 있는 경우라면
        // 실행 리스트에 등록 할 수 있도록 처리함.
        // 즉, 이 함수가 호출되었지만 m_execute가 실행 상태라면 추적중 다시 실행된것이므로...처리
        // 여기선 안씀 참고만~(미션) 다른 방식으로 정리 했다.// IEStep 참고
        else    //@26 재추적 코드. 나중에 이런 식으로도 해봐라~
        {
            // PairNode 노드 생성
            PairNode pairNode = new PairNode();
            // 생성된 노드의 멤버 m_player에 전달받은 플레이서 셋팅 
            pairNode.m_player = Player;
            // 생성된 노드의 멤버 m_target에 전달받은 타겟 셋팅
            pairNode.m_target = Target;
            // IEquatable의 Equals함수 가 대신 호출된다.
            // m_orderlist의 내용이 pairNode와 다를때...
            if( !m_orderlist.Contains( pairNode ) )
            {
                // pairNode를 추가
                m_orderlist.Add(pairNode);
            }
        }
    }

    // 길찾는 코루틴 함수...
    public IEnumerator IEStep( BaseChar player )
    {
        // 현재 시점의 이웃노드를 찾는다.
        Node[] neighbours = m_grid2D.Neighbours(m_currNode);

        // 1. 기준 노드의 이웃노드를 찾고, gCost값과 hCost값을 계산한다.
        // 2. 오픈 노드 리스트에 등록되어 있지 않거나,
        //    이웃 노드의 gCost값이 현재 gCost값보다 크다면 갱신처리한다.

        // 현재 이웃노드 리스트 클리어
        m_currNeighbours.Clear();
        // AddRange 이 함수는 리스트에 지정된 컬렉션(Node)의 요소를 List<T>의 끝에 추가
        /*
            List : "Kim", "Lee", "Jang", "Park"

            1. 제너릭 배열을 추가
                lst.AddRange(new List<string> { "OS", "PS", "QI" });
            결과
                "Kim", "Lee", "Jang", "Park", "OS", "PS", "QI"

            2. string 배열을 추가
                lst.AddRange(new string[] { "OS", "PS", "QI" });
            결과
                "Kim", "Lee", "Jang", "Park", "OS", "PS", "QI"
        */
        m_currNeighbours.AddRange(neighbours);  // 이웃 노드들을 리스트에 전달하고...

        // 현재 이웃 노드를 순차적으로 검색
        for (int i = 0; i < neighbours.Length; ++i)
        {
            // 1. 계산이 완료된 로드라면 다시 순회할 수 있도록 한다.
            // 2. 이동할 수 없는 노드라면 다시 순회할 수 있도록 한다.

            // 클로즈 노드 리스트에(지나갈 수 없는 길) 이웃노드가 포함되어 있다면 continue 키워드에 의해서 skip
            if (m_closedList.Contains(neighbours[i]))
                continue;

            // 벽(장애물)일경우 오픈노드 리스트에서 생략 처리
            if (neighbours[i].NType == NodeType.Wall)
                continue;

            // 현재 노드의 현재 위치에서 시작위치까지의 소비된 비용 + 이웃 노드와 현재 노드와의 소비 비용으로 초기화
            int gCost = m_currNode.GCost + GetDistance(neighbours[i], m_currNode);

            // 오픈 노드 리스트에(지나갈수 있는 길) 이웃노드가 포함되어 있지 않거나 현재 위치에서 시작위치까지의 소비된 비용이 이웃 노드보다 낮을경우...
            if (m_openList.Contains(neighbours[i]) == false ||
                gCost < neighbours[i].GCost)

            {
                // 이웃 노드에서 목표점까지의 거리값을 구함.
                int hCost = GetDistance(neighbours[i], m_targetNode);
                // 이웃 노드에 각각 소비값을 초기화
                neighbours[i].SetGCost(gCost);
                neighbours[i].SetHCost(hCost);
                // 이웃 노드에 부모를 현재 노드로 초기화
                neighbours[i].SetParent(m_currNode);

                // 만약 오픈 노드 리스트에 이웃 노드가 포함되지 않았다면 추가
                if (!m_openList.Contains(neighbours[i]))
                    m_openList.Add(neighbours[i]);
            }
        }

        // 기준 노드의 연산 처리가 끝났으므로, 클로즈 리스트에 추가한다.
        m_closedList.Add(m_currNode);

        // 기준 노드가 오픈 노드 리스트에 담겨져 있다면 삭제 한다.
        if (m_openList.Contains(m_currNode))
            m_openList.Remove(m_currNode);

        // 오픈 노드 리스트에서 최소 비용을 갖는 노드를 찾아
        // 현재 노드로 설정한다.
        if (m_openList.Count > 0)
        {
            // 정렬
            m_openList.Sort(m_nodeComparer);
            // 현재 노드가 NULL이 아닐때...
            if (m_currNode != null)
            {
                // 이전 노드에 현재 노드를 연결하고....
                m_prevNode = m_currNode;
            }
            // 현재 노드에 오픈 노드 리스트에 가장 첫 번째 노드를 연결
            m_currNode = m_openList[0];
        }

        // 색상을 리셋
        ResetColor();

        // 코루틴 종료 하지만 아래는 1번 실행 된다. 함수는 완료가 되야 하므로...
        yield return null;

        // 노드를 찾았으므로 
        // 종료 처리한다.
        if (m_currNode == m_targetNode) //@26 도착했을 때
        {
            // 현재 노드를 기준으로 추적 패스를 다시 설정
            List<Node> nodes = RetracePath(m_currNode);
            // 맴버 변수에 패스 설정
            m_pathNode = nodes;

            // 찾음 띄워주고(필요한것만)
            Debug.Log("찾음.!");

            // 색상 리셋
            ResetColor();

            // 에이스타 알고리즘이 다시 실행될 수 있도록 
            // 플래그 값을 변경한다.
            m_execute = false;

            // 플레이어 길찾기
            player.SetPath(m_pathNode);
        }
        // 노드를 못찾았으므로 다시 코루틴
        else
        {
            // yield return new WaitForSeconds(0.3f);      //@18-1 좀 천천히 찾아오도록 
            StartCoroutine(IEStep(player));
        }
    }

    // 스페이스 키로 하나씩 움직일때 사용...(지금은 안씀 X)
    // public void Step()
    // {
    //     // 현재 시점의 이웃노드를 찾는다.
    //     Node[] neighbours = m_grid2D.Neighbours(m_currNode);

    //     // 1. 기준 노드의 이웃노드를 찾고, gCost값과 hCost값을 계산한다.
    //     // 2. 오픈 노드 리스트에 등록되어 있지 않거나,
    //     //    이웃 노드의 gCost값이 현재 gCost값보다 크다면 갱신처리한다.

    //     // 현재 이웃노드 리스트 클리어
    //     m_currNeighbours.Clear();
    //     // AddRange 이 함수는 리스트에 지정된 컬렉션(Node)의 요소를 List<T>의 끝에 추가
    //     m_currNeighbours.AddRange(neighbours);

    //     // 현재 이웃 노드를 순차적으로 검색
    //     for (int i = 0; i < neighbours.Length; ++i)
    //     {
    //         // 1. 계산이 완료된 로드라면 다시 순회할 수 있도록 한다.
    //         // 2. 이동할 수 없는 노드라면 다시 순회할 수 있도록 한다.

    //         // 클로즈 노드 리스트에 이웃노드가 포함되어 있다면 continue 키워드에 의해서 skip
    //         if (m_closedList.Contains(neighbours[i]))
    //             continue;

    //         // 벽(장애물)일경우 오픈노드 리스트에서 생략 처리
    //         if (neighbours[i].NType == NodeType.Wall)
    //             continue;

    //         // 현재 노드의 현재 위치에서 시작위치까지의 소비된 비용 + 이웃 노드와 현재 노드와의 소비 비용으로 초기화
    //         int gCost = m_currNode.GCost + GetDistance(neighbours[i], m_currNode);

    //         // 오픈 노드 리스트에 이웃노드가 포함되어 있지 않거나 현재 위치에서 시작위치까지의 소비된 비용이 이웃 노드보다 낮을경우...
    //         if (m_openList.Contains(neighbours[i]) == false ||
    //             gCost < neighbours[i].GCost)

    //         {
    //             // 이웃 노드에서 목표점까지의 거리값을 구함.
    //             int hCost = GetDistance(neighbours[i], m_targetNode);
    //             // 이웃 노드에 각각 소비값을 초기화
    //             neighbours[i].SetGCost(gCost);
    //             neighbours[i].SetHCost(hCost);
    //             // 이웃 노드에 부모를 현재 노드로 초기화
    //             neighbours[i].SetParent(m_currNode);

    //             // 만약 오픈 노드 리스트에 이웃 노드가 포함되지 않았다면 추가
    //             if (!m_openList.Contains(neighbours[i]))
    //                 m_openList.Add(neighbours[i]);
    //         }
    //     }

    //     // 기준 노드의 연산 처리가 끝났으므로, 클로즈 리스트에 추가한다.
    //     m_closedList.Add(m_currNode);

    //     // 기준 노드가 오픈 노드 리스트에 담겨져 있다면 삭제 한다.
    //     if (m_openList.Contains(m_currNode))
    //         m_openList.Remove(m_currNode);

    //     // 오픈 노드 리스트에서 최소 비용을 갖는 노드를 찾아
    //     // 현재 노드로 설정한다.
    //     if (m_openList.Count > 0)
    //     {
    //         // 정렬
    //         m_openList.Sort(m_nodeComparer);
    //         // 현재 노드가 NULL이 아닐때...
    //         if (m_currNode != null)
    //         {
    //             // 이전 노드에 현재 노드를 연결하고....
    //             m_prevNode = m_currNode;
    //         }
    //         // 현재 노드에 오픈 노드 리스트에 가장 첫 번째 노드를 연결
    //         m_currNode = m_openList[0];
    //     }

    //     // 노드를 찾았으므로 
    //     // 종료 처리한다.
    //     if (m_currNode == m_targetNode )
    //     {
    //         // 현재 노드를 기준으로 추적 패스를 다시 설정
    //         List<Node> nodes = RetracePath(m_currNode);
    //         // 맴버 변수에 패스 설정
    //         m_pathNode = nodes;

    //         // 찾음 띄워주고(필요한것만)
    //         Debug.Log("찾음.!");
            
    //     }

    //     // 색상 리셋
    //     ResetColor();
    // }

    // 테스트 함수(참고용)(사용 안함 X)
//     public void FindPath( Vector3 player, Vector3 target )
//     {
//         // 오픈,클로즈 노드 리스트 초기화(클리어)
//         m_openList.Clear();
//         m_closedList.Clear();

//         // 플레이어가 위치하고 있는 노드와 
//         // 타겟이 위치하고 있는 노드를 찾는다.
//         Node playerNode = m_grid2D.FindNode(player);
//         Node targetNode = m_grid2D.FindNode(target);

//         // 각 노드를 독립적인 노드로 설정
//         targetNode.SetParent(null);
//         playerNode.SetParent(null);

//         // 현재 노드를 플레이어 노드로 설정
//         Node currentNode = playerNode;

//         // 시작점의 노드의 초기비용을(현재 위치에서 시작위치까지의 소비된 비용) 0으로 초기화한다.
//         playerNode.SetGCost(0);
//         // 시작점의 노드의 초기비용을(현재 위치에서 목적지까지의 비용) 플레이어 노드와 타겟 노드 사이의 거리로 설정
//         playerNode.SetHCost(GetDistance(playerNode, targetNode) );

//         // 오픈 노드 리스트에서 최소 비용을 갖는 노드를 찾아
//         // 현재 노드로 설정한다.
//         do
//         {
//             // 현재 시점의 이웃노드를 찾는다.
//             Node[] neighbours =  m_grid2D.Neighbours(currentNode);

//             // 1. 기준 노드의 이웃노드를 찾고, gCost값과 hCost값을 계산한다.
//             // 2. 오픈 노드 리스트에 등록되어 있지 않거나,
//             //    이웃 노드의 gCost값이 현재 gCost값보다 크다면 갱신처리한다.
            
//              // 현재 이웃 노드를 순차적으로 검색
//             for( int i = 0; i< neighbours.Length; ++ i )
//             {
//                 // 1. 계산이 완료된 로드라면 다시 순회할 수 있도록 한다.
//                 // 2. 이동할 수 없는 노드라면 다시 순회할 수 있도록 한다.

//                 // 클로즈 노드 리스트에 이웃노드가 포함되어 있다면 continue 키워드에 의해서 skip
//                 if (m_closedList.Contains(neighbours[i]))
//                     continue;

//                 // 벽(장애물)일경우 오픈노드 리스트에서 생략 처리
//                 if (neighbours[i].NType == NodeType.Wall )
//                     continue;

//                 // 현재 노드의 현재 위치에서 시작위치까지의 소비된 비용 + 이웃 노드와 현재 노드와의 소비 비용으로 초기화
//                 int gCost = currentNode.GCost + GetDistance(neighbours[i], currentNode);

//                  // 오픈 노드 리스트에 이웃노드가 포함되어 있지 않거나 현재 위치에서 시작위치까지의 소비된 비용이 이웃 노드보다 낮을경우...
//                 if( m_openList.Contains(neighbours[i]) == false  ||
//                     gCost < neighbours[i].GCost )

//                 {
                    
//                     // 이웃 노드에서 목표점까지의 거리값을 구함.
//                     int hCost = GetDistance(neighbours[i], targetNode);
//                     // 이웃 노드에 각각 소비값을 초기화
//                     neighbours[i].SetGCost(gCost);
//                     neighbours[i].SetHCost(hCost);
//                     // 이웃 노드에 부모를 현재 노드로 초기화
//                     neighbours[i].SetParent(currentNode);

//                     // 만약 오픈 노드 리스트에 이웃 노드가 포함되지 않았다면 추가
//                     if( ! m_openList.Contains( neighbours[i]))
//                         m_openList.Add(neighbours[i]);
//                 }
//             }

//             // 기준 노드의 연산 처리가 끝났으므로, 클로즈 리스트에 추가한다.
//             m_closedList.Add(currentNode);

//             // 기준 노드가 오픈 노드 리스트에 담겨져 있다면 삭제 한다.
//             if (m_openList.Contains(currentNode))
//                 m_openList.Remove(currentNode);

//             // 오픈 노드 리스트에서 최소 비용을 갖는 노드를 찾아
//             // 현재 노드로 설정한다.
//             if( m_openList.Count > 0  )
//             {
//                 // 정렬
//                 m_openList.Sort(m_nodeComparer);
//                 // 현재 노드에 오픈 노드 리스트에 가장 첫 번째 노드를 연결
//                 currentNode = m_openList[0];
//             }

//             // 노드를 찾았으므로 
//             // 종료 처리한다.
//             if (currentNode == targetNode)
//             {
//                 // 현재 노드를 기준으로 추적 패스를 다시 설정
//                 List<Node> nodes = RetracePath(currentNode);
                
//                 // 패스의 노드를 돌면서 색상을 회색으로
//                 foreach ( Node n in nodes )
//                 {
//                     n.GetComponent<Renderer>().material.color = Color.gray;
//                 }

//                 // 2초뒤에 ResetNode 호출
//                 Invoke("ResetNode", 2.0f);

//                 // 찾음 띄워주고(필요한것만)
//                 Debug.Log("찾음.!");

//                 break;
//             }


//         } while (m_openList.Count > 0);
//     }
}

/*
    ■ Invoke

    특정 함수를 일정 시간 동안 지연.
    시간은 소수점 단위의 초로 계산.
    ※ Invoke가 적용된 게임 오브젝트가 비활성화 상태일 경우 Invoke는 실행되지 않는다.
     Invoke("지연할 함수", 지연 시간);

    ■ InvokeRepeating

    Invoke 함수를 일정한 시간 간격을 두고 원하는 시간 만큼 반복 실행합니다.
    InvokeRepeating("지연할 함수", 지연 시간, 반복 시간);

    ■ CancelInvoke​

    모든 Invoke 함수를 중단시키거나, 특정 함수만 중단시킨다.
    CancelInvoke(); 모든 Invoke 함수
    CancelInvoke("제외할 함수");



*/

