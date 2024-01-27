using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField]
    private bool _is_initial_scene;
    [SerializeField]
    public int _next_level_scene_index;
    [SerializeField]
    public int _game_over_scene_index;
    [SerializeField]
    public GameObject _player;
    [SerializeField]
    private GameObject _enemies_wrapper;
    [SerializeField]
    private int _enemies_to_kill;
    [SerializeField]
    private List<GameObject> _enemies;
    [SerializeField]
    private List<GameObject> _obstacles;
    [SerializeField]
    private UIManager _ui_manager;
    [SerializeField]
    private Tilemap _tilemap;
    [SerializeField]
    private Transform _spawn;
    [SerializeField]
    private static int _hps;
    [SerializeField]
    private static int _dashes;
    [SerializeField]
    public bool _dash;
    [SerializeField]
    private GameObject _exit;
    [SerializeField]
    private Vector3Int _exit_grid_position;
    [SerializeField]
    private bool _game_over;

    private void Awake()
    {
        instance = this;
        if (_is_initial_scene)
        {
            _hps = 3;
            _dashes = 3;
        }
        LoadEnemies();
    }

    private void LoadEnemies()
    {
        _enemies = new List<GameObject>();
        for (int i = 0; i < _enemies_wrapper.transform.childCount; i++)
            _enemies.Add(_enemies_wrapper.transform.GetChild(i).gameObject);
        _enemies_to_kill = _enemies.Count;
    }

    private void RandomizeEnemyPositions()
    {
        BoundsInt bounds_int = _tilemap.cellBounds;
        List<Vector3Int> player_neighbours = GetNeighbourTiles(GetPlayerGridPosition());
        List<Vector3Int> rand_grid_position_generated = new List<Vector3Int>();
        for (int i=0; i<_enemies.Count; i++)
        {
            Vector3Int rand_grid_position = new Vector3Int(Random.Range(bounds_int.xMin, bounds_int.xMax), Random.Range(bounds_int.yMin, bounds_int.yMax));
            if (_tilemap.HasTile(rand_grid_position) && 
                !player_neighbours.Contains(rand_grid_position) &&
                !rand_grid_position_generated.Contains(rand_grid_position))
            {
                _enemies[i].transform.position = _tilemap.CellToWorld(rand_grid_position);
                rand_grid_position_generated.Add(rand_grid_position);
            }
            else
            {
                i--;
            }
        }
    }

    private void Start()
    {
        Vector3Int spawn_grid_position = _tilemap.WorldToCell(_spawn.position);
        _player.transform.position = _tilemap.CellToWorld(spawn_grid_position);
        _exit_grid_position = _tilemap.WorldToCell(_exit.transform.position);
        _ui_manager.HideBlackScreen();
        UpdateUIStats();
        RandomizeEnemyPositions();
    }

    private void Update()
    {
    }

    public bool IsWin()
    {
        return _enemies_to_kill == 0;
    }

    public bool TryToMove(Vector2 tapped_position)
    {
        Vector3Int tapped_cell_position = _tilemap.WorldToCell(tapped_position);
        Vector3Int player_grid_position = _tilemap.WorldToCell(_player.transform.position);
        List<Vector3Int> neighbour_positions = GetNeighbourTiles(player_grid_position);

        return neighbour_positions.Contains<Vector3Int>(tapped_cell_position) && _tilemap.HasTile(tapped_cell_position);
    }

    public Vector3 GetTargetPosition(Vector2 tapped_position)
    {
        Vector3Int tapped_cell_position = _tilemap.WorldToCell(tapped_position);
        return _tilemap.CellToWorld(tapped_cell_position);
    }

    public bool IsGameOver()
    {
        return _game_over;
    }

    public IEnumerator ActivateEnemies()
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            if (_enemies[i].GetComponent<IEnemy>().IsAlive())
            {
                IEnemy enemy_logic = _enemies[i].GetComponent<IEnemy>();
                yield return StartCoroutine(enemy_logic.Reason());
            }
        }

        if (_game_over)
            StartCoroutine(LoadLevel(_game_over_scene_index));
    }

    public void HighlightPlayerMovementCells()
    {
        Vector3Int player_grid_position = _tilemap.WorldToCell(_player.transform.position);

        List<Vector3Int> neighbour_positions = GetNeighbourTiles(player_grid_position);

        for (int i = 0; i < neighbour_positions.Count; i++)
        {
            if (!_tilemap.HasTile(neighbour_positions[i]))
                continue;
            _tilemap.SetTileFlags(neighbour_positions[i], TileFlags.None);
            _tilemap.SetColor(neighbour_positions[i], new Color(1f, 0f, 0f, 1f));
        }
    }

    public void UnhighlightPlayerMovementCells()
    {
        Vector3Int player_grid_position = _tilemap.WorldToCell(_player.transform.position);
        List<Vector3Int> neighbour_positions = GetNeighbourTiles(player_grid_position);

        for (int i = 0; i < neighbour_positions.Count; i++)
        {
            if (!_tilemap.HasTile(neighbour_positions[i]))
                continue;
            _tilemap.SetTileFlags(neighbour_positions[i], TileFlags.None);
            _tilemap.SetColor(neighbour_positions[i], Color.white);
        }
    }

    public List<GameObject> GetPlayerNearEnemies()
    {
        List<GameObject> near_enemies = new List<GameObject>();
        Vector3Int player_grid_position = GetPlayerGridPosition();
        List<Vector3Int> neighbours_grid_positions = GetNeighbourTilesConsideringAllCells(player_grid_position);

        for (int j = 0; j < _enemies.Count; j++)
        {
            if (_enemies[j].GetComponent<IEnemy>().IsAlive())
            {
                Vector3Int enemy_grid_position = GetGridPosition(_enemies[j].transform.position);
                if (neighbours_grid_positions.Contains(enemy_grid_position))
                {
                    near_enemies.Add(_enemies[j]);
                }
            }
        }
        return near_enemies;
    }

    public void EnemyKilled()
    {
        _enemies_to_kill--;
    }

    public List<Vector3Int> GetNeighbourTilesIgnoreDash(Vector3Int grid_position)
    {
        List<Vector3Int> neighbour_tiles = new List<Vector3Int>();

        if (Mathf.Abs(grid_position.y) % 2 == 0)
        {
            neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y + 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y - 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y + 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y - 1, 0));
        }
        else
        {
            neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y + 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y - 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y + 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y - 1, 0));
        }

        if (!IsWin())
        {
            neighbour_tiles.Remove(_exit_grid_position);
        }

        if (_obstacles != null && _obstacles.Count > 0)
        {
            for (int i = 0; i < _obstacles.Count; i++)
            {
                neighbour_tiles.Remove(_tilemap.WorldToCell(_obstacles[i].transform.position));
            }
        }

        for (int i = 0; i < _enemies.Count; i++)
        {
            if(_enemies[i].GetComponent<IEnemy>().IsAlive())
            {
                neighbour_tiles.Remove(_tilemap.WorldToCell(_enemies[i].transform.position));
            }
        }
        return neighbour_tiles;
    }

    public List<Vector3Int> GetNeighbourTilesConsideringAllCells(Vector3Int grid_position)
    {
        List<Vector3Int> neighbour_tiles = new List<Vector3Int>();

        if (Mathf.Abs(grid_position.y) % 2 == 0)
        {
            neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y + 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y - 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y + 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y - 1, 0));
        }
        else
        {
            neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y + 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y - 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y + 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y - 1, 0));
        }

        return neighbour_tiles;
    }

    public List<Vector3Int> GetNeighbourTiles(Vector3Int grid_position)
    {
        List<Vector3Int> neighbour_tiles = new List<Vector3Int>();

        if (_dash)
        {
            if (Mathf.Abs(grid_position.y) % 2 == 0)
            {
                //center(0, 0)
                //0, 2
                neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y + 2, 0));
                //-1, 2
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y + 2, 0));
                //-2, 1
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 2, grid_position.y + 1, 0));
                //-2, 0
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 2, grid_position.y, 0));
                //-2, -1
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 2, grid_position.y - 1, 0));
                //-1, -2
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y - 2, 0));
                //0, -2
                neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y - 2, 0));
                //1, -2
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y - 2, 0));
                //1, -1
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y - 1, 0));
                //2, 0
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 2, grid_position.y, 0));
                //1, 1
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y + 1, 0));
                //1, 2
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y + 2, 0));
            }
            else
            {
                //center(1, -3)
                //1, -1
                neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y + 2, 0));
                //0, -1
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y + 2, 0));
                //0, -2
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y + 1, 0));
                //-1, -3
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 2, grid_position.y, 0));
                //0, -4
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y - 1, 0));
                //0,-5
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y - 2, 0));
                //1,-5 
                neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y - 2, 0));
                //2, -5
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y - 2, 0));
                //3, -4
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 2, grid_position.y - 1, 0));
                //3, -3
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 2, grid_position.y, 0));
                //3, -2
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 2, grid_position.y + 1, 0));
                //2, -1
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y + 2, 0));
            }
        }
        else
        {
            if (Mathf.Abs(grid_position.y) % 2 == 0)
            {
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y + 1, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y - 1, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y + 1, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y - 1, 0));
            }
            else
            {
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y + 1, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y - 1, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y + 1, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y - 1, 0));
            }
        }

        if (!IsWin())
        {
            neighbour_tiles.Remove(_exit_grid_position);
        }

        if (_obstacles != null && _obstacles.Count > 0)
        {
            for (int i = 0; i < _obstacles.Count; i++)
            {
                neighbour_tiles.Remove(_tilemap.WorldToCell(_obstacles[i].transform.position));
            }
        }

        for (int i = 0; i < _enemies.Count; i++)
        {
            if (_enemies[i].GetComponent<IEnemy>().IsAlive())
            {
                neighbour_tiles.Remove(_tilemap.WorldToCell(_enemies[i].transform.position));
            }
        }

        return neighbour_tiles;
    }

    public List<Vector3Int> GetAttackablePositionsForRanged(Vector3Int enemy_position)
    {
        List<Vector3Int> attackable_tiles_for_ranged = new List<Vector3Int>();
        if (Mathf.Abs(enemy_position.y) % 2 == 0)
        {
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x, enemy_position.y + 2, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x - 1, enemy_position.y + 2, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x - 2, enemy_position.y + 1, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x - 2, enemy_position.y, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x - 2, enemy_position.y - 1, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x - 1, enemy_position.y - 2, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x, enemy_position.y - 2, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x + 1, enemy_position.y - 2, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x + 1, enemy_position.y - 1, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x + 2, enemy_position.y, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x + 1, enemy_position.y + 1, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x + 1, enemy_position.y + 2, 0));
        }
        else
        {
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x, enemy_position.y + 2, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x - 1, enemy_position.y + 2, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x - 1, enemy_position.y + 1, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x - 2, enemy_position.y, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x - 1, enemy_position.y - 1, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x - 1, enemy_position.y - 2, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x, enemy_position.y - 2, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x + 1, enemy_position.y - 2, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x + 2, enemy_position.y - 1, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x + 2, enemy_position.y, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x + 2, enemy_position.y + 1, 0));
            attackable_tiles_for_ranged.Add(new Vector3Int(enemy_position.x + 1, enemy_position.y + 2, 0));
        }
        return attackable_tiles_for_ranged;
    }

    public void DamagePlayer(int damage)
    {
        _hps = Mathf.Clamp(_hps - damage, 0, 100);
        UpdateUIStats();
        if (_hps == 0)
        {
            _game_over = true;
            //game over
        }
    }

    public Vector3Int GetGridPosition(Vector3 position)
    {
        return _tilemap.WorldToCell(position);
    }

    public Vector2 GetWorldPositionFromGridPosition(Vector3Int grid_position)
    {
        return _tilemap.CellToWorld(grid_position);
    }

    public bool HasTile(Vector3Int grid_position)
    {
        return _tilemap.HasTile(grid_position);
    }

    public Vector3Int GetPlayerGridPosition()
    {
        return GetGridPosition(_player.transform.position);
    }

    public void EnableDash()
    {
        if (!_player.GetComponent<PlayerInputController>()._active)
            return;
        _dash = true;
        _player.GetComponent<PlayerInputController>()._selected = true;
        HighlightPlayerMovementCells();
        _ui_manager.DisableInteractionDashButton();
    }

    public void DisableDash()
    {
        _dash = false;
        _ui_manager.EnableInteractionDashButton();
    }

    public void ConsumeDash()
    {
        _dashes--;
        _dash = false;
        if (_dashes == 0)
        {
            _ui_manager.DisableInteractionDashButton();
        }
        else
        {
            _ui_manager.EnableInteractionDashButton();
        }
        UpdateUIStats();
    }

    public IEnumerator LoadLevel(int scene_index)
    {
        _ui_manager.ShowBlackScreen();
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(scene_index);
        yield return null;
    }

    public void UpdateUIStats()
    {
        _ui_manager.SetDashNumber(_dashes);
        _ui_manager.SetHpNumber(_hps);
    }

}
