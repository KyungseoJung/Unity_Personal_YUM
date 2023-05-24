using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  //@9-2

public class SoundLoadScene : MonoBehaviour
{
    void Awake()
    {
        SceneManager.LoadScene("scHome");  
    }
}
