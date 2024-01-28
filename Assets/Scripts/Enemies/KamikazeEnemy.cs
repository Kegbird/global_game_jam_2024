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
    private bool _stun;
    [SerializeField]
    private GameObject _bomb_prefab;
    [SerializeField]
    private Animator _animator;

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

    public bool IsAlive()
    {
        return _alive;
    }

    public void Kill()
    {
        _animator.SetTrigger("dead");
        _alive = false;
        _game_manager.EnemyKilled();
        gameObject.SetActive(false);
    }

    public IEnumerator Reason()
    {
        if(_stun)
        {
            _stun = false;
        }
        else
        {
            Vector3Int grid_position = _game_manager.GetGridPosition(transform.position);
            Vector3Int player_grid_position = _game_manager.GetPlayerGridPosition();
            Vector2 player_world_position = _game_manager.GetWorldPositionFromGridPosition(player_grid_position);


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

                grid_position = _game_manager.GetGridPosition(transform.position);
                //Prova ad attaccare di nuovo
                if (CanAttackPlayer(grid_position, player_grid_position))
                {
                    List<Vector3Int> player_neighbour_tiles = _game_manager.GetNeighbourTilesIgnoreDash(player_grid_position);
                    Vector3Int target_position = player_neighbour_tiles[Random.Range(0, player_neighbour_tiles.Count)];
                    Vector3 target_world_position = _game_manager.GetWorldPositionFromGridPosition(target_position);
                    yield return StartCoroutine(ThrowBomb(target_world_position));
                }
            }
            yield return null;
        }
    }

    private IEnumerator ThrowBomb(Vector2 target_position)
    {
        GameObject bomb = GameObject.Instantiate(_bomb_prefab, transform.position, Quaternion.identity);
        bomb.gameObject.SetActive(true);
        while (Vector2.Distance(bomb.transform.position, target_position) > 0.001f)
        {
            bomb.GetComponent<Rigidbody2D>().MovePosition(Vector2.MoveTowards(bomb.transform.position, target_position, _movement_speed * Time.fixedDeltaTime));
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
        _stun = true;
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
