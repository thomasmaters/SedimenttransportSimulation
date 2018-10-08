using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class UDPSend : MonoBehaviour
{
    public int src_port_ = 1232;
    public int remote_port_ = 1233;
    public string ip_ = "localhost";

    private readonly Queue<byte[]> tasks_queue_ = new Queue<byte[]>();
    
    public void sendData(byte[] data)
    {
        tasks_queue_.Enqueue(data);
    }

    private void Start()
    {
        Debug.Log("ip: " + ip_ + " src_port: " + src_port_ + " remote_port: " + remote_port_);
    }


    // Update is called once per frame
    void Update()
    {
        if (tasks_queue_.Count > 0)
        {
            lock (tasks_queue_)
            {
                if (tasks_queue_.Count > 0)
                {
                    byte[] data = tasks_queue_.Dequeue();
                    using (UdpClient c = new UdpClient(src_port_))
                        c.Send(data, data.Length, ip_, remote_port_);
                }
            }
        }
    }
}
