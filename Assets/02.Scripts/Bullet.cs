// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Bullet : MonoBehaviour
// {
//     //n초 뒤에 사라지도록 해당 오브젝트에 Desturctor 스크립트 추가
//     public enum WEAPON_TYPE {BULLET = 1, MUMMYMONSTER };      //@7-2
//     public WEAPON_TYPE weaponType;                //@7-2
//     [SerializeField]
//     private int weaponPower = 20;     //적에게 총 쏘면 가해지는 힘

//     void Start()
//     {
//         if(weaponType == WEAPON_TYPE.BULLET)
//             weaponPower = 10;
//         if(weaponType == WEAPON_TYPE.MUMMYMONSTER)
//             weaponPower = 50;
//     }
//     void OnCollisionEnter2D(Collision2D col) 
//     {
//         if(col.gameObject.tag == "Enemy")
//         {
//             col.gameObject.GetComponent<Enemy>().GetHurt(weaponPower);

//             Destroy(gameObject);    //적 맞추면 사라지도록
//         }
//     }
// }
