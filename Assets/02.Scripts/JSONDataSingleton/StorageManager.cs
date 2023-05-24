using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleJSON;
using System.IO;


[System.Serializable]   
public class ClothInfo  //@15-2 JSON 데이터 저장 - 창고 저장 데이터 관리 싱글톤 스크립트 만듦.
{
    public string name;
    public string type;
    public int price;
    public int description;
}



public class StorageManager : MonoBehaviour
{
    private ClothInfo clothInfo;
    private List<ClothInfo> clothList;

    private static StorageManager sinfo = null;  //싱글톤 객체(인스턴스)
    public static StorageManager SInfo          //싱글톤 프로퍼티
    {
        get
        {
            if(sinfo == null)
            {
                sinfo = GameObject.FindObjectOfType(typeof(StorageManager)) as StorageManager;

                if(sinfo == null)
                {
                    sinfo = new GameObject("Singleton_StorageManager", typeof(StorageManager)).GetComponent<StorageManager>();
                    DontDestroyOnLoad(sinfo);
                }
            }
            return sinfo;
        }
    }

    void Awake()
    {
        clothList = new List<ClothInfo>();
        clothInfo = new ClothInfo();

        LoadStorageJSONData();
    }

    public ClothInfo GetClothInfo(int index)
    {
        if(index >= clothList.Count)
        {
            Debug.Log("옷장 인덱스 범위 초과");
            return null;
        }    
        return clothList[index];
    }
    
    public void SetClothInfo(int index, ClothInfo clothInfo)
    {
        if(index >= clothList.Count)
        {
            Debug.Log("옷장 인덱스 범위 초과");
            return;   
        }
        clothList[index] = clothInfo;
    }

    public void LoadStorageJSONData()   //JSON 데이터 로드하기(JSON 파일 -> 클래스로)
    {
        TextAsset storageJsonData = Resources.Load<TextAsset>("storage_info");
        string storageStrJsonData = storageJsonData.text;                             //# 데이터를 문자열로 가져와서
        var json = JSON.Parse(storageStrJsonData); //배열 형태로 자동 파싱         //# SimpleJSON을 통해 객체로 생성
    
        // 창고 정보 파싱
        // 그 중 옷장 정보 파싱

        for(int i=0; i<json["옷장"].Count; i++)
        {
            clothInfo.name = json["옷장"][i]["이름"].ToString();
            clothInfo.type = json["옷장"][i]["타입"].ToString();
            clothInfo.price = json["옷장"][i]["가격"].AsInt;
            clothInfo.description = json["옷장"][i]["설명"].AsInt;

            clothList.Add(clothInfo);
        }        
    }

    public void SaveStorageJSONData()   //데이터 저장. (클래스 -> JSON 파일)
    {
        //수정 및 업데이트 - JSON 파일에 저장하기
            // 수정된 데이터를 JSON 파일에 저장하기
        JSONObject storageJson = new JSONObject();

        // 옷 정보     ===========================
        JSONArray clothArray = new JSONArray();
        foreach(ClothInfo cloth in clothList)
        {
            JSONObject clothObject = new JSONObject();
            clothObject.Add("이름", cloth.name);
            clothObject.Add("타입", cloth.type);
            clothObject.Add("가격", cloth.price);
            clothObject.Add("설명", cloth.description);

            clothArray.Add(clothObject);
        }
        storageJson.Add("옷장", clothArray);
    
        // JSON 파일로 저장     ===========================
        string storageJsonString = storageJson.ToString();
        System.IO.File.WriteAllText(Application.dataPath + "/Resources/storage_info.json", storageJsonString);
    }
}
