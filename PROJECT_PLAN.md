# PROJECT PLAN — My Mission (Space Mission Design Game)
**Master context document.** Companion docs: `space_mission_design_game_gdd.md` (design baseline), `dataset_review.md` (component data + data integrity), `ASSET_TRACKING.md` (licenses/provenance), `SUBMISSION_ANSWERS.md` (submission form answers), `tests/GameTestSuite.cs` (regression suite, 107/107).
**Project:** NASA Space Apps Challenge 2026, Cairo · Unity 6000.6.8f1 · Single-player

---

## ✦ BUILD STATUS 2026-11-16 — SUBMISSION READY
- **Mac:** `version1/Builds/Mac/MyMission.app` (151 MB) — built headless, Succeeded
- **Windows:** `version1/Builds/Windows/MyMission.exe` (176 MB) — built headless, Succeeded
- Both include all gameplay: menus, briefing, 5-category configuration with 3D model previews, validation (incl. power/comms/orbital), scoring with ranks + count-up, debrief, unlocks, animated space background. The 3D launch sim scene ships but the LAUNCH button currently runs the deterministic verdict path (sim visuals pending).

## Session 2026-11-15/16 changes (all user-directed)
- **Real units only** — TU/MU/FU removed; kN/tonnes/km/USD everywhere; Settings units toggle removed
- **Power & communications constraints** (challenge features): spacecraft carry powerGenerationW + commCapacityGbPerDay; instruments' W and Gb/day must fit; shown in hub status + spacecraft details; POWER_OVERDRAW / COMM_OVERLOAD block validation. Perseverance rebalanced to 480 W / 8 Gb/day [TUNE]
- **Orbital constraint** surfaced as an Info line (TLI/TMI trajectory requirement)
- **Scoring fixed + gamified**: integer-consistent breakdown (parts sum exactly to the shown total), rank medals (S/A/B/C/D), animated count-up debrief, colored bars
- **Briefing restyled**: classification strip, directorate header, crewed/automated badge, aligned parameter rows
- **Debrief restyled**: rank banner, count-up, failure-reason panel, lessons, expenditure — AutoHeight everywhere (no overlap)
- **3D model preview viewports** in all selection pages (RenderTexture, rotating model): engines, vehicles, spacecraft, pads; procedural fuel-tank stand-ins (colored per fuel); procedural instrument modules
- **Stars everywhere**: two parallax star layers (900+350, random sizes, continuous drift + wrap) behind every screen; Earth + ISS orbit animation; solid readable backdrops on all text screens (credits, info, settings, mission select, briefing, debrief)
- **EXIT → main menu** (config hub)
- **Card status logic fixed**: SELECTED only when the category actually has selections (was: engines/fuels showed SELECTED merely because a vehicle existed); PARTIAL hint state; compact no-clip descriptions; "start with the ROCKETS card" hint
- **Scene corruption fixed**: duplicate GameManager/UIRoot components + stray empty GameBoot removed from SampleScene
- **Deferred-destroy catalog bug fixed** (wrong-selection bug): all rebuild loops use DestroyImmediate — no stale duplicate items
- **Test suite**: 107/107 (units tests updated for real-only; Mars set rebalanced)

## Build pipeline (repeatable)
- Build script: `Assets/Editor/BuildScript.cs` (BuildMac / BuildWindows)
- Close the interactive editor first (graceful `editor quit --save`) — two instances can't share the project
- `Unity -batchmode -nographics -quit -buildTarget StandaloneOSX   -executeMethod SpaceMissionEditor.BuildScript.BuildMac -projectPath ... -logFile ...`
- same with `-buildTarget StandaloneWindows64 ... BuildWindows`
- Windows module installed 2026-11-16 (windows-mono via Hub CLI)

---

## 1. Locked design decisions (all confirmed with the team)

