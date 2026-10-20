using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Fizyka.EditorTools
{
    /// <summary>
    /// Okno "Fizyka > Panel scen": lista scen z przyciskami Otwórz / Play / Generuj ponownie.
    /// </summary>
    public class SceneBrowserWindow : EditorWindow
    {
        Vector2 m_Scroll;

        public static void ShowWindow()
        {
            var w = GetWindow<SceneBrowserWindow>("Fizyka – sceny");
            w.minSize = new Vector2(420, 360);
        }

        void OnGUI()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Sceny demonstracyjne fizyki 3D", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Otwórz – otwiera scenę w edytorze.  ▶ – otwiera i uruchamia Play.\n" +
                                    "↻ – generuje scenę od nowa (ręczne zmiany w tej scenie zostaną nadpisane).", MessageType.None);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Generuj wszystkie sceny", GUILayout.Height(26)))
                    SceneBuilder.GenerateAllMenu();
                if (GUILayout.Button("Zbuduj Windows x64", GUILayout.Height(26)))
                    SceneBuilder.BuildWindowsMenu();
            }

            EditorGUILayout.Space(6);
            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            foreach (var def in SceneBuilder.Scenes)
            {
                string path = SceneBuilder.ScenePath(def.file);
                bool exists = File.Exists(path);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUI.enabled = true;
                    EditorGUILayout.LabelField(def.file, exists ? EditorStyles.label : EditorStyles.miniLabel, GUILayout.MinWidth(200));
                    GUI.enabled = exists && !EditorApplication.isPlaying;
                    if (GUILayout.Button("Otwórz", GUILayout.Width(70)) && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorSceneManager.OpenScene(path);
                    if (GUILayout.Button("▶", GUILayout.Width(30)) && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(path);
                        EditorApplication.isPlaying = true;
                    }
                    GUI.enabled = !EditorApplication.isPlaying;
                    if (GUILayout.Button("↻", GUILayout.Width(30)) && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        SceneBuilder.GenerateScene(def);
                        SceneBuilder.UpdateBuildSettings();
                        AssetDatabase.SaveAssets();
                        EditorSceneManager.OpenScene(path);
                    }
                    GUI.enabled = true;
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
