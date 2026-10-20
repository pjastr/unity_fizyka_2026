using UnityEngine;

namespace Fizyka.EditorTools
{
    public static partial class SceneBuilder
    {
        // =====================================================================
        // 01 – MASA
        // =====================================================================
        static void S01_Masa()
        {
            Begin("01 · Masa (Rigidbody.mass)",
                "Masa [kg] określa <b>bezwładność</b> ciała – jak trudno zmienić jego prędkość.\n\n" +
                "<b>A. Spadek swobodny</b> – kule 1, 10 i 100 kg spadają razem. Grawitacja nadaje wszystkim to samo " +
                "przyspieszenie g = 9,81 m/s² (przy Linear Damping = 0 masa nie wpływa na spadanie – jak u Galileusza).\n\n" +
                "<b>B. Ten sam impuls</b> – każdy klocek dostaje impuls J = 10 N·s (AddForce, ForceMode.Impulse). " +
                "Zmiana prędkości Δv = J / m: 10 m/s, 2 m/s i 0,4 m/s. Klocki są z „lodu” (tarcie 0).\n\n" +
                "<b>C. Zderzenia sprężyste</b> (bounciness = 1, bez tarcia i grawitacji): 1 kg → 1 kg – kula oddaje całą prędkość; " +
                "1 kg → 10 kg – lekka kula się odbija; 10 kg → 1 kg – lekka odlatuje prawie 2× szybciej.\n\n" +
                "LPM – szturchnięcie tym samym impulsem: lekkie ciała odlatują, ciężkie ledwo drgną. " +
                "Masę zmienisz w Inspectorze (Rigidbody → Mass). Zalecany stosunek mas zderzających się ciał < 100.",
                "<b>Spacja</b> – start (zapadnia, impulsy, zderzenia)");
            Floor();
            Cam(new Vector3(9, 11, -22), new Vector3(9, 3, 6));

            // A – spadek
            var gA = Group("A_Spadek");
            SectionLabel("A. Spadek swobodny", new Vector3(21, 10.2f, 0), gA);
            Trapdoor("Zapadnia_A", new Vector3(21, 7.4f, 0), new Vector3(8, 0.2f, 2), gA);
            float[] mA = { 1, 10, 100 };
            for (int i = 0; i < 3; i++)
            {
                var s = Ball($"Kula_{mA[i]}kg", new Vector3(18.5f + 2.5f * i, 8f, 0), 1f, MassMat(i), gA);
                Body(s, mA[i]);
                Label(s, $"m = {mA[i]} kg", WorldLabel.Info.Speed | WorldLabel.Info.Height, 0.9f);
            }

            // B – ten sam impuls
            var gB = Group("B_TenSamImpuls");
            SectionLabel("B. Ten sam impuls 10 N·s", new Vector3(0, 3.2f, -2), gB);
            float[] mB = { 1, 5, 25 };
            for (int i = 0; i < 3; i++)
            {
                var c = Box($"Klocek_{mB[i]}kg", new Vector3(-3 + 3 * i, 0.5f, -2), Vector3.one, MassMat(i), gB);
                Body(c, mB[i]);
                SetPhys(c, PM_Lod);
                Apply(c, ApplyOnAction.Kind.Force, ForceMode.Impulse, new Vector3(0, 0, 10));
                Label(c, $"m = {mB[i]} kg", WorldLabel.Info.Speed, 1f);
            }
            Box("Sciana_B", new Vector3(0, 1, 26), new Vector3(10, 2, 1), M_Gray, gB);

            // C – zderzenia
            var gC = Group("C_Zderzenia");
            SectionLabel("C. Zderzenia sprężyste", new Vector3(11, 3.2f, 0), gC);
            (float a, float b)[] pairs = { (1, 1), (1, 10), (10, 1) };
            for (int i = 0; i < 3; i++)
            {
                float x = 8 + 3 * i;
                var pa = Ball($"Kula_A_{pairs[i].a}kg", new Vector3(x, 1, -3), 1f, MassMat(pairs[i].a > 1 ? 2 : 0), gC);
                var rbA = Body(pa, pairs[i].a, false);
                rbA.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                SetPhys(pa, PM_Sprezysty);
                Apply(pa, ApplyOnAction.Kind.Force, ForceMode.VelocityChange, new Vector3(0, 0, 4));
                Label(pa, $"{pairs[i].a} kg", WorldLabel.Info.VelocityZ, 0.9f);

                var pb = Ball($"Kula_B_{pairs[i].b}kg", new Vector3(x, 1, 1), 1f, MassMat(pairs[i].b > 1 ? 2 : 0), gC);
                var rbB = Body(pb, pairs[i].b, false);
                rbB.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                SetPhys(pb, PM_Sprezysty);
                Label(pb, $"{pairs[i].b} kg", WorldLabel.Info.VelocityZ, 0.9f);
            }
        }

