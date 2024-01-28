using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneRedirect : MonoBehaviour
{
    [SerializeField]
    private Image _black_screen;
    [SerializeField]
    private AudioSource _theme_audio_source;
    [SerializeField]
    private int _target_scene;

    private void Start()
    {
        StartCoroutine(RedirectCoroutine());
    }

    private IEnumerator RedirectCoroutine()
    {
        float offset = 0.5f;
        for (float i = 1f; i >= 0; i -= Time.deltaTime)
        {
            //_theme_audio_source.volume = 1f - i - offset;
            _black_screen.color = new Color(0, 0, 0, i / 1f);
            yield return new WaitForEndOfFrame();
        }
        _black_screen.color = new Color(0, 0, 0, 0f);
        yield return new WaitForSeconds(3f);
        for (float i = 0; i <= 1f; i += Time.deltaTime)
        {
            //_theme_audio_source.volume = 1f - i - offset;
            _black_screen.color = new Color(0, 0, 0, i / 1f);
            yield return new WaitForEndOfFrame();
        }
        _black_screen.color = new Color(0, 0, 0, 1f);
        SceneManager.LoadScene(_target_scene);
    }
}