using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UTJ.MeshSync;

namespace UTJ.MeshSyncEditor
{
    [CustomEditor(typeof(MeshSyncPlayer))]
    public class MeshSyncPlayerEditor : Editor
    {
        protected float m_animationFrameRate = 30.0f;
        protected float m_animationTimeScale = 1.0f;


        public static void DrawPlayerSettings(MeshSyncPlayer t, SerializedObject so)
        {
            // Sync Settings
            EditorGUILayout.LabelField("Sync Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("m_syncVisibility"), new GUIContent("Visibility"));

            EditorGUILayout.PropertyField(so.FindProperty("m_syncTransform"), new GUIContent("Transform"));
            EditorGUILayout.PropertyField(so.FindProperty("m_syncCameras"), new GUIContent("Cameras"));
#if UNITY_2018_1_OR_NEWER
            if (t.syncCameras)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(so.FindProperty("m_usePhysicalCameraParams"), new GUIContent("Physical Camera Params"));
                EditorGUI.indentLevel--;
            }
#endif
            EditorGUILayout.PropertyField(so.FindProperty("m_syncLights"), new GUIContent("Lights"));

            EditorGUILayout.PropertyField(so.FindProperty("m_syncMeshes"), new GUIContent("Meshes"));
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(so.FindProperty("m_updateMeshColliders"));
            EditorGUI.indentLevel--;

            //EditorGUILayout.PropertyField(so.FindProperty("m_syncPoints"), new GUIContent("Points"));
            EditorGUILayout.PropertyField(so.FindProperty("m_syncMaterials"), new GUIContent("Materials"));
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(so.FindProperty("m_findMaterialFromAssets"), new GUIContent("Find From AssetDatabase"));
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            // Import Settings
            EditorGUILayout.LabelField("Import Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("m_animationInterpolation"));
            EditorGUILayout.PropertyField(so.FindProperty("m_zUpCorrection"), new GUIContent("Z-Up Correction"));
            EditorGUILayout.Space();

            // Misc
            EditorGUILayout.LabelField("Misc", EditorStyles.boldLabel);
            //EditorGUILayout.PropertyField(so.FindProperty("m_trackMaterialAssignment"));
            EditorGUILayout.PropertyField(so.FindProperty("m_progressiveDisplay"));
            EditorGUILayout.PropertyField(so.FindProperty("m_logging"));
            EditorGUILayout.PropertyField(so.FindProperty("m_profiling"));
            EditorGUILayout.Space();
        }

        public static void DrawMaterialList(MeshSyncPlayer t, bool allowFold = true)
        {
            Action drawInExportButton = () =>
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Import List", GUILayout.Width(110.0f)))
                {
                    var path = EditorUtility.OpenFilePanel("Select Cache File", "", "asset");
                    t.ImportMaterialList(path);
                }
                if (GUILayout.Button("Export List", GUILayout.Width(110.0f)))
                {
                    var path = EditorUtility.SaveFilePanel("Select Cache File", "", t.name + "_MaterialList", "asset");
                    t.ExportMaterialList(path);
                }
                GUILayout.EndHorizontal();
            };

            if (allowFold)
            {
                var style = EditorStyles.foldout;
                style.fontStyle = FontStyle.Bold;
                t.foldMaterialList = EditorGUILayout.Foldout(t.foldMaterialList, "Material List", true, style);
                if (t.foldMaterialList)
                    DrawMaterialListElements(t);
                drawInExportButton();
                if (GUILayout.Button("Open Material Window", GUILayout.Width(160.0f)))
                    MaterialWindow.Open(t);
            }
            else
            {
                GUILayout.Label("Material List", EditorStyles.boldLabel);
                DrawMaterialListElements(t);
                drawInExportButton();
            }

            EditorGUILayout.Space();
        }

