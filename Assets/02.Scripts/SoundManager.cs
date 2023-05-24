using UnityEngine;
using System.Collections;

//이 선언이 있어야 UI관련 컴포넌트를 연결 및 사용 가능.
using UnityEngine.UI;
using UnityEngine.SceneManagement;		//@9-4 홈 씬으로 돌아가기

//현재 스크립트에서 넓게는 현재 게임오브젝트에서 반드시 필요로하는 컴포넌트를 Attribute로 명시하여 해당 컴포넌트의 자동 생성 및 삭제되는 것을 막는다.
[RequireComponent(typeof(AudioSource))]

public class SoundManager : MonoBehaviour {

	//@3-4
	//오디오 클립 저장 배열 선언
	public AudioClip[] soundFile;
	//@21-1 
	public AudioClip[] gameEndSoundFile;	//0번째 : 승리, 1번째 : 패배
	//@9-4
	public AudioClip homeSoundFile;

	//사운드 Volume 설정 변수
	public float soundVolume = 1.0f;
	//사운드 Mute 설정 변수 
	public bool isSoundMute = false;
	//슬라이더 컴포넌트 연결 변수 
	public Slider sl;
	//토글 컴포넌트 연결 변수 
	public Toggle tg;
	//Sound 오브젝트 연결 변수 
	public GameObject Option;
	//Sound Ui버튼 오브젝트 연결 변수 
	public GameObject PlaySoundBtn;
	public GameObject ConfirmGoHome;

//@2-5 GetComponent 따로 빼주는 용도    //GetComponent 참고 : https://codingmania.tistory.com/147
AudioSource audio;	//@ 경고 나오는 거 정상임~ 무시

//@11-1 포톤 - 싱글플레이 or 멀티플레이
//    public bool singlePlay = true;

//@21-1 승패 상태
	// public bool isWin = true;
	// public Text txtWinOrLose;	
	public GameObject objBtnGoHome;	// 플레이 화면에서만 켜지도록

//@마지막
    public Text playerRecord;           //""님의 게임 기록 - 싱글톤 데이터와 연결
    public Text scoreRecord;            // 스코어 최고 기록 텍스트 (txtScoreRecord)
    public Text starSum;                // 모은 스타 개수 텍스트(txtStarSum)
	public Text playType;				// 플레이 타입 - 싱글톤 데이터와 연결 (txtPlayType)
	public Text skillRange;				// 스킬 적용 범위(txtSkillRange)
	public Text gunAvailable;			// 총 사용 가능 유무(txtGunAvailable)

    public GameObject uiShowRecord;         //@마지막 게임 기록 창

	void Awake(){
		// //이 오브젝트는 씬 전환시 사라지지 않음
		// DontDestroyOnLoad ( this.gameObject );   //@9-2 이미 DontDestroy 스크립트로 따로 추가해줌
        audio = GetComponent<AudioSource>();    //@2-5 GetComponent 따로 빼주는 용도

		LoadData();		//@4-1
	}

	// Use this for initialization

	//초기화 문에서 각 컴포넌트의 요소로 멤버 soundVolume,  isSoundMute 변수를 초기화.
	//이 함수는 각 변경된 컴포넌트 값으로  soundVolume, isSoundMute 의 값을 변경 (따로 분리해서 함수 선언 정의 해도 됨)
	void Start () {
		if(Option.activeSelf)
			Option.SetActive(false);

		if(ConfirmGoHome.activeSelf)
			ConfirmGoHome.SetActive(false);

		if(uiShowRecord.activeSelf)	//@마지막 - 닫은 채로 시작
			uiShowRecord.SetActive(false);

		UpdatePlayerInfo();	//@ 마지막 - 업데이트 한 채로 시작
		
		CanGoHome(false);		//@21-1

		soundVolume = sl.value;
		isSoundMute = tg.isOn;
		
		if(!PlaySoundBtn.activeSelf)
			PlaySoundBtn.SetActive (true);// 비 활성화 되었던 사운드 Ui 실행 버튼이 활성화 되어져 보일것이다.
									//@3-1 scOpen에서 허접함을 막기 위해 비활성화했던 버튼을 scLobby에서 보이도록 설정한 것
		AudioSet ();
	}

	// public	void ConnetWinOrLoseText()	//@21-1	게임 다 끝나고나서야 찾자
	// {
	// 	Invoke("FindText", 2.0f);
	// }
	
	// void FindText()
	// {
		
	// 	txtWinOrLose = GameObject.Find("txtWinOrLose").GetComponent<Text>();
	// 	if(isWin)
	// 		txtWinOrLose.text = "Game Win!";
	// 	else
	// 		txtWinOrLose.text = "Game Lose!";
	// }
	
