using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;


/// </summary>
public class StartGamePopup : Popup
{
#pragma warning disable 649
    [SerializeField] private Text levelText;

    [SerializeField] private Sprite enabledStarSprite;

    [SerializeField] private Image star1Image;

    [SerializeField] private Image star2Image;

    [SerializeField] private Image star3Image;

    [SerializeField] private GameObject goalPrefab;

    [SerializeField] private GameObject goalGroup;

    [SerializeField] private Text goalText;

    [SerializeField] private Text scoreGoalTitleText;

    [SerializeField] private Text scoreGoalAmountText;
#pragma warning restore 649

    private int numLevel;

    /// <summary>
    /// Unity's Awake method.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        Assert.IsNotNull(levelText);
        Assert.IsNotNull(enabledStarSprite);
        Assert.IsNotNull(star1Image);
        Assert.IsNotNull(star2Image);
        Assert.IsNotNull(star3Image);
        Assert.IsNotNull(goalPrefab);
        Assert.IsNotNull(goalGroup);
        Assert.IsNotNull(goalText);
        Assert.IsNotNull(scoreGoalTitleText);
        Assert.IsNotNull(scoreGoalAmountText);
    }

    /// <summary>
    /// Loads the LevelData data corresponding to the specified LevelData number.
    /// </summary>
    /// <param name="levelNum">The number of the LevelData to load.</param>
    public void LoadLevelData(int levelNum)
    {
        numLevel = levelNum;

        var level = LevelDataRegistory.Instance.GetLevelData(1);
        levelText.text = "LevelData " + numLevel;
        var stars = PlayerPrefs.GetInt("level_stars_" + numLevel);
        if (stars == 1)
        {
            star1Image.sprite = enabledStarSprite;
        }
        else if (stars == 2)
        {
            star1Image.sprite = enabledStarSprite;
            star2Image.sprite = enabledStarSprite;
        }
        else if (stars == 3)
        {
            star1Image.sprite = enabledStarSprite;
            star2Image.sprite = enabledStarSprite;
            star3Image.sprite = enabledStarSprite;
        }

        var reachScoreGoal = level.goal;
        if (reachScoreGoal != null)
        {
            goalText.gameObject.SetActive(false);
            //      scoreGoalAmountText.text = ((ReachScoreGoal)reachScoreGoal).score.ToString();
        }
        else
        {
            scoreGoalTitleText.gameObject.SetActive(false);
            scoreGoalAmountText.gameObject.SetActive(false);
        }
    }

    public void OnPlayButtonPressed()
    {
        GetComponent<SceneTransition>().PerformTransition();
    }

    public void OnCloseButtonPressed()
    {
        Close();
    }
}
