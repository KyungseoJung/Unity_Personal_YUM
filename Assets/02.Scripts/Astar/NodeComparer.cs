using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//IComparer<in T> 인터페이스를 상속하여 Compare 함수 정의
public class NodeComparer : IComparer<Node>
{
    // 두 노드를 비교하여 소비값을 셋팅
    public int Compare(Node x, Node y)
    {
        // 소비되는 총 비용이 y 가 더 크다면 리턴 -1
        if (x.FCost < y.FCost)
            return -1;
        // 그 반대라면 리턴 1
        else if (x.FCost > y.FCost)
            return 1;
        // 만약 둘다 같다면
        else if (x.FCost == y.FCost)
        {
            // 현재 위치에서 목적지 까지의 비용이 y 가 더 크다면 리턴 -1
            if (x.HCost < y.HCost)
                return -1;
            // 그렇지않다면 리턴 1
            else if (x.HCost > y.HCost)
                return 1;
        }

        // 해당 조건에 만족하지 않다면 리턴 0
        return 0;
    }
}
