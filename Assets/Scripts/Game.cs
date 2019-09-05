using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public bool EnableServer;
    public bool EnableClient;

    private Server _server;
    private Client _client;
    
    // Start is called before the first frame update
    void Start()
    {
        if (EnableServer)
        {
            _server = new Server();
            _server.Start();
        }

        if (EnableClient)
        {
            _client = new Client();
            _client.Start();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (EnableServer)
        {
            _server.Update();
        }

        if (EnableClient)
        {
            _client.Update();
        }
    }
}
