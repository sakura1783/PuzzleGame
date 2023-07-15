using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private CircleCollider2D circleCol;  //周りの干支を消す用のコライダー

    private GameManager gameManager;  //GameManagerクラスの情報を扱うための変数


    void Start()
    {
        circleCol.enabled = false;
    }

    /// <summary>
    /// ボムをタップした際の処理
    /// </summary>
    public void OnClickBomb()
    {
        //タップした時にenabledをオンにして、周りの干支を削除できるようにする
        circleCol.enabled = true;

        AudioManager.instance.PreparePlaySE(1);
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.TryGetComponent(out Eto eto))
        {
            //Eto[] count = col.GetComponents<Eto>();

            //演出用のパーティクル生成
            GameObject particle = Instantiate(gameManager.eraseEffectParticle, eto.transform);
            particle.transform.SetParent(gameManager.etoSetTran);

            //etoListからeto変数を削除する
            gameManager.etoList.Remove(eto);

            //スコアとスキルポイントを加算
            gameManager.AddScores(eto.etoType, 1);
            gameManager.UiManager.AddSkillPoint(1);

            Destroy(eto.gameObject);

            //削除した分、干支を生成する
            StartCoroutine(gameManager.CreateEtos(1));

            //干支を削除した後にボムも消す
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// GameManagerクラスの情報を取得する
    /// </summary>
    /// <param name="gameManager"></param>
    public void GetGameManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }
}
