using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FoodSpace;    // Food 스크립트 내 enum 변수

namespace FoodSpace
{
    public enum FOOD_KIND 
    { 
        STRAWBERRY = 1, 
        BREAD, 
        DONUT, 
        COOKIE
    }
}

public class Food : MonoBehaviour   //@1-2
{
    public FOOD_KIND foodKind;
    private Rigidbody2D rigidbody2d;

    void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    public void ExplosionBlow(float bombForce)
    {
        rigidbody2d.AddForce(Vector2.up * bombForce);
        
    }


}
