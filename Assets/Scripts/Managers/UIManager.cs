using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Image _blackscreen;


    public void ShowBlackScreen()
    {
        StartCoroutine(ShowBlackScreenUICoroutineUI(1f));
    }
    
    public void HideBlackScreen()
    {
        StartCoroutine(HideBlackScreenUICoroutineUI(1f));
    }

    public IEnumerator ShowBlackScreenUICoroutineUI(float transition_duration)
    {
        _blackscreen.raycastTarget = true;
        float i = 0f;
        while (i <= transition_duration)
        {
            _blackscreen.color = new Color(0, 0, 0, i / transition_duration);
            i += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        _blackscreen.color = new Color(0, 0, 0, 1);
    }

    public IEnumerator HideBlackScreenUICoroutineUI(float transition_duration)
    {
        float i = transition_duration;
        while (i >= 0)
        {
            _blackscreen.color = new Color(0, 0, 0, i / transition_duration);
            i -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        _blackscreen.color = new Color(0, 0, 0, 0);
    }

}
