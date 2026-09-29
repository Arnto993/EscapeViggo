# ESCAPE VIGGO — GAME SPECIFICATION

**Document status:** Living design specification  
**Engine:** Unity 6.3 LTS  
**Rendering:** Universal Render Pipeline  
**Primary platform:** Windows PC  
**Perspective:** First person  
**Genre:** Asymmetric online multiplayer party game  
**Target players:** 5–8 players, ideally 8  
**Networking:** Netcode for GameObjects + Unity Multiplayer Services  
**Development language:** C#

---

# 1. High Concept

**Escape Viggo** is a humorous asymmetric first-person multiplayer game set on Norwegian worksites.

Most players are electricians attempting to complete electrical work throughout the worksite.

One player is **Viggo**, an older, round and easily exhausted electrician whose goal is to distract the other workers and prevent them from completing their jobs efficiently.

During the workday, the electricians can secretly attempt to steal the battery from Viggo's cordless drill.

If Viggo discovers that his battery is missing, he becomes furious and enters **Rage Mode**, temporarily transforming the workday from workplace chaos into a chase.

The game should create memorable situations through player interaction rather than scripted comedy.

A complete match represents one Norwegian working week:

- Monday
- Tuesday
- Wednesday
- Thursday
- Friday

Each day runs from:

**07:00–15:00**

Time is accelerated.

Players accumulate points throughout the week.

---

# 2. Core Fantasy

## Electrician fantasy

The electrician experience should feel like:

> "We are trying to get our work done while this ridiculous coworker keeps making everything harder, and at some point somebody is definitely going to steal his battery."

Electricians should feel:

- More mobile than Viggo.
- Productive when left alone.
- Nervous when committing battery theft.
- Vulnerable once Rage Mode begins.
- Encouraged to cooperate.
- Encouraged to create chaos.
- Free to betray or mislead each other socially.

---

## Viggo fantasy

The Viggo experience should feel like:

> "Nobody is getting any work done while I'm here — and if someone touches my battery, they're finished."

Viggo should feel:

- Physically intimidating.
- Annoying.
- Disruptive.
- Slower and more cumbersome than the electricians.
- Limited by poor stamina.
- Dramatically more dangerous when angry.

Viggo should NOT simply be a horror-game monster.

Much of his gameplay should revolve around workplace disruption, observation, suspicion and social interaction.

---

# 3. Tone

The game is:

- Comedic.
- Chaotic.
- Social.
- Slightly absurd.
- Norwegian in character.
- Inspired by workplaces and construction sites.

The game is NOT:

- A realistic electrician simulator.
- A horror game.
- A serious industrial simulation.
- A tactical shooter.
- A competitive esports title.
- A physics sandbox with no objectives.

The tone should come primarily from:

- Player behaviour.
- Viggo's mechanics.
- Situational comedy.
- Audio.
- Animation.
- Workplace interactions.

Avoid relying entirely on memes or scripted jokes.

---

# 4. Player Count

Target match:

**8 players**

- 1 Viggo
- 7 electricians

The architecture should eventually support approximately:

**5–8 total players**

Possible configurations:

- 4 electricians + Viggo
- 5 electricians + Viggo
- 6 electricians + Viggo
- 7 electricians + Viggo

The earliest prototype only needs enough support to prove multiplayer gameplay with 2–4 simultaneous instances.

Networking architecture must not assume exactly eight players.

---

# 5. Match Structure

A normal complete match represents one work week.

## Monday
Day 1.

## Tuesday
Day 2.

## Wednesday
Day 3.

## Thursday
Day 4.

## Friday
Final day.

Scores accumulate across all five days.

At the end of Friday, the final scoreboard appears.

---

# 6. Workday

Every workday represents:

**07:00–15:00**

The eight-hour shift is compressed into several real-world minutes.

Target normal duration:

**12–15 real minutes**

The duration must remain configurable.

Development presets should include:

