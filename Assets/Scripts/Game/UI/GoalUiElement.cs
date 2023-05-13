using UnityEngine;
using UnityEngine.UI;
using GameVanilla.Core;
using GameVanilla.Game.Common;

public class GoalUiElement : MonoBehaviour
{
    public Image image;
    public Text amountText;
    public Image tickImage;
    public Image crossImage;
    public ParticleSystem shineParticles;
    public ParticleSystem starParticles;

    public bool isCompleted { get; private set; }

    private Goal currentGoal;
    private int targetAmount;
    private int currentAmount;

    private void Awake()
    {
        tickImage.gameObject.SetActive(false);
        crossImage.gameObject.SetActive(false);
    }

    public virtual void Fill(Goal goal)
    {
        currentGoal = goal;
    }

    public virtual void UpdateGoal(GameState state)
    {
    }

    public void SetCompletedTick(bool completed)
    {
        isCompleted = completed;
        amountText.gameObject.SetActive(false);
        if (completed)
        {
            tickImage.gameObject.SetActive(true);
            image.GetComponent<Animator>().SetTrigger("GoalAchieved");
            tickImage.GetComponent<Animator>().SetTrigger("GoalAchieved");
            shineParticles.Play();
            starParticles.Play();
        }
        else
        {
            crossImage.gameObject.SetActive(true);
        }
    }
}