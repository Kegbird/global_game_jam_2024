using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource _theme_audio_source;
    [SerializeField]
    private AudioSource _sound_fx_audio_source;
    [SerializeField]
    private AudioClip[] _sound_fx_audio_clip;


    private void Start()
    {
        StartCoroutine(RaiseThemeMusic());
    }

    public IEnumerator FadeThemeMusic()
    {
        float offset = 0.5f;
        float i = 0;
        while (i <= 1f)
        {
            i += Time.deltaTime;
            _theme_audio_source.volume = 1f - i - offset;
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator RaiseThemeMusic()
    {
        float offset = 0.5f;
        float i = 1f;
        while (i >= 0)
        {
            i -= Time.deltaTime;
            _theme_audio_source.volume = 1f - i - offset;
            yield return new WaitForEndOfFrame();
        }
    }

    public void PlaySoundFx(int index, float volume)
    {
        _sound_fx_audio_source.volume = volume;
        _sound_fx_audio_source.PlayOneShot(_sound_fx_audio_clip[index]);
    }
}
