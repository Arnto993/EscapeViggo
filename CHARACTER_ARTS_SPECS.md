# ESCAPE VIGGO — CHARACTER ART SPECIFICATION

**Document type:** Character art direction and production specification  
**Project:** Escape Viggo  
**Engine:** Unity 6.3 LTS  
**3D source tool:** Blender 5.2 LTS  
**Primary style:** Stylized low-poly  
**Primary platform:** Windows PC  
**Perspective:** First person multiplayer  

---

# 1. Purpose

This document defines the visual and technical direction for playable characters in **Escape Viggo**.

It should guide:

- Character modelling
- Rigging
- Animation
- Material creation
- Blender automation
- Unity import
- Character prefab setup
- Future cosmetics

The goal is not photorealism.

The goal is to create characters that are:

- immediately recognizable
- funny
- readable at gameplay distance
- relatively inexpensive to produce
- technically simple to animate
- compatible with Unity Humanoid animation
- easy to iterate on using Codex and Blender Python

---

# 2. Core Character Art Direction

Characters should use a:

**Stylized low-poly cartoon aesthetic**

The visual language should emphasize:

- Simple geometry
- Strong silhouettes
- Large readable shapes
- Slightly exaggerated proportions
- Limited surface detail
- Clean colour blocks
- Personality through body shape and movement

Characters should feel like they belong in the same world.

They should not look:

- realistic
- photorealistic
- anatomically perfect
- highly detailed
- gritty
- horror-like
- overly cute
- heavily chibi

The target style is closer to:

**stylized workplace caricature**

than:

**realistic construction worker**

---

# 3. Low-Poly Philosophy

Low-poly does not mean crude.

Models should intentionally simplify shapes.

Prefer:

- large faceted surfaces
- bevelled silhouettes
- simplified clothing
- simplified faces
- simple hair masses
- simple gloves
- chunky boots
- simplified tools and accessories

Avoid modelling:

- individual fabric folds
- shoelaces
- tiny pockets
- zippers
- screws
- complex wrinkles
- individual beard hairs
- highly detailed ears
- realistic skin pores

Small detail should usually be represented through:

- colour
- normal direction
- simple geometry
- material separation

rather than dense geometry.

---

# 4. Polygon Density

Character polygon counts should remain modest.

Exact limits are not critical during early development.

Guideline:

## Main character

Approximately:

**8,000–25,000 triangles**

including clothing but excluding separate held props.

Lower counts are welcome when the silhouette remains good.

Viggo may use somewhat more geometry if required to create his large body shape.

Do not increase geometry simply to make the model smoother.

The faceted low-poly character should remain visible as part of the style.

---

# 5. Character Scale

Characters use real-world approximate Unity scale.

Unity convention:

**1 Unity unit ≈ 1 meter**

Electricians should generally be around:

**1.70–1.90 meters**

Viggo should be:

**approximately 1.60–1.68 meters**

Viggo should therefore be visibly shorter than most electricians.

His visual dominance comes from:

- width
- mass
- exaggerated belly
- body language

not height.

---

# 6. Viggo — Core Character Identity

Viggo is the central character of Escape Viggo.

His silhouette should be identifiable instantly.

He is:

- older
- short
- extremely overweight
- broad
- physically heavy
- grey-haired
- grey-bearded
- slightly worn-out
- expressive
- comically intimidating when angry

Viggo should never look athletic.

Even during Rage Mode he should still look like Viggo.

The comedy comes from seeing an extremely round and exhausted older man suddenly become dangerously motivated.

---

# 7. Viggo Body Shape

Viggo's body is defined primarily by his belly.

His proportions should include:

- short legs
- wide hips
- wide torso
- very large round belly
- thick upper arms
- thick forearms
- relatively short neck
- large rounded head
- broad shoulders
- heavy hands
- large work boots

The belly should project significantly forward.

From the side:

Viggo should almost have a circular or pear-shaped body profile.

From the front:

The torso should be substantially wider than a normal electrician.

The belly should remain readable even while wearing workwear.

---

# 8. Viggo Belly

The belly is one of Viggo's most important visual features.

It should be:

- large
- round
- visually soft despite low-poly geometry
- clearly separated from the chest
- prominent from front and side views

The mesh and rig should allow mild deformation during:

- walking
- breathing
- resting
- Rage locomotion

Avoid simulation-heavy soft-body physics.

If secondary belly movement is later desired, use:

- simple animation
- secondary bone
- mild procedural motion

rather than expensive cloth/soft-body simulation.