	public void OpenConfirmGoHome()	//scOpen 씬에서 btnGoHome 버튼에 연결
	{
		if(Option.activeSelf)
			Option.SetActive(false);

		if(!ConfirmGoHome.activeSelf)
			ConfirmGoHome.SetActive(true);
		
	}
	public void GoBackHome()	//@9-4 Home 씬으로 돌아가기	//scOpen 씬에서 btnOk에 연결
	{
		ConfirmGoHome.SetActive(false);		//Home으로 돌아가기 확인 창 닫기
		Option.SetActive(false);			//옵션 자체 창 닫기
		PlaySoundBtn.SetActive(true);		//옵션 버튼은 열기

// if(PhotonNetwork.isMasterClient)
// {
// 		StopAllCoroutines();		//@21-1
//         CancelInvoke();
// }
		PhotonNetwork.LeaveRoom();	//@21-1
        // SceneManager.LoadScene("scHome");
		
		audio.clip = homeSoundFile;		//사운드 변경해서 틀기  
		AudioSet();
		audio.Play();

		CanGoHome(false);	//@21-2 이제 홈 왔으니까, 홈으로 또 가지 않도록 홈 버튼 없애
	}
	public void CancelGoHome()	//다시 옵션창으로
	{
		if(ConfirmGoHome.activeSelf)
			ConfirmGoHome.SetActive(false);

		if(!Option.activeSelf)
			Option.SetActive(true);		
	}

	//Slider 와 Toggle 컴포넌트에서 이벤트 발생시 호출해줄 함수를 선언 (public 키워드에 의해 외부접근 가능)
	public void SetSound(){
		soundVolume = sl.value;
		isSoundMute = tg.isOn;
		AudioSet ();
	}

	//AudioSource 셋팅 (사운드 UI에서 설정 한 값의 적용 )
	void AudioSet(){
		//AudioSource의 볼륨 셋팅 
		//GetComponent<AudioSource>().volume = soundVolume;
        audio.volume = soundVolume;

		//AudioSource의 Mute 셋팅 
		//GetComponent<AudioSource>().mute = isSoundMute;
        audio.mute = isSoundMute;

	}

	//사운드 UI 창 오픈 
	public void SoundUiOpen()
        {
		// 사운드 UI 활성화 
		Option.SetActive (true); 
		// 사운드 UI 오픈 버튼 비활성화 
		PlaySoundBtn.SetActive(false);
	}

	//사운드 UI 창 닫음
	public void SoundUiClose(){
		// 사운드 UI 비 활성화 
		Option.SetActive (false);
		// 사운드 UI 오픈 버튼 활성화 
		PlaySoundBtn.SetActive (true);

		SaveData();	//@4-1
	}

	//@3-4
	//스테이지 시작시 호출되는 함수
	public void PlayBackground(int stage)
	{
		// GetComponent<AudioSource>().clip = soundFile[stage-1];
		// AudioSet();
		// GetComponent<AudioSource>().Play();

		//위에 처럼 적으면 꿀밤~ GetComponent는 Awake에 따로 레퍼런스 선언해주자
		    // AudioSource의 사운드 연결
		audio.clip = soundFile[stage-1];
			// AudioSource 셋팅 
		AudioSet();
		    // 사운드 플레이. Mute 설정시 사운드 안나옴
		audio.Play();
	}

	//@21-1
	// public void PlayGameEndSound(bool _isWin)
	// {
	// 	// audio.clip = gameEndSoundFile[(int)(!_isWin)];
		
	// 	if(_isWin)
	// 		audio.clip = gameEndSoundFile[0];
	// 	else
	// 		audio.clip = gameEndSoundFile[1]; 
		
	// 	// AudioSource 셋팅 
	// 	AudioSet();
	// 	    // 사운드 플레이. Mute 설정시 사운드 안나옴
	// 	audio.Play();
	// }

