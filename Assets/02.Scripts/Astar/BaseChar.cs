using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseChar : MonoBehaviour
{
//@18-1 
private float moveSpeed = 0.01f;   // 노드(Enemy3) 따라오는 이동 속도
// private bool pathCountStart1 = false;    //@18-2 추적 끝나면, ResetNode 하기 위해서
// private bool pathCountStart2 = false;

    // 노드 리스트 선언
    private List<Node> m_path = new List<Node>();

//@18-2
    // public Transform mainEnemy3;
    PathFinding pathfinding;

    void Awake()
    {
        pathfinding = GameObject.FindObjectOfType<PathFinding>();   //@18-2 매번 ResetNode 해주기 위해
    }

    // void Start()
    // {
    //     // mainEnemy3 = GameObject.Find("MainEnemy3").GetComponent<Transform>();
    // }

    // 전달된 리스트로 맴버 리스트를 초기화 하는 함수
    public void SetPath(List<Node> path)
    {
        // 전달된 패스(리스트)가 없다면 리턴
        if (path == null)
            return;

        // 일단 맴버 패스(리스트)는 클리어
        m_path.Clear();

        // 전달된 패스의 Node를 멤버 패스(리스트)에 추가
        foreach (Node p in path)
        {
            m_path.Add(p);
        }
        
        // if(!pathCountStart1)    //@18-2 노드 다 넣은 후 체크하기 위함
        //     pathCountStart1 = true;
    }

    private void Update()
    {
        // 만약, 멤버 패스(리스트)의 데이타가 0 초과라면 즉, 데이타가 존재 한다면.
        if (m_path.Count > 0)
        {
            // 방향 벡터 설정
            // 첫 번째 리스트의 위치에서[즉, 처음부터 단계별로 이동] 내 자신의 위치를 뺀 벡턱값이 이동 방향이 된다.
            Vector3 dir = m_path[0].transform.position - transform.position;
            // 벡터 정규화
            dir.Normalize();

            // 방향으로 이동하자.
            transform.Translate(dir * 1);   //@18-1 크기 (10->)1로 변경

//@18-2
            // mainEnemy3.position = transform.position;   //@18-2 Enemy3에도 그대로 위치 적용하기
            // 목적지와 나와의 거리를 체크
            float distance = Vector3.Distance(m_path[0].transform.position, transform.position);

            // 목적지에 도착 했다면 RemoveAt으로 해당 패스(리스트)값 제거
            if (distance < 1f)   //@18-1 크기 (5->)1로 변경
            {
                m_path.RemoveAt(0);
            }

            // if(pathCountStart1 && !pathCountStart2)     //@18-2
            //     pathCountStart2 = true;
        }
        // if(m_path.Count<=0 && pathCountStart2)   //@18-2
        // {
        //     pathfinding.ResetNode();   
        //     pathCountStart1 = false;
        //     pathCountStart2 = false;
        // }
    }

    // private void Update()
    // {
    //     // 만약, 멤버 패스(리스트)의 데이타가 0 초과라면 즉, 데이타가 존재 한다면.
    //     if (m_path.Count > 0)
    //     {
    //         // 방향 벡터 설정
    //         // 첫 번째 리스트의 위치에서[즉, 처음부터 단계별로 이동] 내 자신의 위치를 뺀 벡턱값이 이동 방향이 된다.
    //         Vector3 dir = m_path[0].transform.position - transform.position;
    //         // 벡터 정규화
    //         dir.Normalize();

    //         // 방향으로 이동하자.
    //         transform.Translate(dir * 1);   //@18-1 크기 (10->)1로 변경

    //         // 목적지와 나와의 거리를 체크
    //         float distance = Vector3.Distance(m_path[0].transform.position, transform.position);

    //         if(distance > 1f)
    //         {
    //             transform.Translate(dir * moveSpeed * Time.deltaTime);  //이동 방향만큼 천천히 이동
    //         }
    //         else if(distance < 1f)
    //         {
    //             m_path.RemoveAt(0);
    //         }

    //         // // 목적지에 도착 했다면 RemoveAt으로 해당 패스(리스트)값 제거
    //         // if (distance < 5f)   //@18-1 크기 (5->)1로 변경
    //         // {
    //         //     m_path.RemoveAt(0);
    //         // }
    //     }
    // }
}
