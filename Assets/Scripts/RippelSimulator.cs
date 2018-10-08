using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

public class RippelSimulator : MonoBehaviour
{
    public GameObject terrain_;

    private float wave_strength_ = 0.0f;
    private ushort wave_direction_ = 0;
    private float d_constant = 0.0f;
    private float beta_constant = 0.0f;

    UDPReceive udp_server_;
    UDPSend udp_client_;

    public Slider slider_wave_direction;
    public Slider slider_wave_strength;
    public Slider slider_d_const;
    public Slider slider_beta_const;

    private void Start()
    {
        udp_client_ = GetComponent<UDPSend>();
        udp_server_ = gameObject.AddComponent(typeof(UDPReceive)) as UDPReceive;
    }

    public void updateValues()
    {
        d_constant = slider_d_const.value;
        beta_constant = slider_beta_const.value;
        wave_strength_ = slider_wave_strength.value;
        wave_direction_ = (ushort)slider_wave_direction.value;
        sendSettingUpdate();
    }

    //Sends slider settings to the wave simulator.
    private void sendSettingUpdate()
    {
        byte[] data = new byte[16];
        byte[] strength_data = BitConverter.GetBytes(wave_strength_);
        byte[] d_data = BitConverter.GetBytes(d_constant);
        byte[] beta_data = BitConverter.GetBytes(beta_constant);
        data[0] = 0xFE;
        data[1] = strength_data[0];
        data[2] = strength_data[1];
        data[3] = strength_data[2];
        data[4] = strength_data[3];
        data[5] = (byte)(wave_direction_ & 0xFF);
        data[6] = (byte)((wave_direction_ >> 8) & 0xFF);
        data[7] = d_data[0];
        data[8] = d_data[1];
        data[9] = d_data[2];
        data[10] = d_data[3];
        data[11] = beta_data[0];
        data[12] = beta_data[1];
        data[13] = beta_data[2];
        data[14] = beta_data[3];
        data[15] = 0xEF;
        udp_client_.sendData(data);
    }

    //Sets the terrainheight.
    public void updateHeight(Slider slider)
    {
        Vector3 temp;
        temp.x = Terrain.activeTerrain.terrainData.size.x;
        temp.z = Terrain.activeTerrain.terrainData.size.z;
        temp.y = slider.value;
        Terrain.activeTerrain.terrainData.size = temp;
    }

    //Decodes received byte data into coordinates for the terrain grid.
    public void updateTerrain(byte[] data)
    {
        ushort x = BitConverter.ToUInt16(data, 0);
        ushort y = BitConverter.ToUInt16(data, 2);
        Debug.Log("x_size: " + x + " y_size: " + y);
        if (data.Length != (x * y + 4))
        {
            Debug.Log("Size mismatch, expected: " + (x * y + 4) + " got: " + data.Length);
            return;
        }
        float[,] test = new float[x, y];
        for (int i = 0; i < x; ++i)
        {
            for (int k = 0; k < y; ++k)
            {
                test[i, k] = (float)data[i * x + k + 4] / 255;
            }
        }

        Terrain.activeTerrain.terrainData.SetHeightsDelayLOD(0, 0, test);
    }
}

