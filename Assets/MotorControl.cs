using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class MotorControl : MonoBehaviour
{
    IPEndPoint motorCtrl = new IPEndPoint(IPAddress.Parse("192.168.50.21"), 6666);
    Socket ctrlSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");
        byte[] buf = {122, 110, 90};
        ctrlSock.SendTo(buf, motorCtrl);
    }
}
