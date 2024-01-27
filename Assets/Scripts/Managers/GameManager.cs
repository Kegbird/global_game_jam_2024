using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private bool _is_initial_scene;
    [SerializeField]
    private int _next_level;
    [SerializeField]
    private GameObject _player;
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
    private bool _win;

    private void Awake()
    {
        if (_is_initial_scene)
        {
            _hps = 3;
            _dashes = 3;
        }
    }

    private void Start()
    {
        Vector3Int spawn_grid_position = _tilemap.WorldToCell(_spawn.position);
        _player.transform.position = _tilemap.CellToWorld(spawn_grid_position);
        _exit_grid_position = _tilemap.WorldToCell(_exit.transform.position);
        _ui_manager.HideBlackScreen();
        UpdateUIStats();
    }

    private void Update()
    {
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

    public IEnumerator ActivateEnemies()
    {
        yield return null;
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

    private List<Vector3Int> GetNeighbourTiles(Vector3Int grid_position)
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
                neighbour_tiles.Add(new Vector3Int(grid_position.x - 1, grid_position.y +2, 0));
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

        if (!_win)
        {
            neighbour_tiles.Remove(_exit_grid_position);
        }

        if(_obstacles != null && _obstacles.Count>0)
        {
            for(int i=0; i < _obstacles.Count; i++)
            {
                neighbour_tiles.Remove(_tilemap.WorldToCell(_obstacles[i].transform.position));
            }
        }

        if(_enemies != null && _enemies.Count>0)
        {
            for(int i=0; i<_enemies.Count; i++)
            {
                neighbour_tiles.Remove(_tilemap.WorldToCell(_enemies[i].transform.position));
            }
        }
        return neighbour_tiles;
    }

    public void EnableDash()
    {
        _dash = true;
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

    public IEnumerator LoadNextLevel()
    {
        _ui_manager.ShowBlackScreen();
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(_next_level);
        yield return null;
    }

    public void UpdateUIStats()
    {
        _ui_manager.SetDashNumber(_dashes);
        _ui_manager.SetHpNumber(_hps);
    }

    public void GameOver()
    {

    }
}
