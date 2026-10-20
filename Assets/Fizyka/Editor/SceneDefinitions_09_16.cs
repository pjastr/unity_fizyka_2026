using UnityEngine;

namespace Fizyka.EditorTools
{
    public static partial class SceneBuilder
    {
        // =====================================================================
        // 09 – ZDARZENIA OnCollision
        // =====================================================================
        static void S09_OnCollision()
        {
            Begin("09 · OnCollisionEnter / Stay / Exit",
                "Zdarzenia kolizji to metody w skrypcie na obiekcie z Colliderem:\n" +
                "• <b>OnCollisionEnter(Collision c)</b> – pierwszy kontakt,\n" +
                "• <b>OnCollisionStay(Collision c)</b> – co krok fizyki, dopóki kontakt trwa,\n" +
                "• <b>OnCollisionExit(Collision c)</b> – koniec kontaktu.\n" +
                "Obiekt Collision zawiera m.in.: gameObject, relativeVelocity, impulse, contactCount, GetContact(i).\n\n" +
                "<b>Lewo:</b> Spacja zrzuca kulkę na statyczny „czujnik”. Czerwony błysk = Enter, żółty = trwa Stay. " +
                "Zepchnij kulkę LPM, aby zobaczyć Exit. Licznik Stay rośnie co krok fizyki (50×/s).\n\n" +
                "<b>Prawo:</b> kinematyczny klocek (MovePosition) przechodzi przez statyczną ścianę BEZ zdarzeń i bez zatrzymania – " +
                "para kinematyczny–statyczny nie generuje kolizji. Gdy pcha dynamiczny klocek – zdarzenia są.\n\n" +
                "Warunek: co najmniej jeden z obiektów musi mieć dynamiczny (nie-kinematyczny) Rigidbody. " +
                "Jeżeli collider ma Is Trigger – zamiast OnCollision* działają OnTrigger* (scena 10).",
                "<b>Spacja</b> – zrzuć kulkę na czujnik", showLog: true);
            Floor();
            Cam(new Vector3(1.5f, 7.5f, -10), new Vector3(1.5f, 1, 4));

            var gL = Group("Czujnik");
            var sensor = Box("Czujnik_statyczny", new Vector3(-4, 0.5f, 4), new Vector3(3, 1, 3), M_Gray, gL);
            var rep = sensor.AddComponent<CollisionReporter>();
            rep.displayName = "Czujnik";
            Label(sensor, "Czujnik (statyczny)", WorldLabel.Info.None, 1.6f, 16);
            var l = Launcher("Zrzut_kulki", new Vector3(-4, 6, 4), Vector3.forward, 0f, 0.6f, 1f, M_Red, gL, false);
            l.lifeTime = 0f;
            l.projectileName = "Kulka";

            var gR = Group("Kinematyczny_przez_sciane");
            var wall = Box("Sciana_statyczna", new Vector3(5, 1, 4), new Vector3(0.4f, 2, 3), M_Gray, gR);
            wall.AddComponent<CollisionReporter>().displayName = "Ściana statyczna";
            Label(wall, "Ściana statyczna", WorldLabel.Info.None, 1.8f, 15);
            var kin = Box("Klocek_kinematyczny", new Vector3(2, 0.55f, 4), Vector3.one, M_Orange, gR);
            var krb = Body(kin, 1f, false);
            krb.isKinematic = true;
            var km = kin.AddComponent<KinematicMover>();
            km.offset = new Vector3(6, 0, 0);
            km.period = 3f;
            kin.AddComponent<CollisionReporter>().displayName = "Kinematyk";
            Label(kin, "Kinematyczny (MovePosition)", WorldLabel.Info.None, 1.2f, 15);
            var dyn = Box("Klocek_dynamiczny", new Vector3(8.75f, 0.5f, 4), Vector3.one, M_Green, gR);
            Body(dyn, 1f);
            dyn.AddComponent<CollisionReporter>().displayName = "Klocek dynamiczny";
            Label(dyn, "Dynamiczny", WorldLabel.Info.None, 1.2f, 15);
        }