	//@3-5 함수 추가 PlayEffect ========================================================
	//사운드 공용함수 정의(어디서든 동적으로 사운드 게임오브젝트 생성)
	public void PlayEffect(Vector3 pos, AudioClip sfx)
	{
		//Mute 옵션 설정시 이 함수를 바로 빠져나가자.
		if(isSoundMute)
		{
			return;
		}

		//게임오브젝트의 동적 생성하자.
		GameObject _soundObj = new GameObject("sfx");
		//사운드 발생 위치 지정하자. 
		_soundObj.transform.position = pos;	//@ 위치를 몬스터의 위치로 잡기 위해(월드 좌표(?))
		//생성한 게임오브젝트에 AudioSource 컴포넌트를 추가하자.
		AudioSource _audioSource = _soundObj.AddComponent<AudioSource>();
		//AudioSource 속성을 설정 
		//사운드 파일 연결하자.
		_audioSource.clip = sfx;	//@사운드 클립(파일)을 sfx로 연결하자
		//설정되어있는 볼륨을 적용시키자. 즉 soundVolume 으로 게임전체 사운드 볼륨 조절.
		_audioSource.volume = soundVolume;	//@볼륨도 설정한 볼륨으로 맞추자
		//사운드 3d 셋팅에 최소 범위를 설정하자.
		_audioSource.minDistance = 15.0f;	//@사운드 작은 구 범위 = 15로 맞추겠다~
		//사운드 3d 셋팅에 최대 범위를 설정하자.
		_audioSource.maxDistance = 30.0f;	

		//사운드를 실행시키자.
		_audioSource.Play();
		
		//모든 사운드가 플레이 종료되면 동적 생성된 게임오브젝트 삭제하자.
		Destroy(_soundObj, sfx.length + 0.2f ); //@3-5 팁 : 여기에 0.2f정도 더해줘야 약간 끊기는 거 해결

	}

	//@4-1 ======================================================
	//게임 사운드데이타 저장
	//@저장하기 : PlayerPrefs.Set자료형(키값, 저장할 값);
	public void SaveData()	//@ SoudnUiClose에 넣기
	{
		PlayerPrefs.SetFloat("SOUNDVOLUME", soundVolume);	//float 그대로 "SOUNDVOLUME"에 저장
  		//PlayerPrefs 클래스 내부 함수에는 bool형을 저장해주는 함수가 없다.
        //bool형 데이타는 형변환을 해야  PlayerPrefs.SetInt() 함수를 사용가능
		PlayerPrefs.SetInt("ISSOUNDMUTE", System.Convert.ToInt32(isSoundMute));	//bool형을 정수형으로 "ISSOUNDMUTE"에 저장하고 있음
	}

	//게임 사운드데이타 불러오기 
    //바로 사운드 UI 슬라이드 와 토글에 적용하자.
	//@불러오기 : PlayerPrefs.Get자료형(저장한 키값, 초기화값);
	public void LoadData() //@Awake에 넣기
	{
		sl.value = PlayerPrefs.GetFloat("SOUNDVOLUME");	//float형 그대로 sl.value에 로드
		  //int 형 데이타는 bool 형으로 형변환.
		tg.isOn = System.Convert.ToBoolean(PlayerPrefs.GetInt("ISSOUNDMUTE")); //bool형은 다시 int형으로 전환해서 tg.isOn에 로드

		 // 첫 세이브시 설정 -> 이 로직없으면 첫 시작시 사운드 볼륨 0
		int isSave = PlayerPrefs.GetInt("ISSAVE");	//처음으로 저장하는 거면 0으로 저장되겠지
		if(isSave ==0)	//처음 로드하는 거라면, 아래 값으로 로드되도록
		{	
			sl.value = 1.0f;
			tg.isOn = false;
			// 첫 세이브는 soundVolume = 1.0f; isSoundMute = false; 이 디폴트 값으로 저장 된다.
			SaveData();
			PlayerPrefs.SetInt("ISSAVE",1);	//이제 1로 저장했음. 다음부턴 if문 안 타
		}
	}

	public void CanGoHome(bool _objActive)	//홈 버튼 활성화/ 비활성화
	{
		objBtnGoHome.SetActive(_objActive);
	}

    public void btnOpenRecord()                 //@마지막 - scOpen에서 관리하자
    {
        uiShowRecord.SetActive(true);
    }

    public void btnCloseRecord()                //@마지막 - scOpen에서 관리하자
    {
        uiShowRecord.SetActive(false);
    }

	public void UpdatePlayerInfo()		//@마지막	//처음 시작할 때, 캐릭터 새로 생성하고 나서, 게임 끝났을 때 호출해야 돼
	{
        playerRecord.text = InfoManager.Info.playerName + "님의 과거 게임 기록";
		scoreRecord.text = InfoManager.Info.topScore + "점";
		starSum.text = InfoManager.Info.numberOfStars + "개";

		playType.text = InfoManager.Info.playType + "타입";

        if(InfoManager.Info.playType ==1)
        {
            skillRange.text = "음식, 폭탄";
            gunAvailable.text = "YES";
        }    
        else
        {
            skillRange.text = "음식, 폭탄, 몬스터";
            gunAvailable.text = "NO";
        }

	}
}
