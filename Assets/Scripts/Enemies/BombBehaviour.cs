using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombBehaviour : MonoBehaviour
{
    public GameManager _game_manager;

    private void Start()
    {
        _game_manager = GameManager.instance;
    }

    public IEnumerator Boom()
    {
        Vector3Int grid_position = _game_manager.GetGridPosition(transform.position);
        List<Vector3Int> grid_positions_in_bomb_range = _game_manager.GetBombRangePositions(grid_position);
        
        List<GameObject> enemies = _game_manager._enemies;
        Vector3Int player_grid_position = _game_manager.GetPlayerGridPosition();

        if(grid_positions_in_bomb_range.Contains(player_grid_position))
        {
            _game_manager.DamagePlayer(3);
        }

        for(int i=0; i< enemies.Count; i++)
        {
            Vector3Int enemy_grid_position = _game_manager.GetGridPosition(enemies[i].transform.position);
            if (grid_positions_in_bomb_range.Contains(enemy_grid_position) && enemies[i].GetComponent<IEnemy>().IsAlive())
            {
                enemies[i].GetComponent<IEnemy>().Kill();
            }
        }

        yield return null;
    }

}
