# MY MISSION — Space Mission Design Game
*NASA Space Apps Challenge 2026 — educational engineering / strategy simulator*

You are the mission director. Pick a real mission — land a crew on the Moon with
Apollo, deliver the Perseverance rover to Mars, or return humans to the lunar
south pole with Artemis III — then engineer it: launch vehicle, engines and fuel
per rocket stage, spacecraft, launch facility, and scientific instruments, all
inside a fixed budget and real physical constraints. Real NASA data powers every
value; the game computes whether your design can fly and shows you exactly why
it succeeds or fails.

---

## 1. Play the game

> **Note:** the prebuilt game apps (150–180 MB binaries) are not stored in this git
> repository (GitHub size limits); they ship with the submission package. To run the
> game from source instead, see section 4 — open the project in Unity and press Play.

### Windows
1. Open the `Builds/Windows` folder.
2. Double-click **MyMission.exe**.
3. If Windows SmartScreen appears ("Windows protected your PC"), click
   **More info → Run anyway** — the app is unsigned (a hackathon build), not unsafe.

### macOS
1. Open the `Builds/Mac` folder.
2. Move **MyMission.app** to your Applications folder (optional but recommended —
   Gatekeeper is stricter on apps run from the same folder they were downloaded to).
3. The app is unsigned, so macOS Gatekeeper will block it on first run:
   **right-click MyMission.app → Open → Open** (one time only).
4. If your Mac blocks it entirely: System Settings → Privacy & Security → scroll down → **Open Anyway**.

> Both builds are self-contained — no installers, no dependencies. ~150–180 MB.

---

## 2. How to play (2-minute guide)

1. **START** → pick a mission (Apollo tutorial unlocks first; missions unlock in sequence as you win).
2. Read the **briefing** — destination, budget, crew, required instruments.
3. Configure your mission through the five cards:
   - **ROCKETS** — start here: choose the launch vehicle and the spacecraft.
   - **ENGINES** — assign one engine type per vehicle stage (thrust must beat gravity).
   - **FUELS** — pick a propellant and load amount per stage (more fuel = more range, more mass, more cost).
   - **LAUNCHPADS** — the facility must support your vehicle (crewed missions need crew access).
   - **PAYLOAD** — instruments add science value but cost money, mass, power and data bandwidth.
4. **VALIDATE** — every blocking issue is explained in plain language.
5. **LAUNCH** — get your verdict, score, rank (S/A/B/C/D) and a debrief that teaches
   what to change. **RETRY** keeps your configuration so you can iterate.

**The rules that matter:** heavier payloads need more fuel and thrust; incompatible
combinations fail; instruments must fit the spacecraft's power and downlink budget;
crewed missions need oxygen generation and human-rated vehicles. Multiple valid
designs exist — there is no single right answer.

---

## 3. Where everything is (file system)