- Debug: 1–2 minutes
- Prototype: 5 minutes
- Short match: 8 minutes
- Normal: 12–15 minutes

The simulation must not depend on a hard-coded real-time duration.

The authoritative game state should represent current workday time independently from UI presentation.

---

# 7. Core Gameplay Loop

## Electricians

1. Spawn at the worksite.
2. Locate available work orders.
3. Travel to a task.
4. Begin the electrical minigame.
5. Complete the job.
6. Earn points.
7. Move to another work order.
8. Deal with Viggo's interference.
9. Watch for opportunities to steal his battery.
10. Steal, hide, carry, drop or later pass the battery.
11. Escape or hide when Rage Mode begins.
12. Survive until 15:00.
13. Review results.
14. Repeat the next workday.

---

## Viggo

1. Spawn at the worksite.
2. Observe the electricians.
3. Locate active work.
4. Distract or interrupt workers.
5. Waste their time.
6. Protect his belongings.
7. Manage stamina.
8. Rest when exhausted.
9. Eventually notice suspicious activity.
10. Discover that the battery is missing.
11. Enter Rage Mode.
12. Hunt for the thief.
13. Recover the battery or catch workers.
14. Continue until 15:00.
15. Review results.

---

# 8. Player Roles

There are two roles.

## Electrician

Multiple players.

Primary objective:

**Complete productive work and accumulate score.**

Secondary objective:

**Steal Viggo's battery and successfully keep it away from him.**

---

## Viggo

Exactly one player per workday.

Primary objective:

**Reduce electrician productivity through disruption.**

Secondary objective:

**Protect or recover the battery and punish the thief.**

Role authority must be determined by the host/server.

Clients must not independently assign themselves roles.

---

# 9. Role Selection

Initial prototype:

- Host/server randomly assigns one player as Viggo.
- Remaining players become electricians.

Later versions may support:

- Role preference.
- Rotation.
- Weighted randomness.
- Preventing the same player from repeatedly becoming Viggo.
- Lobby voting.
- Private games with manual role selection.

These systems are outside the initial prototype.

---

# 10. Electrician Movement

Electricians use conventional first-person controls.

Required:

- WASD movement.
- Mouse look.
- Sprint.
- Jump.
- Crouch.
- Interact.

Potential later additions:

- Leaning.
- Mantling.
- Climbing ladders.
- Carrying large objects.
- Tool animations.

Movement should initially prioritize responsiveness over realism.

---

# 11. Viggo Movement

Viggo uses the same basic first-person controls but has different movement characteristics.

Viggo should feel:

- Heavy.
- Large.
- Surprisingly threatening over short distances.
- Unable to maintain long pursuits under normal conditions.

Configurable values should include:

- Walk speed.
- Chase/sprint speed.
- Acceleration.
- Stamina capacity.
- Stamina drain.
- Stamina recovery.
- Forced rest duration.
- Rage speed multiplier.

Do not spread these values through multiple scripts.

Use centralized tuning data.

---

# 12. Viggo Stamina

Outside Rage Mode, movement consumes Viggo's stamina.

The exact balance will be tuned through playtesting.

General behaviour:

1. Viggo moves.
2. Stamina decreases.
3. Stamina reaches zero.
4. Viggo becomes exhausted.
5. A forced rest begins.
6. Viggo temporarily cannot pursue workers effectively.
7. Stamina recovers.
8. Viggo can move normally again.

A stamina bar should eventually be visible to the Viggo player.

Electricians should not see Viggo's exact stamina percentage.

They should learn his exhaustion state through:

- Animation.
- Sound.
- Heavy breathing.
- Movement behaviour.

---

# 13. Viggo Rest

When Viggo becomes exhausted he must rest.

During rest:

- Movement is disabled or severely reduced.
- Viggo cannot catch players.
- A resting animation will eventually play.
- Viggo produces a voice line.

Signature voice line:

**"Javell."**

The prototype may use text or a placeholder audio clip until final voice material exists.

