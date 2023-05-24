using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   
using UnityEngine.SceneManagement; 

public class PlayManager : MonoBehaviour
{

	// 유저인터페이스를 연결하기 위한 변수
	public GameObject playrUi;
	//로딩(설명)Panel을 연결 하기위한 배열 
	public GameObject explainUi;
	//로딩 progress Text 컴포넌트를 연결하기 위한 변수 
	public Image Loadingprogress;
    Vector3 proScale;

//@11-1 포톤 - 싱글 or 멀티
	private SoundManager soundMgr;

	// Start 보다 먼저 호출 됨 
	// 순서를 지켜 주자 그래야 코딩도 편해지고 성능이 안좋은 플랫폼에서 문제 발생을 방지.
	void Awake () {
		// 로딩(설명)Panel 렌덤으로 선택
		explainUi.SetActive (true);         // 1
		playrUi.SetActive (true);       // 2

//@11-1 
		soundMgr = GameObject.Find("SoundManager").GetComponent<SoundManager>();
	}

	// Use this for initialization
	// 코루틴 함수 호출 
	IEnumerator Start () {
				// 1초 정도는 화면에 설명을 뛰워주자 바로 게임으로 넘어가면 이상함 
		Debug.Log("//@26-1 스타트는 탔어");

		yield return new WaitForSeconds(1.0f);

		// 씬을 비동기방식으로 추가하자.
		//AsyncOperation async = Application.LoadLevelAdditiveAsync ("scStage1");
		// AsyncOperation async;	//@26-1

		Debug.Log("//@26-1 포톤 잠깐 끌게");
		PhotonNetwork.isMessageQueueRunning = false;        //#19 잠시 네트워크 끊기	//@26-1 이거 추가 안 해줬어서 안됐었던 건가?!!!

		// if(soundMgr.singlePlay)
		// {
		// 	async = SceneManager.LoadSceneAsync("scStage2", LoadSceneMode.Additive);
		// }
		// else
		// {
			AsyncOperation async = SceneManager.LoadSceneAsync("scStage1_0516_1", LoadSceneMode.Additive);
		// }
		Debug.Log("//@26-1 스테이지1 비동기 화면 틀게");

        proScale = Loadingprogress.rectTransform.localScale;
		//While 문으로 로딩진행사항을 표시해주자 
		//현재 로딩중일때 (비동기가 끝나지 않았다면)
		while (!async.isDone)
		{
			// async.progress 값은 0~1 값이다 따라서 100을 곱해주자 그래야 % 값 얻음. 
			proScale.x = async.progress ;  //@ 비동기함수 지역변수의 progress(진행상황)(0부터 1까지)
		
			//Text 컴포넌트에 text 요소를 다음과 같이 셋팅 
            Loadingprogress.rectTransform.localScale = proScale;
			
			//  한프레임동안 대기한후 무한루프를 다시 수행한다
			//  IEnumerator문을 바로 탈출하려면 yield break문을 사용하면 된다.
			yield return true;
		}

		//@로딩 다 마친 후(while 루프문 나온 후니까)
        proScale.x = 1f;
        Loadingprogress.rectTransform.localScale = proScale;

        yield return new WaitForSeconds(3.0f);	//다 끝나도 3초만 기다렸다가 다음으로 넘어가자

        // 로딩 완료후 설명서 비활성화 사운드 Ui 활성화 
        explainUi.SetActive (false);
		GameObject.Find ("SoundCanvas").GetComponent<Canvas> ().enabled = true;	//@(맨 처음 씬의) soundCanvas 찾아서 열어라~

		soundMgr.CanGoHome(true);	//@21-1 홈 버튼도 활성화

	}

	//유니티엔진에게 역할을 분담시켜 유니티엔진에 렌더링 루프와 별도로 처리되어 성능향상 .


}
