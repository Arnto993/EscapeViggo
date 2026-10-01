# ESCAPE VIGGO — PROP AND ENVIRONMENT ART DIRECTION

**Document type:** Prop and environmental object art direction  
**Project:** Escape Viggo  
**Engine:** Unity 6.3 LTS  
**3D source tool:** Blender 5.2 LTS  
**Visual style:** Stylized low-poly industrial comedy  

---

# 1. Purpose

This document defines the visual language for physical objects placed throughout Escape Viggo's maps.

This includes:

- Tools
- Electrical equipment
- Furniture
- Construction materials
- Worksite machinery
- Industrial objects
- Office objects
- Storage objects
- Safety equipment
- Environmental clutter
- Map-specific props
- Interactive gameplay objects

The purpose is to ensure that independently created assets still look like they belong in the same game.

---

# 2. Core Prop Style

Props should follow the same art direction as the characters:

**stylized low-poly**

Objects should be:

- recognizable immediately
- simplified
- chunky
- slightly exaggerated
- readable from several meters away
- inexpensive to render
- relatively fast to produce

Real objects should inspire the design.

Exact industrial replication is not required.

---

# 3. Shape Language

Prefer:

- large shapes
- clear silhouettes
- visible bevels
- simplified proportions
- bold forms
- chunky handles
- large switches/buttons

Avoid:

- excessive tiny parts
- realistic screw heads everywhere
- very thin geometry
- complex internal machinery
- high-frequency detail

Objects should remain understandable even at low resolution.

---

# 4. Exaggeration

Props may exaggerate their most recognizable features.

Examples:

## Drill

Increase:

- battery size
- trigger size
- handle thickness

## Multimeter

Increase:

- screen size
- selector dial
- probe visibility

## Electrical cabinet

Increase:

- handle size
- breaker visibility
- door thickness

## Cable drum

Increase:

- cable thickness
- drum profile

Exaggeration should improve gameplay readability.

---

# 5. Bevels

Most manufactured objects should use small bevels.

Perfect 90-degree razor edges should generally be avoided.

Bevels help low-poly objects catch light and remain readable.

Do not create dense bevel geometry.

Use efficient segments.

---

# 6. Material Language

Materials should use:

- simple base colours
- moderate roughness
- limited metallic response
- clear material separation

Avoid:

- complex procedural materials
- photorealistic dirt
- extremely high-resolution texture sets

Important surfaces may include:

- painted metal
- bare metal
- rubber
- plastic
- wood
- concrete
- glass
- fabric

Each should remain stylized.

---

# 7. Texture Philosophy

Use geometry and colour before textures.

Many props should work using:

- solid colours
- small shared texture atlases
- simple decals

Avoid unique 4K textures for ordinary props.

Texture detail should primarily provide:

- labels
- hazard markings
- numbers
- simple wear
- brand-neutral visual interest

---

# 8. Branding

Avoid real commercial branding unless specifically approved.

Do not recreate:

- Milwaukee
- DeWalt
- Makita
- Bosch
- Hilti
- ABB
- Schneider
- Siemens

logos directly.

Create fictionalized generic equivalents.

For example:

**VIGGO POWER**

or other fictional workplace branding may be used later.

Do not copy protected product shapes unnecessarily closely.

---

# 9. Colour Direction

Environment colours should generally be less saturated than characters.

Character readability has priority.

Recommended environment palette:

- grey
- charcoal
- steel blue
- muted green
- concrete
- beige
- brown
- industrial white

Accent colours:

- safety yellow
- safety orange
- red
- blue

Use accent colours for:

- interaction
- safety
- machinery
- gameplay-relevant objects

---

# 10. Gameplay Readability

Important gameplay objects should be visually distinct.

Examples:

- Work tasks
- Battery station
- Interactive switches
- Tools
- Doors
- Objective props

Players should not need to inspect every random object to determine whether it is interactive.

Use:

- shape
- placement
- colour
- UI prompt

rather than excessive glow effects.

---

# 11. Prop Scale

Use consistent real-world approximate scale.

Unity convention:

**1 Unity unit ≈ 1 meter**

Examples:

Door:

~2.0–2.1 m high

Work table:

~0.8–1.0 m high

Electrical cabinet:

~1.5–2.0 m high

Standard tool:

~0.15–0.5 m

Cable drum:

~0.5–1.5 m diameter

Do not make props arbitrarily small because they are stylized.

---

# 12. Prop Complexity Tiers

