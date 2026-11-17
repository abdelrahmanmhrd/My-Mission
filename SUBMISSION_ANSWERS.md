# SUBMISSION ANSWERS — My Mission (NASA Space Apps Challenge 2026)
**Keep this file safe — copy-paste source for the submission form. Edit freely; answers stay under the word limits.**

---

## Summary (50–200 words)

My Mission is a single-player space mission design simulator that puts players in the mission director's chair. Players receive a real mission objective — landing a crew on the Moon with Apollo, delivering the Perseverance rover to Mars, or returning humans to the lunar south pole with Artemis III — then engineer the mission themselves: choosing launch vehicles, assigning engines and fuels to each rocket stage, selecting a spacecraft, launch facility, and scientific instruments, all within a fixed budget and hard physical constraints. Every choice matters: heavier payloads demand more fuel, efficient engines cost more, and incompatible combinations fail. The game's engineering layer uses real NASA data — rocket thrust, spacecraft masses, delta-v requirements — to deterministically compute whether the design can fly, then a 3D simulation visualizes the launch, from ignition and ascent to orbit. A scoring system rewards mission success, science value, resource efficiency, and engineering margin. It addresses the challenge by making mission-design trade-offs tangible: students experience how real engineers balance competing demands, learn from failure with explained causes, and iterate toward better designs.

*(152 words)*

---

## Project Details

**What it does and how it works:**
My Mission is an educational engineering simulator built in Unity. The player acts as mission director for one of three sequential missions (Apollo tutorial, Mars 2020, Artemis III). Gameplay follows the real mission-design process: a classified-style mission briefing defines the objective, destination, budget, and requirements. The player then configures the mission through five engineering categories — engines, fuels, launch vehicles/spacecraft, launch facilities, and scientific instruments. Real physics drives everything behind the scenes: the game computes stage-by-stage delta-v using the rocket equation, thrust-to-weight ratio at liftoff, tank-mass penalties per fuel type, payload capacity, budget consumption, and compatibility rules (an Atlas pad cannot launch a Saturn V; crewed missions need human-rated vehicles and crew-capable pads). Missions require specific instruments: Mars 2020 needs SHERLOC; crewed missions need oxygen generation. Validation explains every blocking issue in plain language, and the deterministic verdict is visualized in a 3D launch simulation — ignition, ascent (compressed to ~35 seconds), telemetry HUD, and camera control (unlocked only if the player mounted a camera mast). Underfueled rockets flame out mid-ascent and fall. A post-mission debrief scores the design (mission success 35%, science 25%, efficiency 20%, reliability 20%), explains exactly what went wrong or right, and unlocks the next mission. Failure teaches: players retry with their configuration intact and the cause explained.

**Benefits and intended impact:**
Space mission design is normally learned through isolated theory. My Mission lets students experiment with an entire mission from concept to outcome, experiencing the real trade-offs — mass versus capability, efficiency versus cost, ambition versus budget — that NASA engineers face. Every value traces to a public NASA source, so players absorb real engineering data while playing. The intended impact: make integrated mission design accessible, hands-on, and replayable for students and aspiring engineers, especially where no hands-on mission-design environment exists.