---

# 9. Viggo Head and Face

Viggo should clearly look older.

Required features:

- grey hair
- grey facial hair
- large nose
- heavy eyebrows
- round cheeks
- slightly flushed face
- expressive eyes

Hair style:

- short
- somewhat messy
- receding or thinning hair is acceptable
- mostly grey

Facial hair:

Preferred:

**short full beard or heavy grey moustache/beard combination**

Keep facial hair as simple geometric masses.

Do not create hair strands.

---

# 10. Viggo Expressions

Viggo should support exaggerated readable expressions.

Important expression categories:

## Neutral

Slightly tired.

## Happy / satisfied

Wide smile.

## Exhausted

- partly closed eyes
- open mouth
- sagging face

## Confused

Useful during battery discovery.

## Angry

Important for Rage Mode.

- lowered eyebrows
- clenched mouth
- wide eyes or narrowed eyes

## Shocked

Used immediately after discovering the missing battery.

Facial animation may begin simple.

Blend shapes can be added later.

Early production may rely on:

- eyebrow bones
- head movement
- jaw movement
- simple eye changes

---

# 11. Viggo Clothing

Viggo should clearly look like a Norwegian worker/electrician.

Preferred clothing:

- high-visibility work jacket or shirt
- dark work trousers
- reflective stripes
- heavy safety boots
- work gloves
- optional hearing protection
- optional workwear belt

Primary Viggo colour:

**high-visibility orange**

This should distinguish him strongly from the electricians.

Avoid excessive details.

The orange/black silhouette should identify Viggo even from far away.

---

# 12. Viggo Headwear

Preferred initial design:

Viggo does not require a hard hat in every context.

Potential default options:

- simple grey hair visible
- hearing protection
- work cap
- no helmet

Maps or future cosmetics may add helmet variants.

Viggo's grey hair should generally remain visible enough to communicate his age.

---

# 13. Viggo Animation Philosophy

Viggo's personality should come primarily from animation.

Animations should be:

- exaggerated
- readable
- slightly slower than realistic
- humorous
- weighty
- deliberate

Avoid overly smooth motion.

The low-poly visual style works well with slightly exaggerated timing and poses.

---

# 14. Viggo Idle

Viggo should never look completely static.

Idle animation may include:

- subtle body sway
- weight shifting
- belly breathing
- shoulder movement
- head scanning
- occasional posture adjustment

He should look like someone who is already slightly tired before doing anything.

---

# 15. Viggo Walk

Normal Viggo locomotion should communicate weight.

Characteristics:

- shorter steps
- wide stance
- side-to-side hip movement
- belly leads slightly
- upper body lags
- shoulders rotate
- arms swing loosely
- mild vertical bounce

The result should be humorous but believable enough to read as walking.

---

# 16. Viggo Tired Movement

As stamina becomes low:

- shorter steps
- reduced arm swing
- more hunched posture
- head slightly lower
- heavier breathing
- increased belly/torso movement
- slower recovery between steps

A player should be able to visually understand:

**Viggo is about to become exhausted.**

---

# 17. Javell Rest Animation

This is a signature character animation.

When Viggo runs out of stamina:

1. Viggo slows/stops.
2. He shifts his feet into a comfortable stance.
3. He leans backward or settles his weight.
4. He places both hands on his large belly.
5. His shoulders relax.
6. His belly visibly rises/falls slightly as he breathes.
7. Viggo says:

**"Javell."**

8. He remains relaxed for several seconds.
9. He slowly prepares to move again.

IMPORTANT:

Viggo should NOT put his hands on his knees in the canonical Javell animation.

The defining pose is:

**both hands resting on the beer belly.**

The pose should feel almost comfortable rather than medically exhausted.

He is taking a short break because walking is inconvenient.

This should become one of the most recognizable animations in the game.

---

# 18. Battery Discovery Animation

Battery discovery should have comedic timing.

Suggested sequence:

1. Viggo approaches the drill/battery station.
2. Looks toward the expected battery.
3. Brief pause.
4. Looks closer.
5. Slight head tilt.
6. Looks at the empty location again.
7. Freezes for approximately 0.3–0.7 seconds.
8. Looks left.
9. Looks right.
10. Expression changes.
11. Body straightens.
12. Rage reaction begins.

The pause before anger is important.

Comedy comes from:

**realization → silence → rage**

rather than instant reaction.

---

# 19. Rage Locomotion

Viggo should NOT transform into a conventional sprinting athlete.

Preferred Rage locomotion:

