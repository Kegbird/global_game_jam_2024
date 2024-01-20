using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Image _black_screen;
    [SerializeField]
    private SoundManager _sound_manager;
    [SerializeField]
    private Image _tutorial_panel;
    [SerializeField]
    private GameObject _credits_panel;
    [SerializeField]
    private int _tutorial_slide_index;
    [SerializeField]
    private Sprite[] _slides;

    private void Start()
    {
        StartCoroutine(HideBlackScreen());
    }

    public void PlayButtonClick()
    {
        _sound_manager.PlaySoundFx(0, 0.25f);
        IEnumerator ShowBlackScreenAndPlay()
        {
            StartCoroutine(ShowBlackScreen());
            yield return StartCoroutine(_sound_manager.FadeThemeMusic());
            SceneManager.LoadScene(Constants.GAME_SCENE);
        }
        StartCoroutine(ShowBlackScreenAndPlay());
    }

    public void TutorialButtonClick()
    {
        _sound_manager.PlaySoundFx(0, 0.25f);
        _tutorial_slide_index = 0;
        _tutorial_panel.gameObject.SetActive(true);
    }

    public void CreditsButtonClick()
    {
        _sound_manager.PlaySoundFx(0, 0.25f);
        _credits_panel.SetActive(true);
    }

    public void NextButtonClick()
    {
        _sound_manager.PlaySoundFx(0, 0.25f);
        _tutorial_slide_index++;
        if (_tutorial_slide_index == 4)
        {
            _tutorial_slide_index = 0;
            _tutorial_panel.gameObject.SetActive(false);
        }
        _tutorial_panel.sprite = _slides[_tutorial_slide_index];

    }

    public void BackButtonClick()
    {
        _sound_manager.PlaySoundFx(0, 0.25f);
        _credits_panel.SetActive(false);
    }

    public void ExitButtonClick()
    {
        _sound_manager.PlaySoundFx(0, 0.25f);
        IEnumerator ShowBlackScreenAndQuit()
        {
            StartCoroutine(ShowBlackScreen());
            yield return StartCoroutine(_sound_manager.FadeThemeMusic());
            Application.Quit();
        }
        StartCoroutine(ShowBlackScreenAndQuit());
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