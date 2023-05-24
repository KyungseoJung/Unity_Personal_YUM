using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;

public class HomeManager : MonoBehaviour   //@9-4
{
    // (JSON에서 가져올 데이터) ==========
    public Text playerName;     //플레이어 이름
    public Text scoreRecord;    // 스코어 최고 기록
    public Text starSum;        //모은 스타 개수

    private string _homeName;   //@15-1 플레이어 데이터 가져와서 큰따옴표("") 제외하고 저장할 문자열

//@10-1 위와 동일하게 JSON에서 데이터 가져올 건데, 멀티플레이 계정에 적용할 데이터
    public Text mulPlayerName;  
    public Text mulScoreRecord;
    public Text mulStarSum;

    public InputField inputRoomName;    //@16-3 방 이름 작성시 오류나는 거 방지

    public GameObject uiHome;
    public GameObject uiLoadData; 
    public GameObject uiMultiPlay;  //@10-1 멀티플레이 화면  

    //웬만하면 이런식으로 실행순서를 맞춰주자. 간혹 명령문의 순서가 잘못되어서 프로그램이 꼬이는 경우가 있다.
//@11-1 싱글플레이 or 멀티플레이 선택
    private SoundManager soundMgr;

    void Awake()
    {
        soundMgr = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }
    void Start()
    {
        InfoManager.Info.LoadJSONData();    //JSON 데이터 불러오기
        InfoManager.Info.LoadScoreJSONData();   //@21-2 점수 데이터 불러오기

        if(!uiHome.activeSelf)          // 처음엔 창 열려있도록
            uiHome.SetActive(true);
        if(uiLoadData.activeSelf)       // 처음엔 창 안 열려있도록
            uiLoadData.SetActive(false);
        if(uiMultiPlay.activeSelf)      // 처음엔 창 안 열려있도록
            uiMultiPlay.SetActive(false);
    
        inputRoomName.onValueChanged.AddListener(OnInputChange);    //@16-3 방 이름 작성시 발생하는 오류 방지 목적 글자수 제한
    }
    
    public void LoadRecord()    //기록 불러오는 버튼(btnLoadRecord)
    {

        InfoManager.Info.LoadJSONData();    //@21-2 JSON 데이터 불러오기
        InfoManager.Info.LoadScoreJSONData();   //@21-2 점수 기록 JSON 데이터 불러오기 

        Invoke("LoadSinglePlay", 3.0f);     // JSON 다 불러와지는 거 기다리고 로드하자 //너무 빨리 방 만들기 하면 튕길 수도 있으니까 기다리기~
    }

    void LoadSinglePlay()   //싱글 플레이 시작 버튼에 연결
    {
        // InfoManager.Info.LoadJSONData(); //@20-1 잘 안 타...

        // soundMgr.singlePlay = true;    //@21-6 //@11-1 포톤 실행하냐 마냐 결정해줌 -> 싱글플레이도 포톤 넣어야지. 방 정원이 1명인 포톤으로!

        // string _homeName = InfoManager.Info.playerName;
        // _homeName = _homeName.Substring(1, name.Length - 2);  // 큰따옴표("")는 안 가져오도록
        _homeName = InfoManager.Info.playerName.Trim('"');

        if(string.IsNullOrEmpty(_homeName))
        {
            return;
        }
        Debug.Log("//@20-1 플레이어 이름 : " + _homeName);
        playerName.text = _homeName;
        scoreRecord.text = "스코어 최고 기록 :" + InfoManager.Info.topScore.ToString();
        starSum.text = "모은 스타 개수 :" + InfoManager.Info.numberOfStars.ToString();


        uiHome.SetActive(false);
        uiLoadData.SetActive(true);
    }
    
    public void GoToLobby0()
    {
        // soundMgr.singlePlay = true;     //@11-1 포톤 실행하냐 마냐 결정해줌
        
        InfoManager.Info.MakeNewPlayer();   //@21-2
        SceneManager.LoadScene("scLobby0");  
    }

    public void GoToLobby2()
    {
        SceneManager.LoadScene("scLobby2");
    }
    public void LoadMultiPlay() //@10-1 멀티플레이 화면 넘어가는 버튼(btnMultiPlay)
    {
        // soundMgr.singlePlay = false;     //@11-1 포톤 실행하냐 마냐 결정해줌

        // string name = InfoManager.Info.playerName;
        // name = name.Substring(1, name.Length - 2);  // 큰따옴표("")는 안 가져오도록
        _homeName = InfoManager.Info.playerName.Trim('"');

        if(_homeName == "")
        {
            return;
        }
            

        uiHome.SetActive(false);
        uiMultiPlay.SetActive(true);

        mulPlayerName.text = InfoManager.Info.playerName;
        mulScoreRecord.text = "스코어 최고 기록 :" + InfoManager.Info.topScore.ToString();
        mulStarSum.text = "모은 스타 개수 :" + InfoManager.Info.numberOfStars.ToString();
    }
    public void ButtonBack()
    {
        uiLoadData.SetActive(false);
        uiMultiPlay.SetActive(false);
        uiHome.SetActive(true);
    }

    public void OnInputChange(string input)
    {
        if(input.Length > 4)
        {
            inputRoomName.text = input.Substring(0,4);
        }
    }

    public void btnQuitGame()   //@마지막 - 게임 종료
    {
        Application.Quit();
        Debug.Log("나가기");
    }
}
