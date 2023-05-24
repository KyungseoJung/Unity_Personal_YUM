using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleJSON;
using System.IO;

using UnityEngine.Networking;   //@22-1 


// [System.Serializable]
// public class GameData
// {
//     public PlayerInfo playerInfo;
//     public List<ItemInfo> itemInfo;
// }

[System.Serializable]
public class PlayerInfo
{
    //@21-2 player_info.json
    public string playerName;
    public int playType;
    public string skillRange;
    public string gunAvailable;
    public Color32 balloonColor;   

    //@21-2 json 따로 저장 ==============
    // score_info.json
    public int topScore =0;
    public int numberOfStars =0;
}


// [System.Serializable]
// public class ItemInfo          //JSON 데이터에 안 넣음
// {
//     public Color ballonColor;    //1부터 순서대로 빨강, 노랑, 초록, 파랑
// }

public class InfoManager : MonoBehaviour
{
    private PlayerInfo playerInfo;

    // private ItemInfo itemInfo;


    private static InfoManager info = null; //싱글톤 객체(인스턴스)
    public static InfoManager Info          //싱글톤 프로퍼티
    {
        get
        {
            if(info == null)
            {
                info = GameObject.FindObjectOfType(typeof(InfoManager)) as InfoManager; 
                    //이런 타입을 가진 오브젝트가 있다면, 그 오브젝트를 InfoManager로서 객체화 해라
                if(info == null)
                {
                    info = new GameObject("Singleton_InfoManager", typeof(InfoManager)).GetComponent<InfoManager>();
                    DontDestroyOnLoad(info);
                }
            }
            return info;
        }
    }

    void Awake()    //Start에 적으면 다른 것들보다 늦게 실행돼서 Null 에러 뜬다.
    {
        playerInfo = new PlayerInfo();
        // playerInfo.playerName = "";     //객체를 초기화 해줘야 null Reference 오류가 발생하지 않아

        // itemInfo = new ItemInfo();

        LoadJSONData();
        LoadScoreJSONData();    //@21-2

    }
    public string playerName
    {
        get { return playerInfo.playerName; }
        set { playerInfo.playerName = value; }
    }

    public int playType
    {
        get {return playerInfo.playType; }
        set {playerInfo.playType = value; }
    }

    public string skillRange
    {
        get {return playerInfo.skillRange;  }
        set {playerInfo.skillRange = value; }
    }
    public string gunAvailable
    {
        get {return playerInfo.gunAvailable; }
        set {playerInfo.gunAvailable = value; }
    }
    public Color32 ballonColor
    {
        get { return playerInfo.balloonColor; }
        set {playerInfo.balloonColor = value; }
    }

    ///////////////////
    public int topScore
    {
        get {return playerInfo.topScore; }
        set {playerInfo.topScore = value; }
    }

    public int numberOfStars
    {
        get {return playerInfo.numberOfStars; }
        set {playerInfo.numberOfStars = value; }
    }
    
    public void LoadJSONData()     //JSON 데이터 로드하기(JSON 파일 -> 클래스로)
    {
        // RemoveMetaFile("player_info.json");
        RemoveStreamingMetaFile("player_info.json");

//@22-1 ====================================================
    string filePath;
    
    if (Application.isEditor)
    {
        filePath = Path.Combine(Application.streamingAssetsPath, "player_info.json");
    }
    else
    {
        filePath = Path.Combine(Application.streamingAssetsPath, "player_info.json");
        filePath = "file://" + filePath;
    }

    StartCoroutine(LoadJSONDataCoroutine(filePath));

    }

IEnumerator LoadJSONDataCoroutine(string filePath)  //@22-1 
{
//@22-1 여기부터 ====================================================    
    Debug.Log("로드 제이슨데이터");
    string StrJsonData;
    if (filePath.Contains("://")) // 실행 파일이 빌드된 상태인 경우
    {
        UnityWebRequest www = UnityWebRequest.Get(filePath);
        yield return www.SendWebRequest();
        StrJsonData = www.downloadHandler.text;
    }
    else // 에디터 상에서 실행 중인 경우
    {
        StrJsonData = File.ReadAllText(filePath);
    }

//@22-1 여기까지 ====================================================


        // TextAsset jsonData = Resources.Load<TextAsset>("player_info");
        // string StrJsonData = jsonData.text;                             //# 데이터를 문자열로 가져와서
        var json = JSON.Parse(StrJsonData); //배열 형태로 자동 파싱         //# SimpleJSON을 통해 객체로 생성

// Debug.Log("플레이어 이름" + json["플레이어 이름"].ToString());
// Debug.Log("테스트용 string hex = json 풍선색 value : " + json["풍선 색"].Value );


//플레이어 정보 파싱

        playerInfo.playerName = json["플레이어 이름"].ToString();
        playerInfo.playType = json["플레이 타입"].AsInt;

        playerInfo.skillRange = json["스킬 적용 범위"].ToString();  
        playerInfo.gunAvailable = json["총 사용 유무"].ToString();
        
        string hex = json["풍선 색"].Value;
        Color32 color = HexToColor32(hex);
        playerInfo.balloonColor = color;
        
        // playerInfo.topScore = json["스코어 최고 기록"].AsInt;
        // playerInfo.numberOfStars = json["스타 개수"].AsInt;
}