        // =====================================================================
        // 10 – TRIGGERY
        // =====================================================================
        static void S10_Triggery()
        {
            Begin("10 · Triggery (Is Trigger)",
                "Collider z zaznaczonym <b>Is Trigger</b> nie zatrzymuje obiektów – działa jak czujnik obszaru. " +
                "Zamiast OnCollision* wywołuje:\n" +
                "• <b>OnTriggerEnter(Collider other)</b> – coś weszło,\n• <b>OnTriggerStay(Collider other)</b> – co krok fizyki, gdy jest w środku,\n" +
                "• <b>OnTriggerExit(Collider other)</b> – wyszło.\n" +
                "Argumentem jest Collider (nie Collision) – brak punktów styku i impulsu.\n\n" +
                "Spacja – kulki spadają na:\n" +
                "1. zwykłą platformę – kulka się zatrzymuje (OnCollisionEnter),\n" +
                "2. strefę-trigger – kulka przelatuje, strefa liczy obiekty w środku i zmienia kolor (wypchnij kulkę LPM → Exit),\n" +
                "3. „komin” – trigger, który w OnTriggerStay dodaje przyspieszenie 14 m/s² w górę (ForceMode.Acceleration, więcej niż g) – " +
                "kulki unoszą się i podskakują przy górnej krawędzi.\n\n" +
                "Zastosowania: punkty kontrolne, pułapki, strefy wiatru/wody, zbieranie przedmiotów. " +
                "Trigger działa, gdy co najmniej jeden z obiektów ma Rigidbody.",
                "<b>Spacja</b> – zrzuć kulki", showLog: true);
            Floor();
            Cam(new Vector3(0, 8, -14), new Vector3(0, 3, 3));

            var g1 = Group("1_Platforma_zwykla");
            var plat = Box("Platforma_zwykla", new Vector3(-6, 2, 3), new Vector3(3, 0.3f, 3), M_Gray, g1);
            plat.AddComponent<CollisionReporter>().displayName = "Platforma";
            Label(plat, "1. Zwykły collider", WorldLabel.Info.None, 1.2f, 16);
            var l1 = Launcher("Zrzut_1", new Vector3(-6, 7, 3), Vector3.forward, 0f, 0.5f, 1f, M_Red, g1, false);
            l1.lifeTime = 0f;

            var g2 = Group("2_Strefa_trigger");
            var zone = Box("Strefa_trigger", new Vector3(0, 1.5f, 3), new Vector3(3, 3, 3), TransparentMat("Szklo_Zielone", new Color(0.2f, 0.9f, 0.4f, 0.25f)), g2);
            zone.GetComponent<Collider>().isTrigger = true;
            zone.AddComponent<TriggerZoneReporter>().displayName = "Strefa";
            Label(zone, "2. Is Trigger = true", WorldLabel.Info.None, 2.0f, 16);
            var l2 = Launcher("Zrzut_2", new Vector3(0, 7, 3), Vector3.forward, 0f, 0.5f, 1f, M_Red, g2, false);
            l2.lifeTime = 0f;

            var g3 = Group("3_Komin");
            var chimney = Box("Komin_trigger", new Vector3(6, 4, 3), new Vector3(3, 8, 3), TransparentMat("Szklo_Niebieskie", new Color(0.3f, 0.6f, 1f, 0.2f)), g3);
            chimney.GetComponent<Collider>().isTrigger = true;
            chimney.AddComponent<UpdraftZone>();
            Label(chimney, "3. Trigger + OnTriggerStay\n(wiatr w górę)", WorldLabel.Info.None, 4.6f, 16);
            var l3 = Launcher("Zrzut_3", new Vector3(6, 10, 3), Vector3.forward, 0f, 0.5f, 1f, M_Red, g3, false);
            l3.lifeTime = 0f;
        }

