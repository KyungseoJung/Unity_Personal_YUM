using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour   //@9-3
{
    public InputField inputField;   //플레이어 이름 적는 텍스트 (InputField)
    public int maxLength = 5;       // 글자 최대 길이
    public Text confirmQuestion;    //이름 확인 텍스트(txtConfirmQ)
    public GameObject uiWriteName;
    public GameObject uiConfirmName;   
    public GameObject[] lobbyBtn;               
        /* 
            0번 : 들어가기 버튼(btnEnterGame)
            1번 : 닫기 버튼 (btnClose)
            2번 : 이름 결정! 버튼(btnConfirm) 
        */
    
    //웬만하면 이런식으로 실행순서를 맞춰주자. 간혹 명령문의 순서가 잘못되어서 프로그램이 꼬이는 경우가 있다.
    private string _inputName;      //플레이어 이름 정보 큰따옴표 제외 부분 확인용
    
    void Start()
    {
        inputField.onValueChanged.AddListener(CheckInputField); // 입력값이 변경될 때마다 체크하는 이벤트 리스너

        if(uiWriteName.activeSelf)      //열린 채로 시작
            uiWriteName.SetActive(false);

        if(uiConfirmName.activeSelf)      //닫은 채로 시작
            uiConfirmName.SetActive(false);
    }
    //인벤 오픈
    public void WriteNameOpen()
    {
        lobbyBtn[0].SetActive(false);       //"들어가기" 버튼 사라져
        uiWriteName.SetActive(true);        //"WriteName" UI창 생겨
    }
    //인벤 클로즈
    public void WriteNameClose()
    {
        uiWriteName.SetActive(false);       //"WriteName" UI창 사라져
        lobbyBtn[0].SetActive(true);        //"들어가기" 버튼 생겨
    }

    public void WriteNameConfirm()  
    {
//@15-1 =====================================
        _inputName = inputField.text;
        if(string.IsNullOrEmpty(_inputName))    //아직 플레이어 정보 안 적었다면 'Confirm' 버튼 안 눌리도록
            return; 

        confirmQuestion.text = " <" + _inputName + "> 님이 맞나요?";
        uiConfirmName.SetActive(true);      //"confirmName" 오브젝트 나타나
    }
    
    public void NameCancel()
    {
        uiConfirmName.SetActive(false);
    }
    public void NameOK()
    {
        // Debug.Log("원래 클래스에 저장되어 있는 값 : " + InfoManager.Info.playerName);

//로드할 필요 없지 않나?       // InfoManager.Info.LoadJSONData();
        // Debug.Log("로드 후 클래스에 저장되어 있는 값 : " + InfoManager.Info.playerName);
        // Debug.Log("내가 적은 값 : " + inputField.text);

        _inputName = InfoManager.Info.playerName.Trim('"');
        // _name = _name.Substring(1, _name.Length - 2);

        // Debug.Log("큰 따옴표 뺀 값 : " + name);

        if( _inputName == inputField.text)
            Debug.Log("저장된 이름 정보 있음");
        else
            Debug.Log("정보 없음");

        InfoManager.Info.playerName = inputField.text;  //JSON데이터, 싱글톤 클래스 저장 - 아직 SaveJSONData는 안 해. 플레이어 정보 모두 입력하면 할 예정

        SceneManager.LoadScene("scLobby1");  
    }
    public void CheckInputField(string input)       // InputField에 값 입력할 때마다 실행되도록 연결
    {
        if(input.Length > maxLength) // 최대 길이)
        {
            inputField.text = input.Substring(0,maxLength);
        }
    }

}
