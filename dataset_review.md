# Dataset Review Draft — Space Mission Design Game
**Status: DRAFT for review — no code written. All values are real-world approximations from public NASA/fact-sheet data; gameplay balance values are marked `[TUNE]`.**

---

## 0. Proposed unit normalization (applies everywhere)

| Real unit | Game unit | Conversion |
|---|---|---|
| kN (thrust) | **TU** | 1 TU = 100 kN |
| kg (mass) | **MU** | 1 MU = 100 kg |
| tonnes (fuel) | **FU** | 1 FU = 1 tonne of fuel |

- **Budget/money is NOT converted** — always displayed in USD ($M), in both game-units and real-units modes.
- Default display = game units; Settings toggle switches to real units (per your decision).
- Example: Merlin 1D thrust 845 kN → **8.5 TU**; mass 470 kg → **4.7 MU**.

---

## 1. ENGINES (6 entries)

Real values: sea-level (SL) / vacuum (vac) thrust in kN, vacuum specific impulse (Isp, sec), dry mass kg.

| ID | Name | Fuel | Thrust SL (kN) | Thrust vac (kN) | Isp vac (s) | Mass (kg) | Stage role | Cost `$M [TUNE]` |
|---|---|---|---|---|---|---|---|---|
| ENG-F1 | F-1 | RP-1/LOX | 6,770 | 7,770 | 304 | 8,400 | Booster only | 40 |
| ENG-J2 | J-2 | LH2/LOX | 1,000* | 1,033 | 421 | 1,788 | Upper (restartable) | 22 |
| ENG-MERLIN | Merlin 1D | RP-1/LOX | 845 | 934 | 311 | 470 | Booster | 9 |
| ENG-MERVAC | Merlin Vacuum | RP-1/LOX | — | 981 | 348 | 470 | Upper | 10 |
| ENG-RD180 | RD-180 | RP-1/LOX | 3,830 | 4,150 | 338 | 5,480 | Booster only | 28 |
| ENG-RS25 | RS-25 | LH2/LOX | 1,860 | 2,279 | 452 | 3,527 | Booster (core) | 55 |
| ENG-RL10 | RL10C-1 | LH2/LOX | — | 106 | 449 | 301 | Upper | 14 |
| ENG-AJ10 | AJ10-137 | MMH/NTO | — | 92 | 314 | 260 | Spacecraft main engine | 8 |

*Notes*
- Engine↔fuel compatibility is automatic: each engine lists exactly one fuel type; the FUELS card shows which engines consume it.
- Stage-role rule: a booster engine can't be assigned to an upper-stage slot (prevents absurd configs); restartable engines (J-2) are required for stages that burn twice (e.g., Saturn V TLI).
- Values sourced from NASA engine fact sheets / public contractor data; J-2 SL thrust is theoretical (ignited in vacuum).

---

## 2. FUELS (4 entries)

| ID | Name | Real propellant | Isp behavior | Density | Storage | Cost per FU `[TUNE]` |
|---|---|---|---|---|---|---|
| FUEL-KEROLOX | Kerosene / LOX | RP-1 + liquid oxygen | Standard Isp | High (dense) — smaller tanks | Standard | 2 |
| FUEL-HYDROLOX | Hydrogen / LOX | LH2 + liquid oxygen | +15–20% Isp on compatible engines | Very low — large, heavy tanks | Cryogenic, boil-off penalty | 4 |
| FUEL-METHALOX | Methane / LOX | CH4 + liquid oxygen | +8–12% Isp | Medium | Cryogenic | 3 |
| FUEL-HYPERGOLIC | Hypergolic storable | MMH + NTO | −10% Isp | High | Storable, no cryo issues | 6 |

*Notes*
- Fuel choice changes engine Isp/performance and tank mass factor (hydrolox tanks are heavier per FU due to low density — this is the educational trade-off).
- Hypergolic is expensive but reliable/storable — good for spacecraft main engines (AJ10).
- Methalox exists for future Starship/sandbox content; no current mission needs it.

---

## 3. ROCKETS / VEHICLES (4 launch vehicles) + SPACECRAFT (4 entries)