| Topic | Decision |
|---|---|
| Game name | **My Mission** |
| Data | I draft JSON datasets with real values + source notes; team reviews (`dataset_review.md`) |
| Money | **Always USD ($M)** — never converted to game units |
| Units | Settings toggle: game units (TU/MU/FU, default) ↔ real units (kN/kg/t) |
| Instruments | Fifth config card; some are **required per mission** (SHERLOC for Mars 2020; MOXIE for all crewed missions) |
| Instrument effects | Mastcam-Z missing → flight camera locked to fixed position; MOXIE missing on crewed mission → life-support disaster (may end the simulation) |
| Failure severity | Defined per mission: block / critical / soft — not a global rule |
| Staging | Vehicle defines its stages (data-driven); ENGINES + FUELS cards assign per stage; fuel amount player-chosen (slider, defaults full) |
| Crew size | Fixed by mission (Apollo 3, Artemis III 4 — matches real roster size) |
| Power/comms | Not in v1 |
| Incompatibility | No pre-launch blocking of "bad" combos beyond validation rules — underfueled/underspecified configs LAUNCH and fail in the simulation; the engineering layer already knows the outcome deterministically, the sim is the visual presentation |
| Progression | Sequential unlock (Apollo → Mars 2020 → Artemis III), persisted (PlayerPrefs) |
| Scores | Local best score per mission on mission select |
| Ascent length | Compressed to 30–45 s; interplanetary travel 10–20 s |
| Spacecraft dv | Precomputed real capability fields (arrivalDvMS / returnDvMS), not propellant math (spacecraft aren't staged in-game) |
| Artemis III architecture | "Orion + Human Landing System" stack (45 t) — requires a Saturn V-class TLI vehicle; SLS alone cannot lift it (real 27 t limit, educational) |
| Departure dv budgets | Model-calibrated gameplay values: lunar 5,500 m/s, Mars 5,000 m/s (documented in dataset_review §8) |
| UI framework | uGUI (programmatic dark control-room theme, `UITheme`) |
| Scenes | Single main scene + UI panels; 3D sim in separate additive scenes |
| NASA branding | Fictional wordmark styling only; no official insignia |
| Simulation outcome | Deterministic from engineering layer; sim visualizes the verdict |

## 2. What is built and verified (all green)

- **Data layer** — 8 JSON datasets loaded via `Resources/Data` + `GameDatabase`
- **Engineering model** (`EngineeringModel`) — rocket-equation stage dv, tank-mass factors, TWR, budget, payload capacity, science/power/data totals
- **Validation engine** — deterministic readiness (NOT READY / VALIDATION REQUIRED / READY FOR LAUNCH), severity-coded issues with player-facing reasons + hints
- **UI screens** — main menu, mission select (locks + best scores), briefing (classified doc), config hub (5 cards + status bar + BRIEF/EXIT), generic selection page (engines w/ stage assignment, fuels w/ amount slider, rockets = vehicle+spacecraft tabs, launchpads, payload), settings (units + audio sliders + CREDITS & LICENSES button), info/encyclopedia, validation modal, debrief screen, credits screen
- **Scoring system** (`ScoringModel`) — deterministic outcome (departure/arrival/return shortfalls → phase-specific failure reason + lesson), provisional GDD weights (success 35 / science 25 / efficiency 20 / reliability 20), letter grades, budget summary
- **Launch gate + flow** — LAUNCH always clickable; blocking errors → validation modal; otherwise `GameManager.RunMission()` → debrief. Success unlocks the next mission + submits best score. RETRY returns to the hub with the configuration intact
- **Credits screen** — authored content, engine/tool licenses, data sources, no-endorsement branding note (mirrors ASSET_TRACKING.md; extend when assets land)
- **Fixes verified in play** — fuel-unit bug, liftoff double-count, OnEnable construction NREs, transient widget leaks, empty-stage crashes, GameDatabase null guards
- **Test suite** — `tests/GameTestSuite.cs`, 108/108 passing (data integrity, engineering math, validation rules, outcome prediction, scoring, progression, units)

### WebGL smoke test #1 — baseline (2026-11-13)

| Metric | Result |
|---|---|
| Build result | **Succeeded**, 0 errors, 12 warnings (all benign: editor renderer quirks, stripped optional FSR shader, IL2CPP informational notes, one deprecated API since fixed) |
| Compressed size | **11.0 MB** (wasm 7.5 MB + data 3.4 MB + code 0.07 MB, Brotli) |
| Build time | ~26 min (first switch + AOT) |
| Serve test | index.html / loader / wasm.br / data.br all HTTP 200 via `python3 -m http.server` in `Builds/WebGL` |
| How to test | serve `Builds/WebGL` with any static server and open `http://localhost:8600` |
| Notes | wasm size includes unused packages (splines, cinemachine, collections MonoScript references) — trimming them later will shrink the download. Real browser playtest pending; models will grow data.br (budget: keep compressed build ≤ 50 MB) |

## 3. Roadmap (approved order)

| # | Task | Owner | Status |
|---|---|---|---|
| 1 | **Scoring system + debrief screen** (before 3D sim) | Agent | in progress |
| 2 | **Credits screen** — Credits button inside Settings | Agent | approved |
| 3 | **Failure→retry flow** — must-have: after failure return to configuration, failure cause explained so the player learns | Agent | approved |
| 4 | **WebGL smoke test** — checkpoints: (a) after scoring lands, (b) after models, (c) after audio | Agent | approved |
| 5 | **3D model integration** — team delivers models to a folder; agent imports + wires | Team → Agent | awaiting models |
| 6 | **3D simulation** — launch/ascent (30–45 s), Earth orbit, trajectory view (solid actual / dotted planned, telemetry), compressed interplanetary travel (10–20 s), arrival + outcome; camera gating: Mastcam-Z missing → fixed camera (must-have) | Agent | approved |
| 7 | **Test-suite extension** — cover scoring, sim determinism, model wiring | Agent | after sim |
| 8 | **UI graphics** — background (main menu Earth/ISS/Moon) | Team → Agent | awaiting assets |
| 9 | **Audio** — music/ambience + SFX (launch, ignition, UI) with licenses recorded | Team → Agent | awaiting assets |
| 10 | Final polish + submission build (WebGL) + credits check | Agent | last |

**Out of scope (v1):** multiplayer, random in-flight events, upkeep costs, optimization add-on, power/comms systems, player-chosen crew size.

## 4. Model delivery expectations (for the team)

- Format: FBX or OBJ, consistent scale (state if meters), one folder per source
- `licenses.txt` per folder (source URL + license) — required by ASSET_TRACKING before import
- Suggested names to match data IDs: `saturn_v`, `falcon_9`, `atlas_v541`, `sls`, `orion_lander`, `perseverance`, `launchpad_39a`, `launchpad_39b`, `launchpad_41`, `launchpad_40`
- Web target ⇒ keep textures ≤ 2K where possible; heavy assets will be compressed

## 5. How to run / test

- **Play in editor:** press Play (boot scene builds all UI at runtime)
- **Regression suite:** `unity command script run --file ../tests/GameTestSuite.cs --entry SpaceMissionTesting.GameTestSuite.RunAll` (run from `version1/version1/`; keep 80+/80 green after any change)
- **WebGL smoke test:** switch build target → build → load locally; record size/errors in this file
- **Verified Apollo reference config:** Saturn V (F-1×5 kerolox 2150 FU / J-2×5 hydrolox 444 / J-2×1 hydrolox 106) + CSM+LM + LC-39A + MOXIE + ALSEP → TWR 1.11, 12,461 m/s, $648M, READY FOR LAUNCH