    public void LoadScoreJSONData()     //JSON 데이터 로드하기(JSON 파일 -> 클래스로)
    {   
        // RemoveMetaFile("player_info.json");
        RemoveStreamingMetaFile("score_info.json");

//@22-1 ====================================================
    string filePath;
    
    if (Application.isEditor)
    {
        filePath = Path.Combine(Application.streamingAssetsPath, "score_info.json");
    }
    else
    {
        filePath = Path.Combine(Application.streamingAssetsPath, "score_info.json");
        filePath = "file://" + filePath;
    }

    StartCoroutine(LoadScoreJSONDataCoroutine(filePath));


    }
    

IEnumerator LoadScoreJSONDataCoroutine(string filePath)  //@22-1 
{
//@22-1 여기부터 ====================================================    
    Debug.Log("로드 Score 제이슨데이터");
    string StrScoreJsonData;
    if (filePath.Contains("://")) // 실행 파일이 빌드된 상태인 경우
    {
        UnityWebRequest www = UnityWebRequest.Get(filePath);
        yield return www.SendWebRequest();
        StrScoreJsonData = www.downloadHandler.text;
    }
    else // 에디터 상에서 실행 중인 경우
    {
        StrScoreJsonData = File.ReadAllText(filePath);
    }

//@22-1 여기까지 ====================================================

        // TextAsset ScorejsonData = Resources.Load<TextAsset>("score_info");
        // string StrScoreJsonData = ScorejsonData.text;                             //# 데이터를 문자열로 가져와서
        var Scorejson = JSON.Parse(StrScoreJsonData); //배열 형태로 자동 파싱         //# SimpleJSON을 통해 객체로 생성

//플레이어 정보 파싱
        playerInfo.topScore = Scorejson["스코어 최고 기록"].AsInt;
        playerInfo.numberOfStars = Scorejson["스타 개수"].AsInt;
}


    public void SaveJSONData()  //데이터 저장. (클래스 -> JSON 파일)
    {
      Debug.Log("//@21-3-3 플레이어 이름 : " + InfoManager.Info.playerName);

        //수정 및 업데이트 - JSON 파일에 저장하기
            // 수정된 데이터를 JSON 파일에 저장하기
        JSONObject json = new JSONObject();

        // 플레이어 정보     ===========================
        json.Add("플레이어 이름", @playerInfo.playerName);
        json.Add("플레이 타입", playerInfo.playType);
        json.Add("스킬 적용 범위", @playerInfo.skillRange);
        json.Add("총 사용 유무", @playerInfo.gunAvailable);

        Color32 color = playerInfo.balloonColor;
        string hex = ColorToHex(color);
        json.Add("풍선 색", hex);


        // json.Add("스코어 최고 기록", playerInfo.topScore);
        // json.Add("스타 개수", playerInfo.numberOfStars);

    //   Debug.Log("//@21-3-4 플레이어 이름 : " + InfoManager.Info.playerName);

        // JSON 파일로 저장     ===========================
        string jsonString = json.ToString();
        System.IO.File.WriteAllText(Application.dataPath + "/StreamingAssets/player_info.json", jsonString);    //@22-1 Resources 부분을 StreamingAssets으로 변경
    }

    public void SaveScoreJSONData()  //데이터 저장. (클래스 -> JSON 파일)
    {
        JSONObject Scorejson = new JSONObject();

        // 플레이어 정보     ===========================
        Scorejson.Add("스코어 최고 기록", playerInfo.topScore);
        Scorejson.Add("스타 개수", playerInfo.numberOfStars);

        // JSON 파일로 저장     ===========================
        string ScorejsonString = Scorejson.ToString();
        System.IO.File.WriteAllText(Application.dataPath + "/StreamingAssets/score_info.json", ScorejsonString);    //@22-1 Resources 부분을 StreamingAssets으로 변경
    }

