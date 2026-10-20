# Fizyka 3D w Unity – sceny demonstracyjne

Projekt Unity **6.3 LTS (6000.3.24f1)** pokazujący różne aspekty fizyki 3D (PhysX) w Unity: masę, opory, tryby sił, kolidery, materiały fizyczne, zdarzenia kolizji, triggery, wykrywanie kolizji, interpolację, ograniczenia osi, usypianie ciał i CharacterController.

Każde zagadnienie ma **osobną scenę** (`01_…` – `16_…`). W każdej scenie jest panel z opisem po polsku, etykiety z wartościami na żywo (prędkość, ω, wysokość, stan uśpienia…) i – tam, gdzie to ma sens – dziennik zdarzeń.

- Render pipeline: **URP** (szablon „Universal 3D”)
- Wejście: **Input System** (nowy), wyłącznie akcje z domyślnego pliku `Assets/InputSystem_Actions.inputactions` (Project-wide Actions) – bez własnych map i akcji
- UI: uGUI (`Canvas` + `Text`) – opisy są widoczne w hierarchii sceny (`UI_Opis`)

---

## 1. Szybki start

### Wariant A – w edytorze
1. Otwórz projekt w Unity Hub (wersja 6000.3.24f1) albo uruchom `Narzedzia/Otworz_projekt_w_Unity.bat`.
2. Sceny są już wygenerowane w `Assets/Fizyka/Scenes`. Gdyby ich brakowało – w górnym menu wybierz **Fizyka → Generuj wszystkie sceny**.
3. **Fizyka → Otwórz pierwszą scenę i uruchom Play** (albo otwórz dowolną scenę i naciśnij Play).
4. Klawiszami **1 / 2** przechodzisz do poprzedniej / następnej sceny (działa także w buildzie).

> Przy pierwszym otwarciu Unity może pokazać okno „URP Material upgrade” – kliknij **OK** (to jednorazowa aktualizacja wersji materiałów).

### Wariant B – gotowy build Windows
1. Uruchom `Narzedzia/Generuj_sceny_i_zbuduj_Windows.bat` (edytor z tym projektem musi być zamknięty). Skrypt w trybie wsadowym generuje sceny i buduje grę do `Builds/Windows/Fizyka3D.exe`. Log: `Logs/batch_build.log`.
2. Uruchom `Builds/Windows/Fizyka3D.exe` albo `Narzedzia/Uruchom_build_z_logiem.bat` (okno 1600×900, log w `Logs/player.log`).

---

## 2. Menu „Fizyka” w edytorze

| Polecenie | Działanie |
|---|---|
| **Generuj wszystkie sceny** | Tworzy od nowa 16 scen, materiały, Physics Materials, siatkę „Kubek” i ustawia Build Settings |
| **Panel scen (otwieranie i generowanie)** | Okno z listą scen: *Otwórz*, ▶ (otwórz i Play), ↻ (wygeneruj tylko tę scenę) |
| **Otwórz pierwszą scenę i uruchom Play** | Skrót do szybkiego pokazu |
| **Zbuduj wersję Windows (x64)** | Build do `Builds/Windows/Fizyka3D.exe` |
| **Napraw ustawienia projektu** | Ustawia *Active Input Handling = Input System Package* i *Project-wide Actions* oraz listę scen w Build Settings |
| **Otwórz instrukcję (README.md)** | Otwiera ten plik |

Sceny są generowane kodem (`Assets/Fizyka/Editor/SceneDefinitions_*.cs`), więc nie trzeba niczego ustawiać ręcznie. **Uwaga:** ponowne generowanie nadpisuje ręczne zmiany w scenach. Jeśli chcesz eksperymentować – zmieniaj wartości w Inspectorze w trakcie Play albo zrób kopię sceny.

### Tryb wsadowy (linia poleceń)
```bat
"C:\Program Files\Unity\Hub\Editor\6000.3.24f1\Editor\Unity.exe" -batchmode -projectPath "<ścieżka projektu>" ^
  -buildTarget Win64 -executeMethod Fizyka.EditorTools.SceneBuilder.BatchGenerateAndBuildWindows -logFile build.log
```
`Fizyka.EditorTools.SceneBuilder.BatchGenerate` – tylko generowanie scen.

---

## 3. Sterowanie (domyślne akcje Input System)

