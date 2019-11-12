using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    private GameObject _playerCameraPosition;
    private bool _isPlayerSet;

    void Start()
    {
        Instance = this;
    }
    
    void FixedUpdate()
    {
        if (_isPlayerSet)
        {
            transform.SetPositionAndRotation(_playerCameraPosition.transform.position, _playerCameraPosition.transform.rotation);
        }
    }

    public void SetPlayer()
    {
        _playerCameraPosition = GameObject.FindGameObjectWithTag("ClientCameraPosition");
        _isPlayerSet = true;
    }
}