        // =====================================================================
        // 02 – LINEAR DAMPING
        // =====================================================================
        static void S02_LinearDamping()
        {
            Begin("02 · Linear Damping (opór ruchu)",
                "<b>Linear Damping</b> (w starszych wersjach „Drag”) to opór ruchu postępowego. W każdym kroku fizyki " +
                "prędkość jest zmniejszana mniej więcej o czynnik 1 / (1 + damping · Δt) – im większa wartość, tym szybciej " +
                "ciało wytraca prędkość.\n\n" +
                "<b>A. Lot poziomy bez grawitacji</b> – kule dostają tę samą prędkość 10 m/s (ForceMode.VelocityChange). " +
                "Damping = 0 leci bez końca (I zasada dynamiki), 0,5 / 2 / 5 hamują coraz szybciej.\n\n" +
                "<b>B. Spadek z 20 m</b> – z oporem ciało osiąga <b>prędkość graniczną</b> (ok. g / damping): przy damping = 5 " +
                "spada jak na spadochronie (ok. 2 m/s).\n\n" +
                "Uwaga: damping w Unity NIE zależy od kształtu ani masy – to uproszczenie, a nie prawdziwy opór powietrza.\n" +
                "W Unity 6 w kodzie: rb.linearDamping (dawniej rb.drag).",
                "<b>Spacja</b> – start (wystrzał kul A i zapadnia B)");
            Floor();
            Cam(new Vector3(10, 14, -26), new Vector3(10, 7, 8));

            float[] d = { 0f, 0.5f, 2f, 5f };
            var gA = Group("A_LotPoziomy");
            SectionLabel("A. Lot poziomy (Use Gravity = off)", new Vector3(3.5f, 3.5f, -4), gA);
            for (int i = 0; i < d.Length; i++)
            {
                var s = Ball($"Kula_damping_{d[i]}", new Vector3(-1 + 3 * i, 1.5f, -4), 1f, Palette[i], gA);
                Body(s, 1f, false, d[i]);
                Apply(s, ApplyOnAction.Kind.Force, ForceMode.VelocityChange, new Vector3(0, 0, 10));
                Label(s, $"damping = {d[i]}", WorldLabel.Info.Speed, 0.9f);
            }
            Box("Sciana_A", new Vector3(3.5f, 2, 60), new Vector3(14, 4, 1), M_Gray, gA);

            var gB = Group("B_Spadek");
            SectionLabel("B. Spadek z wysokości 20 m", new Vector3(17.5f, 22.5f, 0), gB);
            Trapdoor("Zapadnia_B", new Vector3(17.5f, 19.4f, 0), new Vector3(12, 0.2f, 2), gB);
            for (int i = 0; i < d.Length; i++)
            {
                var s = Ball($"Spadajaca_damping_{d[i]}", new Vector3(13 + 3 * i, 20f, 0), 1f, Palette[i], gB);
                Body(s, 1f, true, d[i]);
                Label(s, $"damping = {d[i]}", WorldLabel.Info.Speed | WorldLabel.Info.Height, 0.9f);
            }
        }

