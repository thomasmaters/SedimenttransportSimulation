using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

namespace Simulation
{
    public class RippelSimulator : MonoBehaviour
    {
        //C like defines.
        private static readonly int SIMULATION_SETTINGS_MESSAGE_SIZE = 16;
        private static readonly int SS_STRENGTH_OFFSET = 1;
        private static readonly int SS_DIRECTION_LOW = 5;
        private static readonly int SS_DIRECTION_HIGH = 6;
        private static readonly int SS_D_OFFSET = 7;
        private static readonly int SS_BETA_OFFSET = 11;
        private static readonly byte SS_START_BYTE = 0xFE;
        private static readonly byte SS_END_BYTE = 0xEF;

        private static readonly int RD_X_LOW = 0;
        private static readonly int RD_X_HIGH = 1;
        private static readonly int RD_Y_LOW = 2;
        private static readonly int RD_Y_HIGH = 3;

        public GameObject terrain_;

        private float wave_strength_ = 0.0f;
        private ushort wave_direction_ = 0;
        private float d_constant_ = 0.0f;
        private float beta_constant_ = 0.0f;

        UDPReceive udp_server_;
        UDPSend udp_client_;

        public Slider slider_wave_direction_;
        public Slider slider_wave_strength_;
        public Slider slider_d_const_;
        public Slider slider_beta_const_;

        /// <summary>
        /// Default Unity function.
        /// </summary>
        private void Start()
        {
            udp_client_ = GetComponent<UDPSend>();
            udp_server_ = gameObject.AddComponent(typeof(UDPReceive)) as UDPReceive;
        }

        /// <summary>
        /// Updates variables from slider values.
        /// </summary>
        public void updateValues()
        {
            d_constant_ = slider_d_const_.value;
            beta_constant_ = slider_beta_const_.value;
            wave_strength_ = slider_wave_strength_.value;
            wave_direction_ = (ushort)slider_wave_direction_.value;
            sendSettingUpdate();
        }

        /// <summary>
        /// Sends slider settings to the rippel simulator.
        /// </summary>
        private void sendSettingUpdate()
        {
            byte[] data = new byte[SIMULATION_SETTINGS_MESSAGE_SIZE];
            byte[] strength_data = BitConverter.GetBytes(wave_strength_);
            byte[] d_data = BitConverter.GetBytes(d_constant_);
            byte[] beta_data = BitConverter.GetBytes(beta_constant_);
            data[0] = SS_START_BYTE;
            data[SS_STRENGTH_OFFSET] = strength_data[0];
            data[SS_STRENGTH_OFFSET + 1] = strength_data[1];
            data[SS_STRENGTH_OFFSET + 2] = strength_data[2];
            data[SS_STRENGTH_OFFSET + 3] = strength_data[3];
            data[SS_DIRECTION_LOW] = (byte)(wave_direction_ & 0xFF);
            data[SS_DIRECTION_HIGH] = (byte)((wave_direction_ >> 8) & 0xFF);
            data[SS_D_OFFSET] = d_data[0];
            data[SS_D_OFFSET + 1] = d_data[1];
            data[SS_D_OFFSET + 2] = d_data[2];
            data[SS_D_OFFSET + 3] = d_data[3];
            data[SS_BETA_OFFSET] = beta_data[0];
            data[SS_BETA_OFFSET + 1] = beta_data[1];
            data[SS_BETA_OFFSET + 2] = beta_data[2];
            data[SS_BETA_OFFSET + 3] = beta_data[3];
            data[15] = SS_END_BYTE;
            udp_client_.sendData(data);
        }

        /// <summary>
        /// Sets the terrainheight.
        /// </summary>
        /// <param name="slider"></param>
        public void updateHeight(Slider slider)
        {
            Vector3 temp;
            temp.x = Terrain.activeTerrain.terrainData.size.x;
            temp.z = Terrain.activeTerrain.terrainData.size.z;
            temp.y = slider.value;
            Terrain.activeTerrain.terrainData.size = temp;
        }

        /// <summary>
        /// Decodes received byte data into coordinates for the terrain grid.
        /// </summary>
        /// <param name="data"></param>
        public void updateTerrain(byte[] data)
        {
            ushort x = BitConverter.ToUInt16(data, RD_X_LOW);
            ushort y = BitConverter.ToUInt16(data, RD_Y_LOW);
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
}