        // =====================================================================
        // 11 – COLLISION DETECTION
        // =====================================================================
        static void S11_CollisionDetection()
        {
            Begin("11 · Collision Detection (tunelowanie)",
                "<b>Collision Detection</b> określa, jak wykrywane są kolizje szybkich obiektów:\n" +
                "• <b>Discrete</b> (domyślny) – sprawdza tylko pozycje w kolejnych krokach. Kulka 120 m/s przy kroku 0,02 s " +
                "przesuwa się o 2,4 m na krok – może „przeskoczyć” cienką (10 cm) ścianę = <b>tunelowanie</b>.\n" +
                "• <b>Continuous</b> – ciągłe wykrywanie (CCD) względem colliderów statycznych.\n" +
                "• <b>Continuous Dynamic</b> – CCD także względem innych ciał z trybem Continuous / Continuous Dynamic (najdroższy).\n" +
                "• <b>Continuous Speculative</b> – tańsze, „przewidujące” kontakty; działa też dla obrotów i kinematyków, " +
                "czasem daje fałszywe („duchowe”) kontakty.\n\n" +
                "Spacja – strzał z 4 wyrzutni w cienką ścianę. Wynik pojawia się w dzienniku i nad wyrzutnią. " +
                "Kilka strzałów pokazuje, że Discrete prawie zawsze przenika.\n\n" +
                "Rada: tryb ciągły tylko dla małych, szybkich obiektów (pociski) – kosztuje wydajność.",
                "<b>Spacja</b> – strzał (120 m/s)", showLog: true);
            Floor();
            Cam(new Vector3(0, 7, -21), new Vector3(0, 1.5f, 6));

            var wall = Box("Cienka_sciana_10cm", new Vector3(0, 2, 10), new Vector3(16, 4, 0.1f), M_Orange);
            Label(wall, "Ściana 10 cm (statyczna)", WorldLabel.Info.None, 2.6f, 18);
            Box("Tlo_zatrzymujace", new Vector3(0, 3, 32), new Vector3(16, 6, 4), M_Gray);

            CollisionDetectionMode[] modes =
            {
                CollisionDetectionMode.Discrete, CollisionDetectionMode.Continuous,
                CollisionDetectionMode.ContinuousDynamic, CollisionDetectionMode.ContinuousSpeculative
            };
            var g = Group("Wyrzutnie");
            for (int i = 0; i < modes.Length; i++)
            {
                var l = Launcher("Wyrzutnia_" + modes[i], new Vector3(-6 + 4 * i, 1.5f, -10), Vector3.forward, 120f, 0.3f, 0.2f, Palette[i], g);
                l.collisionDetection = modes[i];
                l.passCheckWall = wall.transform;
                l.passCheckName = modes[i].ToString();
                l.lifeTime = 8f;
                l.projectileName = "Pocisk_" + modes[i];
                Label(l.gameObject, modes[i].ToString(), WorldLabel.Info.None, 0.8f, 17);
            }
        }

