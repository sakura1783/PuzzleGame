using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum BGMType
{
    select,
    Game,
    Result,
}

public enum SEType
{
    OK,
    Erase,
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioSource[] bgmAudioSources;
    [SerializeField] private AudioSource[] seAudioSources;

    private AudioSource playingBGM;  //現在流れているBGM
    private AudioSource playingSE;


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
    /// GBM再生　BGM再生時はこのメソッドを呼び出す
    /// </summary>
    public void PreparePlayBGM(int index)
    {
        StartCoroutine(PlayBGM(index));
    }

    /// <summary>
    /// BGM再生
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayBGM(int index)
    {
        //別の曲が流れている場合、徐々にボリュームを下げる
        foreach (AudioSource audioSource in bgmAudioSources)
        {
            if (audioSource.isPlaying)
            {
                playingBGM = audioSource;

                playingBGM.DOFade(0, 0.75f);
            }
        }

        //前の曲のボリュームが下がるのを待つ
        yield return new WaitForSeconds(0.5f);

        //新しい指定された曲を再生
        bgmAudioSources[index].Play();
        bgmAudioSources[index].DOFade(1, 0.75f);

        if (playingBGM != null)
        {
            //前に流れていた曲のボリュームが0になったら、その(前の)BGMの再生を停止
            yield return new WaitUntil(() => playingBGM.volume == 0);

            playingBGM.Stop();
        }
    }

    /// <summary>
    /// SE再生　SE再生時はこのメソッドを呼び出す
    /// </summary>
    /// <param name="index"></param>
    public void PreparePlaySE(int index)
    {
        StartCoroutine(PlaySE(index));
    }

    /// <summary>
    /// SE再生
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private IEnumerator PlaySE(int index)
    {
        seAudioSources[index].Play();

        playingSE = seAudioSources[index];

        yield return new WaitUntil(() => playingSE.volume == 0);

        playingSE.Stop();
    }
}
