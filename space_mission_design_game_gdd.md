# Space Mission Design Game

## Game Design Document (GDD) --- Design Baseline v1.0

**Project:** NASA Space Apps Challenge 2026\
**Genre:** Educational engineering / strategy / mission-management
simulator\
**Engine:** Unity\
**Target platform:** Web-based game\
**Player mode:** Single-player\
**Visual direction:** Dark, professional aerospace-control-room
aesthetic\
**Document status:** Consolidated design baseline. Items marked **TBD**
are intentionally unresolved and must not be treated as approved
features.

------------------------------------------------------------------------

## 1. Game vision

The Space Mission Design Game is an accessible, interactive
space-mission simulator in which players act as mission directors.
Players receive a defined mission objective, configure a spacecraft and
launch system, manage a limited budget and engineering constraints,
launch the mission, monitor its journey, and review the outcome.

The game combines three experiences:

1.  **Educational simulator:** explains engineering concepts in
    approachable language.
2.  **Strategy game:** asks players to balance competing constraints and
    make trade-offs.
3.  **Mission-management experience:** gives players responsibility for
    planning and executing a complete mission.

Players should not need to write equations or perform manual engineering
calculations. The game performs calculations behind the scenes and
communicates consequences through clear values, indicators,
compatibility warnings, and explanations. The intended conceptual
difficulty is approachable and hands-on, comparable to the
problem-solving feel of Minecraft Redstone rather than a specialist
engineering tool.

### Core design principle

**Every important design choice should have a visible consequence.**

Examples: - A heavier payload increases total mass and can increase fuel
requirements. - More fuel increases available propellant but also
increases vehicle mass and cost. - An engine's thrust, efficiency, fuel
consumption, and compatibility affect whether a mission can launch and
complete its planned route. - A more capable component may improve
performance while consuming more of the mission budget. - A launch
vehicle or launch facility must be compatible with the selected
spacecraft and mission.

The player should be able to understand *why* a configuration is
suitable or unsuitable, not merely receive a pass/fail message.

------------------------------------------------------------------------

## 2. Target experience and scope

### Confirmed scope

-   Single-player.
-   Web-based Unity game.
-   3D rocket and spacecraft simulation.
-   Three planned mission experiences:
    -   Apollo tutorial.
    -   Mars 2020 mission.
    -   Artemis III mission.
-   Defined mission objectives and requirements.
-   Player selects and configures components within mission constraints.
-   Fixed budget for each mission.
-   Engine, fuel, spacecraft, and launchpad selection.
-   Human-led and automated mission distinctions.
-   Launch and interplanetary travel simulation.
-   Mission scoring and post-mission debrief.
-   Dark, professional aerospace-control-room UI.
-   Sound effects and space-themed audio.

### Explicitly not in the initial scope

-   Multiplayer.
-   Random in-flight events (possible future add-on).
-   Upkeep costs (possible future add-on).
-   Optimization add-on (listed in settings as a possible add-on; exact
    behavior TBD).
-   Full manual orbital-mechanics calculations by the player.

### Design approach

Prioritize a complete, coherent single-player experience over breadth.
Features should be modular so that undecided systems can be added later
without redesigning the whole game.

------------------------------------------------------------------------

## 3. Mission structure

### 3.1 Planned missions

#### Tutorial --- Apollo

Purpose: teach the player how to read a mission briefing, select
components, understand resource trade-offs, validate a configuration,
and launch.

The tutorial should be skippable. The preferred direction is a playable
guided mission with short contextual hints, plus the ability to skip
guidance. The exact historical Apollo mission and tutorial scenario
details are TBD.

#### Mission 1 --- Mars 2020

A mission based on delivering a scientific rover/payload to Mars. The
objective and mission requirements are defined by the scenario. Players
choose a compatible configuration and attempt to complete the delivery
within the assigned constraints.

The game should be inspired by the real Mars 2020 mission, but the exact
degree of historical recreation versus alternative player-designed
solutions is TBD.

#### Mission 2 --- Artemis III

A human-led lunar mission scenario with additional crew-related
spacecraft requirements. The objective and mission requirements are
defined by the scenario. The configuration should account for human
safety and crew-support systems.

The precise mission profile and how closely it follows the real Artemis
III architecture are TBD.

### 3.2 Mission objectives

-   Objectives are defined by each mission; players do not invent or
    replace the objective.
-   Each mission briefing communicates the destination, mission type,
    payload or crew requirements, budget, and key constraints.
