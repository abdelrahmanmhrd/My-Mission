# Asset & Data Provenance Tracking
**Purpose:** every file, model, sound, font and dataset in this project must be traceable to its origin. If we did not write it ourselves, its license and attribution obligations are recorded here. This protects the team's Space Apps Challenge submission from disqualification over licensing issues.

**Golden rules**
1. Nothing enters `Assets/` without a row in the tables below.
2. For anything downloaded: record the **source URL**, **license**, **download date**, and **who downloaded it**.
3. If a license requires attribution, the credit must also appear in the in-game credits screen before submission.
4. Real-world engineering values are data, not assets — they still need their source recorded (per GDD §16 data integrity rules).
5. Never assume "it was on the internet so it's free" — check the license page at download time.

---

## 1. Code

| Item | Where | Origin | License | Notes |
|---|---|---|---|---|
| All C# scripts | `Assets/Scripts/**` | **Written by us** (this project) | Ours | GameDatabase, GameState, MissionConfig, EngineeringModel, ValidationEngine, GameManager, all UI scripts |
| Unity Pipeline package (`com.unity.pipeline`) | `Packages/` | Unity / tooling | Unity package license | Installed as part of dev environment, not shipped in the game |

## 2. Datasets (game data we authored)

| Item | Where | Origin | License | Notes |
|---|---|---|---|---|
| engines / fuels / vehicles / spacecraft / launchpads / instruments / missions / planets JSON | `Assets/Resources/Data/*.json` | **Written by us**; numeric values derived from public references listed in each file's `source` field | Ours | Real-world values are approximations for gameplay; each JSON entry carries a `source` string. See GDD §16 data-integrity rules |
| Source references for values | NASA planetary fact sheets (NSSDCA), NASA spacecraft fact sheets (Orion, Mars 2020 press kit, SLS, Saturn V news reference, ALSEP, Perseverance instrument pages), SpaceX/ULA public spec pages | Public info, **not copied files** | Facts are not copyrightable, but keep the citations | Cited in JSON `source` fields and `dataset_review.md` |
| Delta-v requirements (TLI ~3.2 km/s, TMI ~3.6 km/s etc.) | Standard mission-design references | **Written by us** as gameplay values | Ours | Labelled as gameplay approximations |

## 3. Unity-provided content (ships with engine/packages)

| Item | Where | Origin | License | Notes |
|---|---|---|---|---|
| TextMeshPro Essentials (LiberationSans SDF, shaders) | `Assets/TextMesh Pro/` | Imported from the official Unity uGUI package | Unity license | Required for all in-game text |
| Default URP settings/assets | `Assets/Settings/` | Unity URP template | Unity license | Project settings |
| Input System actions asset | `Assets/InputSystem_Actions.inputactions` | Unity template | Unity license | May be trimmed later |

## 4. Third-party art / models / audio — TO BE TRACKED

> **Status: NONE imported yet.** All 3D content currently in the game is procedural placeholder geometry (no external models).
> When the team's 3D models are delivered, add a row per file below **before** it is imported.

| Asset (file path) | Type | Origin (authored / downloaded) | Source URL | License | Attribution required? | Downloaded by / date | Used in |
|---|---|---|---|---|---|---|---|
| *(pending — team 3D models, rocket/spacecraft/pad)* | 3D model | TBD | TBD | TBD | TBD | TBD | Launch sim, selection previews |
| *(planned — Kenney space kit if used)* | 3D models/UI | Kenney.nl | https://kenney.nl/assets | CC0 (verify per pack) | No | TBD | Placeholders |
| *(planned — NASA 3D Resources models if used)* | 3D models | nasa.gov/3D-resources | per-model usage guidelines | NASA usage guidelines — **check each model** | Usually yes (credit NASA) | TBD | If used |
| *(planned — NASA image/video library if used)* | Textures/backgrounds | images.nasa.gov | per-asset usage | NASA guidelines — check each | Usually yes | TBD | Menus/space backgrounds |
| *(planned — Poly Haven if used)* | HDRI/textures | polyhaven.com | per-asset page | CC0 | No | TBD | Space environment |
| *(planned — Freesound if used)* | Audio | freesound.org | per-sound page | CC0/CC-BY/CC-BY — check each | CC-BY requires credit | TBD | Engine/launch/UI sounds |

## 5. NASA branding (GDD §13.3)

| Item | Where | Origin | License | Notes |
|---|---|---|---|---|
| NASA insignia / logo | *(not used yet)* | nasa.gov | NASA insignia usage rules — **restricted** | Only per official NASA media usage guidelines; must not imply endorsement. A NASA-*inspired* fictional interface is used instead |
| "NASA — MISSION DIRECTORATE" text styling | In-game briefing header | **Written by us** (fictional game fiction) | Ours | Fictional styling, no official insignia |

## 6. What the submission needs (checklist)

- [ ] Every third-party asset has a license row above
- [ ] All CC-BY (or similar) attributions collected into an in-game **Credits** screen
- [ ] NASA asset usage complies with NASA media usage guidelines (no implied endorsement)
- [ ] Data sources cited (JSON `source` fields + `dataset_review.md`)
- [ ] Confirm official challenge page submission rules (assets declaration, team info, project page)
- [ ] Final pass: search `Assets/` for any file without a tracking row

## 7. Process for adding a new external asset

1. Download from the official source; save the license page (PDF/screenshot) into `references/` folder.
2. Add a row in section 4 with URL, license, date, who, attribution requirement.
3. Import into `Assets/` (a folder like `Assets/Art/<source>/<name>/`).
4. If attribution is required → add to the credits screen immediately, not at the end.
5. If the license is unclear → **do not import**; find an alternative.
