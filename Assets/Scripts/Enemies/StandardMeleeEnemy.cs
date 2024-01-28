using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardMeleeEnemy : MonoBehaviour, IEnemy, IKnockable
{
    private GameManager _game_manager;
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private float _movement_speed;
    [SerializeField]
    private int _damage;
    [SerializeField]
    private bool _alive;
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private bool _stun;

    private void Awake()
    {
        _alive = true;
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _game_manager = GameManager.instance;
    }

    public void Disable()
    {
        this.gameObject.SetActive(false);
    }

    public IEnumerator Reason()
    {
        if (_stun)
        {
            _stun = true;
        }
        else
        {
            Vector3Int grid_position = _game_manager.GetGridPosition(transform.position);
            Vector3Int player_grid_position = _game_manager.GetPlayerGridPosition();
            Vector2 player_world_position = _game_manager.GetWorldPositionFromGridPosition(player_grid_position);
            List<Vector3Int> neighbour_grid_position = _game_manager.GetNeighbourTilesIgnoreDash(grid_position);

            if (neighbour_grid_position.Contains(player_grid_position))
            {
                _game_manager.DamagePlayer(_damage);
            }
            else
            {
                float distance = float.MaxValue;
                Vector2 closest_position = transform.position;
                List<Vector2> available_positions = new List<Vector2>();
                for (int i = 0; i < neighbour_grid_position.Count; i++)
                {
                    if (!_game_manager.HasTile(neighbour_grid_position[i]))
                    {
                        continue;
                    }
                    Vector2 neighbour_world_position = _game_manager.GetWorldPositionFromGridPosition(neighbour_grid_position[i]);
                    if (Vector2.Distance(neighbour_world_position, player_world_position) < distance)
                    {
                        distance = Vector2.Distance(neighbour_world_position, player_world_position);
                        closest_position = neighbour_world_position;
                    }
                    available_positions.Add(neighbour_world_position);
                }
                if(30<=Random.Range(0, 101))
                    yield return StartCoroutine(MoveToPosition(available_positions[Random.Range(0, available_positions.Count)]));
                else
                    yield return StartCoroutine(MoveToPosition(closest_position));
            }
        }
        yield return null;
    }

    private IEnumerator MoveToPosition(Vector2 target_position)
    {
        while (Vector2.Distance(transform.position, target_position) > 0.001f)
        {
            _rigidbody.MovePosition(Vector2.MoveTowards(transform.position, target_position, _movement_speed * Time.fixedDeltaTime));
            yield return new WaitForFixedUpdate();
        }

        yield return null;
    }

    public void Kill()
    {
        _animator.SetTrigger("dead");
        _alive = false;
        _game_manager.EnemyKilled();
    }

    public bool IsAlive()
    {
        return _alive;
    }

    public IEnumerator Knockback()
    {
        _stun = true;
        Vector3Int player_grid_position = _game_manager.GetPlayerGridPosition();
        Vector3 player_world_position = _game_manager.GetWorldPositionFromGridPosition(player_grid_position);
        Vector3 knockback_vector = transform.position - player_world_position;
        knockback_vector.Normalize();
        Vector3 target_position = transform.position + knockback_vector;
        while (Vector2.Distance(transform.position, target_position) > 0.001f)
        {
            _rigidbody.MovePosition(Vector2.MoveTowards(transform.position, target_position, _movement_speed * Time.fixedDeltaTime));
            yield return new WaitForFixedUpdate();
        }
        Vector3Int grid_position = _game_manager.GetGridPosition(transform.position);
        if(!_game_manager.HasTile(grid_position))
        {
            Kill();
        }
        yield return null;
    }
}
