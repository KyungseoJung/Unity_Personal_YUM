using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// using UnityEngine.EventSystems; //@18-1
using UnityEngine.Tilemaps;     //@18-1

public class Grid2D : MonoBehaviour
{

    //@18-1 맵을 몇 칸으로 나눠서 그리드를 Crate 할 거냐
    private int rowGridNum=20;   //@18-2 넉넉하게 범위 조금 넘어가도 에러 발생하지 않도록(20+10)
    private int colGridNum=40;   //@18-2 넉넉하게 범위 조금 넘어가도 에러 발생하지 않도록(40+10)

    // 노드의 개수 (5 * 5 행열)
    // public int m_nodeCount = 5; //다른 스크립트 어느곳에서도 이거 안 쓰고 있어. 이 내에서 마음껏 바꾸자
    private int m_rowNodeCount = 20;  //@18-1
    private int m_colNodeCount = 40;             //(40+10)
    
    // 노드 프리팹
    private Node m_nodePrefab;      //원래는 public

    // 노드를 레퍼런스 할 수 있는 다차원 배열
    private Node[,] m_nodeArr;      //원래는 public
    // 노드가 그려질 루트
    private Transform m_root;       //원래는 public
    // 이웃 노드를 레퍼런스 할 수 있는 리스트 선언
    private List<Node> m_neighbours = new List<Node>();

//@18-1
    Vector3 tilePosition;
    public Tilemap groundTilemap;   //타일맵 오브젝트 연결해주기 - 땅 
    public Tilemap wallTilemap;     //타일맵 오브젝트 연결해주기 - 벽

    void Awake()
    {
        // 초기화 진행...
        // 노드의 루트을 지정.
        m_root = transform.Find("Root");
        // 노드 프리팹을 연결.
        m_nodePrefab = Resources.Load<Node>("Node");
        // 전달된 값으로 m_nodeCount * m_nodeCount 행열 노드 그리드를 생성

        //         CreateGrid(m_nodeCount);
        CreateGrid(m_rowNodeCount, m_colNodeCount);     //@18-1
    }   

    void Start()
    {
        TileToWallNode();   //장애물 노드 만들기 - 동적으로 Ground, Wall 타일맵 가져와서 
    }


    // 클릭했을때 클릭된 마우스 좌표의 노드를 찾는 함수를 호출.
    // public Node ClickNode()
    // {
    //     if(FindNode(Input.mousePosition) == null)
    //         Debug.Log("//@18-1 2번째");
    //     // 클릭된 오브젝트의 Node 컴포넌트를 반환, 아니면 NULL을 반환
    //     return FindNode(Input.mousePosition);
    // }

