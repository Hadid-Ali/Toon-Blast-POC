using UnityEngine;

public class BackgroundMusic : MonoBehaviourSingleton<BackgroundMusic>
{
    private AudioSource audioSource;

    protected override void SingletonAwake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        audioSource.Play();
    }
}