-   Mission-specific requirements must be used. Do not reuse generic
    values across missions.
-   A mission succeeds when the required objective is achieved and the
    relevant mission constraints have been met.

### 3.3 Mission progression

Whether missions unlock sequentially or are all selectable from the
beginning is TBD. The tutorial must be clearly identified and skippable.

------------------------------------------------------------------------

## 4. Main menu and global navigation

### 4.1 Main menu

The main menu has two primary buttons:

-   **Start:** opens mission selection.
-   **Info:** opens the game information/encyclopedia area.

A settings control is also available.

### 4.2 Main-menu background

The visual environment may include: - Earth and its orbital
environment. - The International Space Station (ISS). - The Moon. - NASA
branding/logo placement, subject to appropriate branding and usage
requirements.

The background should support the atmosphere without competing with the
primary buttons.

### 4.3 Settings

Confirmed/planned settings: - Background sounds / music. - Optimization
add-on (scope and exact function TBD).

Audio settings should distinguish music/ambience from sound effects if
feasible.

### 4.4 Info area

The Info area explains the game's mission parameters and engineering
concepts in plain language. It should be a reference center, not a long
technical manual.

It should explain concepts such as: - Thrust. - Fuel type and fuel
consumption. - Engine efficiency. - Vehicle and payload mass. - Fuel
capacity. - Budget and component costs. - Spacecraft types. - Human-led
versus automated missions. - Launch facilities and compatibility. -
Trajectory and destination. - Other systems if power and communications
are later included.

Contextual explanations should also be available from relevant selection
pages. For example, selecting an engine should make it possible to
understand what its thrust and fuel-consumption values mean.

------------------------------------------------------------------------

## 5. Mission entry and briefing

### 5.1 Entry behavior

When the player selects a mission, the mission briefing appears
automatically before the configuration interface. The briefing can be
reopened later from the small inbox icon on the mission configuration
screen.

### 5.2 Briefing tone and length

The briefing should feel like an authentic mission-control document or
directive, but it is a game interface. Keep it short and readable in
approximately 15--25 seconds. Do not turn it into a lengthy technical
report.

### 5.3 Briefing layout

Header: - NASA branding/logo. - Mission name and identifier. -
Confidential classification styling. - "Authorized personnel only" /
"Top Security Access" flavor text.

Body: - Short mission directive explaining the player's objective. -
Compact mission parameters, such as: - Destination. - Mission type
(automated or human-led). - Payload or crew requirement. - Budget. -
Relevant distance or mission profile information. - Any key
mission-specific constraints.

Footer/message: - Short mission-control message. - Acknowledge/continue
button.

### 5.4 Sample briefing copy --- illustrative only

**MISSION 01: MARS 2020**\
**MISSION DIRECTIVE**

"Your objective is to deliver a scientific rover to Mars. Configure a
spacecraft and launch system capable of completing the journey while
remaining within the mission's engineering and financial constraints."

"Validate all systems before launch. Mission success depends on your
engineering decisions."

All numerical values must come from the approved mission data, not from
this illustrative copy.

------------------------------------------------------------------------

## 6. Mission configuration interface

### 6.1 Main layout: four cards in a 2 × 2 grid

The mission configuration screen contains four large clickable cards
arranged as **two rows and two columns**:

  Position       Category                        Label
  -------------- ------------------------------- ----------------
  Top-left       Engines                         **ENGINES**
  Top-right      Fuels                           **FUELS**
  Bottom-left    Spacecraft / rocket selection   **ROCKETS**
  Bottom-right   Launch facilities               **LAUNCHPADS**

Each card contains: - An image representing the category. - A category
title. - A short explanatory description positioned to the **right of
the image**. - A selected/not-selected status. - The entire card is
clickable and opens the relevant selection page.

The image and text should form one horizontal composition inside each
card. Do not place the category description only below the image.

### 6.2 Initial state

At the beginning of a mission: - All four cards show their default
category image and description. - Each category displays **NOT
SELECTED**. - No component is implied to have been selected.

### 6.3 Selected state

After a player selects a component: - The category title remains
visible. - The selected component's name appears. - Its model image or
representative model preview appears. - The status changes to
**SELECTED**. - The card updates immediately.

For example: - Category: ENGINES - Selected component: Engine E1 -
Status: SELECTED

If the category supports multiple selected components or staged systems,
the card should summarize the selection clearly (for example, number of
engines or selected engine stages). The exact multi-engine UX is TBD.

