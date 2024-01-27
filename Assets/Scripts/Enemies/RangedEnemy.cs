using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : MonoBehaviour, IEnemy
{
    private GameManager _game_manager;
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private float _movement_speed;
    [SerializeField]
    private int _damage;

    private void Awake()
    {
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

        if (CanAttackPlayer(grid_position, player_grid_position))
        {
            Debug.Log("attack by ranged");
            _game_manager.DamagePlayer(_damage);
        }
        else
        {
            List<Vector3Int> neighbour_grid_position = _game_manager.GetNeighbourTilesIgnoreDash(grid_position);
            //Se vicino, si allontana, altrimenti si avvicina
            if (IsNearPlayer(grid_position, player_grid_position))
            {
                float distance = 0;
                Vector2 farthest_position = transform.position;
                for (int i = 0; i < neighbour_grid_position.Count; i++)
                {
                    if (!_game_manager.HasTile(neighbour_grid_position[i]))
                    {
                        continue;
                    }
                    Vector2 neighbour_world_position = _game_manager.GetWorldPositionFromGridPosition(neighbour_grid_position[i]);
                    if (Vector2.Distance(neighbour_world_position, player_world_position) > distance)
                    {
                        distance = Vector2.Distance(neighbour_world_position, player_world_position);
                        farthest_position = neighbour_world_position;
                    }
                }
                yield return StartCoroutine(MoveToPosition(farthest_position));
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
            }
        }

        yield return null;
    }

    private bool IsNearPlayer(Vector3Int grid_position, Vector3Int player_grid_position)
    {
        List<Vector3Int> neighbour_tiles = _game_manager.GetNeighbourTilesIgnoreDash(grid_position);
        return neighbour_tiles.Contains(player_grid_position);
    }

    private bool CanAttackPlayer(Vector3Int grid_position, Vector3Int player_grid_position)
    {
        List<Vector3Int> attackable_tiles = _game_manager.GetAttackablePositionsForRanged(grid_position);
        return attackable_tiles.Contains(player_grid_position);
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

    public void Die()
    {
        throw new System.NotImplementedException();
    }
}
