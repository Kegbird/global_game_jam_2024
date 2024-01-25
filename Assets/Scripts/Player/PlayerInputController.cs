using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField]
    private bool _active;
    [SerializeField]
    private bool _editor;


    private void Awake()
    {
        _active = true;
    }

    private void Update()
    {
        if (!_active)
            return;
    }

    private void OnMouseDown()
    {
        if (!_active)
            return;


    }
}
