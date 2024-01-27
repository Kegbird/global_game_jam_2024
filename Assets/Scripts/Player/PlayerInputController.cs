using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField]
    private bool _active;
    [SerializeField]
    private bool _editor;
    [SerializeField]
    private GameManager _game_manager;
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private bool _selected;
    [SerializeField]
    private float _movement_speed;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _active = true;
    }

    private void Update()
    {
        if (!_active)
            return;

        if (Input.GetMouseButtonDown(0) || Input.touches.Length > 0)
        {
            Vector3 mouse_position = Camera.main.ScreenToWorldPoint(_editor ? Input.mousePosition : Input.touches[0].position);
            RaycastHit2D hit = Physics2D.Raycast(mouse_position, Vector2.zero);
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
                        if (_game_manager._dash)
                        {
                            _game_manager.ConsumeDash();
                        }
                        StartCoroutine(MoveToPosition(target_position));
                        _active = false;
                    }
                    else
                    {
                        _game_manager.UnhighlightPlayerMovementCells();
                        if (_game_manager._dash)
                        {
                            _game_manager.DisableDash();
                        }
                    }
                }
                else
                {
                    if (_game_manager._dash)
                    {
                        _game_manager.DisableDash();
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Exit")
        {
            _active = false;
            StartCoroutine(_game_manager.LoadNextLevel());
        }
    }

    private IEnumerator MoveToPosition(Vector2 target_position)
    {
        while (Vector2.Distance(transform.position, target_position) > 0.001f)
        {
            _rigidbody.MovePosition(Vector2.MoveTowards(transform.position, target_position, _movement_speed * Time.fixedDeltaTime));
            yield return new WaitForFixedUpdate();
        }
        yield return StartCoroutine(_game_manager.ActivateEnemies());
        _active = true;
        yield return null;
    }
}