The ROCKETS card opens a two-part selection: **Launch Vehicle** (defines stages & engine slots) + **Spacecraft** (payload that travels to destination).

### 3.1 Launch vehicles

| ID | Vehicle | Stages | Engine slots | Payload to LEO (t) | Payload to Moon/TLI (t) | Payload to Mars/TMI (t) | Crew-rated | Cost `$M [TUNE]` |
|---|---|---|---|---|---|---|---|---|
| LV-F9 | Falcon 9 | 2 | S1: 9× booster / S2: 1× upper | 22.8 | ~4 | ~4 | Yes | 90 (expended) |
| LV-ATLAS541 | Atlas V 541 | 2 + 5 fixed solid boosters | S1: 1× booster (solids fixed) / S2: 1× upper | 17.4 | ~2.5 | 4.1 | Yes (Starliner heritage) | 110 |
| LV-SATV | Saturn V | 3 | S1: 5× booster / S2: 5× upper / S3: 1× upper (restartable) | 140 | 48.6 | ~15 | Yes | 400 |
| LV-SLS | SLS Block 1 | 2 + 2 fixed solid boosters | S1: 4× booster (solids fixed) / S2: 1× upper | 95 | 27 | — | Yes | 800 |

*Notes*
- Fixed solid boosters (Atlas/SLS) are part of the vehicle data, not player-configurable — keeps staging UI manageable.
- Player assigns one engine per slot from the ENGINES card; slot must match stage role and the vehicle's fuel-family where noted (e.g., SLS core only accepts LH2/LOX engines).
- "Payload to destination" is the capacity the player's spacecraft+fuel mass must fit under — this is the mass validation core.

### 3.2 Spacecraft

| ID | Spacecraft | Crew | Mass (t) | Automated/Crewed | Cost `$M [TUNE]` | Required vehicle capability |
|---|---|---|---|---|---|---|
| SC-CSMLM | Apollo CSM + Lunar Module | 3 | 45 | Crewed | 120 | TLI ≥ 48 t → Saturn V only |
| SC-ORION | Orion (Crew Module + Service Module) | 4 | 26.5 | Crewed | 180 | TLI ≥ ~30 t (Orion + instruments + margin) |
| SC-PERSAV | Perseverance rover package (cruise + descent + rover) | 0 | 4.0 | Automated | 60 | TMI capability, any vehicle ≥ 4.0 t |
| SC-ORBGEN | Automated science orbiter (MAVEN-class) | 0 | 2.5 | Automated | 40 | TMI capability, any vehicle ≥ 2.5 t |

*Compatibility rules*
- Crewed missions can only select crewed spacecraft (and vice versa).
- Spacecraft main-engine (AJ10 slot) comes with crewed spacecraft automatically — no extra player pick (keeps scope lean).

---

## 4. LAUNCHPADS (4 entries)

| ID | Pad | Location | Compatible vehicles | Cost `$M [TUNE]` |
|---|---|---|---|---|
| PAD-39A | LC-39A | Kennedy Space Center, FL | Saturn V, Falcon 9 | 25 |
| PAD-39B | LC-39B | Kennedy Space Center, FL | SLS | 25 |
| PAD-41 | SLC-41 | Cape Canaveral, FL | Atlas V family | 18 |
| PAD-40 | SLC-40 | Cape Canaveral, FL | Falcon 9 (cargo) | 15 |

*Notes*
- Pad↔vehicle compatibility is a real-world-informed rule (this is the "launchpad isn't cosmetic" requirement).
- Crewed missions additionally require a pad with crew-access infrastructure (39A/39B) — so Apollo tutorial = Saturn V + LC-39A, forced by physics of the design, not by arbitrary locks.

---

## 5. INSTRUMENTS (10 entries — fifth config card)

Fields: mass (kg), power (W), data (Gb/day), science value (points), cost `$M [TUNE]`, mission tags.

**Instruments are not just score points — some have required/simulation effects defined per mission:**
- **Required instruments:** each mission lists specific instruments that MUST be selected. Missing them is a validation failure (mission cannot be READY FOR LAUNCH) or, for optional-but-consequential instruments, a simulation consequence.
- **Simulation effects:** some instruments unlock or gate simulation features (e.g., camera control) or mission-critical systems (e.g., crew oxygen).
- **Failure severity:** missing a required instrument blocks launch; missing instruments with mission-critical roles that slip through can cause disaster in the simulation (mission failure), while others only degrade the result — severity is defined per mission in the mission data, not globally.

