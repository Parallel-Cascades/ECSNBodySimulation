# DOTS N-Body Gravity Simulation System

This is a basic implementation of orbital mechanics simulation using Unity's Data-Oriented Technology Stack (DOTS).
It leverages the Entity Component System (ECS) for efficient data management and the Job System for parallel processing, 
allowing for high-performance simulations of gravitational interactions between multiple bodies.
The gravity system applies velocity changes to physics entities, which get simulated through the Unity Entities Physics system.

![NBody Gravity Simulation](nbody-sim.png)

## Setup

### Linked Entity Group Component Setup
If an NBody has child entities (for example a visual model attached to a physics entity, or VFX), add the `LinkedEntityGroupAuthoring` 
component to the parent. This ensures child entities are removed automatically when the parent is deleted (for example when a smaller body collides with a larger one).
If the NBody is spawned from a prefab (e.g. `SatelliteSpawner`), the `DynamicBuffer<LinkedEntityGroup>` is added automatically.

## Orbital Mechanics Math

### Newton's Law of Universal Gravitation
Every object in the universe attracts every other body in the universe with a force directed along the line of centers of the two objects
that is proportional to the product of their masses and inversely proportional to the square of the distance between them:

$$\Large F = G \frac{m_1 m_2}{r^2}$$
where $G$ is the gravitational constant.

This force is applied to every entity with `NBodyEntity` Component tag, by the `NbodyGravitySystem`.

### Initial Velocity Calculation
To automatically set up initial orbits, a velocity is calculated for entities with `NBodyVelocityBakingData` component in the `NBodyInitialOrbitVelocityBakingSystem`.
To calculate a stable circular orbit for a two-body system, we require the centrifugal acceleration to equal the gravitational acceleration, and therefore
the formula for the velocity of a body in a circular orbit at distance r from the center of gravity of mass M:

$$\Large v = \sqrt{\frac{G M}{r}}$$

Note that this is a simplified calculation using only a two-body system in isolation, ignoring the influence of other bodies, yet
it works well enough to produce near stable initial orbits in multi body simulations.

### Inverse Mass
Since body mass is stored as inverse mass in Unity Physics, gravitational force calculation are adjusted to use inverse mass.

## Known Issues

### Floating-point precision

If you try to simulate the solar system with real-world scales, you will quickly run into floating-point precision issues.
This is a common problem in space simulations, and that is why in the sample scenes we use much smaller scales for distances and masses.

### Gizmos in Game View

When enabling Gizmos in Game View in Unity 6000.0.55f1, the following warning gets logged in the editor when exiting play mode.
```
CommandBuffer: built-in render texture type 3 not found while executing  (SetRenderTarget depth buffer)
UnityEngine.GUIUtility:ProcessEvent (int,intptr,bool&)
```