The system must not require finished audio to function.

---

# 14. Rage Mode

Rage Mode is one of the core mechanics of Escape Viggo.

Rage Mode begins when Viggo **discovers that his battery has been stolen.**

Battery theft itself does NOT immediately activate Rage Mode.

During Rage Mode:

- Normal stamina restrictions are removed or greatly reduced.
- Viggo can pursue workers continuously.
- Viggo moves faster.
- Viggo cannot enter normal forced-rest behaviour.
- Audio presentation changes.
- Visual presentation changes.
- Electricians receive clear feedback that Rage Mode has begun.

Rage Mode should produce an obvious change in atmosphere.

Potential future presentation:

- Alarm sting.
- Viggo shouting.
- Music transition.
- UI warning.
- Lighting changes.
- Camera effects.

Keep presentation separate from game state.

---

# 15. Rage Mode End Conditions

Prototype default:

Rage Mode lasts until one of the following occurs:

- Viggo successfully recovers the battery.
- Viggo catches the current battery carrier.
- The workday ends.

This behaviour must be configurable because balancing may later require different rules.

---

# 16. The Battery

Initially there is one important battery object.

The battery belongs to Viggo's cordless drill.

It has several possible states.

Suggested state model:

- AtDrill
- BeingStolen
- CarriedByElectrician
- Dropped
- Recovered
- Secured

The server/host must be authoritative over this state.

The battery must never exist as two legitimate copies because of client disagreement.

---

# 17. Battery Theft

An electrician should be able to approach the battery and attempt to steal it.

Initial interaction:

1. Player gets within interaction range.
2. Player looks at the battery.
3. Interaction prompt appears.
4. Player holds the interaction button.
5. Progress fills for a configurable duration.
6. Theft succeeds.
7. Battery becomes carried by that player.

Initial theft duration:

Approximately **2–4 seconds**, configurable.

The theft should produce some risk.

Potential signals:

- Small tool sound.
- Interaction animation.
- Player must remain stationary.
- Theft cancels if player leaves range.

Do not require complex animation in the prototype.

---

# 18. Battery Discovery

The battery theft must remain initially secret from Viggo.

Viggo discovers the missing battery when he interacts with or checks its normal location.

Future alternatives may include:

- Trying to use the drill.
- Automatic discovery after a long delay.
- A coworker informing Viggo.
- Security systems.

Prototype implementation:

**Viggo must physically return to the battery station and interact/check.**

If the battery is gone:

1. The authoritative game state marks the theft as discovered.
2. Rage Mode begins.
3. All clients receive the Rage Mode state.
4. Appropriate presentation begins.

---

# 19. Battery Carrier

The authoritative game state must know:

- Whether the battery has been stolen.
- Which player currently carries it.
- Who originally stole it.
- Whether Viggo has discovered the theft.
- Whether the battery has been recovered.

The player who originally stole the battery and the current carrier may eventually be different players.

Architecture must therefore avoid assuming:

`OriginalThief == CurrentCarrier`

even if passing the battery is implemented later.

---

# 20. Battery Passing

Not required in the first prototype.

The architecture should make future battery passing possible.

Possible flow:

1. Player A steals battery.
2. Player A gives battery to Player B.
3. Viggo catches Player A.
4. Battery remains with Player B.

This creates social deception and should be supported later.

---

# 21. Catching Electricians

Viggo can catch an electrician when within a defined capture range.

The capture must be validated authoritatively.

A client must not be able to award itself a capture.

Possible prototype interaction:

- Viggo gets sufficiently close.
- Viggo presses Interact/Catch.
- Server validates distance and state.
- Capture succeeds.

On capture:

- Electrician loses points or receives another penalty.
- Carried battery is dropped/recovered as appropriate.
- Viggo receives score.
- Player is briefly removed from immediate action.
- Player later returns.

---

# 22. No Permanent Elimination

Caught electricians must NOT be permanently eliminated for the rest of the workday.