### 6.4 Mission briefing shortcut

A small inbox icon is placed at the top-left area of the configuration
interface. It reopens the mission briefing. It must remain visually
secondary to the four main category cards.

### 6.5 Shared configuration state

All category pages must update one shared mission configuration.
Selecting a fuel must be visible to engine compatibility checks;
selecting a spacecraft must affect mass, payload capacity, and supported
systems; and so on.

The cards are navigation destinations, not isolated forms with
disconnected values.

### 6.6 Persistent mission status

A persistent resource/status area is recommended across configuration
pages. It should show key values such as: - Budget remaining. - Current
total mass / allowed mass or capacity. - Fuel capacity or selected fuel
amount. - Mission readiness.

Exact placement and final set of displayed resources are TBD. Values
must update immediately when a selection changes.

### 6.7 Mission readiness indicator

Use clear readiness states: - **NOT READY:** required selections are
missing or a blocking constraint is violated. - **VALIDATION REQUIRED:**
required components appear selected, but the configuration has not
passed final validation. - **READY FOR LAUNCH:** all required selections
and mission constraints pass validation.

Do not allow a player to mistake a visually complete configuration for a
validated one.

------------------------------------------------------------------------

## 7. Component selection pages

The preferred selection-page direction is **catalog + 3D preview**.

Recommended composition: - Component catalog/list on the left. - Large
3D preview of the selected component on the right. - Technical
information and costs alongside or below the preview. - Clear
selection/confirm control. - Back navigation to the four-card
configuration screen.

The exact final layout can be refined during UI prototyping, but the
page should make comparison and selection easy.

### 7.1 Shared component information

Where relevant, each component entry should display: - Name. - Image or
3D model. - Short description. - Cost. - Mass. - Performance
characteristics. - Compatibility information. - Selected state.

Only show attributes that actually apply to that component. Avoid
filling every card with irrelevant statistics.

### 7.2 Engines

Players select propulsion engines. Engine data should include, as
applicable: - Thrust. - Fuel consumption. - Efficiency. - Engine mass. -
Cost. - Supported fuel types. - Compatible spacecraft or stage types.

The example values E1 = 8, E2 = 6, E3 = 4 are placeholders only. They
are not approved specifications.

Players should see how an engine performs with the selected fuel and how
its characteristics affect the mission.

Whether the player can select multiple engines and configure separate
rocket stages is TBD. A staged/multi-engine system is a possible
extension, but should not be assumed as a required first-version
feature.

### 7.3 Fuels

Different fuel types should have different characteristics. Differences
may include: - Engine compatibility. - Performance/efficiency. - Fuel
consumption. - Cost. - Storage or tank implications, if modeled.

Fuel selection must affect engine behavior and the mission calculations.
The game should clearly identify incompatible engine/fuel combinations.

Specific propellant options and their simplified game statistics require
research and validation.

### 7.4 Rockets / spacecraft

The current category label is **ROCKETS**. Its precise scope must be
clarified in the implementation data model.

The design must distinguish: - **Launch vehicle:** the rocket that
provides launch and ascent capability. - **Spacecraft:** the
vehicle/payload carrier that travels to the mission destination.

A recommended interpretation is to retain the ROCKETS category in the UI
but allow the player to select/configure the spacecraft and its
associated launch vehicle within this area, with clear separate labels.
Whether these become separate subpages or a combined workflow is TBD.

Spacecraft selection should affect applicable characteristics such as: -
Base mass. - Payload capacity. - Supported engine/propulsion
configuration. - Available internal volume or crew capacity. -
Human-rated or automated capability. - Cost.

### 7.5 Launchpads

Players select a launch facility/pad suitable for the mission and launch
vehicle.

Relevant attributes may include: - Compatible launch vehicles. -
Mission/orbit suitability. - Launch facility cost or assigned mission
cost, if modeled. - Any mission-specific restrictions.

The launchpad selection must not be purely cosmetic: it should have
meaningful compatibility rules.

### 7.6 Scientific instruments and payload

Scientific instruments are essential to the challenge's educational and
mission-design purpose.

A separate **Payload / Instruments** configuration step is recommended,
even though it is not yet one of the four primary cards. It may be: - A
fifth category/card, or - A subpage within the spacecraft/ROCKETS
category.

This choice is **TBD** and must be resolved before finalizing the
complete configuration UI.

Instrument choices should affect relevant characteristics such as: -
Scientific value. - Mass. - Cost. - Power consumption, if power is
implemented. - Data generation, if data/communications are implemented.

