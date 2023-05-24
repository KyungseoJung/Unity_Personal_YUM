using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour   //@7-4 아이템 - 스타 - 나중에 상점에서 구매 가능
{
    private bool eaten = false;     //별 먹혔는지 체크
    private ScoreManager scoreMgr;  // 스코어 업데이트




    void Awake()
    {
        scoreMgr = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
        
    }
    void Start()
    {
        // Debug.Log("스타 Start");
    }
    void OnCollisionEnter2D(Collision2D col) 
    {
        // Debug.Log("충돌");
        if(col.gameObject.tag == "Player")
            Eaten();
    }
    void Eaten()
    {
        // Debug.Log("Eaten");
        if(!eaten)
        {
            eaten = true;
            Destroy(gameObject);
            scoreMgr.GetStar(1);
        }    
        
    }
}
