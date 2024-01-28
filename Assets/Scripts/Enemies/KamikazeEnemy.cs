using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KamikazeEnemy : MonoBehaviour, IEnemy, IKnockable
{
    private GameManager _game_manager;
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private float _movement_speed;
    [SerializeField]
    private bool _alive;
    [SerializeField]
    private bool _has_the_bomb;
    [SerializeField]
    private GameObject _bomb;

    private void Awake()
    {
        _has_the_bomb = true;
        _alive = true;
        _bomb = gameObject.transform.GetChild(0).gameObject;
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _game_manager = GameManager.instance;
    }

    public bool IsAlive()
    {
        return _alive;
    }

    public void Kill()
    {
        _alive = false;
        _game_manager.EnemyKilled();
        gameObject.SetActive(false);
    }

    public IEnumerator Reason()
    {
        Vector3Int grid_position = _game_manager.GetGridPosition(transform.position);
        Vector3Int player_grid_position = _game_manager.GetPlayerGridPosition();
        Vector2 player_world_position = _game_manager.GetWorldPositionFromGridPosition(player_grid_position);

        if(!_has_the_bomb)
        {
            //Detonate
            _has_the_bomb = false;
            yield return StartCoroutine(_bomb.GetComponent<BombBehaviour>().Boom());
            _bomb.SetActive(false);
            _bomb.transform.position = transform.position;
            _has_the_bomb = true;
        }
        else
        {
            //Se in range lancia la boma
            if (CanAttackPlayer(grid_position, player_grid_position))
            {
                List<Vector3Int> player_neighbour_tiles = _game_manager.GetNeighbourTilesIgnoreDash(player_grid_position);
                Vector3Int target_position = player_neighbour_tiles[Random.Range(0, player_neighbour_tiles.Count)];
                Vector3 target_world_position = _game_manager.GetWorldPositionFromGridPosition(target_position);
                yield return StartCoroutine(ThrowBomb(target_world_position));
            }
            //Altrimenti si muove
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
        }
    }

    private IEnumerator ThrowBomb(Vector2 target_position)
    {
        _has_the_bomb = false;
        _bomb.gameObject.SetActive(true);
        while (Vector2.Distance(_bomb.transform.position, target_position) > 0.001f)
        {
            _bomb.GetComponent<Rigidbody2D>().MovePosition(Vector2.MoveTowards(_bomb.transform.position, target_position, _movement_speed * Time.fixedDeltaTime));
            yield return new WaitForFixedUpdate();
        }
        yield return null;
    }

    private bool CanAttackPlayer(Vector3Int grid_position, Vector3Int player_grid_position)
    {
        List<Vector3Int> attackable_tiles = _game_manager.GetAttackablePositionsForRanged(grid_position);
        return attackable_tiles.Contains(player_grid_position);
    }

    private bool IsNearPlayer(Vector3Int grid_position, Vector3Int player_grid_position)
    {
        List<Vector3Int> neighbour_tiles = _game_manager.GetNeighbourTilesIgnoreDash(grid_position);
        return neighbour_tiles.Contains(player_grid_position);
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