        // =====================================================================
        // 12 – INTERPOLATE
        // =====================================================================
        static void S12_Interpolate()
        {
            Begin("12 · Interpolate (None / Interpolate / Extrapolate)",
                "Fizyka liczy się w stałych krokach (FixedUpdate, Time.fixedDeltaTime), a obraz w każdej klatce (Update). " +
                "Gdy kroków fizyki jest mniej niż klatek obrazu, ruch może być „szarpany”.\n\n" +
                "W tej scenie krok fizyki jest celowo duży: <b>0,1 s (10 Hz)</b>. Spacja przełącza 0,1 s ↔ 0,02 s.\n" +
                "• <b>None</b> – obiekt rysowany w pozycji z ostatniego kroku fizyki – widać skoki.\n" +
                "• <b>Interpolate</b> – rysowany pomiędzy dwoma ostatnimi krokami – płynnie, ale z opóźnieniem ok. 1 kroku.\n" +
                "• <b>Extrapolate</b> – pozycja przewidywana z prędkości – płynnie, bez opóźnienia, ale przy odbiciu od ściany " +
                "chwilowo „wchodzi” w ścianę.\n\n" +
                "Interpolacja zmienia tylko to, co widać (Transform), a nie wynik symulacji. " +
                "Zwykle: Interpolate dla gracza i obiektów śledzonych kamerą, None dla reszty.",
                "<b>Spacja</b> – przełącz krok fizyki 0,1 s ↔ 0,02 s", showLog: true);
            Floor();
            Cam(new Vector3(0, 7, -11), new Vector3(0, 1, 0));

            var settings = new GameObject("Ustawienia_fizyki");
            settings.transform.position = new Vector3(0, 4.2f, 3);
            settings.AddComponent<FixedStepToggle>();
            var sl = Label(settings, "Time.fixedDeltaTime", WorldLabel.Info.None, 0f, 20);
            sl.color = ColTitle;

            Box("Sciana_L", new Vector3(-8, 1, 0), new Vector3(0.5f, 2, 8), M_Gray);
            Box("Sciana_P", new Vector3(8, 1, 0), new Vector3(0.5f, 2, 8), M_Gray);
            RigidbodyInterpolation[] modes = { RigidbodyInterpolation.None, RigidbodyInterpolation.Interpolate, RigidbodyInterpolation.Extrapolate };
            for (int i = 0; i < modes.Length; i++)
            {
                var s = Ball("Kula_" + modes[i], new Vector3(-3, 1, -2.5f + 2.5f * i), 1f, Palette[i * 2], null);
                var rb = Body(s, 1f, false, 0f, 0f);
                rb.interpolation = modes[i];
                rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                SetPhys(s, PM_Sprezysty);
                var a = Apply(s, ApplyOnAction.Kind.Force, ForceMode.VelocityChange, new Vector3(8, 0, 0), 0f);
                a.applyOnStart = true;
                a.triggerOnMainAction = false;
                Label(s, "Interpolate = " + modes[i], WorldLabel.Info.None, 0.9f, 17);
            }
        }

        // =====================================================================
        // 13 – CONSTRAINTS
        // =====================================================================
        static void S13_Constraints()
        {
            Begin("13 · Constraints (blokowanie osi)",
                "<b>Constraints</b> (Rigidbody → Constraints) blokują ruch lub obrót wzdłuż wybranych osi świata: " +
                "Freeze Position X/Y/Z, Freeze Rotation X/Y/Z.\n\n" +
                "Spacja – każdy klocek dostaje skośny strzał kulką (od lewej):\n" +
                "1. Brak ograniczeń – przesuwa się i obraca.\n" +
                "2. Freeze Rotation X, Y, Z – tylko przesuwa się, nigdy się nie przewraca (typowe dla postaci).\n" +
                "3. Freeze Position Y – „wisi” w powietrzu mimo grawitacji; porusza się tylko poziomo.\n" +
                "4. „2.5D”: Freeze Position Z + Freeze Rotation X, Y – ruch tylko w płaszczyźnie XY (gry platformowe).\n" +
                "5. Freeze Position X, Z + Freeze Rotation X, Z – tylko obrót wokół Y (jak drzwi obrotowe).\n\n" +
                "W kodzie:\nrb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;",
                "<b>Spacja</b> – strzał w klocki");
            Floor();
            Cam(new Vector3(1, 7.5f, -12), new Vector3(1, 1.5f, 4));

            (string n, RigidbodyConstraints c, float y)[] cfg =
            {
                ("1. None", RigidbodyConstraints.None, 0.5f),
                ("2. Freeze Rotation XYZ", RigidbodyConstraints.FreezeRotation, 0.5f),
                ("3. Freeze Position Y", RigidbodyConstraints.FreezePositionY, 2.5f),
                ("4. Freeze Pos Z + Rot X,Y", RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY, 0.5f),
                ("5. Freeze Pos X,Z + Rot X,Z", RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ, 0.5f),
            };
            for (int i = 0; i < cfg.Length; i++)
            {
                var g = Group("Klocek_" + (i + 1));
                Vector3 p = new Vector3(-8 + 4 * i, cfg[i].y, 4);
                var c = Box("Klocek_" + cfg[i].n, p, new Vector3(1, 1, 1), Palette[i], g);
                var rb = Body(c, 1f);
                rb.constraints = cfg[i].c;
                Label(c, cfg[i].n, WorldLabel.Info.None, 1.1f, 15);
                Vector3 target = p + new Vector3(0.3f, 0.2f, 0);
                Vector3 from = p + new Vector3(-2.5f, 0.2f, -4f);
                var l = Launcher("Wyrzutnia_" + (i + 1), from, target - from, 16f, 0.4f, 1f, M_Red, g);
                l.useGravity = false;
                l.lifeTime = 5f;
            }
        }