Long spectator periods would undermine the party-game design.

Prototype punishment:

- Temporary freeze/timeout.
- Respawn at worker spawn after several seconds.
- Score penalty.
- Drop carried battery.

Exact values are configurable.

---

# 23. Hiding

The map should contain areas that naturally break line of sight.

Examples:

- Storage rooms.
- Equipment.
- Containers.
- Offices.
- Machinery.
- Construction materials.
- Technical rooms.
- Corners.
- Toilets.

Initially hiding is primarily physical level design.

Dedicated hide-in-object mechanics such as lockers are optional future additions.

---

# 24. Electrical Work System

Electrical tasks are the main productive activity for electricians.

The task system must be modular.

Base concept:

`WorkTask`

A task should expose data such as:

- Task ID.
- Task type.
- State.
- Difficulty.
- Score reward.
- Interaction location.
- Required duration.
- Required tool, if any.
- Assigned minigame or completion behaviour.

Suggested states:

- Available
- InProgress
- Completed
- Failed
- Interrupted
- Disabled

The server owns completion state.

---

# 25. Electrical Task Philosophy

Electrical tasks should be:

- Recognizable.
- Accessible to non-electricians.
- Inspired by real work.
- Mechanically understandable.
- Short.
- Replayable.
- Slightly skill-based.
- Easy to interrupt.

Electrical accuracy is welcome where it improves flavour.

Realism must not override fun.

Players should not need electrician training to understand the game.

---

# 26. Planned Electrical Tasks

Potential task types include:

## Closing circuits

Possible gameplay:

- Connect wires.
- Choose correct terminals.
- Complete a simplified circuit.

## Measuring voltage

Player uses a multimeter.

Potential interaction:

- Select measurement mode.
- Touch correct measurement points.
- Identify voltage.

## Replace electrical outlet

Potential sequence:

- Remove cover.
- Disconnect conductors.
- Reconnect conductors.
- Install outlet.
- Replace cover.

## Install circuit breaker

Potential sequence:

- Select breaker.
- Install breaker.
- Connect conductors.
- Confirm installation.

## Cable routing

Route cable:

- Along wall.
- Through ceiling.
- Along cable tray.
- Between defined endpoints.

## Troubleshooting

Potential later task:

- Identify a fault.
- Test circuit.
- Replace incorrect component.

---

# 27. First Prototype Task

Only ONE real task type is required initially.

Recommended prototype:

**Simple Circuit Task**

Interaction:

1. Electrician approaches task panel.
2. Starts task.
3. Simple UI appears.
4. Player must activate/connect 3 elements in the correct sequence.
5. Completion awards score.
6. Task becomes completed for all players.

The purpose is to prove:

- Task discovery.
- Interaction.
- UI.
- Network authority.
- Completion.
- Scoring.

It does NOT need to look realistic.

---

# 28. Task Interaction

A generic interaction system should support:

- Tasks.
- Battery.
- Battery station.
- Future doors.
- Pickups.
- Viggo distractions.

Use a reusable interaction interface/component rather than writing a separate raycast implementation for every object.

Possible concept:

`IInteractable`

with player interaction routed through one central player interaction component.

Do not over-engineer this interface before it is necessary.

---

# 29. Viggo Distraction System

Viggo should eventually have several ways to interfere with work.

Potential mechanics:

- Interrupt worker.
- Talk to worker.
- Block access.
- Steal/move tools.
- Create pointless work.
- Shut off power.
- Demand help.
- Move equipment.

Initial prototype requires only ONE basic distraction.

Recommended prototype:

**Task interruption**

Viggo interacts with an electrician performing a task and temporarily interrupts or resets a portion of task progress.

This establishes the gameplay relationship without requiring a large content system.

---

# 30. Scoring Philosophy

Scoring should reward:

- Productivity.
- Risk.
- Successful battery theft.
- Effective disruption.
- Captures.

Exact balance will change repeatedly.