        // =====================================================================
        // 03 – ANGULAR DAMPING
        // =====================================================================
        static void S03_AngularDamping()
        {
            Begin("03 · Angular Damping (opór obrotu)",
                "<b>Angular Damping</b> (dawniej „Angular Drag”) – opór ruchu obrotowego; w każdym kroku zmniejsza prędkość " +
                "kątową ω. Domyślnie 0,05.\n\n" +
                "<b>A. Wirujące klocki</b> (Constraints → Freeze Position, więc tylko się obracają) dostają ten sam obrót " +
                "ω = 15 rad/s (AddTorque, ForceMode.VelocityChange). Damping: 0; 0,05; 0,5; 2.\n" +
                "Ostatni klocek ma damping 0, ale domyślne <b>maxAngularVelocity = 7 rad/s</b> – Unity obcina prędkość kątową! " +
                "(pozostałe mają 50 rad/s, ustawione komponentem RigidbodySetup).\n\n" +
                "<b>B. Toczące się kule</b> – ta sama prędkość początkowa 6 m/s. Tarcie o podłoże sprzęga obrót z ruchem, " +
                "więc większy angular damping działa jak „opór toczenia” – kula szybciej się zatrzymuje.\n\n" +
                "W kodzie: rb.angularDamping (dawniej rb.angularDrag), rb.maxAngularVelocity.",
                "<b>Spacja</b> – zakręć klockami i popchnij kule");
            Floor();
            Cam(new Vector3(0, 7.5f, -17), new Vector3(0, 2.5f, 6));

            var gA = Group("A_Wirujace");
            SectionLabel("A. Ten sam obrót 15 rad/s", new Vector3(0, 6.6f, 6), gA);
            float[] ad = { 0f, 0.05f, 0.5f, 2f, 0f };
            float[] maxW = { 50, 50, 50, 50, 7 };
            for (int i = 0; i < ad.Length; i++)
            {
                var c = Box($"Klocek_angDamping_{ad[i]}" + (i == 4 ? "_max7" : ""), new Vector3(-8 + 4 * i, 4, 6), Vector3.one * 1.2f, Palette[i], gA);
                var rb = Body(c, 1f, false, 0f, ad[i]);
                rb.constraints = RigidbodyConstraints.FreezePosition;
                c.AddComponent<RigidbodySetup>().maxAngularVelocity = maxW[i];
                Apply(c, ApplyOnAction.Kind.Torque, ForceMode.VelocityChange, new Vector3(0, 15, 0));
                Label(c, $"damping = {ad[i]}\nmax ω = {maxW[i]}", WorldLabel.Info.AngularSpeed, 1.2f);
            }

            var gB = Group("B_Toczenie");
            SectionLabel("B. Toczenie – ta sama prędkość 6 m/s", new Vector3(0, 2.6f, -4), gB);
            float[] bd = { 0f, 0.5f, 2f, 10f };
            for (int i = 0; i < bd.Length; i++)
            {
                var s = Ball($"Kula_angDamping_{bd[i]}", new Vector3(-6 + 4 * i, 0.5f, -4), 1f, Palette[i], gB);
                Body(s, 1f, true, 0f, bd[i]);
                Apply(s, ApplyOnAction.Kind.Force, ForceMode.VelocityChange, new Vector3(0, 0, 6));
                Label(s, $"damping = {bd[i]}", WorldLabel.Info.Speed, 0.9f);
            }
            Box("Sciana_B", new Vector3(0, 1, 38), new Vector3(18, 2, 1), M_Gray, gB);
        }

        // =====================================================================
        // 04 – USE GRAVITY / IS KINEMATIC / STATIC
        // =====================================================================
        static void S04_GravityKinematicStatic()
        {
            Begin("04 · Use Gravity / Is Kinematic / Static",
                "Obiekt fizyczny może być:\n• <b>dynamiczny</b> – Rigidbody, pełna symulacja (siły, grawitacja, zderzenia),\n" +
                "• <b>kinematyczny</b> – Rigidbody z <b>Is Kinematic</b>: ruszany tylko skryptem (MovePosition/MoveRotation), " +
                "ignoruje siły i zderzenia, ale PCHA ciała dynamiczne,\n• <b>statyczny</b> – sam Collider bez Rigidbody (podłoga, ściany).\n" +
                "To odpowiednik typów Dynamic / Kinematic / Static z Rigidbody2D.\n\n" +
                "Spacja – wyrzutnie strzelają w 4 cele (od lewej):\n" +
                "1. Dynamic + Use Gravity – spada z podstawy,\n2. Dynamic, Use Gravity = off – odlatuje i dryfuje w powietrzu,\n" +
                "3. Is Kinematic – ani drgnie, kulka się odbija,\n4. Static (brak Rigidbody) – też nieruchomy.\n\n" +
                "Z tyłu: kinematyczne ramię (MoveRotation) zmiata dynamiczne klocki, a przez statyczny słupek po prostu przenika.\n" +
                "Zasada: ruchome obiekty przesuwaj przez Rigidbody (kinematic + MovePosition), a nie przez transform statycznego collidera.",
                "<b>Spacja</b> – strzał w cele");
            Floor();
            Cam(new Vector3(0, 9, -14), new Vector3(0, 2, 7));

            var gT = Group("Cele");
            string[] names = { "1. Dynamic\nUse Gravity = on", "2. Dynamic\nUse Gravity = off", "3. Is Kinematic = on", "4. Static\n(bez Rigidbody)" };
            Material[] mats = { M_Green, M_Teal, M_Orange, M_Gray };
            for (int i = 0; i < 4; i++)
            {
                float x = -6 + 4 * i;
                if (i == 0)
                    Box("Podstawa", new Vector3(x, 1, 4), new Vector3(1, 2, 1), M_Dark, gT);
                var t = Box($"Cel_{i + 1}", new Vector3(x, 2.5f, 4), Vector3.one, mats[i], gT);
                if (i < 3)
                {
                    var rb = Body(t, 1f, i == 0);
                    rb.isKinematic = i == 2;
                    Label(t, names[i], WorldLabel.Info.Speed, 1.1f);
                }
                else
                {
                    Label(t, names[i], WorldLabel.Info.None, 1.1f);
                }
                var l = Launcher($"Wyrzutnia_{i + 1}", new Vector3(x, 2.7f, -4), Vector3.forward, 25f, 0.4f, 0.3f, M_Red, gT);
                l.lifeTime = 12f;
            }

            var gS = Group("Kinematyczne_ramie");
            SectionLabel("Kinematic + MoveRotation", new Vector3(0, 3, 15), gS);
            var arm = Box("Ramie_kinematyczne", new Vector3(0, 0.55f, 15), new Vector3(10, 0.6f, 0.6f), M_Orange, gS);
            var arb = Body(arm, 10f, false);
            arb.isKinematic = true;
            var mover = arm.AddComponent<KinematicMover>();
            mover.motion = KinematicMover.Motion.Rotate;
            mover.degreesPerSecond = 45f;
            var pillar = Box("Slupek_statyczny", new Vector3(3.5f, 1, 15), new Vector3(0.8f, 2, 0.8f), M_Gray, gS);
            Label(pillar, "Static (bez Rigidbody)", WorldLabel.Info.None, 1.4f);
            for (int i = 0; i < 8; i++)
            {
                float ang = i * 45f * Mathf.Deg2Rad + 0.3f;
                float r = 2.2f + (i % 3) * 1f;
                var b = Box($"Klocek_dyn_{i + 1}", new Vector3(Mathf.Cos(ang) * r, 0.31f, 15 + Mathf.Sin(ang) * r), Vector3.one * 0.6f, Palette[i % 7], gS);
                Body(b, 0.5f);
            }
        }

