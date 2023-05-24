using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Underground : MonoBehaviour
{
    private Vector2 startPos;
    private Vector3 destPos;

    void OnTriggerEnter2D(Collider2D col) //이 콜라이더 안에 무언가 들어왔다면
    {
        destPos = col.gameObject.transform.position;
        destPos.y = 11f;   

        col.gameObject.transform.position = destPos;    //순간이동
    }
}
