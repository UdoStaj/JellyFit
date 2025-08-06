using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardItem : MonoBehaviour
{
    public List<Sprite> imageList = new List<Sprite>();

    public Image Image;
    public TMP_Text NameText;
    public TMP_Text ScoreText;
    public TMP_Text RankText;
    public Image BackgroundImage;
    public GameObject glowImage;

    public void SetData(int rank, string name, int score)
    {
        RankText.text = rank.ToString();
        NameText.text = name;
        ScoreText.text = score.ToString();
        Image.sprite = imageList[Random.Range(0, imageList.Count)];

        if(name == "Yomi")
        {
            BackgroundImage.color = new Color32(255, 0, 0, 255); // Change background for "You"
            glowImage.SetActive(true); // Show glow effect for "You"
        }
    }
}
