using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// uGUI의 UI 항목을 사용하기 위한 네임스페이스 설정
using UnityEngine.UI;

// 포톤 추가
public class DisplayUserId : MonoBehaviour {

    // uGUI의 Text 항목 연결을 위한 레퍼런스
    public Text userId;

    //PhotonView 컴포넌트를 할당할 레퍼런스 
    PhotonView pv = null;

    void Awake()
    {
        userId = GameObject.Find("txtUserId").GetComponent<Text>();
    }
    void Start () {

        //컴포넌트를 할당 
        pv = GetComponent<PhotonView>();

        //HUD(Head Up Display)의 유저 아이디 설정 
        userId.text = InfoManager.Info.playerName;  //@22-1 pv.owner.NickName;  //이거 안 하면, 머리 위에 이전 플레이어 기록이 나와버림  //원래 주석이었음   //발표용(싱글플레이)에서 주석 잠깐 해제하자 //@20-1

    }

}
