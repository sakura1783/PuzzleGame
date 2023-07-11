using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text txtScore;
    [SerializeField] private Text txtTimer;


    /// <summary>
    /// 画面表示スコアの更新処理
    /// </summary>
    public void UpdateDisplayScore()
    {
        txtScore.text = GameData.instance.score.ToString();
    }

    public void UpdateDisplayGameTime(float time)
    {
        txtTimer.text = time.ToString("F0");
    }
}