        // =====================================================================
        // 05 – ADDFORCE – TRYBY
        // =====================================================================
        static void S05_AddForce()
        {
            Begin("05 · AddForce – tryby ForceMode",
                "rb.AddForce(wektor, ForceMode) – wektor = 10 wzdłuż osi Z:\n" +
                "• <b>Force</b> – siła ciągła [N], uwzględnia masę: a = F / m. Przykładana co krok (FixedUpdate) przez 1 s.\n" +
                "• <b>Acceleration</b> – przyspieszenie ciągłe [m/s²], <b>ignoruje masę</b>. Przez 1 s.\n" +
                "• <b>Impulse</b> – natychmiastowy impuls [N·s], uwzględnia masę: Δv = J / m. Jednorazowo.\n" +
                "• <b>VelocityChange</b> – natychmiastowa zmiana prędkości [m/s], <b>ignoruje masę</b>. Jednorazowo.\n\n" +
                "W każdej parze: lewy klocek m = 1 kg, prawy m = 4 kg (ciemniejszy). Oczekiwane prędkości:\n" +
                "m = 1 kg: wszystkie 10 m/s.\nm = 4 kg: Force 2,5 · Acceleration 10 · Impulse 2,5 · VelocityChange 10 m/s.\n\n" +
                "Wniosek: Force przez 1 s ≡ Impulse 10 N·s, a Acceleration przez 1 s ≡ VelocityChange 10 m/s – różnią się tylko tym, " +
                "czy działanie jest rozłożone w czasie. Tryby ciągłe – w FixedUpdate (silnik, wiatr), natychmiastowe – jednorazowo (skok, wybuch, strzał).",
                "<b>Spacja</b> – AddForce we wszystkich trybach");
            Floor();
            Cam(new Vector3(0, 20, -20), new Vector3(0, 0, 10));

            ForceMode[] modes = { ForceMode.Force, ForceMode.Acceleration, ForceMode.Impulse, ForceMode.VelocityChange };
            for (int i = 0; i < modes.Length; i++)
            {
                var g = Group("Tryb_" + modes[i]);
                float x = -9 + 6 * i;
                SectionLabel(modes[i].ToString(), new Vector3(x, 2.8f, -8), g);
                for (int k = 0; k < 2; k++)
                {
                    float m = k == 0 ? 1f : 4f;
                    var c = Box($"{modes[i]}_m{m}", new Vector3(x - 1.1f + 2.2f * k, 1f, -8), Vector3.one, k == 0 ? MassMat(0) : MassMat(2), g);
                    var rb = Body(c, m, false);
                    rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                    bool continuous = modes[i] == ForceMode.Force || modes[i] == ForceMode.Acceleration;
                    Apply(c, ApplyOnAction.Kind.Force, modes[i], new Vector3(0, 0, 10), continuous ? 1f : 0f);
                    Label(c, $"m = {m} kg", WorldLabel.Info.Speed, 0.9f, 15);
                }
            }
            Box("Sciana_koncowa", new Vector3(0, 1.5f, 45), new Vector3(26, 3, 1), M_Gray);
        }

