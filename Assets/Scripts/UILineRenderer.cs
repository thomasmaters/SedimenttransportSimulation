// from http://forum.unity3d.com/threads/new-ui-and-line-drawing.253772/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UILineRenderer : Graphic
{
    [SerializeField] Texture texture_;
    [SerializeField] Rect uv_rect_ = new Rect(0f, 0f, 1f, 1f);

    public float line_thickness_ = 4;
    public bool use_margins_;
    public Vector2 margins_;
    public List<Vector2> points_;
    public bool relative_size_;

    public override Texture mainTexture
    {
        get
        {
            return texture_ == null ? s_WhiteTexture : texture_;
        }
    }

    /// <summary>
    /// Texture to be used.
    /// </summary>
    public Texture texture
    {
        get
        {
            return texture_;
        }
        set
        {
            if (texture_ == value)
                return;

            texture_ = value;
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }

    /// <summary>
    /// UV rectangle used by the texture.
    /// </summary>
    public Rect uvRect
    {
        get
        {
            return uv_rect_;
        }
        set
        {
            if (uv_rect_ == value)
                return;
            uv_rect_ = value;
            SetVerticesDirty();
        }
    }

    //protected override void OnFillVBO(List<UIVertex> vbo)
    protected override void OnPopulateMesh(VertexHelper to_fill)
    {
        // requires sets of quads
        if (points_ == null || points_.Count < 2)
        {
            return;
        }

        var sizeX = rectTransform.rect.width;
        var sizeY = rectTransform.rect.height;
        var offsetX = -rectTransform.pivot.x * rectTransform.rect.width;
        var offsetY = -rectTransform.pivot.y * rectTransform.rect.height;

        // don't want to scale based on the size of the rect, so this is switchable now
        if (!relative_size_)
        {
            sizeX = 1;
            sizeY = 1;
        }

        var TempPoints = points_.ToArray();
        if (use_margins_)
        {
            sizeX -= margins_.x;
            sizeY -= margins_.y;
            offsetX += margins_.x / 2f;
            offsetY += margins_.y / 2f;
        }

        to_fill.Clear();
        var vbo = to_fill;

        Vector2 prevV1 = Vector2.zero;
        Vector2 prevV2 = Vector2.zero;

        for (int i = 1; i < TempPoints.Length; i++)
        {
            var prev = TempPoints[i - 1];
            var cur = TempPoints[i];
            prev = new Vector2(prev.x * sizeX + offsetX, prev.y * sizeY + offsetY);
            cur = new Vector2(cur.x * sizeX + offsetX, cur.y * sizeY + offsetY);

            float angle = Mathf.Atan2(cur.y - prev.y, cur.x - prev.x) * 180f / Mathf.PI;

            var v1 = prev + new Vector2(0, -line_thickness_ / 2);
            var v2 = prev + new Vector2(0, +line_thickness_ / 2);
            var v3 = cur + new Vector2(0, +line_thickness_ / 2);
            var v4 = cur + new Vector2(0, -line_thickness_ / 2);

            v1 = RotatePointAroundPivot(v1, prev, new Vector3(0, 0, angle));
            v2 = RotatePointAroundPivot(v2, prev, new Vector3(0, 0, angle));
            v3 = RotatePointAroundPivot(v3, cur, new Vector3(0, 0, angle));
            v4 = RotatePointAroundPivot(v4, cur, new Vector3(0, 0, angle));

            Vector2 uvTopLeft = Vector2.zero;
            Vector2 uvBottomLeft = new Vector2(0, 1);

            Vector2 uvTopCenter = new Vector2(0.5f, 0);
            Vector2 uvBottomCenter = new Vector2(0.5f, 1);

            Vector2 uvTopRight = new Vector2(1, 0);
            Vector2 uvBottomRight = new Vector2(1, 1);

            Vector2[] uvs = new[] { uvTopCenter, uvBottomCenter, uvBottomCenter, uvTopCenter };

            if (i > 1)
                SetVbo(vbo, new[] { prevV1, prevV2, v1, v2 }, uvs);

            if (i == 1)
                uvs = new[] { uvTopLeft, uvBottomLeft, uvBottomCenter, uvTopCenter };
            else if (i == TempPoints.Length - 1)
                uvs = new[] { uvTopCenter, uvBottomCenter, uvBottomRight, uvTopRight };

            vbo.AddUIVertexQuad(SetVbo(vbo, new[] { v1, v2, v3, v4 }, uvs));

            prevV1 = v3;
            prevV2 = v4;
        }
    }

    //protected void SetVbo(UIVertex vbo, Vector2[] vertices, Vector2[] uvs)
    protected UIVertex[] SetVbo(VertexHelper vbo, Vector2[] vertices, Vector2[] uvs)
    {
        UIVertex[] VboVertices = new UIVertex[4];

        for (int i = 0; i < vertices.Length; i++)
        {
            var vert = UIVertex.simpleVert;
            vert.color = color;
            vert.position = vertices[i];
            vert.uv0 = uvs[i];
            VboVertices[i] = vert;
        }

        return VboVertices;
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }
}