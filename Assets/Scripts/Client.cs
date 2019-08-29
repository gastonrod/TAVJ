using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Client : MonoBehaviour
{
    public bool enableClient;
    public String serverAddress;
    public short serverPort;
    public short clientPort;

    private IPAddress _serverAddress;
    private Connection _connection;
    private GameObject[] _cubes;
    private Protocol.Positions _positions;
    
    // Start is called before the first frame update
    void Start()
    {
        if (enableClient)
        {
            _serverAddress = IPAddress.Parse(serverAddress);
            _connection = new Connection(_serverAddress, clientPort, serverPort);
            _cubes = GameObject.FindGameObjectsWithTag("Cube");
            foreach (var cube in _cubes)
            {
                Destroy(cube.GetComponent<Rigidbody>());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (enableClient)
        {
            byte[] data = _connection.ReceiveData();
            if (data != null)
            {
                _positions = Protocol.Deserialize(data);
                _cubes[0].transform.position = new Vector3(_positions.x1, _positions.y1, _positions.z1);
                _cubes[1].transform.position = new Vector3(_positions.x2, _positions.y2, _positions.z2);
            }
        }
    }
}