Use three rough categories.

## Tier 1 — Hero Props

Important gameplay objects.

Examples:

- Viggo's drill
- battery
- battery station
- task electrical cabinet
- multimeter

These receive more detail.

## Tier 2 — Functional Props

Common medium-detail objects.

Examples:

- toolboxes
- ladders
- cable drums
- workbenches
- junction boxes

## Tier 3 — Background Props

Very simple clutter.

Examples:

- boxes
- buckets
- pallets
- pipes
- cones
- generic containers

Do not spend equal time on every prop.

---

# 13. Viggo's Drill

The drill is a signature prop.

It should be:

- chunky
- stylized
- clearly cordless
- easily visible
- slightly oversized

Important features:

- large battery slot
- large removable battery
- thick grip
- readable trigger
- short broad drill body

Colour direction:

Avoid copying a real power-tool brand.

Possible colour:

**dark charcoal + orange accents**

matching Viggo.

---

# 14. Battery

Viggo's battery is one of the most important objects in the game.

It must be recognizable instantly.

The battery should be:

- large enough to read visually
- chunky
- low-poly
- easy to carry
- clearly detachable

Suggested shape:

- rectangular body
- exaggerated top connector
- handle/grip indentation
- orange accent details

Avoid making it physically tiny like a realistic battery pack.

Gameplay readability is more important.

---

# 15. Battery Station

The battery station should make it obvious where the battery normally belongs.

Possible design:

- workbench
- charging station
- drill holder
- battery charger
- small tool shelf

The empty battery location must remain visible after theft.

This supports Viggo's battery discovery animation.

---

# 16. Electrical Cabinets

Electrical cabinets are common map anchors.

Style:

- boxy
- chunky
- clean silhouette
- large door
- visible handle
- simplified internal breakers

Closed cabinets can be background objects.

Interactive task cabinets may open and show:

- oversized breakers
- simplified wiring
- terminals
- indicator lights

Avoid realistic dense wiring layouts.

---

# 17. Circuit Breakers

Breakers should be simplified.

Use:

- large switch
- visible ON/OFF position
- strong contrast
- clear slots

Players must understand their state immediately.

---

# 18. Electrical Outlets

Sockets/outlets should be recognizable but stylized.

Use:

- thicker faceplate
- simplified screws
- clear socket geometry

Task interaction may exaggerate the internal wire connection points.

---

# 19. Wiring

Cables should be slightly thicker than real-world scale.

Thin wires disappear visually.

Use clear colour coding where appropriate.

Potential colours:

- brown
- blue
- yellow/green
- black
- red

Electrical realism may inspire colours, but gameplay readability comes first.

---

# 20. Cable Trays

Cable trays should be:

- simplified
- modular
- reusable
- easy to snap together

Avoid modelling every perforation.

Use broad repeating geometry.

---

# 21. Cable Drums

Cable drums should be common worksite props.

Visual features:

- large wooden/metal sides
- visible thick cable
- simple axle
- slightly exaggerated dimensions

Cable drums are excellent:

- cover
- line-of-sight blockers
- map decoration

---

# 22. Toolboxes

Toolboxes should be chunky and readable.

Variants:

- red metal box
- dark plastic box
- large rolling toolbox

Tools inside do not need individual detail unless used directly.

---

# 23. Hand Tools

Common tools may include:

- screwdriver
- pliers
- cutters
- wrench
- hammer
- tape measure

Model them simply.

Tools held by characters should use exaggerated handles for readability.

---

# 24. Multimeter

The multimeter is an important future task prop.

It should have:

- large screen
- oversized selector dial
- two obvious probes
- clear cable colours

The screen should be readable in task UI.

---

# 25. Workbenches

Workbenches should use:

- thick top
- sturdy legs
- basic drawers
- tool clutter

Possible materials:

- steel frame
- wood top

They can double as:

- battery station
- task locations
- environmental cover

---

# 26. Pallets

Pallets are useful repeated props.

Low-poly geometry is appropriate.

Use:

- simple wooden slats
- slightly exaggerated thickness

Create modular stacks.

---

# 27. Boxes and Containers

Use varied but reusable boxes.

Examples:

- cardboard boxes
- plastic crates
- electrical equipment cartons

Do not create unique meshes for every box.

Use shared base models with:

- scale changes
- colour changes
- decals

---

# 28. Safety Cones and Barriers

Safety cones should be slightly oversized.

Use strong orange/yellow accents.

Barriers may include:

- cones
- plastic fencing
- warning tape
- portable rails

These can guide player movement naturally.

---

# 29. Fire Extinguishers

Simple stylized fire extinguisher:

- red cylinder
- hose
- handle
- simplified label

Useful as environmental colour accents.

---

# 30. Ladders

Create modular ladders.

Potential forms:

- step ladder
- extension ladder

Keep rung geometry simple.

Avoid excessive collider complexity.

---

# 31. Scaffolding

Construction maps require modular scaffolding.

Build reusable modules:

- vertical frame
- horizontal rail
- plank
- stairs

Keep geometry simple enough to repeat many times.

---

# 32. Site Containers

Construction containers should be modular.

Types:

- storage container
- site office
- tool container

External shape:

- recognizable corrugated silhouette
- simplified corrugation
- large doors

Avoid modelling every ridge individually if normal geometry can imply it efficiently.

---

# 33. Industrial Machinery

Industrial building maps may contain large machines.

These should primarily function as:

- landmarks
- cover
- line-of-sight blockers
- environmental storytelling

Use fictional machinery.

Focus on:

- large motors
- pipes
- tanks
- control panels
- conveyors

Avoid engineering-level detail.

---

# 34. Pipes

Pipes should use modular components.

Create:

- straight segment
- elbow
- T-junction
- valve
- flange

Use several diameter classes.

Pipes should help create strong industrial silhouettes.

---

# 35. Valves

Valves should have oversized readable handles.

Red or blue handles may be used as accents.

---

# 36. Ventilation

Industrial ventilation props:

- ducts
- vents
- fans
- extraction units

Keep duct geometry modular.

These can create:

- overhead detail
- obstacles
- hiding spaces

---

# 37. Lighting Fixtures

Map lighting props may include:

- industrial ceiling lights
- temporary construction lamps
- emergency lights
- office fixtures

Visual lights should match the stylized geometry.

Avoid excessive real-time light sources.

---

# 38. Hurtigbåtterminal Props

The Hurtigbåtterminal map should combine:

- public terminal
- technical facility
- maritime environment

Possible props:

- benches
- ticket machine
- signs
- bollards
- mooring equipment
- life rings
- luggage carts
- dock barriers
- technical cabinets
- maintenance tools
- ropes
- outdoor lighting
- gangway equipment

Use Norwegian/coastal flavour without requiring real-world branding.

---

# 39. Construction Site Props

Core construction-site props:

- scaffolding
- pallets
- timber
- temporary fencing
- generators
- cable drums
- ladders
- site containers
- portable lights
- toolboxes
- electrical cabinets
- construction signs
- wheelbarrows
- bags/material stacks

The map should look active but not cluttered beyond gameplay readability.

---

# 40. Industrial Building Props

Possible props:

- large machines
- conveyors
- workbenches
- cabinets
- storage racks
- pallets
- pipe systems
- cable trays
- motors
- control stations
- compressed-air equipment
- warning barriers

---

# 41. Office Props

Small workplace offices may contain:

- desk
- chair
- filing cabinet
- PC
- monitor
- coffee machine
- mug
- paperwork
- coat rack
- wall clock
- whiteboard

Keep all of these low-poly.

---

# 42. Break Room Props

Potentially useful for humour.

Include:

- coffee machine
- coffee mugs
- table
- chairs
- refrigerator
- microwave
- lunch boxes
- vending machine

Future Viggo gameplay may interact with these areas.

---

# 43. Norwegian Worksite Identity

The environment should subtly feel Norwegian.

Possible cues:

- high-visibility workwear
- road/work barriers
- building style
- Norwegian-language signs
- emergency signage
- industrial colour schemes
- coffee culture
- worksite cabins

Avoid excessive stereotypical decoration.

The environment should feel naturally Norwegian rather than themed.

---

# 44. Signage

Use simplified Norwegian signage.

Examples:

- EL-ROM
- LAGER
- KONTOR
- ADGANG FORBUDT
- HØYSPENNING
- HMS
- NØDUTGANG

Signs help both art direction and navigation.

Use fictional company names.

---

# 45. Environmental Storytelling

Props should sometimes create small workplace stories.

Examples:

- unfinished cable job
- abandoned coffee cup
- tools left on bench
- crooked safety sign
- cable drum blocking corridor
- open cabinet
- paperwork stacked badly

Do not overdo clutter.

The map must remain easy to navigate.

---

# 46. Clutter Density

Clutter should follow gameplay needs.