        // =====================================================================
        // 06 – ADDTORQUE – TRYBY
        // =====================================================================
        static void S06_AddTorque()
        {
            Begin("06 · AddTorque – tryby ForceMode",
                "rb.AddTorque(wektor, ForceMode) – jak AddForce, ale dla obrotu. Zamiast masy liczy się <b>moment bezwładności I</b> " +
                "(Unity liczy go automatycznie z masy i kształtu colliderów – widać go jako I(y)).\n" +
                "• <b>Force</b> – moment siły [N·m] przez 1 s: ε = τ / I\n" +
                "• <b>Acceleration</b> – przyspieszenie kątowe [rad/s²] przez 1 s, ignoruje I\n" +
                "• <b>Impulse</b> – impuls momentu [N·m·s]: Δω = L / I\n" +
                "• <b>VelocityChange</b> – natychmiastowa zmiana ω [rad/s], ignoruje I\n\n" +
                "Wartość: 2 wokół osi Y. Przód: mały klocek 1×1×1, m = 1 kg (I ≈ 0,17). Tył: duży 2×2×2, m = 4 kg (I ≈ 2,67).\n" +
                "Mały: Force/Impulse → 12 rad/s, Acceleration/VelocityChange → 2 rad/s.\n" +
                "Duży: Force/Impulse → 0,75 rad/s, Acceleration/VelocityChange → 2 rad/s.\n\n" +
                "maxAngularVelocity podniesiono do 100 rad/s, Angular Damping = 0, pozycje zamrożone (Constraints).",
                "<b>Spacja</b> – AddTorque we wszystkich trybach");
            Floor();
            Cam(new Vector3(0, 11, -17), new Vector3(0, 1.5f, 3));

            ForceMode[] modes = { ForceMode.Force, ForceMode.Acceleration, ForceMode.Impulse, ForceMode.VelocityChange };
            for (int i = 0; i < modes.Length; i++)
            {
                var g = Group("Tryb_" + modes[i]);
                float x = -9 + 6 * i;
                SectionLabel(modes[i].ToString(), new Vector3(x, 4.8f, 5), g);
                for (int k = 0; k < 2; k++)
                {
                    bool big = k == 1;
                    var c = Box($"{modes[i]}_{(big ? "duzy" : "maly")}", new Vector3(x, big ? 2f : 1.2f, big ? 5f : 0f),
                        Vector3.one * (big ? 2f : 1f), Palette[i], g);
                    var rb = Body(c, big ? 4f : 1f, false, 0f, 0f);
                    rb.constraints = RigidbodyConstraints.FreezePosition;
                    c.AddComponent<RigidbodySetup>().maxAngularVelocity = 100f;
                    bool continuous = modes[i] == ForceMode.Force || modes[i] == ForceMode.Acceleration;
                    Apply(c, ApplyOnAction.Kind.Torque, modes[i], new Vector3(0, 2, 0), continuous ? 1f : 0f);
                    Label(c, big ? "duży, m = 4 kg" : "mały, m = 1 kg", WorldLabel.Info.AngularSpeed | WorldLabel.Info.Inertia, big ? 1.6f : 0.9f, 15);
                }
            }
        }