Do not omit payload/instrument configuration from the overall game
design.

------------------------------------------------------------------------

## 8. Engineering model and resource management

### 8.1 General approach

The game performs calculations internally. Players interact with
understandable game units and readable comparisons rather than writing
formulas.

The underlying model should be based on credible engineering
relationships and research, while the displayed values are simplified
and tuned for gameplay.

### 8.2 Core confirmed resources and constraints

-   Budget.
-   Total mass.
-   Payload mass/weight.
-   Fuel type.
-   Fuel capacity and fuel amount.
-   Engine thrust and fuel consumption.
-   Engine/fuel compatibility.
-   Spacecraft compatibility.
-   Launch vehicle / launchpad compatibility.
-   Mission distance and destination.
-   Trajectory and ability to reach the objective.

### 8.3 Budget

Budget is a **core feature**, not an add-on.

-   Each mission has a fixed starting budget.
-   Component purchases and spacecraft configuration consume that
    budget.
-   The player must stay within the assigned budget to launch.
-   Remaining budget should be visible during configuration.
-   Component costs must be defined in mission/component data.
-   The budget should make players weigh performance against
    affordability.

Whether there are additional operational/upkeep costs is deferred to a
possible future add-on.

### 8.4 Simplified game units

Use readable game units instead of exposing intimidating scientific
units everywhere.

Examples of possible display labels: - Thrust Units (TU). - Fuel Units
(FU). - Mass Units (MU). - Credits. - Efficiency rating.

The final unit names, scales, conversion factors, and rounding rules are
TBD.

Internally, store validated physical values and/or well-defined
normalized values. The game-unit display layer should be separate from
the calculation layer so that balance can be adjusted without rewriting
simulation logic.

### 8.5 Mass and payload

The model should account for the combined mass of applicable elements: -
Spacecraft structure. - Engines. - Fuel and tanks. - Payload/scientific
instruments. - Crew-support systems for human-led missions.

Excessive payload can increase total mass and fuel demand. If the
configuration exceeds a hard launch or vehicle limit, the player should
receive a clear warning or validation failure. The exact distinction
between a blocked launch and an in-flight failure should be defined
consistently.

### 8.6 Fuel and propulsion

The model should consider: - Engine thrust. - Engine efficiency. - Fuel
consumption. - Selected fuel. - Fuel quantity/capacity. - Vehicle and
payload mass. - Mission/trajectory demands.

A heavier vehicle should generally require more propulsion effort/fuel
to achieve the same mission profile. The game must use a consistent
calculation model rather than arbitrary isolated penalties.

### 8.7 Compatibility

Compatibility is a core rule: - An engine must support the selected
fuel. - An engine must be suitable for the selected spacecraft/vehicle
configuration. - The launch vehicle and launchpad must be compatible. -
Vehicle/payload mass must be within applicable limits.

An incompatible engine/fuel or engine/spacecraft combination can cause a
catastrophic failure in the fiction of the game. For good player
experience, the recommended default is to detect known incompatibilities
during configuration and explain them before launch. Whether certain
incompatible combinations are allowed to launch and fail dramatically is
TBD.

### 8.8 Power and communications

The challenge description identifies power and communications as
important possible mission constraints. Their inclusion in the first
playable version is **TBD**, as explicitly requested.

Do not silently mark these as required or implemented. They should
remain documented as potential systems.

If implemented later: - Power should compare generation/storage against
spacecraft and instrument demand. - Communications should compare data
generation and transmission capability, and may affect contact or data
delivery.

### 8.9 Random events and upkeep

-   Random events: not in the current confirmed scope; possible future
    add-on.
-   Upkeep/operational costs: possible future add-on.
-   Optimization add-on: mentioned in settings; functionality TBD.

------------------------------------------------------------------------

## 9. Human-led and automated missions

### 9.1 Automated missions

Automated missions do not require crew-support systems. They may still
require scientific payloads, spacecraft systems, power, communications,
and other mission-specific equipment if those systems are implemented.

### 9.2 Human-led missions

Human-led missions require additional systems. Confirmed categories
include: - Life support. - Food. - Water. - Oxygen. - Radiation
protection. - Crew living/sleeping area. - Emergency systems. - Other
crew-support equipment as appropriate.

These systems should influence mass and cost. Where relevant, their
capacity should be checked against crew size and mission duration.

Whether the player chooses crew size is TBD.

------------------------------------------------------------------------

## 10. Launch and 3D simulation