Therefore:

**Scores must be data-driven/configurable.**

Avoid hard-coded values throughout gameplay logic.

---

# 31. Electrician Scoring

Possible sources:

- Complete work task.
- Complete difficult task.
- Steal battery.
- Keep battery until end of day.
- Successfully avoid capture.
- Assist another worker.

Potential future bonuses:

- Speed.
- Accuracy.
- Consecutive jobs.
- Teamwork.
- Perfect electrical work.

---

# 32. Viggo Scoring

Possible sources:

- Interrupt work.
- Cause task failure.
- Delay workers.
- Catch electrician.
- Catch battery carrier.
- Catch original thief.
- Recover battery.
- Prevent productivity.

Viggo should not gain passive points merely for existing.

His score should come from observable disruption.

---

# 33. Score Ownership

The authoritative multiplayer instance owns scores.

Clients may display predicted UI but may not authoritatively decide:

- Task reward.
- Capture reward.
- Battery reward.
- Final score.

Score events should be processed through a central scoring system.

---

# 34. Team Productivity

In addition to individual score, track:

**Electrician Team Productivity**

This can initially equal the combined task score of all electricians.

Potential future uses:

- Daily quota.
- Bonuses.
- End-of-day rating.
- Viggo victory condition.

---

# 35. Work Quota

A future system may give electricians a required amount of work per day.

Example:

> Complete at least 70 productivity points before 15:00.

Missing the quota could benefit Viggo.

This is NOT required for the first playable prototype.

---

# 36. HUD

Electrician HUD should eventually include:

- Crosshair.
- Interaction prompt.
- Current time.
- Current day.
- Personal score.
- Task information.
- Battery indicator when carrying battery.

Viggo HUD should eventually include:

- Crosshair.
- Interaction prompt.
- Current time.
- Score.
- Stamina.
- Rage state.

Prototype HUD may be visually simple.

---

# 37. Workday Clock UI

The workday clock should display an accelerated workplace time such as:

**09:37**

It begins at:

**07:00**

and ends at:

**15:00**

Clock display must derive from authoritative match time.

Do not run independent client clocks that determine gameplay state.

---

# 38. End of Day

At 15:00:

1. Player control may be temporarily limited.
2. Active tasks stop.
3. Rage Mode stops.
4. Daily scoring finalizes.
5. Statistics are collected.
6. End-of-day screen appears.

---

# 39. End-of-Day Screen

Display:

- Day.
- Player names.
- Roles.
- Individual scores.
- Tasks completed.
- Captures.
- Battery outcome.
- Daily productivity.
- Viggo score.

Potential humorous statistics:

- Most productive electrician.
- Least productive electrician.
- Longest chase.
- Most time hiding.
- Most interrupted worker.
- Battery thief.
- Most captures.
- Most exhausted Viggo moment.

Statistics should eventually contribute strongly to the party-game tone.

---

# 40. Work Week Progression

Prototype does not need the full five-day loop.

Final flow:

Lobby  
→ Monday  
→ Results  
→ Tuesday  
→ Results  
→ Wednesday  
→ Results  
→ Thursday  
→ Results  
→ Friday  
→ Final results

Scores carry between days.

Potential future variation:

- Different random task layout each day.
- Different battery location.
- Different events.
- Rotating Viggo.
- Increasing difficulty.

---

# 41. Maps

Planned worksite themes:

## Hurtigbåtterminal

Potential locations:

- Passenger area.
- Dock.
- Technical room.
- Electrical room.
- Storage.
- Office.
- Outdoor service area.

## Construction Site

Potential locations:

- Partially completed building.
- Containers.
- Scaffolding.
- Temporary power.
- Site office.
- Storage.
- Utility tunnels.

## Industrial Building

Potential locations:

- Production floor.
- Electrical room.
- Cable trays.
- Machinery.
- Maintenance room.
- Warehouse.
- Offices.

---

# 42. Prototype Worksite

