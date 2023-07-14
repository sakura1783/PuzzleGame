using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// ゲームの進行状況
    /// </summary>
    public enum GameState
    {
        Select,  //干支の選択中
        Ready,  //ゲームの準備中
        Play,
        Result,  
    }

    public GameState gameState = GameState.Select;

    [SerializeField] private Eto etoPrefab;

    [SerializeField] private Transform etoSetTran;

    [SerializeField] private float maxRotateAngle = 35.0f;  //干支生成時の最大回転角度

    [SerializeField] private float maxRange = 400.0f;  //干支生成時の左右のランダム幅

    [SerializeField] private float fallPos = 1400.0f;  //干支生成時の落下位置

    [SerializeField] private List<Eto> etoList = new List<Eto>();  //生成された干支のリスト

    //[SerializeField] private Sprite[] etoSprites;  //干支の画像データ

    private Eto firstSelectEto;  //最初にドラッグした干支の情報

    private Eto lastSelectEto;  //最後にドラッグした干支の情報

    private EtoType? currentEtoType;  //最初にドラッグした干支の種類

    [SerializeField] private List<Eto> eraseEtoList = new List<Eto>();  //削除対象となる干支を登録するリスト

    [SerializeField] private int linkCount = 0;  //繋がっている干支の数

    public float etoDistance = 1.0f;  //スワイプでつながる干支の範囲

    [SerializeField] private UIManager uiManager;

    private float timer;

    [SerializeField] private ResultPopUp resultPop;

    [SerializeField] private List<GameData.EtoData> selectedEtoDataList = new List<GameData.EtoData>();  //今回のゲームで生成する干支の種類

    [SerializeField] private GameObject eraseEffectParticle;

    [SerializeField] private EtoSelectPopUp etoSelectPop;


    IEnumerator Start()
    {
        AudioManager.instance.PreparePlayBGM(0);

        StartCoroutine(TransitionManager.instance.FadePanel(0));

        GameData.instance.InitGame();

        gameState = GameState.Select;

        yield return StartCoroutine(uiManager.SetUpUIManager());

        //干支の画像を読み込む。この処理が終了するまで、次の処理へは行かないようにする
        //yield return StartCoroutine(LoadEtoSprites());

        //もし干支データのリストが作成されていなければ
        if (GameData.instance.etoDataList.Count == 0)
        {
            //干支データの作成。この処理が終了するまで、次の処理へはいかないようにする
            yield return StartCoroutine(GameData.instance.InitEtoDataList());
        }

        yield return StartCoroutine(etoSelectPop.CreateEtoButtons(this));

        //今回のゲームに登場する干支をランダムで選択。この処理が終了するまで、次の処理へはいかないようにする
        //yield return StartCoroutine(SetUpEtoTypes(GameData.instance.etoTypeCount));

        //uiManager.UpdateDisplayGameTime(GameData.instance.gameTime);

        //引数で指定した数の干支を生成する
        //StartCoroutine(CreateEtos(GameData.instance.createEtoCount));
    }

    void Update()
    {
        if (gameState != GameState.Play)
        {
            return;
        }

        //干支を繋げる処理
        if (Input.GetMouseButtonDown(0) && firstSelectEto == null)
        {
            OnStartDrag();
        }
        //干支のドラッグをやめた(指を離した)際の処理
        else if (Input.GetMouseButtonUp(0))
        {
            OnEndDrag();
        }
        //上のif文がtrueでない場合、(つまりマウスの左ボタンがクリックされたがfirstSelectEtoがnullではない場合)
        else if (firstSelectEto != null)
        {
            //干支のドラッグ(スワイプ)中の処理
            OnDragging();
        }

        //ゲームの残り時間の表示更新処理
        timer += Time.deltaTime;

        if (timer >= 1)
        {
            timer = 0;

            GameData.instance.gameTime--;
        }
        if (GameData.instance.gameTime <= 0)
        {
            GameData.instance.gameTime = 0;

            //ゲーム終了
            StartCoroutine(GameUp());
        }

        uiManager.UpdateDisplayGameTime(GameData.instance.gameTime);
    }

    /// <summary>
    /// ゲームの準備(Startメソッドで削除した処理をこっちに移行)
    /// </summary>
    /// <returns></returns>
    public IEnumerator PrepareGame()
    {
        gameState = GameState.Ready;

        uiManager.UpdateDisplayGameTime(GameData.instance.gameTime);

        //ゲームに登場させる干支の種類を設定する
        yield return StartCoroutine(SetUpEtoTypes(GameData.instance.etoTypeCount));

        //GameDataのselectSkillTypeを渡して、スキルボタンに登録するメソッド(スキル実行時の処理)を設定
        yield return StartCoroutine(SetUpSkill(GameData.instance.selectSkillType));

        //引数で指定した数の干支を生成
        StartCoroutine(CreateEtos(GameData.instance.createEtoCount));
    }

    //private IEnumerator LoadEtoSprites()
    //{
    //    //配列の初期化(12個の画像が入るようにSprite型の配列を12個用意する)
    //    etoSprites = new Sprite[(int)EtoType.Count];

    //    //Resources.LoadAllを行い、分割されている干支の画像を順番に全て読み込んで配列に代入
    //    etoSprites = Resources.LoadAll<Sprite>("Sprites/Eto");

    //    //1つのファイルを12分割していない場合には、以下の処理を行う。12分割している場合には使用しない
    //    //for (int i = 0; i < etoSprites.Length; i++)
    //    //{
    //    //    etoSprites[i] = Resources.Load<Sprite>("Sprites/Eto_" + i);
    //    //}

    //    yield break;
    //}

    /// <summary>
    /// ゲームに登場させる干支の種類を設定する
    /// </summary>
    /// <param name="typeCount"></param>
    /// <returns></returns>
    private IEnumerator SetUpEtoTypes(int typeCount)
    {
        //新しくリストを用意して初期化に合わせてetoDataListを複製して、干支の候補リストとする
        List<GameData.EtoData> candidateEtoDataList = new List<GameData.EtoData>(GameData.instance.etoDataList);

        //選択中の干支を探して生成する干支のリストに追加
        GameData.EtoData myEto = candidateEtoDataList.Find((x) => x.etoType == GameData.instance.selectedEtoData.etoType);
        selectedEtoDataList.Add(myEto);
        candidateEtoDataList.Remove(myEto);
        typeCount--;

        //干支を指定数だけランダムに選ぶ(干支の種類は重複させない)
        while (typeCount > 0)
        {
            int randomValue = Random.Range(0, candidateEtoDataList.Count);

            //今回のゲームで生成する干支リストに追加
            selectedEtoDataList.Add(candidateEtoDataList[randomValue]);

            //干支のリストから選択された干支の情報を削除(干支を重複させないため)
            candidateEtoDataList.Remove(candidateEtoDataList[randomValue]);

            typeCount--;

            yield return null;
        }
    }

    /// <summary>
    /// 干支を生成
    /// </summary>
    /// <param name="generateCount"></param>
    /// <returns></returns>
    private IEnumerator CreateEtos(int count)
    {
        uiManager.ActivateShuffleButton(false);

        for (int i = 0; i < count; i++)
        {
            Eto eto = Instantiate(etoPrefab, etoSetTran, false);

            //生成された干支の回転角度を設定(色々な角度になるように)
            eto.transform.rotation = Quaternion.AngleAxis(Random.Range(-maxRotateAngle, maxRotateAngle), Vector3.forward);  //AngleAxis(回転角度, 回転の軸)

            //生成位置をランダムにして落下位置を変化させる
            eto.transform.localPosition = new Vector2(Random.Range(-maxRange, maxRange), fallPos);

            int randomValue = Random.Range(0, selectedEtoDataList.Count);

            eto.SetUpEto(selectedEtoDataList[randomValue].etoType, selectedEtoDataList[randomValue].sprite);

            etoList.Add(eto);

            yield return new WaitForSeconds(0.03f); 
        }

        uiManager.ActivateShuffleButton(true);

        //gameStateが準備中の時だけGameStateをPlayに変更
        if (gameState == GameState.Ready)
        {
            gameState = GameState.Play;
        }
    }

    /// <summary>
    /// 干支を最初にドラッグした際の処理
    /// </summary>
    private void OnStartDrag()
    {
        //画面をタップした際の位置情報を、CameraクラスのScreenToWorldPointメソッドを利用してCanvas上の位置に変換
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        //干支が繋がっている数を初期化
        linkCount = 0;

        //変換した座標のコライダーをもつゲームオブジェクトがあるか確認
        if (hit.collider != null)
        {
            //ゲームオブジェクトがあった場合、そのゲームオブジェクトがEtoクラスを持っているかどうか確認
            if (hit.collider.gameObject.TryGetComponent(out Eto dragEto))
            {
                //最初にドラッグした干支の情報を変数に代入
                firstSelectEto = dragEto;

                //最後にドラッグした干支の情報を変数に代入(最初のドラッグなので、最後のドラッグも同じ干支)
                lastSelectEto = dragEto;

                //最初にドラッグしている干支の種類を代入 = 後ほど、この情報を使って繋がる干支かどうかを判別する
                currentEtoType = dragEto.etoType;

                //干支の状態が「選択中」であると更新
                dragEto.isSelected = true;

                //干支に何番目に選択されているのか、通し番号を登録
                dragEto.num = linkCount;

                //削除する対象の干支を登録するリストを初期化
                eraseEtoList = new List<Eto>();

                //ドラッグ中の干支を削除の対象としてリストに追加
                AddEraseEtoList(dragEto);
            }
        }
    }

    /// <summary>
    /// 干支のドラッグ(スワイプ)中の処理
    /// </summary>
    private void OnDragging()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        //Rayの戻り値があり(hit変数がnullではない)、hit変数のゲームオブジェクトがEtoクラスを持っていたら
        if (hit.collider != null && hit.collider.gameObject.TryGetComponent(out Eto dragEto))
        {
            //現在選択中の干支の種類がnullなら処理しない
            if (currentEtoType == null)
            {
                return;
            }

            //dragEto変数の干支の種類が最初に選択した干支の種類と同じであり、最後にタップしている干支と現在の干支が違うオブジェクトであり、かつ、現在の干支がすでに「選択中」でなければ
            if (dragEto.etoType == firstSelectEto.etoType && lastSelectEto != dragEto && !dragEto.isSelected)
            {
                //現在タップしている干支の位置情報と最後にタップした干支の位置情報を比べて、差分の値(干支同士の距離)を取る
                float distance = Vector2.Distance(dragEto.transform.position, lastSelectEto.transform.position);

                //干支同士の距離が設定値よりも小さければ(2つの干支が離れていなければ)、干支を繋げる
                if (distance < etoDistance)
                {
                    //現在の干支を選択中にする
                    dragEto.isSelected = true;

                    //最後に選択している干支を現在の干支に更新
                    lastSelectEto = dragEto;

                    //干支のつながった数のカウントを1つ増やす
                    linkCount++;

                    //干支に通し番号を設定
                    dragEto.num = linkCount;

                    //削除リストに現在の干支を追加
                    AddEraseEtoList(dragEto);
                }
            }

            //削除リストに2つ以上の干支が追加されている場合  (このif文内のdragEtoは1つ前の干支に戻った時に選択している干支のことを指す。上の158行目のdragEtoとは違う処理(干支)なので誤解しないよう注意)
            if (eraseEtoList.Count > 1)
            {
                //条件に合致する場合、削除リストから干支を除外する(ドラッグしたまま一つ前の干支に戻る場合、現在の干支を削除リストから除外する)
                if (eraseEtoList[linkCount - 1] != lastSelectEto && eraseEtoList[linkCount - 1].num == dragEto.num && dragEto.isSelected)
                {
                    //選択中の干支を削除リストから取り除く
                    RemoveEraseEtoList(lastSelectEto);

                    lastSelectEto.GetComponent<Eto>().isSelected = false;

                    //最後の干支の情報を、前の干支に戻す
                    lastSelectEto = dragEto;

                    //つながっている干支の数を減らす
                    linkCount--;
                }
            }   
        }
    }

    /// <summary>
    /// 干支のドラッグをやめた(指を画面から離した)際の処理
    /// </summary>
    private void OnEndDrag()
    {
        //つながっている干支が3つ以上あったら削除する処理に移る
        if (eraseEtoList.Count >= 3)
        {
            //選択されている干支を消す
            for (int i = 0; i < eraseEtoList.Count; i++)
            {
                //干支リストから取り除く
                etoList.Remove(eraseEtoList[i]);

                //干支の削除エフェクト生成
                GameObject particle = Instantiate(eraseEffectParticle, eraseEtoList[i].gameObject.transform);

                //エフェクトの位置をetoSetTranに変更(干支の子オブジェクトのままだと、干支が破棄されると同時にエフェクトも破棄されてしまうため)
                particle.transform.SetParent(etoSetTran);

                //干支を削除
                Destroy(eraseEtoList[i].gameObject);
            }

            AudioManager.instance.PreparePlaySE(1);

            //スコアと消した干支の数の加算
            AddScores(currentEtoType, eraseEtoList.Count);

            //スキルポイント加算
            uiManager.AddSkillPoint(eraseEtoList.Count);

            //消した干支の数だけ新しい干支をランダムに生成
            StartCoroutine(CreateEtos(eraseEtoList.Count));

            //削除リストを空にする
            eraseEtoList.Clear();
        }
        //つながっている干支が2つ以下の場合
        else
        {
            //削除リストから、削除候補であった干支を取り除く
            for (int i = 0; i < eraseEtoList.Count; i++)
            {
                //各干支の選択中の状態を解除する
                eraseEtoList[i].isSelected = false;

                ChangeEtoAlpha(eraseEtoList[i], 1.0f);
            }
        }

        //次回の干支を消す処理のために、各変数の値をnullにする
        firstSelectEto = null;
        lastSelectEto = null;
        currentEtoType = null;
    }

    /// <summary>
    /// 選択された干支を削除リストに追加
    /// </summary>
    /// <param name="dragEto"></param>
    private void AddEraseEtoList(Eto dragEto)
    {
        eraseEtoList.Add(dragEto);

        ChangeEtoAlpha(dragEto, 0.5f);
    }

    /// <summary>
    /// 前の干支に戻った際に削除リストから削除
    /// </summary>
    /// <param name="dragEto"></param>
    private void RemoveEraseEtoList(Eto dragEto)
    {
        eraseEtoList.Remove(dragEto);

        ChangeEtoAlpha(dragEto, 1.0f);

        //干支の「選択中」がtrueの場合、falseにして選択中ではない状態に戻す
        if (dragEto.isSelected)
        {
            dragEto.isSelected = false;
        }
    }

    /// <summary>
    /// 干支のアルファ値を変更
    /// </summary>
    /// <param name="dragEto"></param>
    /// <param name="alphaValue"></param>
    private void ChangeEtoAlpha(Eto dragEto, float alphaValue)
    {
        //現在ドラッグしている干支のアルファ値を変更
        dragEto.imgEto.color = new Color(dragEto.imgEto.color.r, dragEto.imgEto.color.g, dragEto.imgEto.color.b, alphaValue);
    }

    /// <summary>
    /// スコアと消した干支の数を加算
    /// </summary>
    /// <param name="etoType"></param>
    /// <param name="count"></param>
    private void AddScores(EtoType? etoType, int count)
    {
        bool isSelectEto = false;

        //選択している干支の場合にはスコアを多く加算
        if (etoType == GameData.instance.selectedEtoData.etoType)
        {
            GameData.instance.score += Mathf.CeilToInt(GameData.instance.etoPoint * count * GameData.instance.etoRate);

            isSelectEto = true;
        }
        else
        {
            //スコアを加算
            GameData.instance.score += GameData.instance.etoPoint * count;
        }

        //消した干支の数を加算
        GameData.instance.eraseEtoCount += count;

        //画面の更新処理
        uiManager.UpdateDisplayScore(isSelectEto);
    }

    /// <summary>
    /// ゲーム終了処理
    /// </summary>
    /// <returns></returns>
    private IEnumerator GameUp()
    {
        //uiManager.ActivateShuffleButton(false);
        uiManager.InActiveButtons();  //シャッフルボタンとスキルボタンを非アクティブにして押せなくする

        //GameStateをResultに変更 = Updateの処理が動かなくなる
        gameState = GameState.Result;

        yield return new WaitForSeconds(1.5f);

        AudioManager.instance.PreparePlayBGM(2);

        //リザルトのポップアップを画面内に移動
        yield return StartCoroutine(MoveResultPopUp());
    }

    /// <summary>
    /// リザルトポップアップを画面内に移動
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveResultPopUp()
    {
        resultPop.transform.DOLocalMoveY(0, 1.0f).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                //リザルト表示(スコアと消した干支の数を)
                resultPop.DisplayResult(GameData.instance.score, GameData.instance.eraseEtoCount);
            });

        yield return new WaitForSeconds(1);
    }

    /// <summary>
    /// 選択されたスキルをボタンに登録
    /// </summary>
    /// <param name="skillType"></param>
    /// <returns></returns>
    private IEnumerator SetUpSkill(SkillType skillType)
    {
        yield return StartCoroutine(uiManager.SetUpSkillButton(GetSkill(skillType)));
    }

    /// <summary>
    /// スキルボタンに登録するスキルのメソッドを取得して戻す
    /// </summary>
    /// <param name="selectSkillType"></param>
    /// <returns></returns>
    public UnityAction GetSkill(SkillType selectSkillType)
    {
        switch (selectSkillType)
        {
            case SkillType.DeleteMaxEtoType:
                return DeleteMaxEtoType;

            //TODO スキルが増えた場合には追加する
        }

        return null;
    }

    /// <summary>
    /// 最も数の多い干支のタイプをまとめて削除する
    /// </summary>
    public void DeleteMaxEtoType()
    {
        //Dictionaryの宣言と定義。干支のタイプとその数を代入できるようにする
        Dictionary<EtoType, int> dictionary = new Dictionary<EtoType, int>();

        //リストの中から干支タイプごとにDictionaryの要素を作成(ここで5つの干支タイプごとにいくつ数があるかわかる)
        foreach (Eto eto in etoList)
        {
            if (dictionary.ContainsKey(eto.etoType))
            {
                //すでにある要素(干支のタイプ)の場合には数のカウントを加算
                dictionary[eto.etoType]++;
            }
            else
            {
                //まだ作られていない要素(干支のタイプ)の場合には新しく要素を作り、カウントを1する
                dictionary.Add(eto.etoType, 1);
            }
        }

        //Debug
        foreach (KeyValuePair<EtoType, int> keyValuePair in dictionary)
        {
            Debug.Log("干支：" + keyValuePair.Key + "数：" + keyValuePair.Value);
        }

        //Dictionaryを検索し、最も数の多い干支のタイプを見つけて、消す干支のタイプと数を決定 
        EtoType maxEtoType = dictionary.OrderByDescending(x => x.Value).First().Key;  //OrderByDescendingで要素を大きい順に並べる
        int removeNum = dictionary.OrderByDescending(x => x.Value).First().Value;

        Debug.Log("消す干支のタイプ：" + maxEtoType + "数：" + removeNum);

        //対象の干支を破壊
        for (int i = 0; i < etoList.Count; i++)
        {
            if (etoList[i].etoType == maxEtoType)
            {
                //演出用のパーティクル生成
                GameObject particle = Instantiate(eraseEffectParticle, etoList[i].gameObject.transform);
                particle.transform.SetParent(etoSetTran);

                Destroy(etoList[i].gameObject);
            }
        }

        AudioManager.instance.PreparePlaySE(1);

        //etoListから対象の干支を削除
        etoList.RemoveAll(x => x.etoType == maxEtoType);

        //点数と消した干支の加算
        AddScores(maxEtoType, removeNum);

        //破壊した干支の数だけ干支を生成
        StartCoroutine(CreateEtos(removeNum));
    }
}
