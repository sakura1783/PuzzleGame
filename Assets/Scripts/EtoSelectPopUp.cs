using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EtoSelectPopUp : MonoBehaviour
{
    [SerializeField] private EtoButton etoButtonPrefab;

    [SerializeField] private Transform etoButtonTran;

    public Button btnStart;

    public CanvasGroup canvasGroup;

    private List<EtoButton> etoButtonList = new List<EtoButton>();

    private GameManager gameManager;


    /// <summary>
    /// 干支ボタンの生成
    /// </summary>
    /// <param name="gameManager"></param>
    /// <returns></returns>
    public IEnumerator CreateEtoButtons(GameManager gameManager)
    {
        btnStart.interactable = false;

        this.gameManager = gameManager;

        //干支データをもとに干支ボタンを作成
        for (int i = 0; i < (int)EtoType.Count; i++)
        {
            EtoButton etoButton = Instantiate(etoButtonPrefab, etoButtonTran, false);

            etoButton.SetUpEtoButton(this, GameData.instance.etoDataList[i]);

            if (i == 0)
            {
                etoButton.imgEto.color = new Color(0.65f, 0.65f, 0.65f);
                GameData.instance.selectedEtoData = GameData.instance.etoDataList[i];
            }

            etoButtonList.Add(etoButton);

            yield return new WaitForSeconds(0.15f);
        }

        btnStart.onClick.AddListener(OnClickButtonStart);

        btnStart.interactable = true;

        Debug.Log("Init Eto Buttons");

        yield break;
    }

    /// <summary>
    /// btnStartを押した際の処理
    /// </summary>
    private void OnClickButtonStart()
    {
        btnStart.interactable = false;

        AudioManager.instance.PreparePlaySE(0);

        AudioManager.instance.PreparePlayBGM(1);

        StartCoroutine(gameManager.PrepareGame());

        canvasGroup.DOFade(0, 0.5f);
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// 選択されたボタンは灰色に、他のボタンは通常の色に変更する
    /// </summary>
    /// <param name="etoType"></param>
    public void ChangeColorToEtoButton(EtoType etoType)
    {
        for (int i = 0; i < etoButtonList.Count; i++)
        {
            if (etoButtonList[i].etoData.etoType == etoType)
            {
                etoButtonList[i].imgEto.color = new Color(0.65f, 0.65f, 0.65f);
            }
            else
            {
                etoButtonList[i].imgEto.color = new Color(1, 1, 1);
            }
        }
    }
}
