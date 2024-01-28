using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : MonoBehaviour, IEnemy, IKnockable
{
    private GameManager _game_manager;
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private float _movement_speed;
    [SerializeField]
    private bool _alive;
    [SerializeField]
    private GameObject _arrow;

    private void Awake()
    {
        _alive = true;
        _arrow = gameObject.transform.GetChild(0).gameObject;
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
            Vector3 throwing_direction = player_world_position - (Vector2)transform.position;
            throwing_direction.Normalize();
            float angle = Mathf.Atan2(throwing_direction.y, throwing_direction.x) * Mathf.Rad2Deg;
            _arrow.transform.position = transform.position;
            _arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            _arrow.transform.rotation *= Quaternion.Euler(0f, 0f, -90f);

            _arrow.gameObject.SetActive(true);
            yield return new WaitForFixedUpdate();

            while (_arrow.activeInHierarchy)
            {
                _arrow.GetComponent<Rigidbody2D>().MovePosition(_arrow.transform.position + throwing_direction * 10f * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }
            _arrow.gameObject.SetActive(false);
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

    public IEnumerator Knockback()
    {
        Vector3Int player_grid_position = _game_manager.GetPlayerGridPosition();
        Vector3 player_world_position = _game_manager.GetWorldPositionFromGridPosition(player_grid_position);
        Vector3 knockback_vector = transform.position - player_world_position;
        knockback_vector.Normalize();
        Vector3 target_position = transform.position + knockback_vector;
        while (Vector2.Distance(transform.position, target_position) > 0.001f)
        {
            _rigidbody.MovePosition(Vector2.MoveTowards(transform.position, target_position, 5f * Time.fixedDeltaTime));
            yield return new WaitForFixedUpdate();
        }
        Vector3Int grid_position = _game_manager.GetGridPosition(transform.position);
        if (!_game_manager.HasTile(grid_position))
        {
            Kill();
        }
        yield return null;
    }
}
