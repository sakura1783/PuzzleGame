using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Eto : MonoBehaviour
{
    public EtoType etoType;

    public Image imgEto;

    public bool isSelected;  //スワイプされた干支である判定。trueの場合、この干支は削除対象となる

    public int num;  //スワイプされた通し番号。スワイプされた順番が代入される


    /// <summary>
    /// 干支の初期設定
    /// </summary>
    /// <param name="etoType"></param>
    /// <param name="sprite"></param>
    public void SetUpEto(EtoType etoType, Sprite sprite)
    {
        this.etoType = etoType;
        name = this.etoType.ToString();

        ChangeEtoImage(sprite);
    }

    /// <summary>
    /// 干支のイメージを変更
    /// </summary>
    /// <param name="sprite"></param>
    public void ChangeEtoImage(Sprite sprite)
    {
        imgEto.sprite = sprite;
    }
}
