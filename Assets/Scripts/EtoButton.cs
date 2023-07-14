using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EtoButton : MonoBehaviour
{
    public GameData.EtoData etoData;  //このボタンの干支データ

    public Image imgEto;

    public Button btnEto;

    [SerializeField] private CanvasGroup canvasGroup;

    private EtoSelectPopUp etoSelectPop;


    /// <summary>
    /// 干支ボタンの初期設定
    /// </summary>
    public void SetUpEtoButton(EtoSelectPopUp etoSelectPop, GameData.EtoData etoData)
    {
        canvasGroup.alpha = 0;

        this.etoSelectPop = etoSelectPop;
        this.etoData = etoData;

        imgEto.sprite = this.etoData.sprite;

        btnEto.onClick.AddListener(() => StartCoroutine(OnClickEtoButton()));

        //アニメさせながらボタンを徐々に表示する
        Sequence sequence = DOTween.Sequence();
        sequence.Append(canvasGroup.DOFade(1, 0.25f).SetEase(Ease.Linear));
        sequence.Join(transform.DOPunchScale(new Vector3(1, 1, 1), 0.5f));
    }

    /// <summary>
    /// 干支ボタンをタップした際の処理
    /// </summary>
    /// <returns></returns>
    private IEnumerator OnClickEtoButton()
    {
        AudioManager.instance.PreparePlaySE(0);

        //選択した干支の情報をGameDataに保存
        GameData.instance.selectedEtoData = etoData;

        //干支ボタンをポップアニメさせる
        transform.DOScale(new Vector3(1.3f, 1.3f, 1.3f), 0.15f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.15f);
        transform.DOScale(new Vector3(1, 1, 1), 0.15f);

        etoSelectPop.ChangeColorToEtoButton(etoData.etoType);
    }
}
