using UnityEngine;

public class Shuffle : MonoBehaviour
{
    [SerializeField] private float shufflePower;

    [SerializeField] private Vector2 shuffleVelocity = new Vector2(10.0f, 10.0f);  //シャッフルの(目標)速度

    [SerializeField] private float duration = 1.0f;

    private CapsuleCollider2D capsuleCol;

    private float shuffleTimer;

    private UIManager uiManager;


    void Update()
    {
        //シャッフル中のみ、shuffleTimerをカウント
        shuffleTimer -= Time.deltaTime;

        if (shuffleTimer <= 0 && capsuleCol.enabled)
        {
            StopShuffle();
        }
    }

    /// <summary>
    /// シャッフルの初期設定
    /// </summary>
    /// <param name="uiManager"></param>
    public void SetUpShuffle(UIManager uiManager)
    {
        this.uiManager = uiManager;

        capsuleCol = GetComponent<CapsuleCollider2D>();

        //シャッフル用のコライダーをオフにしておく
        capsuleCol.enabled = false;
    }

    /// <summary>
    /// シャッフル開始
    /// </summary>
    public void StartShuffle()
    {
        //コライダーをオンにして干支をシャッフルできるようにする
        capsuleCol.enabled = true;

        //シャッフル時間を設定
        shuffleTimer = duration;

        //シャッフルの方向をランダムで取得
        int value = Random.Range(0, 2);

        //シャッフルの方向をシャッフル速度のXに設定(-1 = 左方向、1 = 右方向)(value == 0がtrueであれば左、falseであれば右の値にxを設定する)
        shuffleVelocity.x = value == 0 ? shuffleVelocity.x *= -1 : shuffleVelocity.x *= 1;
    }

    /// <summary>
    /// シャッフル停止
    /// </summary>
    private void StopShuffle()
    {
        shuffleTimer = 0;

        //コライダーをオフにして干支への影響を無くす
        capsuleCol.enabled = false;

        //再度シャッフルボタンを押せるようにする
        //uiManager.ActivateShuffleButton(true);
    }

    /// <summary>
    /// シャッフルの実処理
    /// </summary>
    /// <param name="col"></param>
    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.TryGetComponent(out Rigidbody2D rb))
        {
            //干支に対するシャッフルの相対速度(シャッフルを行うために必要な速度の差分)を計算
            Vector2 relativeVelocity = shuffleVelocity - rb.velocity;

            //干支に力を加える(シャッフル)
            rb.AddForce(shufflePower * relativeVelocity);
        }
    }
}
