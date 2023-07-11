using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Eto etoPrefab;

    [SerializeField] private Transform etoSetTran;

    [SerializeField] private float maxRotateAngle = 35.0f;  //干支生成時の最大回転角度

    [SerializeField] private float maxRange = 400.0f;  //干支生成時の左右のランダム幅

    [SerializeField] private float fallPos = 1400.0f;  //干支生成時の落下位置

    [SerializeField] private List<Eto> etoList = new List<Eto>();  //生成された干支のリスト

    [SerializeField] private Sprite[] etoSprites;  //干支の画像データ

    private Eto firstSelectEto;  //最初にドラッグした干支の情報

    private Eto lastSelectEto;  //最後にドラッグした干支の情報

    private EtoType? currentEtoType;  //最初にドラッグした干支の種類

    [SerializeField] private List<Eto> eraseEtoList = new List<Eto>();  //削除対象となる干支を登録するリスト

    [SerializeField] private int linkCount = 0;  //繋がっている干支の数

    public float etoDistance = 1.0f;  //スワイプでつながる干支の範囲


    IEnumerator Start()
    {
        //干支の画像を読み込む。この処理が終了するまで、次の処理へは行かないようにする
        yield return StartCoroutine(LoadEtoSprites());

        //引数で指定した数の干支を生成する
        StartCoroutine(CreateEtos(GameData.instance.createEtoCount));
    }

    void Update()
    {
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
    } 

    private IEnumerator LoadEtoSprites()
    {
        //配列の初期化(12個の画像が入るようにSprite型の配列を12個用意する)
        etoSprites = new Sprite[(int)EtoType.Count];

        //Resources.LoadAllを行い、分割されている干支の画像を順番に全て読み込んで配列に代入
        etoSprites = Resources.LoadAll<Sprite>("Sprites/Eto");

        //1つのファイルを12分割していない場合には、以下の処理を行う。12分割している場合には使用しない
        //for (int i = 0; i < etoSprites.Length; i++)
        //{
        //    etoSprites[i] = Resources.Load<Sprite>("Sprites/Eto_" + i);
        //}

        yield break;
    }

    /// <summary>
    /// 干支を生成
    /// </summary>
    /// <param name="generateCount"></param>
    /// <returns></returns>
    private IEnumerator CreateEtos(int generateCount)
    {
        for (int i = 0; i < generateCount; i++)
        {
            Eto eto = Instantiate(etoPrefab, etoSetTran, false);

            //生成された干支の回転角度を設定(色々な角度になるように)
            eto.transform.rotation = Quaternion.AngleAxis(Random.Range(-maxRotateAngle, maxRotateAngle), Vector3.forward);  //AngleAxis(回転角度, 回転の軸)

            //生成位置をランダムにして落下位置を変化させる
            eto.transform.localPosition = new Vector2(Random.Range(-maxRange, maxRange), fallPos);

            int randomValue = Random.Range(0, (int)EtoType.Count);

            eto.SetUpEto((EtoType)randomValue, etoSprites[randomValue]);

            etoList.Add(eto);

            yield return new WaitForSeconds(0.03f);
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

                //干支を削除
                Destroy(eraseEtoList[i].gameObject);
            }

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
}
