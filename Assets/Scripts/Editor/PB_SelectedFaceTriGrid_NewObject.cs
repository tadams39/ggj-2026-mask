#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

public class PB_SelectedFaceTriGrid_NewObject : EditorWindow
{
    // Target spacing (meters) for subdivision density.
    // This subdivides triangles based on edge length, so it’s "roughly" this size.
    private float gridSize = 0.25f;

    [MenuItem("Tools/ProBuilder/Subdivide Selected Faces To ~Tri Grid (New Object)")]
    public static void ShowWindow()
    {
        var w = GetWindow<PB_SelectedFaceTriGrid_NewObject>();
        w.titleContent = new GUIContent("PB Face TriGrid");
        w.minSize = new Vector2(360, 110);
        w.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Creates a NEW ProBuilder object from the currently selected faces,", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("subdivided into a roughly-even triangulated grid.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(8);

        gridSize = EditorGUILayout.FloatField(new GUIContent("Grid Size (m)", "Target spacing; result is approximate."), gridSize);
        gridSize = Mathf.Max(0.01f, gridSize);

        GUILayout.Space(8);

        using (new EditorGUI.DisabledScope(!CanRun()))
        {
            if (GUILayout.Button("Create NEW Subdivided Object From Selected Faces", GUILayout.Height(28)))
            {
                CreateNewMeshFromSelectedFaces(gridSize);
            }
        }

        if (!CanRun())
        {
            GUILayout.Space(6);
            EditorGUILayout.HelpBox(
                "Select a ProBuilder mesh, enter Face selection mode, and select at least one face.",
                MessageType.Info
            );
        }
    }

    private static bool CanRun()
    {
        var go = Selection.activeGameObject;
        if (go == null) return false;
        var pb = go.GetComponent<ProBuilderMesh>();
        if (pb == null) return false;
        return pb.selectedFaceCount > 0;
    }

    private static void CreateNewMeshFromSelectedFaces(float targetGrid)
    {
        var srcGo = Selection.activeGameObject;
        if (srcGo == null) return;

        var srcPb = srcGo.GetComponent<ProBuilderMesh>();
        if (srcPb == null) return;

        // Grab selected faces from the source mesh (does not modify source).
        var selectedFaces = srcPb.GetSelectedFaces();
        if (selectedFaces == null || selectedFaces.Length == 0)
        {
            UnityEditor.EditorUtility.DisplayDialog(
                "ProBuilder",
                "No faces selected. Switch to Face selection mode and select some faces.",
                "OK"
            );
            return;
        }

        // We'll build a new positions + faces list for the new mesh.
        // NOTE: This intentionally does NOT weld or share vertices across triangles.
        // For sculpting, the irregularity and extra vertices are fine, and this avoids tricky topology issues.
        var newPositions = new List<Vector3>(1024);
        var newFaces = new List<Face>(2048);

        // Source positions are in the source object's local space.
        // We'll create the new object with the same transform so local-space points land in the same world place.
        var srcPositions = srcPb.positions;

        for (int f = 0; f < selectedFaces.Length; f++)
        {
            var face = selectedFaces[f];
            var tri = face.indexes; // triangle indices (multiple of 3)

            // Subdivide each triangle in this face independently.
            for (int t = 0; t + 2 < tri.Count; t += 3)
            {
                int i0 = tri[t];
                int i1 = tri[t + 1];
                int i2 = tri[t + 2];

                Vector3 p0 = srcPositions[i0];
                Vector3 p1 = srcPositions[i1];
                Vector3 p2 = srcPositions[i2];

                SubdivideTriangleToTriGrid(
                    p0, p1, p2,
                    targetGrid,
                    face.submeshIndex,
                    newPositions,
                    newFaces
                );
            }
        }

        // Create the new ProBuilder object.
        // This creates a new GameObject with ProBuilderMesh + MeshFilter + MeshRenderer.
        var dstPb = ProBuilderMesh.Create(newPositions, newFaces);

        // Name + transform match source so it sits exactly on top of the source object.
        dstPb.gameObject.name = srcGo.name + "_TriGrid";
        var dstT = dstPb.transform;
        var srcT = srcGo.transform;

        dstT.SetParent(srcT.parent, worldPositionStays: false);
        dstT.localPosition = srcT.localPosition;
        dstT.localRotation = srcT.localRotation;
        dstT.localScale = srcT.localScale;

        // Copy materials (submeshIndex references these).
        var srcR = srcGo.GetComponent<MeshRenderer>();
        var dstR = dstPb.GetComponent<MeshRenderer>();
        if (srcR != null && dstR != null)
            dstR.sharedMaterials = srcR.sharedMaterials;

        // Build the Unity Mesh + refresh.
        dstPb.ToMesh();
        dstPb.Refresh();

        // Select new object and select all of its faces.
        Selection.activeGameObject = dstPb.gameObject;
        dstPb.SetSelectedFaces(dstPb.faces);

        // Mark scene dirty for undo/serialization.
        Undo.RegisterCreatedObjectUndo(dstPb.gameObject, "Create ProBuilder TriGrid Object");
        EditorUtility.SetDirty(dstPb);
    }

    private static void SubdivideTriangleToTriGrid(
        Vector3 p0, Vector3 p1, Vector3 p2,
        float gridSize,
        int submeshIndex,
        List<Vector3> positionsOut,
        List<Face> facesOut
    )
    {
        float e0 = Vector3.Distance(p0, p1);
        float e1 = Vector3.Distance(p1, p2);
        float e2 = Vector3.Distance(p2, p0);

        float maxEdge = Mathf.Max(e0, Mathf.Max(e1, e2));

        // How many segments along the longest edge to get ~gridSize spacing.
        int n = Mathf.Max(1, Mathf.CeilToInt(maxEdge / gridSize));

        // Map (i,j) -> vertex index in positionsOut for this triangle.
        // Constraint: i in [0..n], j in [0..n-i]
        int RowStart(int i) => (i * (2 * (n + 1) - (i - 1))) / 2; // triangular number layout

        // Create all vertices.
        int baseIndex = positionsOut.Count;

        for (int i = 0; i <= n; i++)
        {
            for (int j = 0; j <= (n - i); j++)
            {
                float a = (float)i / n;
                float b = (float)j / n;

                // p = p0 + a*(p1-p0) + b*(p2-p0)
                Vector3 p = p0 + (p1 - p0) * a + (p2 - p0) * b;
                positionsOut.Add(p);
            }
        }

        int Idx(int i, int j) => baseIndex + RowStart(i) + j;

        // Create small triangles.
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < (n - i); j++)
            {
                int v00 = Idx(i, j);
                int v10 = Idx(i + 1, j);
                int v01 = Idx(i, j + 1);

                // Triangle 1
                var f1 = new Face(new[] { v00, v10, v01 });
                f1.submeshIndex = submeshIndex;
                facesOut.Add(f1);

                // Triangle 2 (only if the "upper-right" vertex exists in this row)
                if (j + 1 <= (n - (i + 1)))
                {
                    int v11 = Idx(i + 1, j + 1);
                    var f2 = new Face(new[] { v10, v11, v01 });
                    f2.submeshIndex = submeshIndex;
                    facesOut.Add(f2);
                }
            }
        }
    }
}
#endif
