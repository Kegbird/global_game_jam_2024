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
    public List<GameObject> _enemies;
    [SerializeField]
    private List<GameObject> _obstacles;
    [SerializeField]
    private GameObject _obstacles_wrapper;
    [SerializeField]
    private UIManager _ui_manager;
    [SerializeField]
    private Tilemap _tilemap;
    [SerializeField]
    private Transform _spawn;
    [SerializeField]
    private static int _hps;
    [SerializeField]
    private static int _knifes;
    [SerializeField]
    private static int _dashes;
    [SerializeField]
    private static int _knockbacks;
    [SerializeField]
    public bool _dash;
    [SerializeField]
    public bool _knife;
    [SerializeField]
    public bool _knockback;
    [SerializeField]
    private GameObject _exit;
    [SerializeField]
    private Vector3Int _exit_grid_position;
    [SerializeField]
    public bool _altair_used;
    [SerializeField]
    private GameObject _power_up_altair;
    [SerializeField]
    private bool _game_over;

    private void Awake()
    {
        _altair_used = false;
        instance = this;
        if (_is_initial_scene)
        {
            _hps = 3;
            _dashes = 3;
            _knifes = 3;
            _knockbacks = 3;
        }
        LoadObstacles();
        LoadEnemies();
        EvaluateButtonsActivation();
    }

    public void ShowOfferts()
    {
        _ui_manager.ShowOfferts();
    }

    public void IncreaseKnife()
    {
        _altair_used = true;
        _knifes += 2;
        _ui_manager.HideOfferts();
        _player.GetComponent<PlayerInputController>()._active = true;
        UpdateUIStats();
        EvaluateButtonsActivation();
    }

    public void IncreaseHp()
    {
        _altair_used = true;
        _hps += 2;
        _ui_manager.HideOfferts();
        _player.GetComponent<PlayerInputController>()._active = true;
        UpdateUIStats();
        EvaluateButtonsActivation();
    }

    public void IncreasKnockback()
    {
        _altair_used = true;
        _knockbacks += 2;
        _ui_manager.HideOfferts();
        _player.GetComponent<PlayerInputController>()._active = true;
        UpdateUIStats();
        EvaluateButtonsActivation();
    }

    public void IncreaseDash()
    {
        _altair_used = true;
        _dashes += 2;
        _ui_manager.HideOfferts();
        _player.GetComponent<PlayerInputController>()._active = true;
        UpdateUIStats();
        EvaluateButtonsActivation();
    }

    public bool IsPlayerNearAltair()
    {
        Vector3Int player_grid_position = GetPlayerGridPosition();
        Vector3Int altair_grid_position = GetGridPosition(_power_up_altair.transform.position);
        List<Vector3Int> altair_grid_neighbour = GetNeighbourTiles(altair_grid_position);
        return altair_grid_neighbour.Contains(player_grid_position);
    }

    private void EvaluateButtonsActivation()
    {
        if (_knifes == 0)
        {
            _ui_manager.DisableInteractionKnifeButton();
        }
        else
        {
            _ui_manager.EnableInteractionKnifeButton();
        }

        if (_knockbacks == 0)
        {
            _ui_manager.DisableInteractionKnockbackButton();
        }
        else
        {
            _ui_manager.EnableInteractionKnockbackButton();
        }

        if (_dashes == 0)
        {
            _ui_manager.DisableInteractionDashButton();
        }
        else
        {
            _ui_manager.EnableInteractionDashButton();
        }
    }

    private void LoadEnemies()
    {
        _enemies = new List<GameObject>();
        for (int i = 0; i < _enemies_wrapper.transform.childCount; i++)
            _enemies.Add(_enemies_wrapper.transform.GetChild(i).gameObject);
        _enemies_to_kill = _enemies.Count;
    }

    private void LoadObstacles()
    {
        _obstacles = new List<GameObject>();
        for (int i = 0; i < _obstacles_wrapper.transform.childCount; i++)
            _obstacles.Add(_obstacles_wrapper.transform.GetChild(i).gameObject);
    }

    private void RandomizeEnemyPositions()
    {
        BoundsInt bounds_int = _tilemap.cellBounds;
        Vector3Int player_grid_position = GetPlayerGridPosition();
        List<Vector3Int> player_neighbours = GetNeighbourTiles(player_grid_position);
        List<Vector3Int> rand_grid_position_generated = new List<Vector3Int>();
        for (int i = 0; i < _enemies.Count; i++)
        {
            Vector3Int rand_grid_position = new Vector3Int(Random.Range(bounds_int.xMin, bounds_int.xMax), Random.Range(bounds_int.yMin, bounds_int.yMax));
            if (_tilemap.HasTile(rand_grid_position) &&
                !player_neighbours.Contains(rand_grid_position) &&
                !rand_grid_position_generated.Contains(rand_grid_position) &&
                player_grid_position != rand_grid_position)
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
        Vector3Int _power_up_altair_grid_position = _tilemap.WorldToCell(_power_up_altair.transform.position);
        _power_up_altair.transform.position = _tilemap.CellToWorld(_power_up_altair_grid_position);
        _ui_manager.HideBlackScreen();
        UpdateUIStats();
        RandomizeEnemyPositions();
    }

    public bool IsWin()
    {
        return _enemies_to_kill <= 0;
    }

    public bool TryToMove(Vector2 tapped_position)
    {
        Vector3Int tapped_cell_position = _tilemap.WorldToCell(tapped_position);
        Vector3Int player_grid_position = _tilemap.WorldToCell(_player.transform.position);
        List<Vector3Int> neighbour_positions = GetNeighbourTiles(player_grid_position);

        return neighbour_positions.Contains<Vector3Int>(tapped_cell_position) && _tilemap.HasTile(tapped_cell_position);
    }

    public bool TryToKnockback(Vector2 tapped_position)
    {
        Vector3Int target_grid_position = GetGridPosition(tapped_position);
        GameObject enemy_to_knock = _enemies.Find((enemy) =>
        {
            Vector3Int enemy_grid_position = GetGridPosition(enemy.transform.position);
            return enemy_grid_position == target_grid_position && enemy.GetComponent<IEnemy>().IsAlive();
        });

        if (enemy_to_knock == null)
        {
            GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");
            for (int i = 0; i < bombs.Length; i++)
            {
                Vector3Int bomb_grid_position = GetGridPosition(bombs[i].transform.position);
                if (bomb_grid_position == target_grid_position && bombs[i].activeInHierarchy)
                    return true;
            }
            return false;
        }
        return enemy_to_knock != null;
    }

    public bool TryToThrow(Vector2 tapped_position)
    {
        Vector3Int tapped_cell_position = _tilemap.WorldToCell(tapped_position);
        Vector3Int player_grid_position = _tilemap.WorldToCell(_player.transform.position);
        List<Vector3Int> knife_tiles = GetKnifeTiles(player_grid_position);

        return knife_tiles.Contains<Vector3Int>(tapped_cell_position) && _tilemap.HasTile(tapped_cell_position);
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

    public IEnumerator ActivateEnemiesAndBombs()
    {
        //Enemies
        for (int i = 0; i < _enemies.Count && !IsGameOver(); i++)
        {
            if (_enemies[i].GetComponent<IEnemy>().IsAlive())
            {
                IEnemy enemy_logic = _enemies[i].GetComponent<IEnemy>();
                yield return StartCoroutine(enemy_logic.Reason());
            }
        }

        //Bombs
        GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");

        for (int j = 0; j < bombs.Length && !IsGameOver(); j++)
        {
            yield return StartCoroutine(bombs[j].GetComponent<BombBehaviour>().Boom());
        }


        if (_game_over)
        {
            _player.GetComponent<Animator>().SetTrigger("death");
            yield return new WaitForSeconds(1f);
            StartCoroutine(LoadLevel(_game_over_scene_index));
        }
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

    public void HighlightKnifeCells()
    {
        Vector3Int player_grid_position = _tilemap.WorldToCell(_player.transform.position);

        List<Vector3Int> neighbour_positions = GetKnifeTiles(player_grid_position);

        for (int i = 0; i < neighbour_positions.Count; i++)
        {
            if (!_tilemap.HasTile(neighbour_positions[i]))
                continue;
            _tilemap.SetTileFlags(neighbour_positions[i], TileFlags.None);
            _tilemap.SetColor(neighbour_positions[i], new Color(1f, 0f, 0f, 1f));
        }
    }

    public void HighlightKnockbackCells()
    {
        Vector3Int player_grid_position = _tilemap.WorldToCell(_player.transform.position);

        List<Vector3Int> neighbour_positions = GetKnifeTiles(player_grid_position);

        for (int i = 0; i < neighbour_positions.Count; i++)
        {
            if (!_tilemap.HasTile(neighbour_positions[i]))
                continue;
            _tilemap.SetTileFlags(neighbour_positions[i], TileFlags.None);
            _tilemap.SetColor(neighbour_positions[i], new Color(1f, 0f, 0f, 1f));
        }
    }

    public void UnhighlightKnifeCells()
    {
        Vector3Int player_grid_position = _tilemap.WorldToCell(_player.transform.position);

        List<Vector3Int> neighbour_positions = GetKnifeTiles(player_grid_position);

        for (int i = 0; i < neighbour_positions.Count; i++)
        {
            if (!_tilemap.HasTile(neighbour_positions[i]))
                continue;
            _tilemap.SetTileFlags(neighbour_positions[i], TileFlags.None);
            _tilemap.SetColor(neighbour_positions[i], Color.white);
        }
    }

    public void UnhighlightKnockbackCells()
    {
        Vector3Int player_grid_position = _tilemap.WorldToCell(_player.transform.position);

        List<Vector3Int> neighbour_positions = GetKnifeTiles(player_grid_position);

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

    public Vector3Int GetKnockBackTile(Vector3Int grid_origin_position, Vector3Int grid_position)
    {
        return grid_position - grid_origin_position;
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

        for (int i = 0; i < _obstacles.Count; i++)
        {
            neighbour_tiles.Remove(_tilemap.WorldToCell(_obstacles[i].transform.position));
        }

        for (int i = 0; i < _enemies.Count; i++)
        {
            if (_enemies[i].GetComponent<IEnemy>().IsAlive())
            {
                neighbour_tiles.Remove(_tilemap.WorldToCell(_enemies[i].transform.position));
            }
        }

        neighbour_tiles.Remove(_tilemap.WorldToCell(_power_up_altair.gameObject.transform.position));
        return neighbour_tiles;
    }

    public List<Vector3Int> GetBombRangePositions(Vector3Int grid_position)
    {
        List<Vector3Int> bomb_range_positions = new List<Vector3Int>();

        bomb_range_positions.Add(grid_position);

        if (Mathf.Abs(grid_position.y) % 2 == 0)
        {
            bomb_range_positions.Add(new Vector3Int(grid_position.x - 1, grid_position.y, 0));
            bomb_range_positions.Add(new Vector3Int(grid_position.x - 1, grid_position.y + 1, 0));
            bomb_range_positions.Add(new Vector3Int(grid_position.x - 1, grid_position.y - 1, 0));
            bomb_range_positions.Add(new Vector3Int(grid_position.x + 1, grid_position.y, 0));
            bomb_range_positions.Add(new Vector3Int(grid_position.x, grid_position.y + 1, 0));
            bomb_range_positions.Add(new Vector3Int(grid_position.x, grid_position.y - 1, 0));
        }
        else
        {
            bomb_range_positions.Add(new Vector3Int(grid_position.x - 1, grid_position.y, 0));
            bomb_range_positions.Add(new Vector3Int(grid_position.x, grid_position.y + 1, 0));
            bomb_range_positions.Add(new Vector3Int(grid_position.x, grid_position.y - 1, 0));
            bomb_range_positions.Add(new Vector3Int(grid_position.x + 1, grid_position.y, 0));
            bomb_range_positions.Add(new Vector3Int(grid_position.x + 1, grid_position.y + 1, 0));
            bomb_range_positions.Add(new Vector3Int(grid_position.x + 1, grid_position.y - 1, 0));
        }

        return bomb_range_positions;
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
                neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y + 2, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y + 2, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 2, grid_position.y + 1, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 2, grid_position.y, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 2, grid_position.y - 1, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y - 2, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y - 2, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y - 2, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y - 1, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 2, grid_position.y, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y + 1, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y + 2, 0));
            }
            else
            {
                neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y + 2, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y + 2, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y + 1, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 2, grid_position.y, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y - 1, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y - 2, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x, grid_position.y - 2, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y - 2, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 2, grid_position.y - 1, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 2, grid_position.y, 0));
                neighbour_tiles.Add(new Vector3Int(grid_position.x + 2, grid_position.y + 1, 0));
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

        for (int i = 0; i < _obstacles.Count; i++)
        {
            neighbour_tiles.Remove(_tilemap.WorldToCell(_obstacles[i].transform.position));
        }

        for (int i = 0; i < _enemies.Count; i++)
        {
            if (_enemies[i].GetComponent<IEnemy>().IsAlive())
            {
                neighbour_tiles.Remove(_tilemap.WorldToCell(_enemies[i].transform.position));
            }
        }

        neighbour_tiles.Remove(_tilemap.WorldToCell(_power_up_altair.gameObject.transform.position));

        return neighbour_tiles;
    }

    public List<Vector3Int> GetKnifeTiles(Vector3Int grid_position)
    {
        List<Vector3Int> knife_tiles = new List<Vector3Int>();

        if (Mathf.Abs(grid_position.y) % 2 == 0)
        {
            knife_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y, 0));
            knife_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y + 1, 0));
            knife_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y - 1, 0));
            knife_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y, 0));
            knife_tiles.Add(new Vector3Int(grid_position.x, grid_position.y + 1, 0));
            knife_tiles.Add(new Vector3Int(grid_position.x, grid_position.y - 1, 0));
        }
        else
        {
            knife_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y, 0));
            knife_tiles.Add(new Vector3Int(grid_position.x, grid_position.y + 1, 0));
            knife_tiles.Add(new Vector3Int(grid_position.x, grid_position.y - 1, 0));
            knife_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y, 0));
            knife_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y + 1, 0));
            knife_tiles.Add(new Vector3Int(grid_position.x + 1, grid_position.y - 1, 0));
        }

        return knife_tiles;
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

    public void EnableKnife()
    {
        if (!_player.GetComponent<PlayerInputController>()._active)
            return;
        _knife = true;
        _player.GetComponent<PlayerInputController>()._selected = true;
        HighlightKnifeCells();
        _ui_manager.DisableInteractionKnifeButton();
    }

    public void EnableKnockback()
    {
        if (!_player.GetComponent<PlayerInputController>()._active)
            return;
        _knockback = true;
        _player.GetComponent<PlayerInputController>()._selected = true;
        HighlightKnockbackCells();
        _ui_manager.DisableInteractionKnockbackButton();
    }

    public void DisableDash()
    {
        _dash = false;
        _ui_manager.EnableInteractionDashButton();
    }

    public void DisableKnife()
    {
        _knife = false;
        _ui_manager.EnableInteractionKnifeButton();
    }

    public void DisableKnockback()
    {
        _knockback = false;
        _ui_manager.EnableInteractionKnockbackButton();
    }

    public void ConsumeKnockback()
    {
        _knockbacks--;
        _knockback = false;
        if (_knockbacks == 0)
        {
            _ui_manager.DisableInteractionKnockbackButton();
        }
        else
        {
            _ui_manager.EnableInteractionKnockbackButton();
        }
        UpdateUIStats();
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

    public void ConsumeKnife()
    {
        _knifes--;
        _knife = false;
        if (_knifes == 0)
        {
            _ui_manager.DisableInteractionKnifeButton();
        }
        else
        {
            _ui_manager.EnableInteractionKnifeButton();
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
        _ui_manager.SetKnifeNumber(_knifes);
        _ui_manager.SetKnockNumber(_knockbacks);
    }

}
