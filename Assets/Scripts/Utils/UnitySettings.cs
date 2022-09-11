using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitySettings : MonoBehaviour
{
    public static UnitySettings instance;
    [SerializeField]
    private int maxFrameRate = 60;

    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }



        //Set up settings
        Application.targetFrameRate = maxFrameRate;
    }
}
