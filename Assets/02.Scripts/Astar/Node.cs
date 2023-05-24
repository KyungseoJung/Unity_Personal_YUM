using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 노드에 대한 속성 값 : "없음", "벽(장애물)".
public enum NodeType
{
    None,
    Wall,
}

public class Node : MonoBehaviour
{
    // 기본은 노드의 타입은 "없음"으로...
    private NodeType m_nodeType= NodeType.None;

    // 소비되는 총 비용
    private int m_fCost = 0;

    // 현재 위치에서 목적지까지의 비용(이동시 점점 줄어 듬)
    private int m_hCost = 0;

    // 현재 위치에서 시작위치까지의 소비된 비용(이동시 점점 늘어 남)
    private int m_gCost = 0;

    // 행,열
    private int m_row = 0;
    private int m_col = 0;

    // 부모의 노드를 기억하고 있기 위한 값.
    private Node m_parent;

    // 충돌체
    private Collider2D m_collider;

    // 비용을 표시하기 위함         //@18-1
    // private Text m_fText;
    // private Text m_hText;
    // private Text m_gText;

    // 이미지 레퍼런스
    //private Image m_image;        //@18-1
    private SpriteRenderer m_image;

    // 속성 : 행
    public  int Row
    {
        get { return m_row; }
    }

    // 속성 : 열
    public int Col
    {
        get { return m_col; }
    }

    // 속성 : 부모 노드를 리턴
    public Node Parent
    {
        get { return m_parent;  }
    }

    // 속성 : 전달된 값으로 transform 셋팅
    public Vector3 POS
    {
        set
        {
            transform.position = value;
        }
        get { return transform.position; }
    }

    // 속성 : 노드의 타입을 리턴
    public NodeType NType
    {
        get { return m_nodeType; }
    }

    // 전달된 노드 타입으로 색상 노드 타입 설정 및 색상 설정
    public void SetNodeType( NodeType nodeType )
    {
        // 노드 타입이 벽(장애물)일때...
        if (nodeType == NodeType.Wall)
        {
            // 청록색 셋팅
            SetColor(Color.cyan);
        }
        // 전달된 노드 타입 설정
        m_nodeType = nodeType;

    }

    // 리셋하면 노드 타입을 None으로, 각 텍스트를 설정, 부모는 없고, 색상은 기본 화이트
    public void Reset()
    {
        m_nodeType = NodeType.None;
        // m_fText.text = "F : ";   //@18-1
        // m_hText.text = "H : ";
        // m_gText.text = "G : ";
        m_parent = null;
        m_image.color = Color.white;
    }

    // 각 레퍼런스 연결 
    public void Awake()
    {
        m_collider = GetComponent<Collider2D>();
        // m_fText = transform.Find("F").GetComponent<Text>();
        // m_hText = transform.Find("H").GetComponent<Text>();
        // m_gText = transform.Find("G").GetComponent<Text>();
        // m_image = GetComponent<Image>();
        m_image = GetComponent<SpriteRenderer>();

    }

    void Start()
    {
        //맨 아래에 그려지도록
        m_image.sortingLayerID = 0; //@20-1 //m_image.sortingLayerName = "Background";
        m_image.sortingOrder = -1;
    }
    /*
         Collider.Bounds는 GameObject의 충돌체의 측면 또는 경계
         즉, 월드 좌표공간(world space)에서의 충돌체(collider)의 바운딩 크기(bounding volume)를 나타냄.
         콜라이더가 비활성 또는 게임 오브젝트가 비활성인 경우에는 빈 바운딩 박스가 됨.

         public bool Contains(Vector3 point);
         설정한 point가 바운딩 박스에 포함되어 있는지 확인.
    */
    // 전달된 마우스 포지션에 바운딩 박스가 포함되어 있는지 확인하는 함수 (여기선 안씀 X)
    public bool Contains( Vector3 position )
    {
        return m_collider.bounds.Contains(position);
    }

    // 노드의 색상을 지정하는 함수
    public void SetColor( Color color )
    {
        // 노드의 타입이 벽(장애물) 이면 그냥 청록색 유지
        if (m_nodeType == NodeType.Wall)
            return;

        // 이미지 레퍼런스가 NULL 이 아니라면 전달된 색상으로 셋팅
        if (m_image != null)
            m_image.color = color;

    }

    // 전달된 부모 노드로 현재 노드의 부모를 설정
    public void SetParent( Node parent )
    {
        m_parent = parent;
    }

    // 노드의 행과 열을 셋팅
    public void SetNode( int row, int col )
    {
        m_row = row;
        m_col = col;
    }

    // 속성 : 소비되는 총 비용을 반환
    public int FCost
    {
        get { return m_hCost + m_gCost; }
    }

    // 속성 : 현재 위치에서 목적지까지의 비용을 반환
    public int HCost
    {
        get { return m_hCost;  }
    }

    // 속성 : 현재 위치에서 시작위치까지의 소비된 비용을 반환
    public int GCost
    {
        get { return m_gCost; }
    }

    // 전달된 값으로 현재 위치에서 목적지까지의 비용 및 소비되는 총 비용을 해당 변수에 셋팅 및 TEXT 설정
    public void SetHCost(int cost)
    {
        // m_hText.text = "H : " + cost;                //@18-1
        m_hCost = cost;

        // m_fText.text = "F : " + (m_hCost + m_gCost); //@18-1
    }

    // 전달된 값으로 현재 위치에서 시작위치까지의 소비된 비용을 해당 변수에 셋팅 및 TEXT 설정
    public void SetGCost(int cost)
    {
        // m_gText.text = "G : " + cost;                //@18-1
        m_gCost = cost;
    }
}
