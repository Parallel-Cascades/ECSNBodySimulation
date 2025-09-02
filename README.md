# ECS N-Body Simulation

This is a basic implementation of orbital mechanics simulation using Unity's Data-Oriented Technology Stack (DOTS).
It leverages the Entity Component System (ECS) for efficient data management and the Job System for parallel processing, 
allowing for high-performance simulations of gravitational interactions between multiple bodies.
The gravity system applies velocity changes to physics entities, which get simulated through the Unity Entities Physics system.

![NBody Gravity Simulation](nbody-sim.png)

This asset is currently in early release/preview and subject to breaking changes. The documentation will also be updated over time.

## Setup

### Requirements

Built with Unity 6000.0 and uses Unity Entities 1.3.14 and Unity Physics 1.3.14

## Usage

### Sample scenes
Check out the two sample scenes to quickly get started using the NBody simulation.
Assets/ParallelCascades/ECSNBodySimulation/Samples/Scenes/NBody Simulation Sample Scene
Assets/ParallelCascades/ECSNBodySimulation/Samples/Scenes/Satellite Spawning Sample Scene

### NBodyAuthoring
To make a body take part in the NBody simulation, add the `NBodyAuthoring` component to a GameObject in a subscene. You can additionally choose
whether an object does not receive gravity (for example if you want to have a static central Star that affects other bodies, but is not affected itself).
Also, you can choose if a body does not contribute to gravity - for example if you want to have small asteroids that are affected by larger bodies, but do not affect them in return.

### NBodyOrbitAuthoring
If you want to automatically make a body orbit around another body at start, add the `NBodyOrbitAuthoring` component to the GameObject. This calculates
a basic initial velocity for a circular orbit. Note that this initial simplified calculating does not take additional bodies into account,
so it's possible orbits quickly become unstable in multi-body simulations, especially with large bodies close together.

You can choose to calculate he orbit direction automatically based on a choice between clockwise or counter-clockwise, or set the Up axis for the
orbit manually, which is useful in some edge cases where the orbiting body lies exactly on the Z axis, above its primary body.

### Orbit Line Authoring
If you want to preview the orbit path of a body in the Editor, add the `OrbitLineAuthoring` component to the GameObject. This will calculate future positions
of the body during baking. If you enable Gizmos in the Scene view or Game view, the orbit path will be drawn through an Editor system.

### Satellite Spawner Authoring
To spawn multiple bodies in orbit around a central body, add the `SatelliteSpawnerAuthoring` component to a GameObject. This will spawn multiple bodies in orbit around the central body
when the scene is loaded.

### Linked Entity Group Component
If an NBody has child entities (for example a visual model attached to a physics entity, or VFX), add the `LinkedEntityGroupAuthoring`
component to the parent. This ensures child entities are removed automatically when the parent is deleted (for example when a smaller body collides with a larger one).
If the NBody is spawned from a prefab (e.g. `SatelliteSpawner`), the `DynamicBuffer<LinkedEntityGroup>` is added automatically.

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

