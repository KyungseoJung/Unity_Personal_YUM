using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGizmo : MonoBehaviour    //#8-1
{
    //기즈모 색상
    public Color Mycolor = Color.red;
    //기즈모 반지름
    public float Myradius = 0.5f;

    //유니티 콜백함수   //# 함수 이름 오타 없도록 주의해야 한대
    void OnDrawGizmos()
    {
        Gizmos.color = Mycolor;
        Gizmos.DrawSphere(transform.position, Myradius);
    }
    
}