**Tools, languages, software:**
Unity 6000 (C#, Universal Render Pipeline, TextMeshPro, Input System), JSON data-driven component database authored by the team, Blender 4.5 (asset conversion/decimation), Python (data processing), an AI coding assistant (opencode with GLM) for code generation and debugging under team direction, and Unity batch-mode build pipeline for Windows/macOS players.

**Creativity:**
The game is built on a strict "the program already knows" design: the engineering layer deterministically decides the mission outcome from real physics, and the 3D simulation is the visual story of that verdict — so educational clarity and visual spectacle never conflict. Consequence-driven instruments (no mast camera = fixed flight camera), model-calibrated delta-v budgets derived from real mission performance, and a scoring system that rewards margin and efficiency rather than raw spending.

**Factors considered:**
Educational value (every displayed number is real or documented as a gameplay approximation — GDD §16 data integrity), accessibility (game units by default, real engineering units optional), licensing compliance (every third-party asset tracked with license and in-game attribution), web/desktop performance (decimated meshes, 1K textures), and honest NASA branding boundaries (fictional NASA-inspired interface, no official insignia).

---

## Use of Super Intelligence (SI)

**SI tools were used.** The team used an AI coding assistant (opencode, powered by the GLM large language model) to accelerate development. Specifically:

- **Code and data:** the majority of the game's C# code, JSON mission/component datasets, and automated test suite were generated or co-written with the AI assistant under team direction, review, and iteration. This is acknowledged here and in the project's documentation (PROJECT_PLAN.md, dataset_review.md).
- **No SI-generated images, videos, or audio** are used in the project. All 3D models are either NASA public-domain assets or CC-BY Sketchfab models by credited human authors; textures are NASA-provided or CC-BY.
- No NASA branding or design elements were generated or modified by SI tools; the game deliberately uses a fictional, NASA-inspired interface without the NASA insignia.

(Select on the form: the option(s) indicating code/data SI assistance.)

---

## NASA Data (link list — copy each row into the form)

| Link Text | Link Url |
|---|---|
| NASA 3D Resources (Saturn V, Atlas V, SLS, Apollo Lunar Module, Apollo-Soyuz CSM, Orion capsule, Perseverance rover, MAVEN orbiter, launch gantry, ISS) | https://www.nasa.gov/3d-resources/ |
| NASA NSSDCA Planetary Fact Sheets (Earth, Moon, Mars, Venus, Sun physical data) | https://nssdc.gsfc.nasa.gov/planetary/planetfact.html |
| NASA Mars 2020 / Perseverance Press Kit & instrument pages (Mastcam-Z, SuperCam, PIXL, SHERLOC, MEDA, MOXIE, RIMFAX) | https://science.nasa.gov/mission/mars-2020-perseverance/ |
| NASA Space Launch System fact sheet | https://www.nasa.gov/humans-in-space/space-launch-system/ |
| NASA Orion spacecraft fact sheet | https://www.nasa.gov/humans-in-space/artemis/ |
| NASA Saturn V News Reference (stage and F-1/J-2 engine data) | https://www.nasa.gov/wp-content/uploads/static/history/alsj/SaturnVNewsRef.pdf |
| NASA Images and Media Usage Guidelines | https://www.nasa.gov/nasa-brand-center/images-and-media/ |

---

## Space Agency Partner & Other Data (link list — copy each row into the form)

| Link Text | Link Url |
|---|---|
| Sketchfab: "F1 Rocketdyne Engine" by Rowen Halpern (CC-BY-4.0) | https://sketchfab.com/3d-models/f1-rocketdyne-engine-9d84d45055094a06bf75a232b36713ba |
| Sketchfab: "J-2" by MartianDays (CC-BY-4.0) | https://skfb.ly/6TORt |
| Sketchfab: "Merlin 1D" by MartianDays (CC-BY-4.0) | https://skfb.ly/6Rpvy |
| Sketchfab: "Merlin 1D Vacuum" by MartianDays (CC-BY-4.0) | https://skfb.ly/6RBww |
| Sketchfab: "RD-180" by MartianDays (CC-BY-4.0) | https://skfb.ly/6RDPS |
| Sketchfab: "RS-25" by MartianDays (CC-BY-4.0) | https://skfb.ly/6WWLF |
| Sketchfab: "Rocket engine" (AJ10 visual stand-in) by Savinien B. (CC-BY-4.0) | https://skfb.ly/prpAp |
| Sketchfab: "Falcon 9 - SpaceX" by Stanley Creative (CC-BY-4.0) | https://skfb.ly/6RIC9 |
| Sketchfab: "Earth" by Akshat (CC-BY-4.0) | https://skfb.ly/6TwGG |
| Sketchfab: "Moon" by Akshat (CC-BY-4.0) | https://skfb.ly/6TwGU |
| Sketchfab: "Mars" by Mieke Roth (CC-BY-4.0) | https://skfb.ly/IVF8 |
| Unity Technologies — Unity 6000 game engine (C#, URP, TextMeshPro, Input System) | https://unity.com/ |
| Blender Foundation — Blender 4.5 LTS (asset conversion and decimation) | https://www.blender.org/ |
| opencode (AI coding assistant, GLM model) — code and dataset generation under team direction | https://opencode.ai |

---

*Saved 2026-11-15. Update the NASA Data / Other Data tables if new assets enter the project (sync with ASSET_TRACKING.md).*
