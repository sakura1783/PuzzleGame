using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text txtScore;
    [SerializeField] private Text txtTimer;

    [SerializeField] private Button btnShuffle;
    [SerializeField] private Button btnSkill;

    [SerializeField] private Image imgSkillPoint;

    [SerializeField] private Shuffle shuffle;

    private Tweener tweener = null;  //DOTweenの処理を代入する変数

    private UnityEvent unityEvent;  //UnityEventとしてメソッドを代入する変数


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
        AudioManager.instance.PreparePlaySE(0);

        Debug.Log("シャッフルボタンが押されました");

        ActivateShuffleButton(false);

        //シャッフル開始
        shuffle.StartShuffle();
    }

    /// <summary>
    /// 選択した干支の持つスキルを登録
    /// </summary>
    /// <returns></returns>
    public IEnumerator SetUpSkillButton(UnityAction unityAction)
    {
        btnSkill.interactable = false;

        //スキルの登録がない場合、スキルボタンには何も登録しない
        if (unityAction == null)
        {
            yield break;
        }

        //UnityEvent初期化
        unityEvent = new UnityEvent();

        //UnityEventにUnityActionを登録(UnityActionにはメソッドが代入されている)
        unityEvent.AddListener(unityAction);

        btnSkill.onClick.AddListener(OnClickButtonSkill);
    }

    /// <summary>
    /// スキルポイント加算
    /// </summary>
    /// <param name="count">消した干支の数</param>
    public void AddSkillPoint(int count)
    {
        //FillAmountの現在地を取得
        float amount = imgSkillPoint.fillAmount;

        float value = amount += count * 0.05f;

        imgSkillPoint.DOFillAmount(value, 0.5f);

        //FillAmountが1になり、スキルボタンがアニメしていなければ
        if (imgSkillPoint.fillAmount >= 1 && tweener == null)
        {
            btnSkill.interactable = true;

            //ループ処理を行い、スキルボタンが押されるまでスケールを変化させるアニメを実行する
            tweener = imgSkillPoint.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.25f).SetEase(Ease.InCirc).SetLoops(-1, LoopType.Yoyo);
        }
    }

    /// <summary>
    /// btnSkillを押した際の処理
    /// </summary>
    public void OnClickButtonSkill()
    {
        btnSkill.interactable = false;

        AudioManager.instance.PreparePlaySE(0);

        //登録されているスキル(UnityActionに登録されているメソッド)を使用
        unityEvent.Invoke();

        imgSkillPoint.DOFillAmount(0, 1.0f);

        //スキルボタンのループアニメを破棄し、tweener変数をnullにする
        tweener.Kill();
        tweener = null;

        //スキルボタンのサイズをもとの大きさに戻す
        imgSkillPoint.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 複数のボタンを押せないようにする
    /// </summary>
    public void InActiveButtons()
    {
        btnShuffle.interactable = false;
        btnSkill.interactable = false;
    }
}