        // =====================================================================
        // 14 – SLEEPING
        // =====================================================================
        static void S14_Sleeping()
        {
            Begin("14 · Uśpienie ciał (Sleeping)",
                "Uśpienie to optymalizacja: ciało, którego energia ruchu (na kg masy) spadnie poniżej <b>Sleep Threshold</b>, " +
                "przestaje być symulowane, dopóki coś go nie obudzi (kolizja, AddForce, zmiana prędkości, WakeUp()).\n" +
                "Kolor: <color=#ffb347>pomarańczowy</color> = aktywny, <color=#7fb2ff>niebieski</color> = śpi (rb.IsSleeping()).\n\n" +
                "Spacja – obie kule dostają 3 m/s. Lewa ma domyślny próg 0,005 – toczy się aż do zatrzymania. " +
                "Prawa ma próg 1,0 – „zasypia” nagle, jeszcze w ruchu!\n" +
                "Wieża klocków po chwili zasypia; LPM ją budzi (sąsiedzi budzą się przy kontakcie).\n\n" +
                "Metody: rb.IsSleeping(), rb.Sleep(), rb.WakeUp(). Domyślny próg: Project Settings → Physics → Sleep Threshold " +
                "(dla pojedynczego ciała: rb.sleepThreshold – tu ustawiany komponentem RigidbodySetup).\n" +
                "Jak w przykładzie RigidbodyChecker z wykładu: można logować rb.linearVelocity.magnitude oraz rb.IsSleeping().",
                "<b>Spacja</b> – popchnij kule", showLog: true);
            Floor();
            Cam(new Vector3(0, 6.5f, -12), new Vector3(0, 1.5f, 4));

            var gT = Group("Wieza");
            SectionLabel("Wieża klocków", new Vector3(-5, 7, 4), gT);
            for (int i = 0; i < 6; i++)
            {
                var c = Box("Klocek_" + (i + 1), new Vector3(-5, 0.5f + i * 1.0f, 4), Vector3.one, M_Orange, gT);
                Body(c, 1f);
                var si = c.AddComponent<SleepIndicator>();
                si.logChanges = i == 5;
                if (i == 5) Label(c, "górny klocek", WorldLabel.Info.Sleeping, 1f, 15);
            }

            var gS = Group("Kule");
            float[] th = { 0.005f, 1.0f };
            for (int i = 0; i < 2; i++)
            {
                var s = Ball($"Kula_sleepThreshold_{th[i]}", new Vector3(2 + 4 * i, 0.5f, -2), 1f, M_Orange, gS);
                Body(s, 1f, true, 0f, 0.8f);
                s.AddComponent<RigidbodySetup>().sleepThreshold = th[i];
                s.AddComponent<SleepIndicator>();
                Apply(s, ApplyOnAction.Kind.Force, ForceMode.VelocityChange, new Vector3(0, 0, 3));
                Label(s, $"sleepThreshold = {th[i]}", WorldLabel.Info.Speed | WorldLabel.Info.Sleeping, 0.9f, 15);
            }
            Box("Sciana", new Vector3(4, 1, 40), new Vector3(10, 2, 1), M_Gray, gS);
        }

