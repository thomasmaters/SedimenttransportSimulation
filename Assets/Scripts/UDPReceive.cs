using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

public class UDPReceive : MonoBehaviour
{
    // receiving Thread
    Thread receive_thread_;

    // udpclient object
    UdpClient client_;
    RippelSimulator rippel_simulator_;

    public int port_ = 1234;

    private static readonly Queue<byte[]> tasks_queue_ = new Queue<byte[]>();

    // start from unity3d
    public void Start()
    {
        client_ = new UdpClient(port_);
        rippel_simulator_ = gameObject.GetComponent<RippelSimulator>();
        try
        {
            client_.BeginReceive(new AsyncCallback(receiveData), null);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void Update()
    {
        if (tasks_queue_.Count > 0)
        {
            byte[] task = null;

            //Get a entry from the queue.
            lock (tasks_queue_)
            {
                if (tasks_queue_.Count > 0)
                {
                    task = tasks_queue_.Dequeue();
                }
            }
            //Update terrain data.
            rippel_simulator_.updateTerrain(task);
        }
    }

    // Reveive data async.
    private void receiveData(IAsyncResult res)
    {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, port_);
        byte[] data = client_.EndReceive(res, ref RemoteIpEndPoint);
        //Stop the received data in a queue so it can be processed by the gameloop.
        lock (tasks_queue_)
        {
            tasks_queue_.Enqueue(data);
        }
        Debug.Log(data);
        client_.BeginReceive(new AsyncCallback(receiveData), null);
    }
}