        // =====================================================================
        // 07 – KOLIDERY
        // =====================================================================
        static void S07_Collidery()
        {
            Begin("07 · Kolidery – kształty i różnice",
                "Collider określa <b>kształt używany przez fizykę</b> – nie musi pokrywać się z wyglądem (Mesh Renderer)!\n\n" +
                "<b>Rampa</b> (Spacja – znika półka), od lewej:\n" +
                "1. Box Collider – zsuwa się / koziołkuje,\n2. Sphere Collider – toczy się (najtańszy kształt),\n" +
                "3. Capsule Collider (kapsuła leżąca) – toczy się prosto,\n4. Cylinder + Mesh Collider (Convex) – prawdziwy walec,\n" +
                "5. Sześcian z Sphere Colliderem – wygląda jak kostka, toczy się jak kula!\n" +
                "6. Collider złożony (compound) – kilka prostych colliderów w dzieciach jednego Rigidbody (hantla).\n\n" +
                "<b>Walce</b> (na podłodze, lekko przechylone): domyślny prymityw Cylinder ma <b>Capsule Collider</b> z zaokrąglonym spodem – " +
                "przewraca się; z Mesh Colliderem (Convex) staje na płaskiej podstawie.\n\n" +
                "<b>Kubki</b>: ta sama siatka jako Mesh Collider. Non-convex dokładnie odwzorowuje wnętrze – kulki wpadają do środka. " +
                "Convex = otoczka wypukła – zamyka otwór, kulki leżą na wierzchu. Non-convex może być tylko statyczny lub kinematyczny.\n" +
                "Wspólne pola: Is Trigger, Material, Center, Size/Radius/Height/Direction. Kolidery widać w oknie Scene (zielone linie).",
                "<b>Spacja</b> – zdejmij półkę, puść walce, zrzuć kulki do kubków");
            Floor();
            Cam(new Vector3(8, 12, -17), new Vector3(8, 2, 5));

            // --- rampa ---
            var gR = Group("Rampa_kolidery");
            var ramp = Ramp("Rampa", new Vector3(0, 0, 0), 16f, 14f, 18f, M_Ramp, null, gR);
            float surfY(float z) => Mathf.Tan(18f * Mathf.Deg2Rad) * z;
            float shelfZ = 11f;
            float shelfTop = surfY(shelfZ + 1.2f) + 0.55f;
            Trapdoor("Polka", new Vector3(0, shelfTop - 0.1f, shelfZ), new Vector3(16, 0.2f, 2.4f), gR);
            float baseY = shelfTop + 0.02f;
            float[] xs = { -6.25f, -3.75f, -1.25f, 1.25f, 3.75f, 6.25f };

            var box = Box("1_BoxCollider", new Vector3(xs[0], baseY + 0.5f, shelfZ), Vector3.one, M_Red, gR);
            Body(box, 1f);
            Label(box, "1. Box Collider", WorldLabel.Info.None, 1f, 15);

            var sph = Ball("2_SphereCollider", new Vector3(xs[1], baseY + 0.5f, shelfZ), 1f, M_Orange, gR);
            Body(sph, 1f);
            Label(sph, "2. Sphere Collider", WorldLabel.Info.None, 1f, 15);

            var cap = Prim(PrimitiveType.Capsule, "3_CapsuleCollider", new Vector3(xs[2], baseY + 0.4f, shelfZ), Vector3.one * 0.8f, M_Yellow, gR,
                Quaternion.Euler(0, 0, 90));
            Body(cap, 1f);
            Label(cap, "3. Capsule Collider", WorldLabel.Info.None, 1f, 15);

            var cyl = Prim(PrimitiveType.Cylinder, "4_Cylinder_MeshConvex", new Vector3(xs[3], baseY + 0.4f, shelfZ), new Vector3(0.8f, 0.6f, 0.8f), M_Green, gR,
                Quaternion.Euler(0, 0, 90));
            Object.DestroyImmediate(cyl.GetComponent<Collider>());
            var mc = cyl.AddComponent<MeshCollider>();
            mc.sharedMesh = BuiltinMesh("Cylinder.fbx");
            mc.convex = true;
            Body(cyl, 1f);
            Label(cyl, "4. Mesh Collider (Convex)", WorldLabel.Info.None, 1f, 15);

            var fake = Box("5_Szescian_SphereCollider", new Vector3(xs[4], baseY + 0.5f, shelfZ), Vector3.one, M_Teal, gR);
            Object.DestroyImmediate(fake.GetComponent<Collider>());
            fake.AddComponent<SphereCollider>().radius = 0.5f;
            Body(fake, 1f);
            Label(fake, "5. Sześcian + Sphere Collider", WorldLabel.Info.None, 1f, 15);

            var dumb = new GameObject("6_Compound_Hantla");
            dumb.transform.SetParent(gR, true);
            dumb.transform.position = new Vector3(xs[5], baseY + 0.4f, shelfZ);
            Ball("Kula_L", dumb.transform.position + new Vector3(-0.7f, 0, 0), 0.8f, M_Purple, dumb.transform);
            Ball("Kula_P", dumb.transform.position + new Vector3(0.7f, 0, 0), 0.8f, M_Purple, dumb.transform);
            Prim(PrimitiveType.Cylinder, "Drazek", dumb.transform.position, new Vector3(0.25f, 0.6f, 0.25f), M_Dark, dumb.transform, Quaternion.Euler(0, 0, 90));
            Body(dumb, 2f);
            Label(dumb, "6. Compound (3 kolidery)", WorldLabel.Info.None, 1f, 15);

            // --- walce na podłodze ---
            var gC = Group("Walce");
            SectionLabel("Cylinder: Capsule vs Mesh (Convex)", new Vector3(23.5f, 3.6f, -3), gC);
            var c1 = Prim(PrimitiveType.Cylinder, "Cylinder_CapsuleCollider", new Vector3(22, 1.06f, -3), new Vector3(0.8f, 1f, 0.8f), M_Yellow, gC, Quaternion.Euler(0, 0, 6));
            var rb1 = Body(c1, 1f);
            rb1.isKinematic = true;
            c1.AddComponent<ReleaseOnAction>();
            Label(c1, "Capsule Collider\n(domyślny)", WorldLabel.Info.None, 1.6f, 15);
            var c2 = Prim(PrimitiveType.Cylinder, "Cylinder_MeshCollider_Convex", new Vector3(25, 1.06f, -3), new Vector3(0.8f, 1f, 0.8f), M_Green, gC, Quaternion.Euler(0, 0, 6));
            Object.DestroyImmediate(c2.GetComponent<Collider>());
            var mc2 = c2.AddComponent<MeshCollider>();
            mc2.sharedMesh = BuiltinMesh("Cylinder.fbx");
            mc2.convex = true;
            var rb2 = Body(c2, 1f);
            rb2.isKinematic = true;
            c2.AddComponent<ReleaseOnAction>();
            Label(c2, "Mesh Collider\n(Convex)", WorldLabel.Info.None, 1.6f, 15);

            // --- kubki ---
            var gK = Group("Kubki");
            Mesh cup = CupMesh();
            for (int k = 0; k < 2; k++)
            {
                bool convex = k == 1;
                float x = convex ? 16f : 11.5f;
                var go = new GameObject(convex ? "Kubek_MeshCollider_Convex" : "Kubek_MeshCollider_NonConvex");
                go.transform.SetParent(gK, true);
                go.transform.position = new Vector3(x, 0, 2);
                go.AddComponent<MeshFilter>().sharedMesh = cup;
                go.AddComponent<MeshRenderer>().sharedMaterial = convex ? M_Blue : M_Purple;
                var col = go.AddComponent<MeshCollider>();
                col.sharedMesh = cup;
                col.convex = convex;
                Label(go, convex ? "Mesh Collider\nConvex = true" : "Mesh Collider\nConvex = false", WorldLabel.Info.None, 2.2f, 16);
                Vector3[] offs = { new Vector3(-0.5f, 0, -0.3f), new Vector3(0.4f, 0, 0.2f), new Vector3(0, 0, 0.6f), new Vector3(0.1f, 0, -0.6f) };
                for (int j = 0; j < offs.Length; j++)
                {
                    var l = Launcher($"Zrzut_{(convex ? "C" : "N")}_{j + 1}", new Vector3(x, 4.5f + j * 0.6f, 2) + offs[j], Vector3.forward, 0f, 0.4f, 0.2f, M_Red, gK, false);
                    l.lifeTime = 0f;
                }
            }
        }

