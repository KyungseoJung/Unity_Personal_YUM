using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FoodSpace;    // Food 스크립트 내 enum 변수

public class Goal : MonoBehaviour   //@1-2
{
    public FOOD_KIND goalKind;
    private bool goalEnd = false;           //골에 도착했는지
    private SpriteRenderer ren;
    public Sprite goalSuccess;

    private ScoreManager scoreManager;

//@21-1 게임 승패
    private LevelManager levelManager;

    void Awake()
    {
        ren = transform.GetComponent<SpriteRenderer>();
        scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>(); 

//@21-1
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
    }

    void OnCollisionEnter2D(Collision2D col) 
    {
        if(!goalEnd && (col.gameObject.tag == "Food"))
        {
            if(goalKind == col.gameObject.GetComponent<Food>().foodKind)    //골의 Kind와 음식의 Kind가 같다면
            {
                Destroy(col.gameObject);    //도착했으면, 음식 프리팹은 사라지도록
                ren.sprite = goalSuccess;   //골에 도착했으면 알맞게 이미지 변경
                scoreManager.GetScore(100);  //점수 증가
                gameObject.layer = 12;      //도착했으니 그 어느것과도 충돌하지 않도록

            levelManager.UpdateGoalState();
            levelManager.CheckWinOrLose();  //@21-1

            }


        }
    }
}
