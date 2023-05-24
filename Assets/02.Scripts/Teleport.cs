using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeleportSpace;

namespace TeleportSpace
{
    public enum TELE_KIND   //순간이동 위치 종류
    { 
        UNDERGROUND = 1, 
        TWINCAVE1,
        TWINCAVE2
    }
}

public class Teleport : MonoBehaviour
{
    public TELE_KIND teleKind;
    private Vector2 startPos;
    private Vector3 destPos;

    private Transform twinCave1;
    private Transform twinCave2;

    void Awake()
    {
        Transform parentTranform = transform.parent;

        twinCave1 = GameObject.Find("twinCave1").transform;
        twinCave2 = GameObject.Find("twinCave2").transform;
    }

    void OnCollisionEnter2D(Collision2D col)  //이 콜라이더 안에 무언가 들어왔다면
    {
        if((col.gameObject.tag == "Player") || (col.gameObject.tag == "Enemy") 
            || (col.gameObject.tag == "Food")|| (col.gameObject.tag == "Bomb"))
        {
            if(teleKind == TELE_KIND.UNDERGROUND)   //천장으로 순간이동
            {
                destPos = col.gameObject.transform.position;
                destPos.y = 10f-0.1f;    //-destPos.y;  //@18-2 Astar 에러 방지용으로 10을 넘어가지 않도록

                col.gameObject.transform.position = destPos;    
            }

            if(teleKind == TELE_KIND.TWINCAVE1) //다른 동굴로 순간이동
            {
                destPos = twinCave2.position; 
                
                col.gameObject.transform.position = destPos;
            }
            
            if(teleKind == TELE_KIND.TWINCAVE2) 
            {
                destPos = twinCave1.position; 
                
                col.gameObject.transform.position = destPos;
            }
        }
        
       
    }
}