The first development map is:

**Prototype Worksite**

It is constructed primarily using Unity primitives.

Required areas:

- Main work area.
- Corridor.
- Electrical room.
- Storage room.
- Small office.
- Viggo spawn.
- Electrician spawn.
- Battery station.
- At least five task positions.
- Multiple line-of-sight breaks.

The map exists to test gameplay, not graphics.

Do not spend early development time creating final art.

---

# 43. Level Design Principles

Maps should support:

- Multiple routes.
- Shortcuts.
- Chases.
- Line-of-sight breaks.
- Visible activity.
- Areas where workers naturally gather.
- Risky battery access.
- Opportunities for Viggo to intercept players.

Avoid:

- Excessive dead ends.
- Very long empty corridors.
- Huge unused spaces.
- Hiding locations with no counterplay.
- One dominant route.

---

# 44. Multiplayer Architecture

Use:

**Netcode for GameObjects**

with Unity's GameObject/MonoBehaviour workflow.

Use Unity's current Multiplayer Services package for session/Relay functionality when that milestone is reached.

Prototype networking may begin with local Host/Client connections before internet sessions are implemented.

---

# 45. Network Authority

Important game state must be host/server authoritative.

This includes:

- Role assignment.
- Player registration.
- Workday state.
- Match timer.
- Task completion.
- Score.
- Battery state.
- Battery carrier.
- Rage Mode.
- Captures.
- Daily result state.

Clients own local input.

Clients should not be trusted to declare gameplay rewards.

---

# 46. Networked Player

Each network player needs data including:

- Network client ID.
- Display name.
- Role.
- Score.
- Capture state.
- Battery ownership state where relevant.

Player movement networking should prioritize acceptable party-game responsiveness over competitive shooter precision.

---

# 47. Host Model

Initial architecture:

**Listen server / player host**

One player's machine acts as:

- Server authority.
- Local client.

Other players connect as clients.

Dedicated servers are outside initial scope.

Code should avoid unnecessary assumptions that make later migration impossible, but do NOT build a dedicated-server architecture prematurely.

---

# 48. Sessions and Relay

Local multiplayer should be proven before cloud services are added.

Later milestone:

- Create private session.
- Generate join code.
- Join using code.
- Connect through Unity Relay/session infrastructure.

Matchmaking is NOT required initially.

---

# 49. Multiplayer Testing

Development should support multiple local players as conveniently as practical.

Use Unity-supported multiplayer testing tooling where appropriate.

The project should permit testing:

- Host.
- At least one client.
- Role assignment.
- Movement.
- Task synchronization.
- Battery synchronization.
- Rage Mode.
- Capture.

---

# 50. Scene Architecture

Suggested scenes:

## Bootstrap

Persistent startup and service initialization.

## MainMenu

Menu and multiplayer connection flow.

## PrototypeWorksite

Gameplay scene.

More maps may later become additional gameplay scenes.

Do not create many scenes before needed.

---

# 51. Game State

Use an explicit game-phase model.

Suggested phases:

- Boot
- Lobby
- Loading
- PreDay
- Workday
- Rage
- EndOfDay
- WeekResults

Rage may also be implemented as a match modifier/state rather than replacing Workday if that produces cleaner architecture.

Avoid scattered boolean combinations such as:

`isGameStarted && !isEnded && isRaging && !isPaused`

when a clear state model would be simpler.

---

# 52. Architecture Principles

Prefer:

- Small focused components.
- Composition.
- Explicit responsibilities.
- ScriptableObjects for tuning/configuration where appropriate.
- Events for decoupling presentation from game state.
- Interfaces only when they solve an actual boundary.

Avoid:

- Giant universal GameManager.
- Excessive singleton usage.
- Static global mutable state.
- Premature abstraction.
- Deep inheritance hierarchies.
- Framework-building for imagined future needs.

---

# 53. Suggested Major Systems

The project will eventually contain systems approximately corresponding to:

