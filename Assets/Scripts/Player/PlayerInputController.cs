using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField]
    public bool _active;
    [SerializeField]
    private GameManager _game_manager;
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    public bool _selected;
    [SerializeField]
    private float _movement_speed;
    [SerializeField]
    private GameObject _knife;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _knife = gameObject.transform.GetChild(0).gameObject;
        _active = true;
    }

    private void Update()
    {
        if (!_active)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouse_position, Vector2.zero);

            if (_game_manager._knife)
            {
                if (_game_manager.TryToThrow(mouse_position))
                {
                    _game_manager.UnhighlightKnifeCells();
                    Vector2 target_position = _game_manager.GetTargetPosition(mouse_position);
                    _game_manager.ConsumeKnife();
                    StartCoroutine(ThrowKnife(target_position));
                    _active = false;
                    _selected = false;
                }
                else
                {
                    _game_manager.UnhighlightKnifeCells();
                    _game_manager.DisableKnife();
                    _selected = false;
                }
            }
            if (_game_manager._dash)
            {
                if (_game_manager.TryToMove(mouse_position))
                {
                    _game_manager.UnhighlightPlayerMovementCells();
                    Vector2 target_position = _game_manager.GetTargetPosition(mouse_position);
                    _game_manager.ConsumeDash();
                    StartCoroutine(MoveToPosition(target_position));
                    _active = false;
                    _selected = false;
                }
                else
                {
                    _game_manager.UnhighlightPlayerMovementCells();
                    _game_manager.DisableDash();
                    _selected = false;
                }
            }
            if(!_game_manager._dash && !_game_manager._knife)
            {
                if (hit.collider != null && hit.collider.tag == "Player")
                {
                    _selected = true;
                    _game_manager.HighlightPlayerMovementCells();
                }
                else
                {
                    if (_selected)
                    {
                        _selected = false;
                        if (_game_manager.TryToMove(mouse_position))
                        {
                            _game_manager.UnhighlightPlayerMovementCells();
                            Vector2 target_position = _game_manager.GetTargetPosition(mouse_position);
                            StartCoroutine(MoveToPosition(target_position));
                            _active = false;
                        }
                        else
                        {
                            _game_manager.UnhighlightPlayerMovementCells();
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Exit" && _game_manager.IsWin())
        {
            _active = false;
            StartCoroutine(_game_manager.LoadLevel(_game_manager._next_level_scene_index));
        }
    }

    private IEnumerator MoveToPosition(Vector2 target_position)
    {
        while (Vector2.Distance(transform.position, target_position) > 0.001f)
        {
            _rigidbody.MovePosition(Vector2.MoveTowards(transform.position, target_position, _movement_speed * Time.fixedDeltaTime));
            yield return new WaitForFixedUpdate();
        }
        List<GameObject> near_enemies = _game_manager.GetPlayerNearEnemies();

        for (int i = 0; i < near_enemies.Count; i++)
            near_enemies[i].GetComponent<IEnemy>().Kill();

        yield return StartCoroutine(_game_manager.ActivateEnemies());
        if (!_game_manager.IsGameOver())
        {
            _active = true;
        }
        yield return null;
    }

    private IEnumerator ThrowKnife(Vector2 target_position)
    {
        Vector3 throwing_direction = target_position - (Vector2)transform.position;
        throwing_direction.Normalize();
        float angle = Mathf.Atan2(throwing_direction.y, throwing_direction.x) * Mathf.Rad2Deg;
        _knife.transform.position = transform.position;
        _knife.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        _knife.transform.rotation *= Quaternion.Euler(0f, 0f, -90f);

        _knife.gameObject.SetActive(true);

        while(_knife.activeInHierarchy)
        {
            _knife.GetComponent<Rigidbody2D>().MovePosition(_knife.transform.position + throwing_direction * 10f * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        _knife.gameObject.SetActive(false);
        yield return StartCoroutine(_game_manager.ActivateEnemies());
        if (!_game_manager.IsGameOver())
        {
            _active = true;
        }
        yield return null;
    }
}