    // 행열 노드 블럭을 만드는 함수이다.(그리드)(정방 행렬/정사각형 행렬)
    // void CreateGrid(int nodeCount)
    void CreateGrid(int rowNodeCount, int colNodeCount)
    {
        // 전달된 노드 수
        // m_nodeCount = nodeCount;


        // 로직 운영 변수
        int count = 0;

        // 전달된 행열 크기에 관련된 다차원배열을 만든다.(정방 행렬/정사각형 행렬)
        // m_nodeArr = new Node[nodeCount, nodeCount];      //@18-1
        m_nodeArr = new Node[rowNodeCount, colNodeCount];   

        // 센터를 잡고...
        //float center = (float)(nodeCount * 150) / 2;

        // 모든 행,열을 돌면서 노드를 생성함. 
        // for (int row = 0; row < nodeCount; ++row)        //@18-1
        for (int row = 0; row < rowNodeCount; ++row)
        {
            // for (int col = 0; col < nodeCount; ++col)
            for (int col = 0; col < colNodeCount; ++col)
            {
                // 부모를 지정하여 노드를 생성
                Node node = Instantiate(m_nodePrefab,
                    Vector3.zero,
                     Quaternion.identity,
                     m_root);

                // 노드(Node) 다차원 배열을 돌면서 생성된 노드를 하나씩 연결 
                m_nodeArr[row, col] = node;

                // 노드의 네임을 지정
                node.name = "Node : " + count++;
                
                // 생성된 노드에 행/열 셋팅
                node.SetNode(row, col);

                // 생성된 노드의 위치를 정렬 시킨다. 열 행 순으로...
                node.transform.localPosition = new Vector3(
                         col* MiniMapConstants.MAP_WIDTH /colGridNum,   //*5
                         -row *MiniMapConstants.MAP_HEIGHT/rowGridNum,  //*5
                          0
                     );
                //@18-1 노드들 크기 좀 줄여주자 - 5배 정도 - 쌤이 주신 파일에서 5씩 거리가 있으니까, 내 맵에 적용하려면 크기를 5배 줄이고 거리를 1로 해줘야겠다.
                node.transform.localScale = new Vector3( 0.2f, 0.2f, 0.2f );
                // node.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }

    }

//@18-1 ==========================================================
    void TileToWallNode()
    {
        //@18-1 wall의 위치에 있는 노드값(FindNode(Vector3 pos))을 IsWall(Node node) 처리 해주기
        //땅 타일맵의 모든 타일 위치를 가져와서 리스트에 추가
        foreach(Vector3Int position in groundTilemap.cellBounds.allPositionsWithin)
        {
            if(groundTilemap.HasTile(position))
            {
                tilePosition = groundTilemap.CellToWorld(position) + new Vector3(0.5f, 0.5f, 0);    
                        //타일 중앙 좌표 계산을 위해서(좌측 상단보다는 중심점 가져와서 계산하는 게 좋대 : (0f, 0f, 0f)
                IsWall(FindNode(tilePosition));     //@18-1 장애물 노드로 설정
            }
        }

        //벽 타일맵의 모든 타일 위치를 가져와서 리스트에 추가
        foreach(Vector3Int position in wallTilemap.cellBounds.allPositionsWithin)
        {
            if(wallTilemap.HasTile(position))
            {
                tilePosition = wallTilemap.CellToWorld(position) + new Vector3(0.5f, 0.5f, 0);
                IsWall(FindNode(tilePosition));     //@18-1 장애물 노드로 설정
            }
        }
    }

    void IsWall(Node node)    //@18-1 Ground, Wall 위치를 가져와서 노드로 Wall 설정해주기
    {
        if(node != null)
            node.SetNodeType(NodeType.Wall);
        // if(node == null)
            // Debug.Log("범위 초과");   
    }

    // 행, 열을 전달하여 만들어진 노드 그리드의 범위를 넘는지 확인하는 함수. 즉, 범위안에 있으면 true로 체크
    private bool CheckNode(int row, int col)
    {
        // if (row < 0 || row >= m_nodeCount)   //@8-1
        if (row < 0 || row >= m_rowNodeCount)   //@8-1
            return false;

        // if (col < 0 || col >= m_nodeCount)   //@18-1
        if (col < 0 || col >= m_colNodeCount)   //@18-1
            return false;

        return true;
    }

    // 위치값을 받아 노드를 찾고, 
    // 찾은 노드의 이웃 노드를 얻어오는 함수.(여기선 안씀 X)
    public Node[] Neighbours(Vector3 position)
    {
        // 모든 그리드 안에 노드를 찾는다.
        // for (int row = 0; row < m_nodeCount; ++row)  //@18-1
        for (int row = 0; row < m_rowNodeCount; ++row)  
        {
            // for (int col = 0; col < m_nodeCount; ++col)  //@18-1
            for (int col = 0; col < m_colNodeCount; ++col)  
            {
                // 해당 Node 컴포넌트에 Contains 함수로 클릭된 좌표를 기준으로 충돌 확인
                if (m_nodeArr[row, col].Contains(position))
                {
                    // 1. 선택 노드(Node)를 Neighbours 함수의 인자로 전달.
                    // 2. 리턴된 이웃 노드(Node) 배열을 반환
                    return Neighbours(m_nodeArr[row, col]);
                }
            }
        }
        // 이웃 노드가 없다.
        return null;
    }

    // 전달된 노드의 이웃 노드를 찾아서 반환하는 함수 (리스트 => 배열로 반환)
    public Node[] Neighbours(Node node)
    {
        // 최초 모든 Node 리스트를 초기화 한다.
        m_neighbours.Clear();

        // 이중 순환문을 사용하여 이웃노드를 얻어오는 방법
        //for( int row = -1; row <=1; ++row  )
        //{
        //    for (int col = -1; col <= 1; ++col)
        //    {
        //        if (row == 0 && col == 0)
        //            continue;

        //        if (CheckNode(node.Row + row, node.Col + col))
        //        {
        //            m_neighbours.Add(m_nodeArr[node.Row + row, node.Col + col]);
        //        }
        //    }
        //}
        //return m_neighbours.ToArray();

        // 아래의 코드는 절차적인 방법으로 이웃노드를 얻어오는 방법 //@29 아래 코드 이해 되면, 아래 주석하고, 위 코드 주석풀어서 사용하면 돼

        // 해당 노드가 존재한다면 해당 노드를 리스트로 추가
        // 좌측 상단
        if (CheckNode(node.Row - 1, node.Col - 1))
            m_neighbours.Add(m_nodeArr[node.Row - 1, node.Col - 1]);
        // 상단
        if (CheckNode(node.Row - 1, node.Col))
            m_neighbours.Add(m_nodeArr[node.Row - 1, node.Col]);

        // 우측 상단
        if (CheckNode(node.Row - 1, node.Col + 1))
            m_neighbours.Add(m_nodeArr[node.Row - 1, node.Col + 1]);

        // 좌측 
        if (CheckNode(node.Row, node.Col - 1))
            m_neighbours.Add(m_nodeArr[node.Row, node.Col - 1]);


        // 우측
        if (CheckNode(node.Row, node.Col + 1))
            m_neighbours.Add(m_nodeArr[node.Row, node.Col + 1]);


        // 좌측 하단
        if (CheckNode(node.Row + 1, node.Col - 1))
            m_neighbours.Add(m_nodeArr[node.Row + 1, node.Col - 1]);

        // 하단
        if (CheckNode(node.Row + 1, node.Col))
            m_neighbours.Add(m_nodeArr[node.Row + 1, node.Col]);

        // 우측 하단
        if (CheckNode(node.Row + 1, node.Col + 1))
            m_neighbours.Add(m_nodeArr[node.Row + 1, node.Col + 1]);

        // 리스트를 배열로 리턴 (맨 아래 주석 확인)
        return m_neighbours.ToArray();
    }

    //  마우스 클릭된 위치에 해당하다는 노드를 찾아서 리턴 (5 * 5) (99라인 대신 쓰임 : 충돌 체크)
    public Node FindNode(Vector3 pos)   //@18-1 특정 위치에 row행 col열의 노드를 알려주기
    {
        int posCol = (int) (pos.x + 20f);   //좌측 상단값이 (-20,10)이니까
        int posRow = - (int) (pos.y -10f);    // 행(y축의 값)은 0~20까지 음수니까

        if((posRow >=0) && (posRow < m_rowNodeCount) && (posCol >=0) && (posCol <m_colNodeCount))
            return m_nodeArr[posRow, posCol];
        else
            return null;

/*
        node.transform.localPosition = new Vector3(
            col* MiniMapConstants.MAP_WIDTH /colGridNum,   //*5
            -row *MiniMapConstants.MAP_HEIGHT/rowGridNum,  //*5
            0
        );
*/




        // // for (int row = 0; row < m_nodeCount; ++row) //@18-1
        // for (int row = 0; row < m_rowNodeCount; ++row) 
        // {
        //     // for (int col = 0; col < m_nodeCount; ++col) //@18-1
        //     for (int col = 0; col < m_colNodeCount; ++col) 
        //     {
        //         // 순서대로 모든 행/열의 RectTransform을 찾음
        //         RectTransform r = m_nodeArr[row, col].GetComponent<RectTransform>();

        //         /*
        //          RectangleContainsScreenPoint : 포인터가 특정 사각형(범위)안에 위치하는 지 체크
        //          즉, 스크린상의 특정 포인트가 지정된 사각 범위 꼭지점 안에 포함되는지 결과를 리턴.(TRUE이면 포인트가 그 영역내)
        //          Canvas 오브젝트에 주로 사용할 수 있는 함수이다.
        //          RectTransform 컴포넌트와 관련된 함수이므로, 이 함수를 사용하기 위해서는 오브젝트에 RectTransform 컴포넌트가
        //          Attach 되어 있어야 한다.
        //         */
        //         if (RectTransformUtility.
        //              RectangleContainsScreenPoint(r, pos))
        //         {
        //             // 결과는 배열이 가리키는 컴포넌트의 이름과 1차원적(메모리형태) 구조상의 인덱스이다.
        //             Debug.Log("클릭 + " + m_nodeArr[row, col]);

        //             if(m_nodeArr[row, col] == null)
        //                 Debug.Log("//@18-1 3번째");
        //             else
        //                 Debug.Log("//@18-1 3번째 row : " + row + "// col : " + col);

        //             return m_nodeArr[row, col];
        //         }
        //     }
        // }
        // return null;
    }


// 마우스 클릭된 위치에 해당하다는 노드를 찾아서 리턴 (5 * 5) (99라인 대신 쓰임 : 충돌 체크)
// public Node FindNode(Vector3 pos)
// {
//     // 레이캐스트를 위한 이벤트 데이터 생성
//     PointerEventData eventData = new PointerEventData(EventSystem.current);
//     // 이벤트 데이터에 클릭 위치 할당
//     eventData.position = pos;

//     // 레이캐스트 결과를 저장할 리스트 생성
//     List<RaycastResult> results = new List<RaycastResult>();

//     // 레이캐스트 수행
//     EventSystem.current.RaycastAll(eventData, results);

//         Debug.Log("//@18-1 if문 바깥1");

//     // 레이캐스트 결과가 있는 경우
//     if (results.Count > 0)
//     {
//         Debug.Log("//@18-1 if문 안에");

//         // 첫번째 결과를 가져옴
//         RaycastResult result = results[0];
//         // 결과로부터 클릭된 노드 가져오기
//         Node node = result.gameObject.GetComponent<Node>();
//         // 노드가 있는 경우 리턴
//         if (node != null)
//         {
//             Debug.Log("클릭 + " + node);
//             return node;
//         }
//     }
//         Debug.Log("//@18-1 if문 바깥2");

//     return null;
// }



    // 모든 그리드 안에 노드를 리셋하는 함수
    public void ResetNode()
    {
        // for (int row = 0; row < m_nodeCount; ++row) //@18-1
        for (int row = 0; row < m_rowNodeCount; ++row)
        {
            // for (int col = 0; col < m_nodeCount; ++col) //@8-1
            for (int col = 0; col < m_colNodeCount; ++col)
            {
                   // 모든 노드를 리셋
                   m_nodeArr[row, col].Reset();  
            }
        }
    }
}


/*
    List에서 Array로 복사하기/ Array를 List로 복사하기

    internal class Test
    {
        public string name { get; set; }
        public double weight { get; set; }
    }

    1) List를 Array로 복사
        List<Test> testList = new List<Test>();

        Test test = new Test();
        test.name = "aaa";
        test.weight = 50.3;
        testList.Add(test);

        test.name = "bbb";
        test.weight = 77.7;
        testList.Add(test);

        test.name = "ccc";
        test.weight = 65.3;
        testList.Add(test);

        Test[] testArray = testList.ToArray();

    2) Array를 List로 복사
        Test[] testArray = new Test[3];

        Test test = new Test();
        test.name = "aaa";
        test.weight = 50.3;
        testArray[0] = test;

        test.name = "bbb";
        test.weight = 77.7;
        testArray[1] = test;

        test.name = "ccc";
        test.weight = 65.3;
        testArray[2] = test;

        List<Test> testList = testArray.ToList();

*/
