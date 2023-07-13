using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameData : MonoBehaviour
{
    public static GameData instance;

    public int etoTypeCount;  //ゲームに登場する干支の最大種類数

    public int createEtoCount;  //ゲーム開始時に生成する干支の数

    public int score = 0;

    public int etoPoint = 100;

    public int eraseEtoCount = 0;  //消した干支の数

    [SerializeField] private int initTime = 60;  //1回のゲーム時間

    public float gameTime;  //残り時間

    [System.Serializable]
    public class EtoData
    {
        public EtoType etoType;

        public Sprite sprite;

        //コンストラクタ(インスタンス(new)時に用意している引数への値の代入を強制するメソッド)
        public EtoData(EtoType etoType, Sprite sprite)
        {
            this.etoType = etoType;
            this.sprite = sprite;
        }
    }

    public List<EtoData> etoDataList = new List<EtoData>();


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
        gameTime = initTime;

        Debug.Log("Init Game");
    }

    /// <summary>
    /// 現在のゲームシーンを再読み込み
    /// </summary>
    /// <returns></returns>
    public IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(1.0f);

        //現在のゲームシーンを取得し、シーンの名前を使ってLoadScene処理を行う(再度、同じゲームシーンを呼び出す)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        //初期化　GameDataゲームオブジェクトはシーン遷移されても破棄されない設定になっているので、ここで再度、初期化の設定を行う必要がある
        InitGame();
    }

    public IEnumerator InitEtoDataList()
    {
        //干支の画像を読み込むための変数を配列で用意(GameManagerの宣言フィールドで用意していたものを、このメソッド内のみで使用するように変更)
        Sprite[] etoSprites = new Sprite[(int)EtoType.Count];

        //Resources.LoadAllを使って分割されている干支の画像を順番に全て読み込んで配列に代入
        etoSprites = Resources.LoadAll<Sprite>("Sprites/Eto");

        //ゲームに登場する12種類の干支データを作成
        for (int i = 0; i < (int)EtoType.Count; i++)
        {
            //干支のデータを扱うクラスEtoDataをインスタンス(new EtoData())し、コンストラクタを使って値を代入
            EtoData etoData = new EtoData((EtoType)i, etoSprites[i]);

            //干支データをListに追加
            etoDataList.Add(etoData);
        }
        yield break;
    }
}
