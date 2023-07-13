using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class TransitionManager : MonoBehaviour
{
    public static TransitionManager instance;

    [SerializeField] private CanvasGroup canvasGroup;

    public float duration;


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
    }

    /// <summary>
    /// Panelの透明度を操作してゲーム画面のフェード処理を行う
    /// </summary>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public IEnumerator FadePanel(float alpha)
    {
        canvasGroup.DOFade(alpha, duration).SetEase(Ease.Linear);

        yield return new WaitForSeconds(duration);
    }
}
