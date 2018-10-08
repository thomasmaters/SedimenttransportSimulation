using System.Collections.Generic;
using UnityEngine;

// Recommended to attach this script to the Main Camera instance.
// Will not render in play mode if not attached to Camera.
public class GLLineRenderer : MonoBehaviour
{
    public Material material_;
    public Material material_const_;

    public List<Vector3> points_;
    public List<Vector3> const_points_;
    public GameObject origin_;

    void OnPostRender()
    {
        renderLines(points_);
    }

    void OnDrawGizmos()
    {
        renderLines(points_);
    }

    public void renderLines(List<Vector3> points)
    {
        if (points == null || points.Count < 1)
        {
            return;
        }

        GL.Begin(GL.LINES);
        material_.SetPass(0);
        for (int i = 0; i < points.Count; i++)
        {
            GL.Vertex(origin_.transform.position - origin_.transform.up);
            GL.Vertex(points[i]);
        }
        GL.End();
        GL.Begin(GL.LINES);
        material_const_.SetPass(0);
        for (int i = 0; i < const_points_.Count; i++)
        {
            GL.Vertex(origin_.transform.position - origin_.transform.up);
            GL.Vertex(origin_.transform.position + const_points_[i]);
        }
        GL.End();
    }

}