        static void DrawMaterialListElements(MeshSyncPlayer t)
        {
            // calculate label width
            float labelWidth = 60; // minimum
            {
                var style = GUI.skin.box;
                foreach (var md in t.materialData)
                {
                    var size = style.CalcSize(new GUIContent(md.name));
                    labelWidth = Mathf.Max(labelWidth, size.x);
                }
                // 100: margin for color and material field
                labelWidth = Mathf.Min(labelWidth, EditorGUIUtility.currentViewWidth - 100);
            }

            foreach (var md in t.materialData)
            {
                var rect = EditorGUILayout.BeginHorizontal();
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 16, 16), md.color);
                EditorGUILayout.LabelField("", GUILayout.Width(16));
                EditorGUILayout.LabelField(md.name, GUILayout.Width(labelWidth));
                {
                    var tmp = EditorGUILayout.ObjectField(md.material, typeof(Material), true) as Material;
                    if (tmp != md.material)
                    {
                        Undo.RecordObject(t, "MeshSyncServer");
                        md.material = tmp;
                        t.ReassignMaterials();
                        t.ForceRepaint();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        public static void DrawTextureList(MeshSyncPlayer t)
        {

        }


        public void DrawAnimationSettings(MeshSyncPlayer t)
        {
            var style = EditorStyles.foldout;
            style.fontStyle = FontStyle.Bold;
            t.foldAnimationSettings = EditorGUILayout.Foldout(t.foldAnimationSettings, "Animation Settings", true, style);
            if (t.foldAnimationSettings)
            {
                // Override Frame Rate
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Override Frame Rate", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                m_animationFrameRate = EditorGUILayout.FloatField("Frame Rate", m_animationFrameRate);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Apply", GUILayout.Width(120.0f)))
                    ApplyFrameRate(t.GetAnimationClips(), m_animationFrameRate);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                GUILayout.EndVertical();

                // Time Scale
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Time Scale", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                m_animationTimeScale = EditorGUILayout.FloatField("Scale Value", m_animationTimeScale);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Apply", GUILayout.Width(120.0f)))
                    ApplyTimeScale(t.GetAnimationClips(), m_animationTimeScale);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                GUILayout.EndVertical();
            }

            EditorGUILayout.Space();
        }


        public void ApplyFrameRate(IEnumerable<AnimationClip> clips, float frameRate)
        {
            foreach (var clip in clips)
            {
                Undo.RecordObject(clip, "ApplyFrameRate");
                clip.frameRate = frameRate;

                Debug.Log("Applied frame rate to " + AssetDatabase.GetAssetPath(clip));
            }
            // repaint animation window
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        public void ApplyTimeScale(IEnumerable<AnimationClip> clips, float timeScale)
        {
            foreach (var clip in clips)
            {
                var curves = new List<AnimationCurve>();
                var bindings = new List<EditorCurveBinding>();
                var events = AnimationUtility.GetAnimationEvents(clip);

                // gather curves
                bindings.AddRange(AnimationUtility.GetCurveBindings(clip));
                bindings.AddRange(AnimationUtility.GetObjectReferenceCurveBindings(clip));
                foreach (var b in bindings)
                    curves.Add(AnimationUtility.GetEditorCurve(clip, b));

                int eventCount = events.Length;
                int curveCount = curves.Count;

                // transform keys/events
                foreach (var curve in curves)
                {
                    var keys = curve.keys;
                    var keyCount = keys.Length;
                    for (int ki = 0; ki < keyCount; ++ki)
                        keys[ki].time = keys[ki].time * timeScale;
                    curve.keys = keys;
                }
                for (int ei = 0; ei < eventCount; ++ei)
                    events[ei].time = events[ei].time * timeScale;

                // apply changes to clip
                Undo.RecordObject(clip, "ApplyTimeScale");
                clip.frameRate = clip.frameRate / timeScale;
                AnimationUtility.SetAnimationEvents(clip, events);
                for (int ci = 0; ci < curveCount; ++ci)
                    AnimationUtility.SetEditorCurve(clip, bindings[ci], curves[ci]);

                Debug.Log("Applied time scale to " + AssetDatabase.GetAssetPath(clip));
            }
            // repaint animation window
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
}
