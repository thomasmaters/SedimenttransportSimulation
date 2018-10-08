using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorSimulation : MonoBehaviour
{
    public int beams_ = 480;
    public float max_range_ = 20.0f;
    public float ping_rate_ = 30.0f;

    public float start_angle_ = -60.0f;
    public int angle_range_ = 120;
    public float speed_ = 20.0f;

    public Terrain terrain_;
    public Canvas canvas_;
    public GameObject gl_line_renderer_attached_camera_; //To draw lines in 3D using GL, we need the render script attached to the camera.
    private UILineRenderer ui_line_renderer_; //Draws in 2D;
    private GLLineRenderer gl_line_renderer_; //Draws in 3D;
    private TerrainCollider terrain_collider_;

    private Ray ray_;
    private RaycastHit raycast_hit_ = new RaycastHit();
    private UDPSend udp_client_;

    private byte[] send_data_ = new byte[256 + 2 * 480];
    private static readonly byte[] byte_data_ = new byte[] { 0x38, 0x33, 0x50, 0x0a, 0x04, 0xc0, 0x20, 0x20, 0x31, 0x32, 0x2d, 0x4a, 0x55, 0x4e, 0x2d, 0x32, 0x30, 0x31, 0x35, 0x20, 0x31, 0x32, 0x3a, 0x35, 0x39, 0x3a, 0x32, 0x34, 0x20, 0x2e, 0x37, 0x33, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x03, 0x84, 0x83, 0x84, 0x20, 0x20, 0x01, 0xe0, 0x01, 0xf4, 0x20, 0x78, 0x2e, 0xed, 0x19, 0x20, 0x05, 0x02, 0xa3, 0xb9, 0xd0, 0x20, 0x01, 0x20, 0x1e, 0x20, 0xb4, 0x20, 0x67, 0x20, 0x20, 0x20, 0x01, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x2e, 0x37, 0x33, 0x34, 0x20, 0x20, 0x20, 0x05, 0x20, 0x20, 0x01, 0x20, 0x20, 0x05, 0x0f, 0xa0, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };


    // Start is called before the first frame update
    void Start()
    {
        gl_line_renderer_ = gl_line_renderer_attached_camera_.GetComponent<GLLineRenderer>();
        ui_line_renderer_ = canvas_.GetComponent<UILineRenderer>();
        //Initalize a default header.
                for(int i = 0; i < byte_data_.Length; ++i)
        {
            send_data_[i] = byte_data_[i];
        }

        udp_client_ = GetComponent<UDPSend>();
        terrain_collider_ = terrain_.GetComponent<TerrainCollider>();

        gl_line_renderer_.const_points_.Add(Quaternion.Euler(start_angle_, transform.rotation.eulerAngles.y + 90, 0) * -transform.up * max_range_ - transform.up);
        gl_line_renderer_.const_points_.Add(Quaternion.Euler(start_angle_ + angle_range_, transform.rotation.eulerAngles.y + 90, 0) * -transform.up * max_range_ - transform.up);
    }

    // Update is called once per frame
    void Update()
    {
        ui_line_renderer_.points_.Clear();
        gl_line_renderer_.points_.Clear();
        for (int i = 0; i <= beams_; ++i)
        {
            //Construct a raycast and let in only collide with the terrain.
            ray_.origin = transform.position - transform.up;
            ray_.direction = Quaternion.Euler(start_angle_ + (float)i / beams_ * angle_range_, transform.rotation.eulerAngles.y + 90, 0) * -transform.up * max_range_;
            terrain_collider_.Raycast(ray_, out raycast_hit_, max_range_);

            if(raycast_hit_.collider != null)
            {
                //Render in 3D.
                gl_line_renderer_.points_.Add(new Vector3(raycast_hit_.point.x, raycast_hit_.point.y, raycast_hit_.point.z));

                //Render in 2D.
                Vector3 relative_pos = transform.position - raycast_hit_.point;
                ui_line_renderer_.points_.Add(new Vector2(125 + relative_pos.x * 7.0f, 20 + raycast_hit_.point.y * 7.0f));

                //Set beam value in package.
                setBeamPackageData(i, raycast_hit_.distance);
            }
            else
            {
                setBeamPackageData(i, 0);
            }
        }

        //Force the UI to rerender.
        ui_line_renderer_.SetVerticesDirty();

        //Send measurement data.
        udp_client_.sendData(send_data_);

        //Let us control the position of the sensor.
        updateMovement();
    }

    //Converts a beam range into bytes for the ProfilePointData protocol.
    private void setBeamPackageData(int beam, float range)
    {
        if(beam >= 480)
        {
            return;
        }
        byte[] bytes = System.BitConverter.GetBytes((ushort)(range * 1000));
        send_data_[256 + beam * 2] = bytes[0];
        send_data_[256 + beam * 2 + 1] = bytes[1];
    }

    //Let us control the simulated sensor.
    private void updateMovement()
    {
        if(Input.GetKey(KeyCode.Keypad8))
        {
            transform.position += transform.forward * Time.deltaTime * speed_;
        }
        if (Input.GetKey(KeyCode.Keypad5))
        {
            transform.position -= transform.forward * Time.deltaTime * speed_;
        }
        if (Input.GetKey(KeyCode.Keypad6))
        {
            transform.position += transform.right * Time.deltaTime * speed_;
        }
        if (Input.GetKey(KeyCode.Keypad4))
        {
            transform.position -= transform.right * Time.deltaTime * speed_;
        }
        if (Input.GetKey(KeyCode.Keypad7))
        {
            transform.Rotate(-transform.up * Time.deltaTime * speed_ * 3);
        }
        if (Input.GetKey(KeyCode.Keypad9))
        {
            transform.Rotate(transform.up * Time.deltaTime * speed_ * 3);
        }
        if (Input.GetKey(KeyCode.Keypad1))
        {
            transform.position += transform.up * Time.deltaTime * speed_;
        }
        if (Input.GetKey(KeyCode.Keypad3))
        {
            transform.position -= transform.up * Time.deltaTime * speed_;
        }
    }
}
