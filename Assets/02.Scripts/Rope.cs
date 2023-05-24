using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour   //@5-2 밧줄 n개 생성
{
    public GameObject rope; 
    public int numberOfRope;

//연결용
    public Rigidbody2D firstPoint;
    FixedJoint2D curJoint;       //현재 조인트 지점
    FixedJoint2D exJoint;       //이전 조인트 지점


    void Start()
    {
        for(int i=0; i<numberOfRope; i++)
        {
            curJoint = Instantiate(rope.transform).GetComponent<FixedJoint2D>();
            curJoint.transform.localPosition = new Vector3(0, (i+1) * -0.5f, 0);    //이전 밧줄과의 거리 조정
            if(i ==0)
            {
                curJoint.connectedBody = firstPoint;
            }
            else
            {
                curJoint.connectedBody = exJoint.GetComponent<Rigidbody2D>();
            }

            exJoint = curJoint;

            //마지막 밧줄에는 무게를 줘서 전체적으로 축 늘어나게. 
            //그리고 자연스러운 무빙효과를 위해 보이지 않게
            if(i == numberOfRope -1)    
            {
                curJoint.GetComponent<Rigidbody2D>().mass = 20;
                curJoint.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }
}
