
using UnityEngine;
using UnityEngine.Assertions;

public class SoundFx : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(AudioClip clip, bool loop = false)
    {
        if (clip == null)
        {
            return;
        }

        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.Play();
        Invoke("DisableSoundFx", clip.length + 0.1f);
    }

    private void DisableSoundFx()
    {
        GetComponent<PooledObject>().pool.ReturnObject(gameObject);
    }
}