```
mymission/version1/                     ← project root
│
├── README.md                           ← this walkthrough
├── LICENSE.md                          ← proprietary license (our code)
├── THIRD_PARTY_NOTICES.md              ← third-party attributions (CC-BY / NASA)
├── PROJECT_PLAN.md                     ← master context: decisions, state, build pipeline
├── ASSET_TRACKING.md                   ← provenance table for every asset
├── SUBMISSION_ANSWERS.md               ← copy-paste answers for the challenge form
├── space_mission_design_game_gdd.md    ← the game design document (GDD)
├── dataset_review.md                   ← component data + sources + integrity rules
│
├── Builds/                             ← THE PLAYABLE GAMES
│   ├── Mac/MyMission.app               ← macOS build (ships with submission package,
│   └── Windows/MyMission.exe           ←  not in this repo — binary size; build via §4)
│
├── version1/                           ← the Unity project (open this folder in Unity)
│   ├── Assets/
│   │   ├── Scenes/                     ← SampleScene (main, builds all UI at runtime) + SimLaunch
│   │   ├── Scripts/
│   │   │   ├── Core/                   ← GameManager, GameDatabase, settings, progress, ModelLibrary
│   │   │   ├── State/                  ← MissionConfig (the shared configuration)
│   │   │   ├── Engineering/            ← EngineeringModel (physics/math), ValidationEngine, ScoringModel
│   │   │   ├── UI/                     ← all screens + theme + 3D preview viewports + space background
│   │   │   ├── Simulation/             ← launch/ascent simulation (visual polish pending)
│   │   │   └── Editor/                 ← BuildScript (Mac/Windows build pipeline)
│   │   ├── Resources/
│   │   │   ├── Data/                   ← the game database: engines/fuels/vehicles/spacecraft/
│   │   │   │                              launchpads/instruments/missions/planets (JSON, source-cited)
│   │   │   ├── Models/                 ← imported 3D models (FBX, decimated for performance)
│   │   │   └── Textures/               ← Earth/Moon textures
│   │   ├── Art/                        ← textures + planet models (source copies)
│   │   └── TextMesh Pro/               ← font assets (Unity)
│   ├── Packages/manifest.json          ← package dependencies
│   └── ProjectSettings/
│
├── tests/GameTestSuite.cs              ← 107-check regression suite (run via unity CLI)
├── 3dmodels/                           ← original delivered model files (source, not in the build)
│                                         (the ~700 MB of largest originals — NASA Project.zip,
│                                         F1 engine, SLS, ISS — exceed GitHub file-size limits
│                                         and are excluded from this repo; available on request)
├── credits/ + references/              ← license texts for every downloaded asset
└── myMission2/                         ← (scratch folder — can be deleted)
```

---

## 4. Open the project in Unity (for development)

1. Install **Unity Hub** + **Unity 6000.6.8f1** (with Windows/Mac build support if you want to build).
2. Unity Hub → **Add** → select the `version1/version1` folder (the one containing `Assets/`).
3. Open it. First import takes a few minutes; after that it's incremental.
4. Press **Play** — the whole UI builds itself at runtime (the scene is nearly empty by design).

### Rebuild the game apps
With the editor closed (two instances can't share a project), run from `version1/version1`:

```
# Mac
/Applications/Unity/Hub/Editor/6000.6.8f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -buildTarget StandaloneOSX \
  -executeMethod SpaceMissionEditor.BuildScript.BuildMac \
  -projectPath "$(pwd)" -logFile /tmp/build-mac.log

# Windows (identical, but -buildTarget StandaloneWindows64 and BuildWindows)
```

### Run the test suite
```
unity command script run --file ../tests/GameTestSuite.cs \
  --entry SpaceMissionTesting.GameTestSuite.RunAll
```
(107 checks: data integrity, engineering math, validation rules, scoring, progression, units.)

---

## 5. Design & data integrity

- **The program already knows the outcome.** The engineering layer deterministically
  decides mission success from real physics (rocket-equation delta-v, thrust-to-weight,
  budget, compatibility, power and comms budgets); the 3D simulation visualizes that verdict.
- **Every value is sourced.** Real numbers come from public NASA fact sheets (see the
  `source` field in every JSON entry); gameplay approximations are labeled as such.
- **Licensing is clean.** Our code is proprietary (LICENSE.md); third-party assets are
  CC-BY (credited in-game) or NASA public domain (THIRD_PARTY_NOTICES.md); no NASA
  insignia is used — the agency-style interface is fictional.
- **SI units everywhere** — kN, tonnes, km, USD.

## 6. Known status (honest list)

- ✔ Full game loop: configure → validate → verdict → score/rank → unlock → retry
- ✔ 3D model previews, animated space background (Earth + ISS + drifting starfield)
- ✔ Power / communications / orbital constraints, instrument requirements, camera-gating rule
- ◔ 3D launch simulation: the scene, rocket assembly, effects and telemetry exist and run,
  but LAUNCH currently presents the deterministic verdict directly — the full animated
  ascent is the next polish item.
- Empty: audio (no sound files delivered yet), instruments use procedural stand-in models.

---

*Built by the My Mission team for the NASA Space Apps Challenge 2026 (Cairo).*
*This is an educational fan-made simulation — not produced by, endorsed by, or affiliated with NASA.*