### 10.1 Simulation goals

The simulation should visually communicate the consequences of the
player's engineering decisions. It should show the outside of the
rocket/spacecraft, engine thrust, flame, smoke, flight progression, and
the mission route.

The simulation is 3D. The player can change the camera angle while the
rocket is flying.

### 10.2 Visual phases

#### Phase 1 --- Launchpad and liftoff

-   Show the launch platform and surrounding launch-site/NASA-style
    buildings.
-   Show the rocket before launch.
-   Show engine ignition, flame, smoke, and liftoff.
-   Display relevant flight telemetry.
-   Provide camera control.

#### Phase 2 --- Atmospheric ascent

-   Rocket climbs through the atmosphere.
-   The ground and launch site become less visible.
-   Earth becomes increasingly prominent.
-   Environmental/background presentation changes with altitude and
    flight progress.

#### Phase 3 --- Earth orbit / departure

-   Transition to an Earth-orbit view.
-   Show spacecraft/rocket state and relevant telemetry.
-   The game should represent departure from Earth orbit in a way
    appropriate to the mission.

The current concept describes the launch-site view remaining until the
rocket reaches "inner orbit," followed by a more distant Earth view, and
then a solar-system view after leaving "outer orbit." These labels are
informal placeholders and must be replaced with clearly defined
simulation phases and thresholds. Do not treat "inner orbit" and "outer
orbit" as finalized technical terms.

#### Phase 4 --- Interplanetary travel

-   Transition to an orbital/solar-system view.
-   Show the destination planet.
-   Show current spacecraft position.
-   Show actual/completed trajectory as a solid line.
-   Show planned/projected trajectory as a dotted line.
-   Show remaining fuel, distance, estimated arrival time, and velocity.
-   Accelerate the long travel segment so it takes approximately
    **10--20 seconds** in the game, rather than reproducing real
    interplanetary travel duration.

#### Phase 5 --- Destination approach and arrival

-   As the spacecraft approaches the destination, return to a closer
    spacecraft view.
-   Show the relevant arrival/orbit-insertion/delivery sequence.
-   For a Mars rover delivery, show the mission-specific delivery
    outcome.
-   For a human-led lunar mission, show the appropriate lunar mission
    outcome.
-   Determine success or failure based on the mission objective and
    simulation state.

### 10.3 Time behavior

-   The launch/ascent sequence should be visually paced to show the
    rocket's flight.
-   The player has not specified a final duration for ascent.
-   Interplanetary travel is compressed to approximately 10--20 seconds.
-   Destination arrival should return to a close spacecraft view.

Whether players can pause or change simulation speed is TBD.

### 10.4 Camera

Confirmed: - Player can change the camera angle while the rocket is
flying.

Recommended: - Camera reset/follow option. - Smooth camera transitions
between launch, ascent, orbit, and deep-space phases.

Additional camera modes and full free-camera behavior are TBD.

### 10.5 Telemetry display

The simulation should display: - Current spacecraft position. -
Destination planet. - Planned trajectory. - Actual trajectory. - Fuel
remaining. - Distance to destination. - Estimated arrival time. -
Orbital velocity.

Telemetry should be readable and visually subordinate to the
spacecraft/trajectory view. Use clear units and concise labels.

### 10.6 Audio

Include: - Rocket launch and engine sound effects. - Appropriate
ignition and flight audio. - Space-themed ambient sounds/music. -
Mission/interface feedback sounds where useful.

Use separate controls for music/ambience and sound effects if feasible.
Audio assets must be appropriately licensed for the game.

------------------------------------------------------------------------

## 11. Failure conditions and feedback

### 11.1 Confirmed failure concepts

-   **Insufficient fuel:** the rocket/spacecraft cannot complete the
    required flight and may lose the ability to sustain its planned
    ascent/trajectory. The current concept describes the rocket falling
    out of the sky during an ascent failure.
-   **Excessive payload:** additional mass increases fuel demand; if
    fuel or vehicle capability is insufficient, the mission can fail.
-   **Incorrect trajectory:** the spacecraft fails to reach the mission
    objective/destination.
-   **Incompatible engine/fuel or engine/spacecraft:** may cause a
    catastrophic engine/vehicle failure, represented by an explosion in
    the simulation.

### 11.2 Undecided failure systems

The following are explicitly **TBD**: - Poor communication / loss of
communications. - Insufficient power. - Other in-flight malfunctions or
random events.

### 11.3 Player feedback

