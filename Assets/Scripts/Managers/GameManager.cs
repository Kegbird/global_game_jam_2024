using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private int _next_level;
    [SerializeField]
    private GameObject _player;
    [SerializeField]
    private List<GameObject> _enemies;
    [SerializeField]
    private UIManager _ui_manager;

    // Start is called before the first frame update
    void Start()
    {
        _ui_manager.HideBlackScreen();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator LoadNextLevel()
    {
        yield return StartCoroutine(_ui_manager.HideBlackScreenUICoroutineUI(1f));
        SceneManager.LoadScene(_next_level);
        yield return null;
    }

    public void GameOver()
    {

    }


}
