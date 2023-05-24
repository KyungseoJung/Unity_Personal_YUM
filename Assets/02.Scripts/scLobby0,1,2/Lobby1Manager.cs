using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;

public class Lobby1Manager : MonoBehaviour   //@9-3
{
    int playType =1;       //1 또는 2
    bool colorCheck = false;

    public Text typeNum;         //숫자 텍스트 (txtNum)
    public Text typeChoose;      // 타입 텍스트 )txtTypeChoose)
    public Text skillRange;      // 스킬 텍스트 (txtSkillRange)
    public Text gunAvailable;    // 스킬 텍스트 ("txtGunAvailable)

    public Text closetName;     // 옷장 텍스트 (txtClosetName)

    public GameObject uiPlayType; //플레이 타입 선택 게임오브젝트 (choosePlayType)
    public GameObject uiCloset; // 옷장 게임오브젝트 (chooseCloth)

    public GameObject[] colorBtn;  //
    /*
    0번 : 빨강
    1번 : 노랑 (btnBack)
    2번 : 초록 (btnChooseCloth)
    3번 : 파랑
    */

    Color balloonColor;         //선택한 모자 색
    Image chooseColorImage;         //선택한 모자 이미지 -> 색 빼오기
    public Image decidedBalloon;             //선택한 색깔 -> 모자에 적용하기 (imgDecidedCap)



    // public GameObject[] updownBtn;

        /* 
            0번 : 들어가기 버튼(btnWriteName)
            1번 : 닫기 버튼 (btnClose)
            2번 : 이름 결정! 버튼(btnConfirm) 
        */
    void Start()
    {   
        if(!uiPlayType.activeSelf)          //열린 채로 시작
            uiPlayType.SetActive(true);

        if(uiCloset.activeSelf)             //닫은 채로 시작
            uiCloset.SetActive(false);

        closetName.text = InfoManager.Info.playerName + "님의 풍선";    //싱글톤 JSON 데이터 가져오기
        UpdatePlayType();

    }

    public void NumberUp()
    {
        if(playType <2)
            playType++;
            
        UpdatePlayType();
    }

    public void NumberDown()
    {
        if(playType >1)
            playType--;
        
        UpdatePlayType();
    }

    void UpdatePlayType()
    {
        typeNum.text = playType.ToString("D1");

        if(playType ==1)
        {
            typeChoose.text = "커비1";
            skillRange.text = "음식, 폭탄";
            gunAvailable.text = "YES";
        }    
        else
        {
            typeChoose.text = "커비2";
            skillRange.text = "음식, 폭탄, 몬스터";
            gunAvailable.text = "NO";
        }
    }

    public void ConfirmPlayType()  // (btnChooseType) 버튼
    {
        // Debug.Log("플레이어 타입 저장");
        InfoManager.Info.playType = playType;    //싱글톤 JSON 데이터 저장하기
        InfoManager.Info.skillRange = skillRange.text;
        InfoManager.Info.gunAvailable = gunAvailable.text;

        uiCloset.SetActive(true);       //옷장 열기
    }

    public void GoBack()   // (btnBack) 버튼
    {
        uiCloset.SetActive(false);      //옷장 닫기
    }

    public void ChooseColor(int index)  //빨강, 노랑, 초록, 파랑 모자 버튼에 각각 연결하기
    {
        chooseColorImage =  colorBtn[index].GetComponent<Image>();
        balloonColor = chooseColorImage.color;
        // Debug.Log(index+1 + " 번째 모자의 색깔은 : " + balloonColor);
        decidedBalloon.color = balloonColor;

        if(!colorCheck)
            colorCheck = true;
    }

    public void ConfirmCloth()  //옷 확정 버튼 (btnChooseCloth) 버튼
    {
        if(!colorCheck)    //풍선 색 선택해야 다음으로 넘어갈 수 있도록
        {
            Debug.Log("색 선택 필수");
            return;
        }    

        InfoManager.Info.ballonColor = balloonColor;    //싱글톤 데이터 저장 

        // InfoManager.Info.SaveJSONData();        //Lobby0,1 (2에서는 데이터 저장할 거 없음)에서 적은 데이터를 JSON에 저장하기 -> Lobby2Manager에서 한번에 하자
        
        // Debug.Log("JSON 저장해보기");
        SceneManager.LoadScene("scLobby2");  
    }


}