Failure should teach the player what happened. A failure should not be a
silent game-over.

Recommended flow: 1. Show the failure event in the simulation. 2.
Display a concise result. 3. Explain the cause in plain language. 4.
Identify the relevant design choices or constraints. 5. Offer a return
to configuration so the player can revise the design.

The exact failure-screen flow and whether every failure is immediately
recoverable are TBD.

------------------------------------------------------------------------

## 12. Mission scoring and debrief

A final score/ranking is desired.

### 12.1 Provisional scoring model

  Category                    Provisional weight
  ------------------------- --------------------
  Mission success                            35%
  Scientific value                           25%
  Resource efficiency                        20%
  Engineering reliability                    20%
  **Total**                             **100%**

These weights are a starting proposal, not final balancing.

### 12.2 Debrief contents

The post-mission report should show: - Mission success or failure. -
Overall score/grade. - Mission objective outcome. - Total expenditure
compared with budget. - Fuel efficiency/performance. - Payload delivery
status. - Important engineering problems. - A short explanation of why
the design performed as it did. - Suggestions for improving the next
attempt.

Scientific value should be based on the mission's payload/objective, not
merely on how many instruments are selected.

------------------------------------------------------------------------

## 13. Visual design system

### 13.1 Overall style

Use a dark, professional aerospace-control-room aesthetic rather than a
colorful arcade-game interface.

Visual direction: - Deep navy and near-black backgrounds. - Restrained
blue accents. - Neutral gray secondary text. - Clear high-contrast
primary text. - Technical, legible typography. - Fine borders and subtle
panel separation. - Restrained glow/hover effects. - Realistic aerospace
imagery and 3D previews. - Minimal decorative sci-fi elements.

The desired feel is a clean engineering workstation / mission-control
interface, not an overloaded science-fiction HUD.

### 13.2 Interaction styling

-   Entire category cards are clickable.
-   Hover/focus state should be visible, using a subtle blue
    accent/border.
-   Selected components have a clear selected state.
-   Missing or incompatible selections have clear warnings.
-   Mission readiness uses consistent status colors and labels.
-   Avoid relying on color alone; include text/icon indicators.

### 13.3 NASA branding

NASA branding/logo placement is part of the intended experience. Use
official branding assets only in accordance with applicable NASA
insignia and usage guidance. Do not imply NASA endorsement or official
NASA production unless authorized. A NASA-inspired fictional interface
may be used for game UI elements.

------------------------------------------------------------------------

## 14. Recommended screen inventory

1.  **Main Menu**
    -   Start.
    -   Info.
    -   Settings.
    -   Space/Earth/ISS/Moon background.
2.  **Mission Selection**
    -   Apollo tutorial.
    -   Mars 2020.
    -   Artemis III.
    -   Mission summaries and availability/lock state if progression is
        added.
3.  **Mission Briefing**
    -   Automatically opens after mission selection.
    -   Short, classified-style directive.
    -   Mission-specific parameters.
    -   Acknowledge/continue.
    -   Reopenable from inbox icon.
4.  **Mission Configuration Hub**
    -   Four 2 × 2 category cards.
    -   Each card has image on the left and category title/description
        on the right.
    -   Initial state: NOT SELECTED.
    -   Selected state: component name and model/image.
    -   Inbox icon.
    -   Persistent resource/readiness area recommended.
5.  **Engine Selection**
    -   Engine catalog.
    -   3D preview.
    -   Engine characteristics.
    -   Fuel and spacecraft compatibility.
6.  **Fuel Selection**
    -   Fuel catalog.
    -   Fuel characteristics.
    -   Engine compatibility.
    -   Cost and capacity information where applicable.
7.  **Rockets / Spacecraft Selection**
    -   Spacecraft/vehicle catalog.
    -   Clear distinction between spacecraft and launch vehicle.
    -   3D preview and relevant capabilities.
8.  **Launchpad Selection**
    -   Launch facility catalog.
    -   Compatibility and mission suitability.
9.  **Payload / Instrument Selection**
    -   Recommended additional configuration screen; exact placement
        TBD.
10. **Mission Validation**
    -   Checks budget, mass, fuel, compatibility, and mission
        requirements.
    -   Displays blocking issues and warnings.
    -   Allows launch only when required conditions pass.
11. **Launch / Flight Simulation**
    -   3D rocket and spacecraft.
    -   Launch effects.
    -   Camera controls.
    -   Telemetry.
    -   Flight phase transitions.