| Klawisz / mysz | Akcja z `InputSystem_Actions` | Działanie w demie |
|---|---|---|
| **Spacja** | Player/Jump | akcja główna sceny (start eksperymentu, strzał, zapadnia…) |
| **LPM** (lub Enter) | Player/Attack | „szturchnięcie” obiektu pod kursorem – impuls 4 N·s (`AddForceAtPosition`, `ForceMode.Impulse`) |
| **PPM + ruch myszy** | UI/RightClick + Player/Look | obrót kamery |
| **W A S D / strzałki** | Player/Move | ruch kamery (w scenie 16 – ruch postaci) |
| **Shift** | Player/Sprint | szybszy ruch kamery / bieg postaci |
| **Kółko myszy** | UI/ScrollWheel | przybliżanie / oddalanie |
| **C** | Player/Crouch | pokaż / ukryj panel opisu |
| **przytrzymaj E** | Player/Interact (Hold) | reset sceny |
| **1 / 2** | Player/Previous / Player/Next | poprzednia / następna scena |
| **Esc** | UI/Cancel | wyjście (tylko w buildzie) |

Kod: `Assets/Fizyka/Scripts/Core/DemoInput.cs` – akcje są pobierane z `InputSystem.actions.FindAction("Player/Jump")` itd.

---

## 4. Opis scen

| Scena | Co pokazuje | Akcja (Spacja) |
|---|---|---|
| **01_Masa** | A: kule 1/10/100 kg spadają razem (g nie zależy od masy). B: ten sam impuls 10 N·s → Δv = J/m (10; 2; 0,4 m/s). C: zderzenia sprężyste 1→1, 1→10, 10→1 kg | zapadnia, impulsy, zderzenia |
| **02_LinearDamping** | opór ruchu: lot poziomy bez grawitacji (damping 0; 0,5; 2; 5) i spadek z 20 m – prędkość graniczna ≈ g/damping | start |
| **03_AngularDamping** | wirujące klocki (damping 0…2) + limit `maxAngularVelocity` = 7 rad/s; toczące się kule – angular damping jak „opór toczenia” | obrót / pchnięcie |
| **04_Gravity_Kinematic_Static** | Dynamic z grawitacją, Dynamic bez grawitacji, Is Kinematic, Static (bez Rigidbody); kinematyczne ramię (MoveRotation) zmiata klocki i przenika przez statyczny słupek | strzał w cele |
| **05_AddForce_ForceMode** | Force, Acceleration, Impulse, VelocityChange dla m = 1 i 4 kg – porównanie prędkości końcowych | AddForce |
| **06_AddTorque_ForceMode** | to samo dla obrotu; rolę masy pełni moment bezwładności I (mały i duży klocek) | AddTorque |
| **07_Collidery** | Box, Sphere, Capsule, Mesh Collider (Convex), sześcian z Sphere Colliderem, collider złożony; walec z Capsule vs Mesh Collider; kubek non-convex vs convex | półka, walce, kulki |
| **08_PhysicsMaterial** | tarcie statyczne/dynamiczne na rampie 25°, bounciness 0…1, tryby Friction Combine (Average/Minimum/Multiply/Maximum) | blokada, kule, pchnięcie |
| **09_OnCollision** | OnCollisionEnter/Stay/Exit (licznik, impuls, prędkość względna); kinematyczny klocek przez statyczną ścianę – brak zdarzeń | zrzut kulki |
| **10_Triggery** | Is Trigger: OnTriggerEnter/Stay/Exit, licznik obiektów w strefie, „komin” z siłą w OnTriggerStay | zrzut kulek |
| **11_CollisionDetection** | Discrete vs Continuous vs Continuous Dynamic vs Continuous Speculative – pociski 120 m/s i ściana 10 cm (tunelowanie) | strzał |
| **12_Interpolate** | None / Interpolate / Extrapolate przy kroku fizyki 0,1 s | przełącz 0,1 s ↔ 0,02 s |
| **13_Constraints** | Freeze Position / Freeze Rotation – 5 konfiguracji (m.in. „2.5D”, drzwi obrotowe) | strzał w klocki |
| **14_Sleeping** | IsSleeping, sleepThreshold (0,005 vs 1,0), wieża klocków zasypia i budzi się | pchnięcie kul |
| **15_Velocity_vs_VelocityChange** | `AddForce(VelocityChange)` (sumuje) vs `linearVelocity =` (nadpisuje) vs `MovePosition` (kinematyk) vs `transform.position` (źle!) | ruch – naciśnij kilka razy |
| **16_CharacterController** | Slope Limit (rampy 20°/35°/50°), Step Offset (stopnie 0,2/0,25/0,5 m), isGrounded, pchanie skrzyń w OnControllerColliderHit | – (Spacja = skok) |

---

## 5. Jak to działa (architektura)