    public void MakeNewPlayer() //@21-2 새로운 플레이어 정보를 만들면 - 'btnLobby0' 누를 때 실행되는 GoToLobby0 함수에 넣기
    {
        playerInfo.topScore = 0;
        playerInfo.numberOfStars = 0;

        SaveScoreJSONData();
    }

//@22-1
    private void RemoveMetaFile(string fileName)    //#19-1 0506추가
    {
    #if UNITY_EDITOR
        string assetPath = "Assets/Resources/" + fileName;
        string metaPath = assetPath + ".meta";

        UnityEditor.AssetDatabase.ImportAsset(assetPath);
        UnityEditor.AssetDatabase.DeleteAsset(metaPath);
        UnityEditor.AssetDatabase.Refresh();
    #else
        Debug.LogWarning("RemoveMetaFile()는 에디터에서만 지원됩니다.");
        // RemoveMetaFileNonEditor(fileName);

    #endif
    }

    private void RemoveStreamingMetaFile(string fileName)
    {
    #if UNITY_EDITOR
        string assetPath = "StreamingAssets/Resources/" + fileName;
        string metaPath = assetPath + ".meta";

        UnityEditor.AssetDatabase.ImportAsset(assetPath);
        UnityEditor.AssetDatabase.DeleteAsset(metaPath);
        UnityEditor.AssetDatabase.Refresh();
    #else
        Debug.LogWarning("RemoveMetaFile()는 에디터에서만 지원됩니다.");
        // RemoveMetaFileNonEditor(fileName);

    #endif        
    }

    private Color32 HexToColor32(string hex)
    {
        // HEX 문자열을 RGB 값으로 분리
        byte r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

        // Color32로 변환하여 반환
        return new Color32(r, g, b, 255);
    }

    private string ColorToHex(Color32 color)
    {
        // R, G, B 값을 HEX 문자열로 변환
        string r = color.r.ToString("X2");
        string g = color.g.ToString("X2");
        string b = color.b.ToString("X2");

        // '#' 문자열과 결합하여 반환
        return "#" + r + g + b;
    }
}

/*
    public TextAsset jsonData = null;
    public string StrJsonData = null;

    void Start()
    {
        jsonData = Resources.Load<TextAsset>("player_info");
        StrJsonData = jsonData.text;
        var json = JSON.Parse(StrJsonData); //배열형태로 자동 파싱.

//플레이어 정보 파싱
        PlayerInfo playerInfo = new PlayerInfo();

        playerInfo.playerName = json["플레이어 이름"].ToString();
        playerInfo.playType = json["플레이 타입"].ToString();
        
        playerInfo.skills = new List<string>();
        for(int i=0; i<json["Inhale/ Exhale 스킬 적용 범위"].Count; i++)
        {
            playerInfo.skills.Add(json["Inhale/ Exhale 스킬 적용 범위"][i].ToString());
        }

        playerInfo.topScore = json["스코어 최고 기록"].AsInt;
        playerInfo.numberOfStars = json["스타 개수"].AsInt;

//아이템 정보 파싱
        List<ItemInfo> itemList = new List<ItemInfo>();
        for(int i=0; i<json["옷장"].Count; i++)
        {
            ItemInfo itemInfo = new ItemInfo();
            itemInfo.name = json["옷장"][i]["이름"].ToString();
            itemInfo.type = json["옷장"][i]["타입"].ToString();
            itemInfo.price = json["옷장"][i]["가격"].AsInt;
            itemInfo.description = json["옷장"][i]["설명"].AsInt;

            itemList.Add(itemInfo);
        }
        
        //파싱된 정보 출력
        Debug.Log("플레이어 정보: " + playerInfo.playerName + ", " + playerInfo.playType + ", " + 
                  string.Join(", ", playerInfo.skills) + ", " + playerInfo.topScore + ", " + 
                  playerInfo.numberOfStars);
        
        foreach(string skill in playerInfo.skills)
        {
            Debug.Log("스킬 : " + skill);
        }
            
        foreach(ItemInfo item in itemList)
        {
            Debug.Log("Name : " + item.name);
            Debug.Log("Type : " + item.type);
            Debug.Log("Price : " + item.price);
            Debug.Log("Description : " + item.description);

        }



    }
*/