        // =====================================================================
        // 15 – VELOCITYCHANGE vs USTAWIENIE PRĘDKOŚCI
        // =====================================================================
        static void S15_VelocityMethods()
        {
            Begin("15 · AddForce(VelocityChange) vs velocity vs MovePosition",
                "Porównanie sposobów nadawania ruchu (Spacja działa na wszystkie tory – naciśnij kilka razy!):\n" +
                "<b>A. rb.AddForce(v, ForceMode.VelocityChange)</b> – DODAJE 4 m/s do bieżącej prędkości (naciśnięcia się sumują). " +
                "To nadal fizyka: tarcie, kolizje, ściana zatrzymuje.\n" +
                "<b>B. rb.linearVelocity = v</b> – USTAWIA prędkość dokładnie na 4 m/s (nadpisuje poprzednią, także składową pionową – " +
                "np. spadanie). Pełna kontrola kosztem „fizyczności”.\n" +
                "<b>C. Is Kinematic + rb.MovePosition()</b> – ruch po zadanej trasie (5 s); pcha klocki, ale przechodzi przez ścianę.\n" +
                "<b>D. transform.position += …</b> na dynamicznym Rigidbody – teleportacja co klatkę; silnik „wypycha” obiekt z kolizji " +
                "szarpnięciami. Tak NIE należy ruszać Rigidbody!\n\n" +
                "VelocityChange vs velocity =: oba nie zależą od masy i działają natychmiast; VelocityChange symuluje oddziaływanie " +
                "(skoki, odrzut, zmiana kierunku), ustawianie velocity daje precyzyjną kontrolę prędkości.",
                "<b>Spacja</b> – ruch we wszystkich torach", showLog: true);
            Floor();
            Cam(new Vector3(0, 7.5f, -15), new Vector3(0, 0.5f, 2));

            (string n, VelocityMethodDemo.Method m)[] lanes =
            {
                ("A. AddForce\nVelocityChange", VelocityMethodDemo.Method.AddForceVelocityChange),
                ("B. linearVelocity =", VelocityMethodDemo.Method.SetLinearVelocity),
                ("C. Kinematic\nMovePosition", VelocityMethodDemo.Method.MovePositionKinematic),
                ("D. transform.position\n(źle!)", VelocityMethodDemo.Method.TransformPosition),
            };
            for (int i = 0; i < lanes.Length; i++)
            {
                float x = -6 + 4 * i;
                var g = Group("Tor_" + (char)('A' + i));
                var mover = Box("Ruchomy_" + (char)('A' + i), new Vector3(x, 0.5f, -6), Vector3.one, Palette[i], g);
                var rb = Body(mover, 2f);
                rb.isKinematic = lanes[i].m == VelocityMethodDemo.Method.MovePositionKinematic;
                SetPhys(mover, PM_Slisko);
                var vm = mover.AddComponent<VelocityMethodDemo>();
                vm.method = lanes[i].m;
                vm.velocity = new Vector3(0, 0, 4);
                vm.moveDuration = 5f;
                Label(mover, lanes[i].n, WorldLabel.Info.Speed, 1.1f, 15);

                var small = Box("Klocek_do_pchania_" + (char)('A' + i), new Vector3(x, 0.3f, 3.5f), Vector3.one * 0.6f, M_White, g);
                Body(small, 0.5f);
                Box("Sciana_" + (char)('A' + i), new Vector3(x, 1, 10), new Vector3(3, 2, 0.3f), M_Gray, g);
            }
        }