```
Assets/
  InputSystem_Actions.inputactions   ← domyślne akcje (Project-wide Actions)
  Settings/                          ← ustawienia URP z szablonu
  Fizyka/
    Scenes/            01_Masa.unity … 16_CharacterController.unity (generowane)
    Materials/         materiały URP (kolory, szachownica podłogi)
    PhysicsMaterials/  Physics Materials (Lod, Guma, Drewno, Sprezysty_bezTarcia, Odbicie_x, Lod_<Combine> …)
    Meshes/            Kubek.asset (wklęsła siatka do porównania Mesh Collider convex/non-convex)
    Scripts/Core/      DemoInput, DemoHud, DemoActionBase, FreeCamera, PokeTool, WorldLabel
    Scripts/Actions/   komponenty eksperymentów (opis niżej)
    Editor/            SceneBuilder (menu Fizyka, build), SceneDefinitions_01_08/09_16, SceneBrowserWindow
Narzedzia/            pliki .bat (generowanie + build, otwarcie projektu, uruchomienie buildu)
```

**Rdzeń**
- `DemoInput` – dostęp do akcji z `InputSystem.actions` (mapy *Player* i *UI*).
- `DemoHud` – panel opisu, pasek podpowiedzi, dziennik zdarzeń, globalne skróty (Spacja, E, C, 1/2, Esc).
- `DemoActionBase` – klasa bazowa: każdy komponent dziedziczący reaguje na Spację (`OnMainAction`).
- `FreeCamera`, `PokeTool` (LPM), `WorldLabel` (etykiety z parametrami Rigidbody na żywo).

**Komponenty eksperymentów** (widoczne w Inspectorze, można zmieniać parametry)
- `ApplyOnAction` – `AddForce` / `AddTorque` w wybranym `ForceMode` (tryby ciągłe przez *duration* sekund w `FixedUpdate`).
- `DisableOnAction` – zapadnia / blokada znikająca po Spacji; `ReleaseOnAction` – `isKinematic = false` po Spacji.
- `DemoLauncher` – wyrzutnia kulek (prędkość, masa, Collision Detection, sprawdzanie tunelowania).
- `KinematicMover` – `MovePosition` / `MoveRotation` kinematycznego ciała.
- `CollisionReporter`, `TriggerZoneReporter`, `UpdraftZone` – zdarzenia kolizji i triggerów.
- `FixedStepToggle` – zmiana `Time.fixedDeltaTime` (scena 12; przywracane po wyjściu ze sceny).
- `SleepIndicator`, `RigidbodySetup` (`maxAngularVelocity`, `sleepThreshold` – te pola nie są zapisywane w scenie przez sam Rigidbody).
- `VelocityMethodDemo` – cztery sposoby nadawania ruchu (scena 15).
- `PlayerCharacter`, `FollowCamera` – CharacterController i kamera trzecioosobowa (scena 16).

---

## 6. Uwagi dotyczące Unity 6

- Nazwy w API: `rb.linearDamping` (dawniej `drag`), `rb.angularDamping` (dawniej `angularDrag`), `rb.linearVelocity` (dawniej `velocity`), klasa **`PhysicsMaterial`** (dawniej `PhysicMaterial`), `PhysicsMaterialCombine`.
- Priorytet trybów Combine: **Average < Minimum < Multiply < Maximum** – przy dwóch różnych trybach wygrywa wyższy.
- Podłoga w scenach ma jawny Physics Material `Podloga` (0,6 / 0,6, Average). W testach tego projektu (Unity 6000.3.24f1) kontakt z colliderem **bez** przypisanego materiału dawał tarcie uśrednione (Average) mimo trybu Minimum na drugim obiekcie – dlatego wszystkie powierzchnie mają przypisany materiał.
- Domyślny `maxAngularVelocity` = 7 rad/s (scena 03 pokazuje skutki).
- Zalecany stosunek mas zderzających się ciał to mniej niż ok. 1:100 (stabilność solvera).

---

## 7. Rozwiązywanie problemów

| Problem | Rozwiązanie |
|---|---|
| Klawisze nie działają | **Fizyka → Napraw ustawienia projektu** (Active Input Handling = Input System, Project-wide Actions) i zrestartuj edytor |
| Brak scen / 1–2 nie przełączają | **Fizyka → Generuj wszystkie sceny** (dodaje sceny do Build Settings) |
| Różowe obiekty | projekt musi używać URP (Edit → Project Settings → Graphics → `PC_RPAsset`) |
| `.bat` kończy się błędem | zamknij edytor z tym projektem; sprawdź `Logs/batch_build.log` |
| Okno „URP Material upgrade” | kliknij OK – jednorazowa aktualizacja materiałów |

---

Materiały źródłowe: wykłady PGWSU 2025/26 (fizyka w Unity: Rigidbody, Collidery, Physics Material, OnCollision/OnTrigger, AddForce/AddTorque, CharacterController).
