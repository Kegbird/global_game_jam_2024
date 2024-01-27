using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardMeleeEnemy : MonoBehaviour, IEnemy
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

    private void Awake()
    {
        _alive = true;
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _game_manager = GameManager.instance;
    }

    public IEnumerator Reason()
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
            }
            yield return StartCoroutine(MoveToPosition(closest_position));
            //Try to attack
            grid_position = _game_manager.GetGridPosition(transform.position);
            neighbour_grid_position = _game_manager.GetNeighbourTilesIgnoreDash(grid_position);
            if (neighbour_grid_position.Contains(player_grid_position))
            {
                _game_manager.DamagePlayer(_damage);
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

    public void TakeDamage(int damage)
    {
        throw new System.NotImplementedException();
    }

    public void Kill()
    {
        _alive = false;
        _game_manager.EnemyKilled();
        gameObject.SetActive(false);
    }

    public bool IsAlive()
    {
        return _alive;
    }
}
