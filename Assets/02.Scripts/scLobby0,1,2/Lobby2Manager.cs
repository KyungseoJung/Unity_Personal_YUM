using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;

public class Lobby2Manager : MonoBehaviour
{
    public Text playChar;               //""님의 플레이 캐릭터
    public Text typeChoose;             // 플레이 타입 텍스트 (txtTypeChoose)
    public Text skillRange;             // 스킬 적용 범위 텍스트 (txtSkillRange)
    public Text gunAvailable;           // 총 사용 유무 텍스트 (txtGunAvailable)
    public Image balloon;                //풍선 이미지(imgBallon)

    // public Text playerRecord;           //""님의 게임 기록 - 싱글톤 데이터와 연결
    // public Text scoreRecord;            // 스코어 최고 기록 텍스트 (txtScoreRecord)
    // public Text starSum;                // 모은 스타 개수 텍스트(txtStarSum)

    public GameObject uishowCharInfo;        //캐릭터 확인 창

    // public GameObject uiShowRecord;         //게임 기록 창

    private SoundManager soundMgr;             //@마지막

    void Awake()        //@마지막
    {
        soundMgr = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }
    void Start()
    {
        if(!uishowCharInfo.activeSelf)          //열린 채로 시작
            uishowCharInfo.SetActive(true);

        // if(uiShowRecord.activeSelf)          //@마지막 - scOpen에서 관리하자
        //     uiShowRecord.SetActive(false);      //닫은 채로 시작

        playChar.text = InfoManager.Info.playerName + "님의 플레이 캐릭터";
        typeChoose.text = InfoManager.Info.playType.ToString();
        skillRange.text = InfoManager.Info.skillRange;
        gunAvailable.text = InfoManager.Info.gunAvailable;

        balloon.color = InfoManager.Info.ballonColor;

        // playerRecord.text = InfoManager.Info.playerName + "님의 게임 기록";
    }

    // public void OpenRecord()                 //@마지막 - scOpen에서 관리하자
    // {
    //     uishowCharInfo.SetActive(false);
    //     uiShowRecord.SetActive(true);
    // }

    // public void CloseRecord()                //@마지막 - scOpen에서 관리하자
    // {
    //     uiShowRecord.SetActive(false);

    //     uishowCharInfo.SetActive(true);
    // }

    //사운드 중지 (로딩중엔 사운드 없다)
    //사운드 ui 비 활성화 (로딩창엔 실수로도 아무것도 안나오게 하자)
    //scPlayUi 씬 로드
   
    /*
    @3-1 플레이하면, 이렇게 실행되도록(사운드 멈추고, 캔버스 사라지고, 다음 창인 scPlayUi 뜨도록)
    많이 하는 실수 : scPalyUi 만들어놓아야 함. 빌드창에 추가해놓아야 함.
    */

    /* @중요
    Generic 타입으로 함수를 호출한 경우!! 특정 컴포넌트를 가져와서 바로 적용 가능
    일반함수 호출 타입과 대조. 일반함수 호출의 경우 typeof 지정해줘야 함.
    자세한 건 블로그(Act3) 참고 
    */
    public void PlayGame()      // PlayGame이 연결된 버튼을 누르면
    {
        /*
        GameObject.Find("SoundManager").GetComponent<AudioSource>().Stop();     //1 노래 멈춰
        GameObject.Find("SoundCanvas").GetComponent<Canvas>().enabled = false;  //2 캔버스 숨겨

        //Application.LoadLevel("scPlayUi); //3 //옛날 버전이래
        SceneManager.LoadScene("scPlayUi"); //3 //다음 씬 띄워
        */

        InfoManager.Info.SaveJSONData();        //Lobby0,1 (2에서는 데이터 저장할 거 없음)에서 적은 커스터마이징 데이터를 JSON에 저장하기

        Invoke("GoHome", 2.0f); //JSON 저장할 시간 주자
        
    }

    void GoHome()   //Invoke로 호출
    {
        SceneManager.LoadScene("scHome");   //@20-1
        soundMgr.UpdatePlayerInfo();    
    }
}