**aggressive fast power-walk**

Characteristics:

- torso leaning forward
- fast short steps
- strong forward momentum
- stiff or aggressive arms
- clenched fists
- focused head
- less sideways wobble
- unexpectedly high speed

The contrast should be funny:

Normal Viggo:

slow, exhausted, round

Rage Viggo:

still round, but suddenly terrifyingly determined

---

# 20. Viggo Catch Animation

Capture should remain comedic rather than violent.

Possible behaviour:

- Viggo grabs worker by shoulder
- points aggressively
- briefly shakes fist
- leans toward them
- sends them away

Do not depict:

- punching
- graphic violence
- serious injury

Gameplay capture state remains authoritative.

Animation is presentation only.

---

# 21. Electrician Character Direction

Electricians belong to the same stylized low-poly world.

They should look:

- competent
- practical
- slightly cartoonish
- younger/more mobile than Viggo
- visually lighter

The electrician silhouette should contrast strongly with Viggo.

Electricians should appear:

- taller
- narrower
- less heavy
- more mobile

---

# 22. Electrician Body Variety

Future electrician characters should include varied body types.

Possible variants:

## Standard

Average build.

## Slim

Tall/slimmer worker.

## Stocky

Broader/stronger worker.

## Female

Same art style and workwear language.

Body variation should not affect gameplay.

All characters share equivalent collision/gameplay rules unless explicitly changed by game design.

---

# 23. Electrician Clothing

Core clothing elements:

- work trousers
- work jacket/shirt
- reflective vest or high-visibility panels
- safety boots
- gloves
- tool belt
- hard hat

Electricians should generally use:

**yellow high-visibility accents**

to contrast with Viggo's orange identity.

Potential secondary colours:

- blue
- charcoal
- grey
- red
- purple
- green

These can later help distinguish players.

---

# 24. Electrician Helmets

Hard hats are an important silhouette element.

Potential colours:

- Yellow
- White
- Red
- Blue

Helmet colours can eventually support cosmetics.

Do not attach gameplay meaning to helmet colour by default.

---

# 25. Electrician Faces

Faces should remain simple.

Use:

- large readable eyes
- simplified noses
- simplified mouths
- simple eyebrows

Possible character variation:

- glasses
- beard
- moustache
- ponytail
- different hair silhouettes

Avoid high-detail faces.

Players primarily see other characters at medium distance.

---

# 26. Electrician Animation Style

Electricians should feel much more agile than Viggo.

Movement characteristics:

- longer stride
- faster leg movement
- stronger arm swing
- more upright torso
- responsive changes in direction

Animations can remain slightly exaggerated.

---

# 27. Shared Locomotion Set

Initial shared animation library:

- Idle
- Walk
- Run
- Sprint
- JumpStart
- JumpAir
- Land
- CrouchIdle
- CrouchWalk

These should work with Unity Humanoid retargeting where practical.

---

# 28. Electrician Task Animations

Future contextual animations:

- inspect electrical cabinet
- use screwdriver
- connect wire
- use multimeter
- mount socket
- carry cable
- handle breaker
- interact with panel

Do not create unique full-body animation for every task immediately.

Use a combination of:

- shared animations
- upper-body animation
- IK
- hand constraints

where possible.

---

# 29. Battery Carry Animation

Battery carrier should visually carry or store the battery.

Early version may use:

- battery held in one hand
- battery attached near belt
- battery visibly carried against torso

The carrier should remain recognizable.

Do not make carrier identity obvious at long range if it damages the stealth/social gameplay.

A subtle representation is preferred.

---

# 30. Character Silhouette Rules

Every playable character should remain recognizable when rendered:

- without textures
- at medium distance
- during motion
- against industrial backgrounds

Silhouette readability has higher priority than clothing detail.

---

# 31. Colour Language

Suggested base identity:

## Viggo

- Orange high visibility
- Dark charcoal trousers
- Grey hair
- Grey beard
- Black/dark gloves
- Dark boots

## Electricians

- Yellow high visibility
- Blue/grey/black base workwear
- variable helmet colours
- optional secondary player colours

Do not use highly saturated rainbow colours everywhere.

Industrial environments should allow characters to remain visually dominant.

---

# 32. Material Style

Materials should be simple.

Use:

- low to moderate roughness
- minimal metallic surfaces on clothing
- flat/simple colour variation
- subtle normal detail where needed

Avoid photorealistic fabric shaders.

Skin:

- simple stylized matte material

Hair:

- flat low-poly material

Workwear:

- broad colour areas

