using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Image _blackscreen;
    [SerializeField]
    private Button _dash_button; 
    [SerializeField]
    private Button _knife_button;
    [SerializeField]
    private Button _knockback_button;
    [SerializeField]
    private GameObject _offert_panel;
    [SerializeField]
    private TextMeshProUGUI _dash_number_text;
    [SerializeField]
    private TextMeshProUGUI _knife_number_text;
    [SerializeField]
    private TextMeshProUGUI _hp_number_text;
    [SerializeField]
    private TextMeshProUGUI _knockback_number_text;
    [SerializeField]
    private Animator _offert_animator;

    public void ShowOfferts()
    {
        _offert_panel.SetActive(true);
    }

    public void HideOfferts()
    {
        _offert_animator.SetTrigger("hide");
    }

    public void ShowBlackScreen()
    {
        StartCoroutine(ShowBlackScreenUICoroutineUI(1f));
    }
    
    public void HideBlackScreen()
    {
        StartCoroutine(HideBlackScreenUICoroutineUI(1f));
    }

    public void SetHpNumber(int hps)
    {
        this._hp_number_text.text = hps.ToString();
    }

    public void SetKnifeNumber(int knifes)
    {
        this._knife_number_text.text = knifes.ToString();
    }

    public void SetDashNumber(int dashes)
    {
        this._dash_number_text.text = dashes.ToString();
    }

    public void SetKnockNumber(int knockbacks)
    {
        this._knockback_number_text.text = knockbacks.ToString();
    }

    public void EnableInteractionDashButton()
    {
        _dash_button.interactable = true;
    }

    public void DisableInteractionDashButton()
    {
        _dash_button.interactable = false;
    }

    public void EnableInteractionKnifeButton()
    {
        _knife_button.interactable = true;
    }

    public void DisableInteractionKnifeButton()
    {
        _knife_button.interactable = false;
    }

    public void EnableInteractionKnockbackButton()
    {
        _knockback_button.interactable = true;
    }

    public void DisableInteractionKnockbackButton()
    {
       _knockback_button.interactable = false;
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
