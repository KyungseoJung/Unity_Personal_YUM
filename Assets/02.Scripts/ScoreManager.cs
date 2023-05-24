using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;      //@8-2

public class ScoreManager : MonoBehaviour //@8-2
{
   private int score =0;
   private int star =0;
   private float leftTime;
   private int time;

   public Text scoreSum;
   public Text starSum;
   public Text timeLeft;
//@ 마지막 오류 해결
   public Animator playerHanim;  // 그냥 ScoreManager에서 관리해주자

//@21-2
   public GameObject pnlWinOrLose;
   public Text txtWinOrLose;
//@21-5 오디오
   public AudioClip scoreClip;
   public AudioClip starClip;

//@ 마지막
    private SoundManager soundMgr;             //@마지막

    void Awake()        //@마지막
    {
        soundMgr = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }
   void Start()
   {
      pnlWinOrLose.SetActive(false);   //@21-2
      leftTime = 500;
   }

   void Update()
   {
      if(leftTime >0)
         leftTime -= Time.deltaTime;

      time = (int) leftTime;
      timeLeft.text = time.ToString("D3");

      scoreSum.text = score.ToString("D4");
      starSum.text = " X " + star.ToString("D2");
   }

   public void GetScore(int _score)   //@21-2 점수 추가 - 변수가 아닌 이 함수에 접근하도록
   {
      score += _score;
      AudioSource.PlayClipAtPoint(scoreClip, transform.position); //@21-5
   }
   public void GetStar(int _star)    //@21-2 스타 개수 추가 - 변수가 아닌 이 함수에 접근하도록
   {
      star += _star;
      AudioSource.PlayClipAtPoint(starClip, transform.position); //@21-5
   }

   public void SetPlayerHanim(int aniType)
   {
      switch (aniType)
      {
         case 1 : 
            playerHanim.SetTrigger("Hurt");
            break;
         case 2 :
            playerHanim.SetTrigger("Die");
            break;
      }
   }

   public void SaveRecordData()   //@21-2
   {
      Debug.Log("//@21-3-1 플레이어 이름 : " + InfoManager.Info.playerName);

      if(score >= InfoManager.Info.topScore)    // 최고 기록 저장
      {
         InfoManager.Info.topScore  = score;
      }
      
      InfoManager.Info.numberOfStars += star;   // 스타 개수 추가

      Invoke("SaveRecordJSONData", 2.0f);
   }
   void SaveRecordJSONData()   //@21-2 Invoke로 호출
   {
      // Debug.Log("//@21-3-2 플레이어 이름 : " + InfoManager.Info.playerName);
      // InfoManager.Info.SaveJSONData(); //이건 커스터마이징 관련 JSON
      InfoManager.Info.SaveScoreJSONData();
      
      Invoke("UpdatePlayerInfo", 2.0f);   //@마지막
   }

   void UpdatePlayerInfo()             //@마지막 - 기록 종이에도 게임기록 업데이트 해주자
   {
      soundMgr.UpdatePlayerInfo();  
   }
   
   public void ConnectWinOrLoseText(bool _iswin)
   {
      pnlWinOrLose.SetActive(true);   //@21-2

      if(_iswin)
         txtWinOrLose.text = "Game Win !";
      else
         txtWinOrLose.text = "Game Lose...";
   }



   
}
