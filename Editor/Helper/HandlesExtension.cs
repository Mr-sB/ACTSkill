using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    public static class HandlesExtension
    {
        public const int DEFAULT_CIRCLE_LENGTH = 30;
        private static readonly Stack<Color> colorStack = new Stack<Color>();
        private static Color outlineColor => new Color(1, 1, 1, Handles.color.a);
        private static readonly int[] cubeIndices = new int[]
        {
            0, 1, 2, 3, //上
            4, 5, 6, 7, //下
            2, 6, 5, 3, //左
            0, 4, 7, 1, //右
            1, 7, 6, 2, //前
            0, 3, 5, 4, //后
        };

        // private static readonly Mesh sphereMesh;
        // private static readonly Material unlitMat;
        // private static readonly int ColorID = Shader.PropertyToID("_Color");
        //
        // static HandlesExtension()
        // {
        //     sphereMesh = typeof(Handles).GetProperty("sphereMesh", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null, null) as Mesh;
        //     unlitMat = new Material(Shader.Find("GUI/Text Shader"));
        // }

        public static void DrawRect(Vector2 size, Matrix4x4 matrix, Color? color = null, bool outline = true)
        {
            float halfW = size.x * 0.5f;
            float halfH = size.y * 0.5f;

            Vector3[] vertices = new Vector3[]
            {
                matrix.MultiplyPoint3x4(new Vector3(-halfW, halfH)),
                matrix.MultiplyPoint3x4(new Vector3(halfW, halfH)),
                matrix.MultiplyPoint3x4(new Vector3(halfW, -halfH)),
                matrix.MultiplyPoint3x4(new Vector3(-halfW, -halfH)),
            };
            
            DrawPolygon(vertices, color, outline);
        }

        public static void DrawRect(Vector2 size, Vector3 position, Quaternion rotation, Vector3 scale, Color? color = null, bool outline = true)
        {
            DrawRect(size, Matrix4x4.TRS(position, rotation, scale), color, outline);
        }
        
        public static void DrawCircle(float radius, Matrix4x4 matrix, Color? color = null, bool outline = true)
        {
            DrawPolygon(CalcCircleVertices(radius, matrix), color, outline);
        }

        public static void DrawCircle(float radius, Vector3 position, Quaternion rotation, Vector3 scale, Color? color = null, bool outline = true)
        {
            DrawCircle(radius, Matrix4x4.TRS(position, rotation, scale), color, outline);
        }
        
        public static void DrawBox(Vector3 size, Matrix4x4 matrix, Color? color = null, bool outline = true)
        {
            Vector3 halfSize = size * 0.5f;

            Vector3[] vertices = new Vector3[8];

            //Up
            //3 0
            //2 1
            vertices[0] = matrix.MultiplyPoint3x4(new Vector3(halfSize.x, halfSize.y, halfSize.z));
            vertices[1] = matrix.MultiplyPoint3x4(new Vector3(halfSize.x, halfSize.y, -halfSize.z));
            vertices[2] = matrix.MultiplyPoint3x4(new Vector3(-halfSize.x, halfSize.y, -halfSize.z));
            vertices[3] = matrix.MultiplyPoint3x4(new Vector3(-halfSize.x, halfSize.y, halfSize.z));

            //Down
            //5 4
            //6 7
            vertices[4] = matrix.MultiplyPoint3x4(new Vector3(halfSize.x, -halfSize.y, halfSize.z));
            vertices[5] = matrix.MultiplyPoint3x4(new Vector3(-halfSize.x, -halfSize.y, halfSize.z));
            vertices[6] = matrix.MultiplyPoint3x4(new Vector3(-halfSize.x, -halfSize.y, -halfSize.z));
            vertices[7] = matrix.MultiplyPoint3x4(new Vector3(halfSize.x, -halfSize.y, -halfSize.z));

            Vector3[] polygon = new Vector3[4];
            for (int i = 0; i < 6; i++)
            {
                polygon[0] = vertices[cubeIndices[i * 4]];
                polygon[1] = vertices[cubeIndices[i * 4 + 1]];
                polygon[2] = vertices[cubeIndices[i * 4 + 2]];
                polygon[3] = vertices[cubeIndices[i * 4 + 3]];
                DrawPolygon(polygon, color, outline);
            }
        }
        
        public static void DrawBox(Vector3 size, Vector3 position, Quaternion rotation, Vector3 scale, Color? color = null, bool outline = true)
        {
            DrawBox(size, Matrix4x4.TRS(position, rotation, scale), color, outline);
        }
        
        public static void DrawSphere(float radius, Matrix4x4 matrix, Color? color = null, bool outline = true)
        {
            // Use circle polygon to draw sphere face to camera projection
            SceneView sceneView = SceneView.currentDrawingSceneView;
            if (sceneView != null)
            {
                var camera = sceneView.camera;
                var cameraTrans = camera.transform;
                //Face to camera
                Vector3 position = Handles.matrix.MultiplyPoint3x4(matrix.GetPosition());
                Quaternion rotation;
                float size;
                if (camera.orthographic)
                {
                    rotation = Quaternion.LookRotation(-cameraTrans.forward, cameraTrans.up);
                    size = radius;
                }
                else
                {
                    var vec = cameraTrans.position - position;
                    // Calc distance and normalize
                    var distance = vec.magnitude;
                    Vector3 forward;
                    if (distance > 9.999999747378752E-06)
                        forward = vec / distance;
                    else
                        forward = Vector3.zero;
                    var offset = radius * radius / distance;
                    // Add offset
                    position += forward * offset;
                    rotation = Quaternion.LookRotation(forward, cameraTrans.up);
                    size = radius * Mathf.Sin(Mathf.Acos(offset / radius));
                }
                var oldMatrix = Handles.matrix;
                var lookMatrix = Matrix4x4.TRS(position, rotation, Vector3.Scale(oldMatrix.lossyScale, matrix.lossyScale));
                Handles.matrix = Matrix4x4.identity;
                DrawCircle(size, lookMatrix, color);
                Handles.matrix = oldMatrix;
            }
            // Use 3d sphere model to draw sphere face to camera projection
            // if (color.HasValue)
            //     PushColor(color.Value);
            // unlitMat.SetPass(0);
            // unlitMat.SetColor(ColorID, Handles.color);
            // //Draw a real sphere model.
            // Graphics.DrawMeshNow(sphereMesh, Handles.matrix * matrix * Matrix4x4.Scale(Vector3.one * (radius * 2)));
            // if (color.HasValue)
            //     PopColor();

            if (outline)
            {
                PushColor(outlineColor);
                Vector3[] vertices = CalcCircleVertices(radius, matrix);
                DrawLines(vertices);
                vertices = CalcCircleVertices(radius, matrix * Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0)), cache: vertices);
                DrawLines(vertices);
                vertices = CalcCircleVertices(radius, matrix * Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0)), cache: vertices);
                DrawLines(vertices);
                PopColor();
            }
        }

        public static void DrawSphere(float radius, Vector3 position, Quaternion rotation, Vector3 scale, Color? color = null, bool outline = true)
        {
            DrawSphere(radius, Matrix4x4.TRS(position, rotation, scale), color, outline);
        }
        
        
        public static void DrawPolygon(Vector3[] vertices, Color? color = null, bool outline = true)
        {
            if (color.HasValue)
                PushColor(color.Value);
            FillPolygon(vertices);
            if (color.HasValue)
                PopColor();
            
            //Outline
            if (outline)
            {
                PushColor(outlineColor);
                DrawLines(vertices);
                PopColor();
            }
        }

        public static void FillPolygon(Vector3[] vertices)
        {
            Handles.DrawAAConvexPolygon(vertices);
        }

        public static void DrawLines(IReadOnlyList<Vector3> vertices, int count = -1)
        {
            if (count < 0)
                count = vertices.Count;
            for (int i = 0; i < count; i++)
                DrawLine(vertices[i], vertices[(i + 1) % count]);
        }
        
        public static void DrawLine(Vector3 start, Vector3 end)
        {
            Handles.DrawLine(start, end);
        }

        public static Vector3[] CalcCircleVertices(float radius, Matrix4x4 matrix, int length = DEFAULT_CIRCLE_LENGTH, Vector3[] cache = null)
        {
            if (length < 3) length = DEFAULT_CIRCLE_LENGTH;
            Vector3[] vertices;
            if (cache != null && cache.Length == length)
                vertices = cache;
            else
                vertices = new Vector3[length];
            for (int i = 0; i < length; i++)
            {
                float rad = (float)i / length * 2 * Mathf.PI;
                Vector2 pos = new Vector2(radius * Mathf.Cos(rad), radius * Mathf.Sin(rad));
                vertices[i] = matrix.MultiplyPoint3x4(pos);
            }
            return vertices;
        }
        
        public static void PushColor(Color color)
        {
            colorStack.Push(color);
            Handles.color = color;
        }
        
        public static void PopColor()
        {
            if (colorStack.Count <= 0) return;
            Handles.color = colorStack.Pop();
        }
    }
}