12. **Interplanetary Trajectory View**
    -   Solar-system view.
    -   Destination.
    -   Current position.
    -   Solid actual trajectory.
    -   Dotted projected trajectory.
    -   Fuel, distance, arrival time, velocity.
13. **Mission Result / Debrief**
    -   Success/failure.
    -   Score.
    -   Engineering explanation.
    -   Budget and resource summary.
    -   Retry/reconfigure path.
14. **Info / Encyclopedia**
    -   Plain-language explanations of parameters and systems.
15. **Settings**
    -   Music/background sound.
    -   Sound effects.
    -   Optimization add-on entry (TBD).

------------------------------------------------------------------------

## 15. Technical design principles for implementation

This document describes product behavior, not a Unity setup guide. The
Unity CLI/tooling environment is already configured by the project team.

### 15.1 Separate game logic from visuals

Keep the engineering calculation system independent of the 3D
presentation. The same configuration should produce the same validation
and mission results regardless of camera, scene, or visual effects.

Recommended conceptual layers:

1.  **Mission data:** mission objective, destination, budget, payload
    requirements, mission type, constraints.
2.  **Component data:** engines, fuels, spacecraft, launch vehicles,
    launchpads, instruments.
3.  **Configuration state:** the player's current selections and
    quantities.
4.  **Engineering calculation layer:** mass, fuel demand,
    thrust/performance, budget, compatibility, trajectory feasibility.
5.  **Validation layer:** blocking issues, warnings, readiness state.
6.  **Mission simulation layer:** flight phases, simulated position,
    fuel state, trajectory, arrival/failure outcome.
7.  **Presentation layer:** UI, 3D models, camera, effects, audio,
    telemetry.

### 15.2 Data-driven components

Store component specifications as data rather than hard-coding values
into UI logic. Each component should have a stable identifier and the
attributes relevant to its type.

This allows the team to: - Tune game balance without rebuilding UI
logic. - Add components later. - Reuse component data across catalog,
validation, and simulation. - Keep mission-specific restrictions in
mission data.

### 15.3 Shared configuration state

The selected engine, fuel, spacecraft, launch vehicle, launchpad,
payload, and other options must be held in one mission configuration
state. All screens read from and update that state.

### 15.4 Deterministic validation

Given the same mission and configuration, validation should produce the
same result. Each failure should be associated with a specific reason
and, where possible, a recommended correction.

### 15.5 Simulation abstraction

The interplanetary phase is intentionally compressed. The simulation
should preserve the appearance and logic of a journey without attempting
to run the real mission duration in real time.

Use a clear simulation timeline and phase state so the visual sequence,
telemetry, fuel model, and outcome remain synchronized.

### 15.6 Web performance

Because the target is a web-based 3D game: - Optimize 3D models and
textures. - Limit expensive real-time effects. - Use appropriate levels
of detail. - Keep UI responsive at common browser resolutions. - Provide
sensible loading states. - Avoid making the mission experience dependent
on unnecessarily large assets.

------------------------------------------------------------------------

## 16. Research and data requirements

Realistic source data is needed before final numerical balancing. The
simplified game units must be derived consistently from trustworthy
source values.

Research should cover: - Engine thrust and propellant compatibility. -
Propellant characteristics and performance. - Fuel consumption /
specific impulse or an equivalent validated performance measure. -
Engine and spacecraft mass where available. - Launch vehicle payload
capacity and destination/orbit capability. - Spacecraft mass and
payload/crew capability. - Launch facility and vehicle compatibility
where relevant. - Mission destination and approximate mission
distance/profile. - Human life-support requirements if modeled. -
Scientific instrument mass, cost proxy, power demand, data generation,
and scientific value if modeled.

### Data integrity rules

-   Clearly distinguish real-world source data from gameplay balancing
    values.
-   Do not invent real specifications.
-   Record the source and units for every real-world engineering value.
-   Document any scaling or conversion into game units.
-   If a value is simplified for playability, label it as a gameplay
    approximation.
-   Validate component compatibility against credible sources.
-   Avoid implying that a simplified game simulation is an operational
    mission-design tool.

------------------------------------------------------------------------

## 17. Open decisions and unresolved requirements