Reflective clothing elements may have slightly brighter material behaviour.

---

# 33. Rigging Standard

Playable characters must use a consistent humanoid rig compatible with Unity's Humanoid Avatar system.

Required standard bones include:

- Hips
- Spine
- Chest
- Neck
- Head
- LeftUpperArm
- LeftLowerArm
- LeftHand
- RightUpperArm
- RightLowerArm
- RightHand
- LeftUpperLeg
- LeftLowerLeg
- LeftFoot
- RightUpperLeg
- RightLowerLeg
- RightFoot

Additional bones may exist for:

- fingers
- jaw
- eyebrows
- belly
- clothing
- accessories

The core hierarchy must remain humanoid-compatible.

---

# 34. Rig Consistency

Electrician and Viggo rigs should share a consistent logical structure.

This enables:

- shared animations
- animation retargeting
- reusable IK
- simpler Codex automation

Do not create completely incompatible custom skeletons unless absolutely necessary.

---

# 35. Viggo Secondary Rigging

Viggo may include extra non-Humanoid bones.

Potential examples:

- Belly
- BellySecondary
- Beard
- Jacket

These bones should not interfere with the Unity Humanoid mapping.

The belly bone may later provide subtle:

- walking bounce
- breathing
- resting motion

Keep secondary animation subtle.

---

# 36. Animation Root Motion

Player locomotion animations should be:

**IN PLACE**

Do not use root motion to control multiplayer player movement.

Gameplay movement and network synchronization already determine:

- position
- rotation
- velocity

Animator uses these values to choose animation.

---

# 37. Animator Parameters

Expected future inputs may include:

- Speed
- IsGrounded
- IsCrouching
- IsSprinting
- IsViggo
- ViggoStaminaNormalized
- IsResting
- IsRaging
- IsCaptured
- HasBattery

Exact implementation may evolve.

Art assets should support these states without owning gameplay logic.

---

# 38. Animation Technical Rules

Animations should:

- loop cleanly where applicable
- avoid unintended root translation
- use consistent frame rates
- begin/end in sensible poses
- minimize foot sliding
- preserve humanoid bone mapping

Recommended working frame rate:

**30 FPS**

Higher frame rates are unnecessary unless a particular animation requires them.

---

# 39. Blender Source Structure

Recommended:

ArtSource/
Characters/
BaseHuman/
Electrician/
Viggo/

Animations/
Shared/
Viggo/

Source files should remain separate from Unity runtime assets.

---

# 40. Unity Export Structure

Export character assets into:

Assets/EscapeViggo/Art/Characters/

Suggested structure:

Characters/
Electrician/
Models/
Materials/

Viggo/
Models/
Materials/

Animations/
Shared/
Viggo/

---

# 41. Blender Automation

Codex should prefer procedural Blender Python where it is useful.

Suitable automation targets:

- initial low-poly mesh generation
- armature creation
- naming
- material assignment
- animation generation
- FBX export
- validation renders

Do not force procedural generation where manual Blender editing clearly produces better results.

The pipeline should support both approaches.

---

# 42. Character Production Order

Recommended order:

## C0

Blender → Unity pipeline validation

## C1

Base humanoid test

## C2

Humanoid rig validation

## C3

Basic shared locomotion

## C4

First electrician

## C5

Viggo base model

## C6

Viggo locomotion

## C7

Javell animation

## C8

Battery discovery

## C9

Rage locomotion

## C10

Capture animation

## C11

Polish and secondary animation

---

# 43. Art Review Criteria

Every character iteration should be reviewed for:

1. Silhouette
2. Proportions
3. Personality
4. Readability
5. Rig integrity
6. Animation compatibility
7. Unity scale
8. Multiplayer presentation

Do not judge early models primarily by texture detail.

---

# 44. Canonical Viggo Summary

Viggo should be recognizable from this description alone:

> A short, extremely fat older Norwegian worker with grey hair, grey facial hair, orange workwear, short legs, huge round beer belly and a tired expression. He walks heavily and awkwardly. When exhausted, he places both hands on his large belly, relaxes for a few seconds and says "Javell." When somebody steals his drill battery, the same slow older man suddenly becomes an aggressively fast power-walker.

This description defines the core Viggo character direction.

---

# 45. Character Art Priority

When choosing between:

**more detail**

and

**stronger personality**

choose personality.

When choosing between:

**more geometry**

and

**clearer silhouette**

choose silhouette.

When choosing between:

**realistic animation**

and

**fun readable animation**

choose fun readable animation.