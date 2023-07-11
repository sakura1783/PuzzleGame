using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData instance;

    public int etoTypeCount;  //ゲームに登場する干支の最大種類数

    public int createEtoCount;  //ゲーム開始時に生成する干支の数

    public int score = 0;

    public int etoPoint = 100;

    public int eraseEtoCount = 0;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitGame();
    }

    /// <summary>
    /// ゲームの初期化
    /// </summary>
    private void InitGame()
    {
        score = 0;
        eraseEtoCount = 0;

        Debug.Log("Init Game");
    }
}