High clutter:

- storage room
- workshop
- construction staging

Low clutter:

- chase routes
- corridors
- important task zones

Do not obstruct movement merely to make the map look detailed.

---

# 47. Collision Rules

Visual complexity and collider complexity should be separated.

Prefer:

- simple box colliders
- capsule colliders
- primitive compound colliders

Avoid MeshCollider for ordinary props unless necessary.

Do not create complex collision from detailed decorative geometry.

---

# 48. Modular Asset Philosophy

Create reusable sets.

Examples:

## Electrical Kit

- cabinet
- breaker
- junction box
- socket
- cable tray
- cable

## Construction Kit

- scaffold
- pallet
- barrier
- cone
- container
- ladder

## Industrial Kit

- pipe
- valve
- motor
- tank
- control box

This allows Codex to populate maps efficiently.

---

# 49. Variation Through Assembly

Avoid creating dozens of unique meshes.

Create variation through:

- scale
- rotation
- material
- modular assembly
- decals
- different prop combinations

Example:

One crate model can become five variants through colour and stacked placement.

---

# 50. LOD Strategy

LOD is not initially required for small props.

For large repeated assets or large maps, future LOD levels may be useful.

Do not create LOD complexity before profiling shows a need.

---

# 51. Prop Animation

Some props may animate.

Examples:

- electrical cabinet doors
- switches
- breaker toggles
- drill trigger
- battery insertion
- sliding doors
- fans
- machinery

Animations should be simple.

Gameplay state controls animation.

Animation should never own gameplay state.

---

# 52. Interactive Object Hierarchy

Interactive objects should be visually distinguishable but still belong to the environment.

Recommended:

- slightly stronger colour contrast
- cleaner placement
- readable controls
- interaction prompt

Avoid glowing every usable object permanently.

---

# 53. Hero Prop Priority

Initial hero props should be produced in this order:

1. Viggo's battery
2. Viggo's drill
3. Battery station
4. Simple circuit task cabinet
5. Multimeter
6. Toolbox
7. Cable drum
8. Electrical cabinet
9. Common hand tools

These assets support existing gameplay directly.

---

# 54. Blender Workflow

Source assets should live outside runtime Unity folders.

Recommended:

ArtSource/
Props/
Tools/
Electrical/
Construction/
Industrial/
Office/

Exports:

Assets/EscapeViggo/Art/Props/

Codex may create procedural Blender scripts for modular assets.

---

# 55. Prop Naming

Use descriptive names.

Examples:

`PROP_Drill_Viggo_01`

`PROP_Battery_Viggo_01`

`PROP_CableDrum_01`

`PROP_ElectricalCabinet_01`

`PROP_Toolbox_Red_01`

Avoid:

`Cube123`

`NewModelFinal2`

---

# 56. Prefab Naming

Unity prefabs:

`PF_Drill_Viggo`

`PF_Battery_Viggo`

`PF_CableDrum_A`

`PF_ElectricalCabinet_Task`

Keep source mesh naming and gameplay prefab naming separate.

---

# 57. Pivot Placement

Prop pivots should be intentional.

Examples:

Door:

pivot at hinge.

Battery:

pivot near logical grip/center.

Tool:

pivot suitable for hand attachment.

Crate:

bottom centre.

Large machinery:

ground centre.

Correct pivots make procedural placement and animation significantly easier.

---

# 58. Transform Rules

Before export:

- Apply scale
- Ensure predictable orientation
- Clean unused objects
- Keep sensible origin
- Avoid negative scale

Unity import should normally result in:

scale approximately:

`1,1,1`

---

# 59. Technical Validation

Every generated prop should be checked for:

- correct scale
- correct orientation
- correct pivot
- valid mesh
- expected materials
- successful FBX import
- no missing references

Hero props should additionally be inspected visually in Unity.

---

# 60. Art Production Principle

The environment exists to support the players.

When choosing between:

**more clutter**

and

**clearer gameplay**

choose clearer gameplay.

When choosing between:

**realistic detail**

and

**stronger shape**

choose stronger shape.

When choosing between:

**many unique assets**

and

**a strong reusable modular kit**

choose the modular kit.

---

# 61. Canonical Prop Style Summary

An Escape Viggo prop should look like:

> A simplified, slightly chunky low-poly interpretation of a recognizable Norwegian workplace object, with large readable shapes, modest bevels, simple materials and enough exaggeration to remain obvious during fast multiplayer gameplay.

This is the core prop art direction.