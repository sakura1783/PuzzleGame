using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Bomb : MonoBehaviour
{
    //[SerializeField] private CircleCollider2D circleCol;  //周りの干支を消す用のコライダー

    //private GameManager gameManager;  //GameManagerクラスの情報を扱うための変数

    [SerializeField] private Button btnBomb;

    private float radius;

    [SerializeField] private bool onGizmos = true;


    //void Start()
    //{
    //    //circleCol.enabled = false;

    //    //Debug用のテスト処理
    //    //SetUpBomb(null, 1.0f);
    //}

    public void SetUpBomb(GameManager gameManager, float bombRadius)
    {
        if (TryGetComponent(out btnBomb))
        {
            btnBomb.onClick.AddListener(() => OnClickBomb(gameManager, bombRadius));
        }
        else
        {
            Debug.Log("ボムのボタンが取得できません");
        }

        //OnDrawGizmos用
        radius = bombRadius;
    }

    /// <summary>
    /// ボムをタップした際の処理
    /// </summary>
    public void OnClickBomb(GameManager gameManager, float bombRadius)
    {
        //タップした時にenabledをオンにして、周りの干支を削除できるようにする
        //circleCol.enabled = true;

        AudioManager.instance.PreparePlaySE(1);

        //List<Eto> eraseEtos = new();
        //Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, bombRadius);

        //foreach (Collider2D col in cols)
        //{
        //    if (col.TryGetComponent(out Eto eto))
        //    {
        //        eraseEtos.Add(eto);
        //    }
        //}

        //Linqを使用した場合
        List<Eto> eraseEtos = Physics2D.OverlapCircleAll(transform.position, bombRadius)
            .Select(col => col.GetComponent<Eto>())
            .Where(eto => eto != null)
            .ToList();

        //GameManagerへの干支の削除命令を出す
        if (gameManager != null)
        {
            gameManager.AddRangeEraseEtoList(eraseEtos);
        }

        Debug.Log("ボム実行");

        Destroy(gameObject);
    }

    //private void OnTriggerStay2D(Collider2D col)
    //{
    //    if (col.TryGetComponent(out Eto eto))
    //    {
    //        //Eto[] count = col.GetComponents<Eto>();

    //        //演出用のパーティクル生成
    //        GameObject particle = Instantiate(gameManager.eraseEffectParticle, eto.transform);
    //        particle.transform.SetParent(gameManager.etoSetTran);

    //        //etoListからeto変数を削除する
    //        gameManager.etoList.Remove(eto);

    //        //スコアとスキルポイントを加算
    //        gameManager.AddScores(eto.etoType, 1);
    //        gameManager.UiManager.AddSkillPoint(1);

    //        Destroy(eto.gameObject);

    //        //削除した分、干支を生成する
    //        StartCoroutine(gameManager.CreateEtos(1));

    //        //干支を削除した後にボムも消す
    //        Destroy(gameObject);
    //    }
    //}

    /// <summary>
    /// GameManagerクラスの情報を取得する
    /// </summary>
    /// <param name="gameManager"></param>
    //public void GetGameManager(GameManager gameManager)
    //{
    //    this.gameManager = gameManager;
    //}

    private void OnDrawGizmos()
    {
        if (!onGizmos)
        {
            return;
        }

        //コライダーの可視化
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