        // =====================================================================
        // 08 – MATERIAŁY FIZYCZNE
        // =====================================================================
        static void S08_PhysicsMaterial()
        {
            Begin("08 · Physics Material – tarcie i sprężystość",
                "<b>Physics Material</b> (pliki w Assets/Fizyka/PhysicsMaterials, przypisane w polu Material collidera) opisuje powierzchnię:\n" +
                "• <b>Dynamic Friction</b> – tarcie podczas ruchu,  • <b>Static Friction</b> – tarcie, które trzeba pokonać, by ruszyć,\n" +
                "• <b>Bounciness</b> – sprężystość (0 = glina, 1 = idealne odbicie),\n" +
                "• <b>Friction / Bounce Combine</b> – jak łączyć wartości dwóch materiałów: Average, Minimum, Multiply, Maximum " +
                "(przy różnych trybach wygrywa wyższy priorytet: Average < Minimum < Multiply < Maximum).\n\n" +
                "<b>A. Rampa 25°</b> (Spacja zdejmuje blokadę): Lód (0/0) zjeżdża szybko, Drewno (0,3/0,35) wolniej, Guma (0,9/1) stoi. " +
                "Ostatni: Static 0,8 / Dynamic 0,1 – stoi, ale szturchnięty LPM zjeżdża szybko! (tan 25° ≈ 0,47)\n" +
                "Rampa ma tarcie 1 i Combine = Minimum, więc decyduje materiał klocka.\n\n" +
                "<b>B. Odbicia</b> – kule z Bounciness 0; 0,25; 0,5; 0,8; 1 (Bounce Combine = Maximum).\n\n" +
                "<b>C. Combine</b> – 4 klocki z lodu (tarcie 0) pchnięte z 6 m/s po gumie (0,9) z różnym Friction Combine: " +
                "Average → 0,45 · Minimum → 0 · Multiply → 0 · Maximum → 0,9.",
                "<b>Spacja</b> – zdejmij blokadę, zrzuć kule, pchnij klocki");
            Floor();
            Cam(new Vector3(4, 15, -31), new Vector3(4, 1, -1));

            // --- A: tarcie na rampie ---
            var gA = Group("A_Tarcie_rampa");
            var rampPM = PhysMat("Rampa_tarcie1_Minimum", 1f, 1f, 0f, PhysicsMaterialCombine.Minimum, PhysicsMaterialCombine.Average);
            var ramp = Ramp("Rampa_25st", new Vector3(-1, 0, 0), 9f, 10f, 25f, M_Ramp, rampPM, gA);
            SectionLabel("A. Tarcie (rampa 25°)", ramp.TransformPoint(new Vector3(0, 2.2f, 9f)), gA);
            (string n, float dyn, float st, Material m)[] fr =
            {
                ("Lod", 0f, 0f, M_Ice), ("Drewno", 0.3f, 0.35f, M_Wood), ("Guma", 0.9f, 1f, M_Rubber), ("Statyczne08_Dynamiczne01", 0.1f, 0.8f, M_Purple)
            };
            for (int i = 0; i < fr.Length; i++)
            {
                var pm = PhysMat("Rampa_" + fr[i].n, fr[i].dyn, fr[i].st, 0f);
                var b = Box("Klocek_" + fr[i].n, ramp.TransformPoint(new Vector3(-3 + 2 * i, 0.52f, 7.5f)), Vector3.one, fr[i].m, gA, ramp.rotation);
                Body(b, 1f);
                SetPhys(b, pm);
                Label(b, $"{fr[i].n} ({fr[i].st} / {fr[i].dyn})", WorldLabel.Info.None, i % 2 == 0 ? 0.9f : 1.5f, 14);
            }
            var stop = Box("Blokada", ramp.TransformPoint(new Vector3(0, 0.4f, 6.6f)), new Vector3(9, 0.8f, 0.3f), M_Trapdoor, gA, ramp.rotation);
            stop.AddComponent<DisableOnAction>();

            // --- B: odbicia ---
            var gB = Group("B_Odbicia");
            SectionLabel("B. Bounciness", new Vector3(12, 8.5f, 4), gB);
            Trapdoor("Zapadnia_B", new Vector3(12, 5.4f, 4), new Vector3(11, 0.2f, 1.5f), gB);
            float[] e = { 0f, 0.25f, 0.5f, 0.8f, 1f };
            for (int i = 0; i < e.Length; i++)
            {
                var pm = PhysMat("Odbicie_" + e[i].ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), 0.6f, 0.6f, e[i], PhysicsMaterialCombine.Average, PhysicsMaterialCombine.Maximum);
                var s = Ball("Kula_bounciness_" + e[i].ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), new Vector3(8 + 2 * i, 6f, 4), 1f, Palette[i], gB);
                Body(s, 1f);
                SetPhys(s, pm);
                Label(s, $"bounciness = {e[i]}", WorldLabel.Info.Height, 0.9f, 15);
            }

