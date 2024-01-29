using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    public GameObject _title;
    [SerializeField]
    public GameObject _play_button;
    [SerializeField]
    public GameObject _credits_button;
    [SerializeField]
    public GameObject _tutorial_button;
    [SerializeField]
    public GameObject _tutorial_panel;
    [SerializeField]
    public Image _black_screen;
    [SerializeField]
    private SoundManager _sound_manager;
    [SerializeField]
    private GameObject _credits_panel;

    private void Start()
    {
        StartCoroutine(HideBlackScreen());
    }

    public void PlayButtonClick()
    {
        _sound_manager.PlaySoundFx(0, 0.25f);
        IEnumerator ShowBlackScreenAndPlay()
        {
            yield return StartCoroutine(ShowBlackScreen());
            yield return StartCoroutine(_sound_manager.FadeThemeMusic());
            SceneManager.LoadScene(Constants.GAME_SCENE);
        }
        StartCoroutine(ShowBlackScreenAndPlay());
    }

    public void CreditsButtonClick()
    {
        _title.SetActive(false);
        _play_button.SetActive(false);
        _credits_button.SetActive(false);
        _tutorial_button.SetActive(false);
        _sound_manager.PlaySoundFx(0, 0.25f);
        _credits_panel.SetActive(true);
    }

    public void TutorialButtonClick()
    {
        _title.SetActive(false);
        _play_button.SetActive(false);
        _credits_button.SetActive(false);
        _tutorial_button.SetActive(false);
        _sound_manager.PlaySoundFx(0, 0.25f);
        _tutorial_panel.SetActive(true);
    }

    public void BackButtonClick()
    {
        _title.SetActive(true);
        _play_button.SetActive(true);
        _credits_button.SetActive(true);
        _tutorial_button.SetActive(true);
        _sound_manager.PlaySoundFx(0, 0.25f);
        _tutorial_panel.SetActive(false);
        _credits_panel.SetActive(false);
    }

    private IEnumerator HideBlackScreen()
    {
        _black_screen.raycastTarget = true;
        for (float i = 1f; i >= 0; i -= Time.deltaTime)
        {
            _black_screen.color = new Color(0, 0, 0, i / 1f);
            yield return new WaitForEndOfFrame();
        }
        _black_screen.color = new Color(0, 0, 0, 0f);
        _black_screen.raycastTarget = false;
    }

    private IEnumerator ShowBlackScreen()
    {
        _black_screen.raycastTarget = true;
        for (float i = 0; i <= 1f; i += Time.deltaTime)
        {
            _black_screen.color = new Color(0, 0, 0, i / 1f);
            yield return new WaitForEndOfFrame();
        }
        _black_screen.color = new Color(0, 0, 0, 1f);
        _black_screen.raycastTarget = false;
    }

}