### Mars rover set (real Perseverance payload — Mission: Mars 2020)

| ID | Instrument | Real basis | Mass | Power | Data | Science | Cost | Effect / note |
|---|---|---|---|---|---|---|---|---|
| INS-MCAMZ | Zoom camera mast | Mastcam-Z | 4 | 60 | 3.0 | 30 | 15 | **Camera control** — without it the flight camera is FIXED position (no player camera movement) |
| INS-SUPCAM | Laser spectrometer mast | SuperCam | 5.6 | 70 | 2.5 | 30 | 18 | — |
| INS-PIXL | X-ray rock analyzer | PIXL | 4.8 | 80 | 1.0 | 35 | 20 | — |
| INS-SHERLOC | UV mineral scanner | SHERLOC | 3.7 | 60 | 1.0 | 35 | 20 | **Required** by Mars 2020 (mission requirement) |
| INS-MEDA | Weather station | MEDA | 3.4 | 50 | 0.5 | 15 | 8 | — |
| INS-MOXIE | Oxygen generator demo | MOXIE | 17.7 | 300 | 0.1 | 20 | 25 | **Required** by all human-led missions (crew oxygen) — mission-critical, omission can end the mission in disaster |
| INS-RIMFAX | Ground-penetrating radar | RIMFAX | 1.7 | 30 | 0.8 | 20 | 10 | — |

### Lunar crewed set (Apollo & Artemis III)

| ID | Instrument | Real basis | Mass | Power | Data | Science | Cost | Mission tag | Effect / note |
|---|---|---|---|---|---|---|---|---|---|
| INS-ALSEP | Surface science station | ALSEP suite | 120 | 70 (RTG) | 0.2 | 50 | 40 | Apollo, Artemis | — |
| INS-REGOLITH | Sample collection & return kit | Apollo sample return | 45 | 0 | 0 | 45 | 25 | Apollo, Artemis | — |
| INS-EVAKIT | Crew EVA science kit | Artemis III surface payloads | 60 | 20 | 0.5 | 40 | 30 | Artemis | — |

*Note: MOXIE (INS-MOXIE) sits in the Mars set above but its availability must be extended to crewed lunar missions, since your rule makes it required for all human-led missions. Its fiction becomes "oxygen generation demo for crew life support." — flagged for your confirmation.*

*Notes*
- Mission defines a **minimum required science value**; extra instruments raise the score's Scientific Value (25% weight) but consume budget, mass, and (future) power.
- Mars 2020 requires at least: one mast instrument + one analyser (educational: cameras see, analysers do science).

---

## 6. MISSIONS (3 entries)

| | M0 — Apollo Tutorial | M1 — Mars 2020 | M2 — Artemis III |
|---|---|---|---|
| Type | Crewed lunar landing | Automated rover delivery | Crewed lunar landing (south pole) |
| Crew | 3 | 0 | 4 |
| Required spacecraft | Apollo CSM + LM | Perseverance package | Orion |
| Vehicle requirement | Human-rated, TLI ≥ 48 t | TMI ≥ 4.0 t | Human-rated, TLI ≥ 30 t |
| Pad | Any compatible | Any compatible | Crewed-infrastructure pad |
| **Required instruments** | MOXIE + (tutor teaches why) | **SHERLOC** + one mast instrument + one analyzer | **MOXIE** |
| Distance (cruise) | 384,400 km | ~470 M km (actual path) | 384,400 km |
| Min science value | 50 | 95 | 90 |
| Budget `$M [TUNE]` | 700 | 300 | 1,200 |
| Success condition | Crew lands and returns | Rover delivered to Mars surface | Crew lands and returns |
| Tutorial? | Yes — guided hints, skippable | No | No |

*Each mission carries its own objectives and requirement set (instruments, vehicle, pad, crew, science minimum) in mission data — nothing is shared generically (per GDD §3.2).*

