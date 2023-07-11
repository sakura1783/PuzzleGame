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


    IEnumerator Start()
    {
        //干支の画像を読み込む。この処理が終了するまで、次の処理へは行かないようにする
        yield return StartCoroutine(LoadEtoSprites());

        //引数で指定した数の干支を生成する
        StartCoroutine(CreateEtos(GameData.instance.createEtoCount));
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
}
