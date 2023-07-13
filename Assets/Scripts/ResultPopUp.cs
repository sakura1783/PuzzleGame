using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ResultPopUp : MonoBehaviour
{
    [SerializeField] private Text txtScore;
    [SerializeField] private Text txtEraseEtoCount;

    [SerializeField] private Button btnClosePop;

    //[SerializeField] private CanvasGroup btnClosePopCanvasGroup;

    private float posY;  //ResultPopUpのY軸位置保存用(元の位置に戻す際に使う)


    void Start()
    {
        btnClosePop.interactable = false;
        btnClosePop.image.raycastTarget = false;
        //btnClosePopCanvasGroup.blocksRaycasts = false;

        posY = transform.position.y;

        btnClosePop.onClick.AddListener(OnClickButtonClosePopUp);

        btnClosePop.gameObject.GetComponent<CanvasGroup>().alpha = 0;
    }

    /// <summary>
    /// ゲーム結果(点数と消した干支の数)をアニメ表示
    /// </summary>
    public void DisplayResult(int score, int eraseEtoCount)
    {
        //計算用の初期値を設定
        //int initValue = 0;

        //DOTweenのSequence(シーケンス)機能を初期化して使えるようにする
        Sequence sequence = DOTween.Sequence();

        //シーケンスを利用して、DOTweenの処理を制御したい順番で記述する。まずは①スコアの表示をアニメして表示
        //sequence.Append(DOTween.To(() => initValue,
        //    (num) => 
        //    {
        //        initValue = num;
        //        txtScore.text = num.ToString();  //initValueをscoreまで、1秒でアニメーションさせながら表示する処理。numは現在の値を示す
        //    },
        //    score, 1.0f).OnComplete(() => { initValue = 0; }).SetEase(Ease.InCirc));  //次の処理のためにOnCompleteを使ってinitValueを初期化
        sequence.Append(txtScore.DOCounter(0, score, 1.0f).SetEase(Ease.InCirc));

        //②シーケンス処理を0.5秒だけ待機
        sequence.AppendInterval(0.5f);

        //③消した干支の数をアニメして表示
        //sequence.Append(DOTween.To(() => initValue,
        //    (num) =>
        //    {
        //        initValue = num;
        //        txtEraseEtoCount.text = num.ToString();
        //    },
        //    eraseEtoCount, 1.0f).SetEase(Ease.InCirc));
        sequence.Append(txtEraseEtoCount.DOCounter(0, eraseEtoCount, 1.0f).SetEase(Ease.InCirc));

        //④シーケンス処理を0.5秒だけ待機
        sequence.AppendInterval(0.5f);

        //⑤透明になっているbtnClosePopUpとその子要素をCanvasGroupのAlphaを使用して徐々に表示
        sequence.Append(btnClosePop.gameObject.GetComponent<CanvasGroup>().DOFade(1.0f, 1.0f).SetEase(Ease.Linear))
            .OnComplete(() =>
            {
                btnClosePop.image.raycastTarget = true;
                btnClosePop.interactable = true;
            });
    }

    /// <summary>
    /// btnClosePopUpを押した際の処理(リザルト表示を元の位置に戻して、ゲームを再スタートする)
    /// </summary>
    private void OnClickButtonClosePopUp()
    {
        btnClosePop.interactable = false;
        btnClosePop.image.raycastTarget = false;
        //btnClosePopCanvasGroup.blocksRaycasts = false;

        //ResultPopUpを画面外の位置に戻す
        transform.DOMoveY(posY, 1.0f);

        //ゲームの再スタート処理
        StartCoroutine(GameData.instance.RestartGame());
    }
}