            // --- C: combine ---
            var gC = Group("C_FrictionCombine");
            SectionLabel("C. Friction Combine: lód na gumie", new Vector3(0, 3, -19), gC);
            var rubber = Box("Pas_gumowy", new Vector3(0, 0.05f, -12), new Vector3(12, 0.1f, 16), M_Rubber, gC);
            SetPhys(rubber, PhysMat("Guma_pas", 0.9f, 0.9f, 0f));
            PhysicsMaterialCombine[] cmb = { PhysicsMaterialCombine.Average, PhysicsMaterialCombine.Minimum, PhysicsMaterialCombine.Multiply, PhysicsMaterialCombine.Maximum };
            for (int i = 0; i < cmb.Length; i++)
            {
                var pm = PhysMat("Lod_" + cmb[i], 0f, 0f, 0f, cmb[i], PhysicsMaterialCombine.Average);
                var b = Box("Lod_" + cmb[i], new Vector3(-4.5f + 3 * i, 0.61f, -19), Vector3.one, M_Ice, gC);
                Body(b, 1f);
                SetPhys(b, pm);
                Apply(b, ApplyOnAction.Kind.Force, ForceMode.VelocityChange, new Vector3(0, 0, 6));
                Label(b, "Combine = " + cmb[i], WorldLabel.Info.Speed, 1f, 15);
            }
            Box("Sciana_C", new Vector3(0, 0.5f, -3.6f), new Vector3(12, 1, 0.4f), M_Gray, gC);
        }
    }
}