The following items remain open and should not be treated as settled:

  -----------------------------------------------------------------------
  Topic                               Current status
  ----------------------------------- -----------------------------------
  Exact Apollo tutorial scenario      TBD

  Historical fidelity of Mars 2020    TBD
  and Artemis III                     

  Mission objectives and numerical    Need mission-specific definition
  constraints                         

  Sequential unlocking versus all     TBD
  missions available                  

  Exact scope of ROCKETS category     Must distinguish spacecraft from
                                      launch vehicle

  Scientific instruments: fifth card  TBD
  or subpage                          

  Multiple engines / staged rockets   TBD

  Player-selected crew size           TBD

  Exact engine/fuel datasets and      Research required
  game-unit scaling                   

  Exact mass, fuel, thrust, and       Engineering model required
  trajectory formulas                 

  Power management                    TBD

  Communications management           TBD

  Random in-flight events             Future add-on, not initial scope

  Upkeep costs                        Future add-on, not initial scope

  Optimization add-on behavior        TBD

  Exact launch/ascent duration        TBD

  Pause and simulation-speed controls TBD

  Exact camera modes beyond changing  TBD
  angle                               

  Detailed failure-screen interaction TBD

  Final scoring weights and score     Provisional
  calculations                        

  Multiplayer                         Not in initial scope
  -----------------------------------------------------------------------

------------------------------------------------------------------------

## 18. Recommended implementation priorities

### Priority 1 --- Playable core

-   Main menu and mission selection.
-   Short mission briefing.
-   Four-card 2 × 2 configuration hub.
-   Component selection pages.
-   Shared mission configuration state.
-   Budget and core mass/fuel/compatibility validation.
-   Mission readiness state.

### Priority 2 --- Mission simulation

-   3D rocket and spacecraft.
-   Launchpad scene.
-   Ignition, flame, smoke, ascent.
-   Camera controls.
-   Earth-orbit transition.
-   Solar-system trajectory view.
-   Compressed interplanetary travel.
-   Destination arrival and mission outcome.

### Priority 3 --- Learning and evaluation

-   Info/encyclopedia.
-   Contextual explanations.
-   Mission score.
-   Success/failure debrief.
-   Audio and polish.

### Priority 4 --- Expansion

-   Power and communications, if selected.
-   More instruments and spacecraft components.
-   Staged/multi-engine configuration.
-   Random events.
-   Upkeep/operational costs.
-   Optimization add-on.
-   Additional missions.

The team should aim for a complete end-to-end playable mission before
adding optional systems.

------------------------------------------------------------------------

## 19. Acceptance criteria for the first complete playable version

The first complete version should satisfy the following:

-   [ ] Player can open the main menu and enter mission selection.
-   [ ] Player can select a mission and automatically receives its short
    briefing.
-   [ ] Player can reopen the briefing from the inbox icon.
-   [ ] Configuration hub displays four cards in a 2 × 2 grid.
-   [ ] Each card uses an image on the left and title/description on the
    right.
-   [ ] All four categories initially show NOT SELECTED.
-   [ ] Selecting a component updates its card with the component name
    and model/image.
-   [ ] All selection pages update the same mission configuration.
-   [ ] Player can see the relevant component characteristics and costs.
-   [ ] Budget is fixed per mission and updates as components are
    selected.
-   [ ] Engine/fuel and other defined compatibility rules are checked.
-   [ ] Mass, payload, and fuel constraints are validated.
-   [ ] Player receives understandable reasons for blocking issues.
-   [ ] A valid configuration can proceed to launch.
-   [ ] Launch simulation shows a 3D rocket, engine thrust, flame, and
    smoke.
-   [ ] Player can change the camera angle during flight.
-   [ ] Simulation transitions through launch/ascent, Earth/orbit, and
    deep-space presentation as appropriate.
-   [ ] Interplanetary travel is visually compressed to approximately
    10--20 seconds.
-   [ ] Actual and projected trajectory use distinct solid and dotted
    line styles.
-   [ ] Required telemetry is displayed.
-   [ ] Mission success/failure is evaluated against the mission
    objective.
-   [ ] Mission result includes a score and useful engineering debrief.
-   [ ] The UI consistently follows the dark professional
    aerospace-control-room visual direction.

------------------------------------------------------------------------

## 20. Product north star

The game should make players feel that they are responsible for a real
mission, while keeping the experience understandable and enjoyable.

A successful player should finish a mission understanding that: -
Engineering decisions are connected. - More powerful or capable
components are not automatically the best choices. - Mass, fuel, cost,
payload, and mission objectives compete with one another. -
Compatibility and planning matter. - Mission failure can be understood
and learned from. - There can be multiple viable designs, not one
universal perfect solution.

**The player's job is to make the best mission possible with the
resources and constraints they have been given.**