- Bootstrap
- Networking
- Lobby/session
- Player spawning
- Player movement
- Player interaction
- Role management
- Match state
- Workday clock
- Work tasks
- Scoring
- Battery
- Viggo stamina
- Viggo Rage Mode
- Capture
- UI
- Audio
- Statistics

Names may evolve if a cleaner architecture emerges.

---

# 54. Data and Tuning

Important gameplay tuning must be easily adjustable.

Examples:

- Day duration.
- Player speed.
- Viggo speed.
- Viggo stamina.
- Rest time.
- Rage speed.
- Interaction range.
- Battery theft time.
- Capture range.
- Capture penalty.
- Task rewards.
- Battery rewards.

Use centralized configuration assets where appropriate.

---

# 55. Save Data

Persistent save progression is NOT required in the prototype.

Future possibilities:

- Player cosmetics.
- Statistics.
- Settings.
- Achievements.

Weekly match state only needs to persist for the duration of the current match.

---

# 56. Audio

Audio is essential to final game feel.

Important categories:

- Worksite ambience.
- Electrical tools.
- Footsteps.
- Viggo footsteps.
- Viggo breathing.
- Interaction sounds.
- Task sounds.
- Battery theft.
- Battery discovery.
- Rage transition.
- Chase music.
- End-of-day sound.
- Voice lines.

Development must work with placeholder audio.

---

# 57. Viggo Voice

Signature line:

**"Javell."**

Intended delivery:

- Northern Norwegian flavour.
- Distinctive.
- Comedic.
- Slightly weary.

Additional lines may eventually trigger for:

- Resting.
- Discovering battery theft.
- Catching a worker.
- Seeing poor work.
- Being annoyed.
- End of day.

Final voice recordings are outside programming scope until assets are supplied.

---

# 58. Art Direction

Target:

- Stylized.
- Readable.
- Exaggerated.
- Colourful industrial environments.
- Strong silhouettes.
- Moderate graphical requirements.

Avoid photorealism as a requirement.

Characters should remain readable during multiplayer chaos.

---

# 59. Viggo Character Design

Viggo is:

- Older.
- Large and round.
- Distinctive.
- Norwegian workwear-inspired.
- Immediately recognizable.

His shape should support comedic animation.

Early prototype:

Use primitive/capsule placeholder representation.

Do NOT delay gameplay development waiting for a finished Viggo model.

If the character is based on a real individual, final commercial use of name, likeness or voice should only use material the project has permission to use.

---

# 60. Electrician Characters

Early prototype:

Identical placeholder characters with simple visual differentiation.

Later:

- Workwear colours.
- Helmets.
- Cosmetic customization.
- Character styles.
- Name tags.

Avoid gameplay advantages from cosmetics.

---

# 61. Communication

Future multiplayer communication may include:

- Voice chat.
- Proximity voice.
- Text chat.

Proximity voice is potentially highly valuable for the game's social gameplay.

It is NOT part of the first prototype.

The gameplay architecture must function without voice chat.

---

# 62. Input

Initial target:

Keyboard + mouse.

Suggested defaults:

- WASD — move.
- Mouse — look.
- Shift — sprint.
- Space — jump.
- Ctrl/C — crouch.
- E — interact.
- Escape — menu.

Use Unity's current Input System.

Input mappings should be configurable and not embedded directly into gameplay logic.

---

# 63. Accessibility and Settings

Not prototype-critical, but architecture should permit:

- Mouse sensitivity.
- Volume controls.
- FOV.
- Key rebinding.
- Subtitle support.
- Motion effect settings.

Avoid implementing all settings during initial milestones.

---

# 64. Security/Cheating Scope

This is initially a casual party game.

Do not build extensive anti-cheat.

However, maintain reasonable server authority over:

- Scoring.
- Tasks.
- Battery.
- Captures.
- Role state.
- Match progression.

This prevents obvious client-side manipulation while avoiding unnecessary complexity.

