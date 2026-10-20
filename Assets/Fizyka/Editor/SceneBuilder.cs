using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fizyka.EditorTools
{
    /// <summary>
    /// Generator wszystkich scen demonstracyjnych + polecenia w górnym menu "Fizyka".
    /// Sceny są tworzone kodem, więc zawsze można je odtworzyć (Fizyka > Generuj wszystkie sceny).
    /// </summary>
    public static partial class SceneBuilder
    {
        public const string Root = "Assets/Fizyka";
        public const string ScenesDir = Root + "/Scenes";
        const string MatDir = Root + "/Materials";
        const string PhysDir = Root + "/PhysicsMaterials";
        const string MeshDir = Root + "/Meshes";
        const string TexDir = Root + "/Textures";
        const string ActionsPath = "Assets/InputSystem_Actions.inputactions";
        public const string BuildPath = "Builds/Windows/Fizyka3D.exe";

        public struct SceneDef
        {
            public string file;
            public Action build;
            public SceneDef(string file, Action build) { this.file = file; this.build = build; }
        }

        /// <summary>Lista scen w kolejności numeracji.</summary>
        public static readonly SceneDef[] Scenes =
        {
            new SceneDef("01_Masa", S01_Masa),
            new SceneDef("02_LinearDamping", S02_LinearDamping),
            new SceneDef("03_AngularDamping", S03_AngularDamping),
            new SceneDef("04_Gravity_Kinematic_Static", S04_GravityKinematicStatic),
            new SceneDef("05_AddForce_ForceMode", S05_AddForce),
            new SceneDef("06_AddTorque_ForceMode", S06_AddTorque),
            new SceneDef("07_Collidery", S07_Collidery),
            new SceneDef("08_PhysicsMaterial", S08_PhysicsMaterial),
            new SceneDef("09_OnCollision", S09_OnCollision),
            new SceneDef("10_Triggery", S10_Triggery),
            new SceneDef("11_CollisionDetection", S11_CollisionDetection),
            new SceneDef("12_Interpolate", S12_Interpolate),
            new SceneDef("13_Constraints", S13_Constraints),
            new SceneDef("14_Sleeping", S14_Sleeping),
            new SceneDef("15_Velocity_vs_VelocityChange", S15_VelocityMethods),
            new SceneDef("16_CharacterController", S16_CharacterController),
        };

        public static string ScenePath(string file) => $"{ScenesDir}/{file}.unity";

        // =====================================================================
        //  MENU
        // =====================================================================

        [MenuItem("Fizyka/Generuj wszystkie sceny", priority = 0)]
        public static void GenerateAllMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            GenerateAll();
            EditorSceneManager.OpenScene(ScenePath(Scenes[0].file));
            EditorUtility.DisplayDialog("Fizyka",
                $"Wygenerowano {Scenes.Length} scen w {ScenesDir} i dodano je do Build Settings.\n\n" +
                "Naciśnij Play. Klawisze 1/2 przełączają sceny.", "OK");
        }

        [MenuItem("Fizyka/Panel scen (otwieranie i generowanie)", priority = 1)]
        public static void OpenBrowser() => SceneBrowserWindow.ShowWindow();

        [MenuItem("Fizyka/Otwórz pierwszą scenę i uruchom Play", priority = 2)]
        public static void OpenFirstAndPlay()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            if (!File.Exists(ScenePath(Scenes[0].file)))
                GenerateAll();
            EditorSceneManager.OpenScene(ScenePath(Scenes[0].file));
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Fizyka/Zbuduj wersję Windows (x64)", priority = 20)]
        public static void BuildWindowsMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            var report = BuildWindows();
            bool ok = report != null && report.summary.result == BuildResult.Succeeded;
            if (ok)
            {
                EditorUtility.DisplayDialog("Fizyka", $"Build zakończony powodzeniem:\n{Path.GetFullPath(BuildPath)}", "OK");
                EditorUtility.RevealInFinder(BuildPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Fizyka", "Build nie powiódł się – szczegóły w oknie Console.", "OK");
            }
        }

        [MenuItem("Fizyka/Napraw ustawienia projektu (Input System, Build Settings)", priority = 21)]
        public static void FixProjectSettingsMenu()
        {
            FixProjectSettings();
            UpdateBuildSettings();
            EditorUtility.DisplayDialog("Fizyka",
                "Ustawiono:\n• Active Input Handling = Input System Package (New)\n" +
                "• Project-wide Actions = Assets/InputSystem_Actions.inputactions\n• sceny 01–16 w Build Settings", "OK");
        }

        [MenuItem("Fizyka/Otwórz instrukcję (README.md)", priority = 40)]
        public static void OpenReadme()
        {
            string p = Path.GetFullPath("README.md");
            if (File.Exists(p))
                EditorUtility.OpenWithDefaultApp(p);
            else
                Debug.LogWarning("Nie znaleziono README.md w katalogu projektu.");
        }

        // =====================================================================
        //  BATCH MODE (wywołania z linii poleceń: -executeMethod ...)
        // =====================================================================

        /// <summary>Unity.exe -batchmode -projectPath ... -executeMethod Fizyka.EditorTools.SceneBuilder.BatchGenerate</summary>
        public static void BatchGenerate()
        {
            int code = 0;
            try
            {
                GenerateAll();
                Debug.Log("[Fizyka] BatchGenerate: OK");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                code = 1;
            }
            EditorApplication.Exit(code);
        }

        /// <summary>Generuje sceny i buduje wersję Windows x64 (sprawdzenie kompilacji).</summary>
        public static void BatchGenerateAndBuildWindows()
        {
            int code = 0;
            try
            {
                GenerateAll();
                var report = BuildWindows();
                if (report == null || report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError("[Fizyka] Build Windows: NIEPOWODZENIE " + (report != null ? report.summary.result.ToString() : "brak raportu"));
                    code = 2;
                }
                else
                {
                    Debug.Log($"[Fizyka] Build Windows: OK – {report.summary.totalSize / (1024 * 1024)} MB, {report.summary.totalTime}, ostrzeżeń: {report.summary.totalWarnings}, błędów: {report.summary.totalErrors}");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                code = 1;
            }
            EditorApplication.Exit(code);
        }

        // =====================================================================
        //  GENEROWANIE
        // =====================================================================

        public static void GenerateAll()
        {
            FixProjectSettings();
            EnsureFolders();
            for (int i = 0; i < Scenes.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Fizyka", "Generowanie " + Scenes[i].file, (float)i / Scenes.Length);
                try
                {
                    GenerateScene(Scenes[i]);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void GenerateScene(SceneDef def)
        {
            EnsureFolders();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            s_Hud = null;
            s_Camera = null;
            def.build();
            string path = ScenePath(def.file);
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log("[Fizyka] Zapisano scenę: " + path);
        }

        public static void UpdateBuildSettings()
        {
            var list = new List<EditorBuildSettingsScene>();
            foreach (var d in Scenes)
            {
                string p = ScenePath(d.file);
                if (File.Exists(p))
                    list.Add(new EditorBuildSettingsScene(p, true));
            }
            EditorBuildSettings.scenes = list.ToArray();
        }

        public static BuildReport BuildWindows()
        {
            UpdateBuildSettings();
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                GenerateAll();
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            }
            Directory.CreateDirectory(Path.GetDirectoryName(BuildPath));
            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = BuildPath,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None,
            };
            return BuildPipeline.BuildPlayer(opts);
        }

        public static void FixProjectSettings()
        {
            // 1) Active Input Handling = Input System Package (New)
            var psAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset").FirstOrDefault();
            if (psAsset != null)
            {
                var so = new SerializedObject(psAsset);
                var prop = so.FindProperty("activeInputHandler");
                if (prop != null && prop.intValue == 0)
                {
                    prop.intValue = 1;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    Debug.LogWarning("[Fizyka] Ustawiono Active Input Handling = Input System Package (New). Uruchom ponownie edytor.");
                }
            }

            // 2) Project-wide actions = domyślny plik InputSystem_Actions
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
            if (actions != null && InputSystem.actions != actions)
            {
                InputSystem.actions = actions;
                Debug.Log("[Fizyka] Ustawiono Project-wide Actions: " + ActionsPath);
            }
            else if (actions == null)
            {
                Debug.LogWarning("[Fizyka] Nie znaleziono " + ActionsPath + " – sterowanie nie będzie działać.");
            }
        }

        static void EnsureFolders()
        {
            foreach (var f in new[] { Root, ScenesDir, MatDir, PhysDir, MeshDir, TexDir })
            {
                if (AssetDatabase.IsValidFolder(f))
                    continue;
                string parent = Path.GetDirectoryName(f).Replace('\\', '/');
                AssetDatabase.CreateFolder(parent, Path.GetFileName(f));
            }
        }

        // =====================================================================
        //  POMOCNICZE – materiały, tekstury, fizyczne materiały, siatki
        // =====================================================================

        static Shader LitShader
        {
            get
            {
                var s = Shader.Find("Universal Render Pipeline/Lit");
                if (s == null) s = Shader.Find("Standard");
                return s;
            }
        }

        static Material Mat(string name, Color c)
        {
            string path = $"{MatDir}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(LitShader);
                AssetDatabase.CreateAsset(m, path);
            }
            m.color = c;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.35f);
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material TransparentMat(string name, Color c)
        {
            var m = Mat(name, c);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
            m.SetShaderPassEnabled("ShadowCaster", false);
            m.SetShaderPassEnabled("DepthOnly", false);
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material s_FloorMat;

        static Material FloorMat()
        {
            if (s_FloorMat != null)
                return s_FloorMat;
            string texPath = $"{TexDir}/Szachownica.png";
            if (!File.Exists(texPath))
            {
                const int size = 256;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
                Color a = new Color(0.58f, 0.6f, 0.63f);
                Color b = new Color(0.46f, 0.48f, 0.51f);
                Color line = new Color(0.36f, 0.38f, 0.41f);
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    bool cell = ((x / (size / 2)) + (y / (size / 2))) % 2 == 0;
                    bool border = x % (size / 2) < 2 || y % (size / 2) < 2;
                    tex.SetPixel(x, y, border ? line : (cell ? a : b));
                }
                tex.Apply();
                File.WriteAllBytes(texPath, tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(texPath);
                var imp = (TextureImporter)AssetImporter.GetAtPath(texPath);
                imp.wrapMode = TextureWrapMode.Repeat;
                imp.anisoLevel = 8;
                imp.filterMode = FilterMode.Trilinear;
                imp.SaveAndReimport();
            }
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            var m = Mat("Podloga", Color.white);
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", t);
            m.mainTexture = t;
            m.mainTextureScale = new Vector2(40, 40); // podłoga 80 m → kratka 1 m
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.15f);
            EditorUtility.SetDirty(m);
            s_FloorMat = m;
            return m;
        }

        static PhysicsMaterial PhysMat(string name, float dynamicFriction, float staticFriction, float bounciness,
            PhysicsMaterialCombine frictionCombine = PhysicsMaterialCombine.Average,
            PhysicsMaterialCombine bounceCombine = PhysicsMaterialCombine.Average)
        {
            string path = $"{PhysDir}/{name}.asset";
            var pm = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
            if (pm == null)
            {
                pm = new PhysicsMaterial(name);
                AssetDatabase.CreateAsset(pm, path);
            }
            pm.dynamicFriction = dynamicFriction;
            pm.staticFriction = staticFriction;
            pm.bounciness = bounciness;
            pm.frictionCombine = frictionCombine;
            pm.bounceCombine = bounceCombine;
            EditorUtility.SetDirty(pm);
            return pm;
        }

        // często używane materiały fizyczne
        static PhysicsMaterial PM_Lod => PhysMat("Lod", 0f, 0f, 0f, PhysicsMaterialCombine.Minimum, PhysicsMaterialCombine.Average);
        static PhysicsMaterial PM_Sprezysty => PhysMat("Sprezysty_bezTarcia", 0f, 0f, 1f, PhysicsMaterialCombine.Minimum, PhysicsMaterialCombine.Maximum);
        static PhysicsMaterial PM_Podloga => PhysMat("Podloga", 0.6f, 0.6f, 0f, PhysicsMaterialCombine.Average, PhysicsMaterialCombine.Average);
        static PhysicsMaterial PM_Slisko => PhysMat("Slisko", 0.05f, 0.05f, 0f, PhysicsMaterialCombine.Minimum, PhysicsMaterialCombine.Average);

        static Mesh BuiltinMesh(string file) => Resources.GetBuiltinResource<Mesh>(file);

        /// <summary>Kubek (pudełko bez wieczka) – wklęsła siatka do porównania Mesh Collider convex / non-convex.</summary>
        static Mesh CupMesh()
        {
            string path = $"{MeshDir}/Kubek.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
                return existing;

            Mesh cube = BuiltinMesh("Cube.fbx");
            const float w = 2.4f, h = 1.2f, t = 0.15f;
            var parts = new List<CombineInstance>();
            void Add(Vector3 pos, Vector3 size) =>
                parts.Add(new CombineInstance { mesh = cube, transform = Matrix4x4.TRS(pos, Quaternion.identity, size) });
            Add(new Vector3(0, t / 2, 0), new Vector3(w, t, w));                      // dno
            Add(new Vector3(-(w - t) / 2, h / 2, 0), new Vector3(t, h, w));           // ściany
            Add(new Vector3((w - t) / 2, h / 2, 0), new Vector3(t, h, w));
            Add(new Vector3(0, h / 2, -(w - t) / 2), new Vector3(w - 2 * t, h, t));
            Add(new Vector3(0, h / 2, (w - t) / 2), new Vector3(w - 2 * t, h, t));
            var mesh = new Mesh { name = "Kubek" };
            mesh.CombineMeshes(parts.ToArray(), true, true);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        // =====================================================================
        //  POMOCNICZE – obiekty sceny
        // =====================================================================

        static DemoHud s_Hud;
        static Camera s_Camera;

        static readonly Color ColTitle = new Color(1f, 0.85f, 0.4f);

        static Material M_Red => Mat("Czerwony", new Color(0.86f, 0.27f, 0.23f));
        static Material M_Orange => Mat("Pomaranczowy", new Color(0.95f, 0.55f, 0.15f));
        static Material M_Yellow => Mat("Zolty", new Color(0.95f, 0.82f, 0.25f));
        static Material M_Green => Mat("Zielony", new Color(0.3f, 0.75f, 0.35f));
        static Material M_Teal => Mat("Turkusowy", new Color(0.2f, 0.7f, 0.75f));
        static Material M_Blue => Mat("Niebieski", new Color(0.25f, 0.45f, 0.9f));
        static Material M_Purple => Mat("Fioletowy", new Color(0.6f, 0.35f, 0.85f));
        static Material M_Gray => Mat("Szary", new Color(0.55f, 0.57f, 0.6f));
        static Material M_Dark => Mat("Ciemny", new Color(0.22f, 0.23f, 0.26f));
        static Material M_White => Mat("Bialy", new Color(0.92f, 0.92f, 0.92f));
        static Material M_Wood => Mat("Drewno", new Color(0.6f, 0.42f, 0.25f));
        static Material M_Ice => Mat("Lod", new Color(0.7f, 0.9f, 1f));
        static Material M_Rubber => Mat("Guma", new Color(0.15f, 0.15f, 0.17f));
        static Material M_Ramp => Mat("Rampa", new Color(0.45f, 0.52f, 0.62f));
        static Material M_Trapdoor => Mat("Zapadnia", new Color(0.95f, 0.75f, 0.1f));

        static Material[] Palette => new[] { M_Red, M_Orange, M_Yellow, M_Green, M_Teal, M_Blue, M_Purple, M_White };

        /// <summary>Kolor zależny od "ciężkości" (0 = lekki/jasny, 1 = ciężki/ciemny).</summary>
        static Material MassMat(int level)
        {
            Color[] c =
            {
                new Color(0.65f, 0.85f, 1f), new Color(0.3f, 0.55f, 0.95f), new Color(0.12f, 0.2f, 0.55f)
            };
            level = Mathf.Clamp(level, 0, c.Length - 1);
            return Mat("Masa_" + level, c[level]);
        }

        static void Begin(string title, string description, string hint, bool showLog = false, bool freeCamera = true)
        {
            // Oświetlenie otoczenia (bez wypiekania)
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.68f, 0.78f);
            RenderSettings.ambientEquatorColor = new Color(0.45f, 0.46f, 0.48f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.21f, 0.2f);
            RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            light.color = new Color(1f, 0.97f, 0.92f);
            lightGo.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            s_Camera = camGo.AddComponent<Camera>();
            s_Camera.farClipPlane = 500f;
            s_Camera.fieldOfView = 60f;
            camGo.AddComponent<AudioListener>();
            if (freeCamera)
                camGo.AddComponent<FreeCamera>();
            camGo.AddComponent<PokeTool>();

            s_Hud = CreateHud(title, description, hint, showLog, freeCamera);
        }

        static void Cam(Vector3 pos, Vector3 lookAt)
        {
            s_Camera.transform.position = pos;
            s_Camera.transform.LookAt(lookAt);
        }

        static DemoHud CreateHud(string title, string description, string hint, bool showLog, bool freeCamera)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvasGo = new GameObject("UI_Opis", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // etykiety (pod panelami)
            var labels = NewRect("Etykiety", canvasGo.transform);
            labels.anchorMin = Vector2.zero;
            labels.anchorMax = Vector2.zero;
            labels.pivot = Vector2.zero;
            labels.anchoredPosition = Vector2.zero;
            labels.sizeDelta = Vector2.zero;

            // panel opisu – lewy górny róg
            var panel = NewPanel("PanelOpisu", canvasGo.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(12, -12), 540f,
                new Color(0.04f, 0.06f, 0.09f, 0.82f));
            var titleText = NewText("Tytul", panel, font, 24, FontStyle.Bold, ColTitle, TextAnchor.UpperLeft);
            titleText.text = title;
            var descText = NewText("Opis", panel, font, 16, FontStyle.Normal, new Color(0.93f, 0.94f, 0.96f), TextAnchor.UpperLeft);
            descText.text = description;
            descText.lineSpacing = 1.05f;

            // pasek podpowiedzi – dół ekranu
            var hintPanel = NewRect("PasekPodpowiedzi", canvasGo.transform);
            hintPanel.anchorMin = new Vector2(0, 0);
            hintPanel.anchorMax = new Vector2(1, 0);
            hintPanel.pivot = new Vector2(0.5f, 0);
            hintPanel.anchoredPosition = Vector2.zero;
            hintPanel.sizeDelta = new Vector2(0, 60);
            hintPanel.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.06f, 0.09f, 0.78f);
            var hlg = hintPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 8, 8);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            var hfit = hintPanel.gameObject.AddComponent<ContentSizeFitter>();
            hfit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var hintText = NewText("Podpowiedz", hintPanel, font, 16, FontStyle.Normal, new Color(0.9f, 0.92f, 0.95f), TextAnchor.MiddleCenter);
            hintText.text = hint;

            // dziennik zdarzeń – prawy górny róg
            var logPanel = NewPanel("PanelDziennika", canvasGo.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-12, -12), 520f,
                new Color(0.04f, 0.06f, 0.09f, 0.78f));
            var logText = NewText("Dziennik", logPanel, font, 15, FontStyle.Normal, new Color(0.93f, 0.94f, 0.96f), TextAnchor.UpperLeft);
            logText.text = "<b>Dziennik zdarzeń</b>";
            logPanel.gameObject.SetActive(showLog);

            var mgr = new GameObject("DemoManager");
            var hud = mgr.AddComponent<DemoHud>();
            hud.title = title;
            hud.description = description;
            hud.sceneHint = hint;
            hud.freeCameraControls = freeCamera;
            hud.showLog = showLog;
            hud.descriptionPanel = panel.gameObject;
            hud.titleText = titleText;
            hud.descriptionText = descText;
            hud.hintText = hintText;
            hud.logPanel = logPanel.gameObject;
            hud.logText = logText;
            hud.labelsRoot = labels;
            hud.font = font;
            return hud;
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static RectTransform NewPanel(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, float width, Color bg)
        {
            var rt = NewRect(name, parent);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, 100);
            rt.gameObject.AddComponent<Image>().color = bg;
            var vlg = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(14, 14, 10, 12);
            vlg.spacing = 6;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            var fit = rt.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rt;
        }

        static Text NewText(string name, Transform parent, Font font, int size, FontStyle style, Color color, TextAnchor align)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = align;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        static Transform Group(string name)
        {
            return new GameObject(name).transform;
        }

        static GameObject Prim(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat, Transform parent = null, Quaternion? rot = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (parent != null) go.transform.SetParent(parent, true);
            go.transform.position = pos;
            go.transform.rotation = rot ?? Quaternion.identity;
            go.transform.localScale = scale;
            if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        static GameObject Box(string name, Vector3 pos, Vector3 size, Material mat, Transform parent = null, Quaternion? rot = null)
            => Prim(PrimitiveType.Cube, name, pos, size, mat, parent, rot);

        static GameObject Ball(string name, Vector3 pos, float diameter, Material mat, Transform parent = null)
            => Prim(PrimitiveType.Sphere, name, pos, Vector3.one * diameter, mat, parent);

        static Rigidbody Body(GameObject go, float mass, bool gravity = true, float linearDamping = 0f, float angularDamping = 0.05f)
        {
            var rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.useGravity = gravity;
            rb.linearDamping = linearDamping;
            rb.angularDamping = angularDamping;
            return rb;
        }

        static void SetPhys(GameObject go, PhysicsMaterial pm)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>())
                c.sharedMaterial = pm;
        }

        static WorldLabel Label(GameObject go, string header, WorldLabel.Info info = WorldLabel.Info.None, float offsetY = 1f, int fontSize = 15)
        {
            var l = go.AddComponent<WorldLabel>();
            l.header = header;
            l.info = info;
            l.worldOffset = new Vector3(0, offsetY, 0);
            l.fontSize = fontSize;
            return l;
        }

        static GameObject SectionLabel(string text, Vector3 pos, Transform parent = null)
        {
            var go = new GameObject("Napis_" + text.Split('\n')[0]);
            if (parent != null) go.transform.SetParent(parent, true);
            go.transform.position = pos;
            var l = Label(go, text, WorldLabel.Info.None, 0f, 20);
            l.color = ColTitle;
            return go;
        }

        static GameObject Floor()
        {
            var f = Box("Podloga", new Vector3(0, -0.5f, 0), new Vector3(80, 1, 80), FloorMat());
            // Jawny materiał podłogi: bez niego (domyślny materiał) tryby Combine innych materiałów nie działają zgodnie z priorytetami.
            SetPhys(f, PM_Podloga);
            return f;
        }

        static GameObject Trapdoor(string name, Vector3 center, Vector3 size, Transform parent = null)
        {
            var t = Box(name, center, size, M_Trapdoor, parent);
            t.AddComponent<DisableOnAction>();
            return t;
        }

        static ApplyOnAction Apply(GameObject go, ApplyOnAction.Kind kind, ForceMode mode, Vector3 v, float duration = 1f, bool log = false)
        {
            var a = go.AddComponent<ApplyOnAction>();
            a.kind = kind;
            a.mode = mode;
            a.vector = v;
            a.duration = duration;
            a.logToHud = log;
            return a;
        }

        static DemoLauncher Launcher(string name, Vector3 pos, Vector3 direction, float speed, float diameter, float mass, Material projectileMat, Transform parent = null, bool visible = true)
        {
            GameObject go;
            if (visible)
            {
                go = Prim(PrimitiveType.Cylinder, name, pos, new Vector3(0.35f, 0.4f, 0.35f), M_Dark, parent,
                    Quaternion.LookRotation(direction.sqrMagnitude > 0 ? direction : Vector3.forward) * Quaternion.Euler(90, 0, 0));
                UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
                // korpus lufy wskazuje kierunek; komponent wyrzutni na dziecku z poprawną orientacją
                var muzzle = new GameObject(name + "_Wylot");
                muzzle.transform.SetParent(go.transform, false);
                muzzle.transform.position = pos;
                muzzle.transform.rotation = Quaternion.LookRotation(direction.sqrMagnitude > 0 ? direction : Vector3.forward);
                go = muzzle;
            }
            else
            {
                go = new GameObject(name);
                if (parent != null) go.transform.SetParent(parent, true);
                go.transform.position = pos;
                go.transform.rotation = Quaternion.LookRotation(direction.sqrMagnitude > 0 ? direction : Vector3.forward);
            }
            var l = go.AddComponent<DemoLauncher>();
            l.speed = speed;
            l.radius = diameter * 0.5f;
            l.mass = mass;
            l.material = projectileMat;
            return l;
        }

        /// <summary>
        /// Rampa: korzeń stoi na dolnej krawędzi (na podłodze), lokalna oś +Z biegnie pod górę,
        /// lokalne y = 0 to powierzchnia rampy. Zwraca korzeń.
        /// </summary>
        static Transform Ramp(string name, Vector3 lowEdgeCenter, float width, float length, float angleDeg, Material mat, PhysicsMaterial pm = null, Transform parent = null, float thickness = 0.5f)
        {
            var root = new GameObject(name).transform;
            if (parent != null) root.SetParent(parent, true);
            root.position = lowEdgeCenter;
            root.rotation = Quaternion.Euler(-angleDeg, 0, 0);
            var surf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surf.name = name + "_Powierzchnia";
            surf.transform.SetParent(root, false);
            surf.transform.localPosition = new Vector3(0, -thickness / 2f, length / 2f);
            surf.transform.localScale = new Vector3(width, thickness, length);
            surf.GetComponent<Renderer>().sharedMaterial = mat;
            if (pm != null) surf.GetComponent<Collider>().sharedMaterial = pm;
            return root;
        }
    }
}