**Consequences of missing instruments (defined per mission, not globally):**
- Missing a **required** instrument → validation failure, launch blocked with explanation.
- **MOXIE on a crewed mission** treated as mission-critical life support → if the config slips through, oxygen fails during the mission → disaster ending the simulation (crew lost).
- **Mastcam-Z on Mars 2020** → launch allowed, but flight camera is locked to a fixed position during the whole simulation (no player camera movement).
- Other instrument gaps → mission completes but with degraded score/warnings in the debrief.

*Budget math sanity check (Mars 2020):* Atlas V 110 + pad 18 + spacecraft 60 + instruments (4 picks ≈ 73) + fuel ≈ 290 → just under 300 [TUNE]. Apollo: Saturn V 400 + pad 25 + CSM/LM 120 + ALSEP+samples ≈ 70 + fuel ≈ 690. These are first-pass balance numbers — expect tuning after the engineering model is built.

---

## 7. PLANETS / BODIES (5 entries — trajectory view & background)

| ID | Body | Mass (kg) | Radius (km) | Surface gravity (m/s²) | Atmosphere | Distance from Earth |
|---|---|---|---|---|---|---|
| PLNT-EARTH | Earth | 5.972×10²⁴ | 6,371 | 9.81 | Yes (N₂/O₂) | — |
| PLNT-MOON | Moon | 7.342×10²² | 1,737 | 1.62 | None | 384,400 km |
| PLNT-MARS | Mars | 6.417×10²³ | 3,390 | 3.71 | Thin (CO₂) | 55–400 M km; ~470 M km typical cruise path |
| PLNT-VENUS | Venus | 4.867×10²⁴ | 6,052 | 8.87 | Dense (CO₂) | 40–260 M km |
| PLNT-SUN | Sun | 1.989×10³⁰ | 696,000 | — | — | 149.6 M km (1 AU) |

*Venus and Sun are visual/scene anchors for the solar-system trajectory view; no missions target them in v1.*

---

## 8. Data integrity notes (per GDD §16)

- Real engineering values above come from public NASA/contractor fact sheets; each field will carry a `source` key in the final JSON.
- All `[TUNE]` values (costs, budgets, science points) are gameplay balancing approximations, clearly separated from real specs.
- Fuel Isp modifiers, tank mass factors, and trajectory fuel demand will live in one calculation layer (GDD §8.6: consistent model, no arbitrary penalties).
- **Departure delta-v requirements are model-calibrated gameplay values, not raw vacuum-burn numbers:** Apollo/Artemis departure = 5,500 m/s, Mars departure = 5,000 m/s in `missions.json`. Real TLI/TMI figures (~3.2/3.6 km/s) don't apply directly because the game's stage model treats fixed solid boosters as dead mass and includes tank mass — the calibrated budgets produce correct outcomes (full tanks pass with margin, underfueled configs fail) inside this model. Arrival/return values remain real spacecraft capabilities.
- Automated regression suite: `tests/GameTestSuite.cs` (run from the project root with `unity command script run --file ../tests/GameTestSuite.cs --entry SpaceMissionTesting.GameTestSuite.RunAll`) — 80 checks covering data integrity, engineering math, validation rules, outcome prediction, and unit formatting. Keep it green whenever data or model changes land.

---

## 9. Review checklist for you

- [ ] Engines: add/remove any? (e.g., swap RD-180 for an American engine for branding sensitivity?)
- [ ] Vehicles: is Starship needed for v1? (I left it out to keep scope tight)
- [ ] Costs/budgets: any overall scale preference (keep $M-realistic vs. rounder numbers)?
- [ ] Instruments: OK to keep MOXIE (it's famous but low science-per-mass — that's the trade-off teaching moment)?
- [ ] MOXIE fiction: making the oxygen generator demo required for crewed lunar missions (Apollo + Artemis) — confirm this is intended?
- [ ] Mars 2020 required set: SHERLOC + one mast instrument + one analyzer — is one mast enough, or must it specifically be Mastcam-Z?
- [ ] Missions: budgets and required science values feel right?
- [ ] Anything to rename for the audience (RD-180 is Russian-built — some teams prefer generic names)?
