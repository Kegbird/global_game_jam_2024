using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private int _next_level;
    [SerializeField]
    private GameObject _player;
    [SerializeField]
    private List<GameObject> _enemies;
    [SerializeField]
    private UIManager _ui_manager;
    [SerializeField]
    private Tilemap _tilemap;
    [SerializeField]
    private Transform _spawn;
    [SerializeField]
    private int _player_hps;
    [SerializeField]
    private int _player_dashes;
    [SerializeField]
    private GameObject _exit;
    [SerializeField]
    private Vector3Int _exit_grid_position;
    [SerializeField]
    private bool _win;

    private void Awake()
    {
    }

    private void Start()
    {
        Vector3Int spawn_grid_position = _tilemap.WorldToCell(_spawn.position);
        _player.transform.position = _tilemap.CellToWorld(spawn_grid_position);
        _exit_grid_position = _tilemap.WorldToCell(_exit.transform.position);
        _ui_manager.HideBlackScreen();
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

    public void HighlightPlayerMovementCells()
    {
        Vector3Int player_grid_position = _tilemap.WorldToCell(_player.transform.position);

        List<Vector3Int> neighbour_positions = GetNeighbourTiles(player_grid_position);

        for (int i=0; i < neighbour_positions.Count; i++)
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

    private List<Vector3Int> GetNeighbourTiles(Vector3Int grid_psoition)
    {
        List<Vector3Int> neighbour_tiles = new List<Vector3Int>();
        if(Mathf.Abs(grid_psoition.y) % 2 == 0)
        {
            neighbour_tiles.Add(new Vector3Int(grid_psoition.x - 1, grid_psoition.y, 0));
            neighbour_tiles.Add(new Vector3Int(grid_psoition.x - 1, grid_psoition.y + 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_psoition.x - 1, grid_psoition.y - 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_psoition.x + 1, grid_psoition.y, 0));
            neighbour_tiles.Add(new Vector3Int(grid_psoition.x, grid_psoition.y + 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_psoition.x, grid_psoition.y - 1, 0));
        } 
        else
        {
            neighbour_tiles.Add(new Vector3Int(grid_psoition.x - 1, grid_psoition.y, 0));
            neighbour_tiles.Add(new Vector3Int(grid_psoition.x, grid_psoition.y + 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_psoition.x, grid_psoition.y - 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_psoition.x + 1, grid_psoition.y, 0));
            neighbour_tiles.Add(new Vector3Int(grid_psoition.x + 1, grid_psoition.y + 1, 0));
            neighbour_tiles.Add(new Vector3Int(grid_psoition.x + 1, grid_psoition.y - 1, 0));
        }
        if(!_win)
        {
            neighbour_tiles.Remove(_exit_grid_position);
        }
        return neighbour_tiles;
    }

    public IEnumerator LoadNextLevel()
    {
        _ui_manager.ShowBlackScreen();
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(_next_level);
        yield return null;
    }

    public void GameOver()
    {

    }


}