---

# 65. Prototype Scope

The first complete playable prototype must contain:

- First-person controls.
- Multiplayer host/client.
- Multiple synchronized players.
- Viggo role.
- Electrician role.
- Prototype worksite.
- Workday timer.
- One electrical task.
- Scoring.
- Viggo stamina.
- Forced Viggo rest.
- Battery theft.
- Battery discovery.
- Rage Mode.
- Basic chase.
- Capture.
- End-of-day screen.

No final character models are required.

No finished animation is required.

No final audio is required.

No matchmaking is required.

No voice chat is required.

No five-day progression is required.

---

# 66. First Playable Success Criteria

The first playable is successful when:

1. Two or more game instances can join the same session.
2. One player becomes Viggo.
3. Other players become electricians.
4. Players can move independently.
5. All players see synchronized movement.
6. Electricians can interact with a work task.
7. Completing the task awards authoritative score.
8. Viggo consumes stamina.
9. Viggo is forced to rest.
10. An electrician can steal the battery.
11. Viggo does not immediately know the battery is missing.
12. Viggo can inspect the battery station.
13. Missing battery activates Rage Mode.
14. Rage Mode changes Viggo's movement/stamina behaviour.
15. Viggo can catch an electrician.
16. Captures are synchronized.
17. The workday reaches 15:00.
18. Gameplay stops correctly.
19. An end-of-day scoreboard appears.
20. The project compiles without errors.

This establishes the core game.

---

# 67. Prototype Non-Goals

Do NOT prioritize these before the core prototype is stable:

- Finished art.
- Character customization.
- Steam integration.
- Matchmaking.
- Dedicated servers.
- Voice chat.
- Achievements.
- Cosmetics.
- Progression system.
- Multiple maps.
- Multiple task minigames.
- Advanced animation.
- Dynamic weather.
- Full five-day progression.
- AI players.
- Complex inventory.
- Steam Workshop.
- Console support.

---

# 68. Expansion Possibilities

The concept is intentionally expandable.

Possible future features:

- Battery passing.
- Multiple batteries.
- Tool theft.
- Work quotas.
- Lunch break.
- Coffee break.
- Overtime.
- Apprentice role.
- Foreman.
- Safety inspector.
- Random workplace incidents.
- Power outages.
- Broken tools.
- Additional Viggo distractions.
- Security cameras.
- Evidence of theft.
- Footprints.
- Player accusations.
- Proximity voice.
- Random work orders.
- Daily modifiers.
- Multiple Viggo abilities.
- Cosmetic progression.
- Map events.
- Environmental hazards.
- Seasonal content.

These are ideas, not commitments.

---

# 69. Player Experience Goal

The most important design principle is:

**The systems should create stories.**

A successful match should produce moments such as:

> "I stole the battery while Viggo was talking to someone else, hid in the electrical room, gave it to another electrician, and Viggo spent the rest of the shift chasing the wrong guy."

That kind of unscripted story is more valuable than a large amount of scripted content.

---

# 70. Development Priority

When a choice must be made, prioritize in this order:

1. Does multiplayer work?
2. Is the core interaction fun?
3. Is the game state reliable?
4. Can it be tested easily?
5. Can the system be expanded?
6. Does it communicate clearly to players?
7. Does it look good?

Gameplay and multiplayer correctness come before polish.

---

# 71. Design Authority

This file defines the intended game.

When implementation choices conflict with this document:

- Preserve the core game fantasy.
- Preserve multiplayer authority.
- Prefer configurable solutions.
- Prefer the simplest implementation that supports the current milestone.

If an important design decision is genuinely unspecified, do not silently invent a major new system.

Record the open question and use the simplest reversible default.

---

# 72. Definition of the Core of Escape Viggo

At its simplest, Escape Viggo is:

**Electricians trying to work, Viggo trying to prevent them from working, and everybody knowing that eventually someone is going to steal his battery.**

Every major feature should strengthen that loop.