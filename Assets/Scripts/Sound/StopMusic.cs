using System.Collections;
using UnityEngine;

public class StopMusic : MonoBehaviour
{
    public AudioSource backgroundMusic; 
    public float stopTime     = 240f;   
    public float fadeDuration = 3f;  

    void Start()
    {
        if (backgroundMusic == null)
        {
            Debug.LogWarning("StopMusic: no hay AudioSource asignado.");
            return;
        }

        backgroundMusic.loop = true;   
        backgroundMusic.Play();
        Invoke(nameof(StartFadeOut), stopTime);
    }

    void StartFadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    System.Collections.IEnumerator FadeOutCoroutine()
    {
        float startVolume = backgroundMusic.volume;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            backgroundMusic.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        backgroundMusic.Stop();
        backgroundMusic.volume = startVolume;
    }
}