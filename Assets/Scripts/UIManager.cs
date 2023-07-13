using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text txtScore;
    [SerializeField] private Text txtTimer;

    [SerializeField] private Button btnShuffle;

    [SerializeField] private Shuffle shuffle;


    /// <summary>
    /// UIManagerの初期設定
    /// </summary>
    /// <returns></returns>
    public IEnumerator SetUpUIManager()
    {
        ActivateShuffleButton(false);

        shuffle.SetUpShuffle(this);

        btnShuffle.onClick.AddListener(OnClickButtonShuffle);

        yield break;
    }

    /// <summary>
    /// 画面表示スコアの更新処理
    /// </summary>
    public void UpdateDisplayScore(bool isSelectEto = false)
    {
        if (isSelectEto)
        {
            //選択している干支の場合にはスコアを大きく表示する演出を入れる
            Sequence sequence = DOTween.Sequence();
            sequence.Append(txtScore.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.1f).SetEase(Ease.InCirc));
            sequence.AppendInterval(0.1f);
            sequence.Append(txtScore.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.Linear));
        }

        txtScore.text = GameData.instance.score.ToString();
    }

    /// <summary>
    /// 残り時間の表示更新処理
    /// </summary>
    /// <param name="time"></param>
    public void UpdateDisplayGameTime(float time)
    {
        txtTimer.text = time.ToString("F0");
    }

    /// <summary>
    /// シャッフルボタンの活性化/非活性化の切り替え
    /// </summary>
    /// <param name="isSwitch"></param>
    public void ActivateShuffleButton(bool isSwitch)
    {
        btnShuffle.interactable = isSwitch;
    }

    /// <summary>
    /// btnShuffleを押した際の処理
    /// </summary>
    private void OnClickButtonShuffle()
    {
        Debug.Log("シャッフルボタンが押されました");

        ActivateShuffleButton(false);

        //シャッフル開始
        shuffle.StartShuffle();
    }
}