        // =====================================================================
        // 16 – CHARACTER CONTROLLER
        // =====================================================================
        static void S16_CharacterController()
        {
            Begin("16 · CharacterController",
                "<b>CharacterController</b> to kapsuła do sterowania postacią <b>bez Rigidbody</b>: ruch wykonuje controller.Move(), " +
                "a kolizje są rozwiązywane bez sił – postać nie przewraca się i nie odbija.\n" +
                "Najważniejsze pola:\n" +
                "• <b>Slope Limit</b> = 45° – maks. nachylenie, po którym da się wejść (rampy 20°, 35°, 50°),\n" +
                "• <b>Step Offset</b> = 0,3 – maks. wysokość stopnia bez skoku (schody 0,2 / 0,25 / 0,5 m),\n" +
                "• Skin Width, Min Move Distance, Center, Radius, Height.\n" +
                "Grawitację i skok liczymy sami w skrypcie (jak w przykładzie PlayerController z wykładu); " +
                "isGrounded mówi, czy postać stoi na podłożu.\n" +
                "CharacterController sam NIE pcha obiektów z Rigidbody – robimy to w OnControllerColliderHit (pomarańczowe skrzynie).\n\n" +
                "Sterowanie: WASD – ruch, Shift – bieg, Spacja – skok, PPM + mysz – obrót kamery, kółko – odległość kamery.",
                "<b>WASD</b> – ruch postaci, <b>Shift</b> – bieg, <b>Spacja</b> – skok, <b>PPM + mysz</b> – kamera, <b>kółko</b> – odległość",
                showLog: false, freeCamera: false);
            Floor();

            // postać
            var player = Prim(PrimitiveType.Capsule, "Gracz", new Vector3(0, 1.1f, -8), Vector3.one, M_Blue);
            Object.DestroyImmediate(player.GetComponent<Collider>());
            var cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = Vector3.zero;
            cc.slopeLimit = 45f;
            cc.stepOffset = 0.3f;
            cc.skinWidth = 0.08f;
            cc.minMoveDistance = 0.001f;
            var nose = Prim(PrimitiveType.Cube, "Nos", player.transform.position + new Vector3(0, 0.5f, 0.45f), new Vector3(0.3f, 0.2f, 0.3f), M_Dark, player.transform);
            Object.DestroyImmediate(nose.GetComponent<Collider>());
            var pc = player.AddComponent<PlayerCharacter>();
            pc.cameraTransform = s_Camera.transform;
            Label(player, "CharacterController", WorldLabel.Info.None, 1.6f, 15);

            var follow = s_Camera.gameObject.AddComponent<FollowCamera>();
            follow.target = player.transform;
            follow.pitch = 22f;
            follow.distance = 8f;
            s_Camera.transform.position = new Vector3(0, 5, -16);
            s_Camera.transform.LookAt(player.transform);

            // rampy
            var gR = Group("Rampy");
            float[] ang = { 20f, 35f, 50f };
            for (int i = 0; i < ang.Length; i++)
            {
                float x = -10 + 4 * i;
                var r = Ramp($"Rampa_{ang[i]}st", new Vector3(x, 0, 2), 3f, 8f, ang[i], ang[i] > 45f ? M_Red : M_Ramp, null, gR);
                SectionLabel($"{ang[i]}°" + (ang[i] > 45 ? "\n> Slope Limit" : ""), r.TransformPoint(new Vector3(0, 1.2f, 1f)), gR);
            }

            // schody
            var gS = Group("Schody");
            float[] h = { 0.2f, 0.25f, 0.5f };
            for (int s = 0; s < h.Length; s++)
            {
                float x = 4 + 4 * s;
                for (int i = 0; i < 6; i++)
                {
                    float hh = h[s] * (i + 1);
                    Box($"Stopien_{h[s]}_{i + 1}", new Vector3(x, hh / 2f, 2.35f + 0.7f * i), new Vector3(3, hh, 0.7f),
                        h[s] > 0.3f ? M_Red : M_Ramp, gS);
                }
                SectionLabel($"stopnie {h[s]} m" + (h[s] > 0.3f ? "\n> Step Offset" : ""), new Vector3(x, 1.4f, 1.2f), gS);
            }

            // skrzynie do pchania
            var gB = Group("Skrzynie");
            for (int i = 0; i < 5; i++)
            {
                var b = Box("Skrzynia_" + (i + 1), new Vector3(-7 + 1.6f * i, 0.5f, -3), Vector3.one, M_Orange, gB);
                Body(b, 2f);
            }
            SectionLabel("Skrzynie z Rigidbody\n(pchane w OnControllerColliderHit)", new Vector3(-3.8f, 2.2f, -3), gB);
        }
    }
}
