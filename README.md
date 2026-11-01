# By divine decree... EXPLODE!

A planetary billiards puzzle game built in Unity for [The Very Serious Juniper Dev Game Jam](https://itch.io/jam/very-serious-juniper-dev-game-jam).

**[Play the game on itch.io](https://brandonthepika.itch.io/by-divine-decree-explode)**

Have you ever thought of the universe as one giant pool table?

Neither did I. But apparently, the stars make pretty good billiard balls.

Take control of a bored cosmic god with an unusual hobby: smashing planets together. Each level is a physics puzzle where gravity, momentum, and careful positioning determine whether a shot succeeds or sends an entire solar system into chaos.

## Gameplay

The player launches a celestial body through a field of planets, using their gravitational pull to redirect its trajectory. Planets can be added and repositioned during the planning phase, allowing the player to construct a gravitational path before committing to a shot.

The objective is to trigger a chain of collisions and destroy the designated targets.

### Core Mechanics

- **Planetary Gravity:** Celestial bodies exert gravitational forces on nearby objects, bending trajectories and creating opportunities for slingshots.
- **Trajectory Prediction:** Preview a launch's curved path before committing to it, accounting for the gravitational influence of multiple sources.
- **Planet Placement:** Place, reposition, and remove planets during the planning phase. Each level provides a limited number of celestial bodies.
- **Momentum Transfer:** Collisions transfer momentum between objects, allowing one impact to redirect or launch another body.
- **Checkpoints:** Reach checkpoints to establish new restart positions as levels become more complex.
- **Chain Reactions:** Use carefully planned collisions to trigger larger sequences of destruction.

## Physics Implementation

Rather than relying entirely on Unity's built-in gravity, I implemented a custom gravitational force system using `Rigidbody2D` and `AddForce`.

Each gravity source calculates its influence on nearby bodies independently. The resulting forces are combined and applied to the affected object's rigidbody.

The gravitational model uses several parameters to make planetary interactions controllable and suitable for puzzle design:

- **Influence Radius:** Limits how far a gravity source can affect other objects.
- **Distance Falloff:** Reduces gravitational strength as an object approaches the edge of the influence radius.
- **Softening:** Prevents forces from becoming excessively strong at very small distances.
- **Tangential Assist:** Adds a sideways force near a planet, helping objects enter curved or slingshot-like trajectories.
- **Escape Assist:** Provides an outward force when an object is moving too slowly near a gravity source.

The force model is intentionally tuned for gameplay rather than intended to be a physically exact simulation of planetary gravity.

### Trajectory Prediction

One of the central technical challenges was allowing players to see where a launch would travel before actually launching it.

The trajectory preview uses a separate simulation loop. Starting with the proposed launch velocity, it repeatedly calculates the combined gravitational forces acting on the simulated object, updates its velocity, and advances its position.

The predicted positions are then displayed as a sequence of sprites.

This allows the player to see how multiple gravity sources interact with a single trajectory without having to commit to the shot. The preview also accounts for the tangential and escape assists used by the actual gravity system.

### Planning and Simulation

Gameplay is divided into planning and running phases.

During planning, gravity is disabled and the player can aim, adjust a launch, and manipulate planets. When the simulation begins, gravity is enabled and the objects move according to the force system.

The player can return to planning and restore the previous simulation snapshot, making it possible to revise a shot without permanently losing the current setup.

Planet placement also has its own validation system. The planner checks for overlapping objects, prevents invalid placements, and manages the remaining planet allowance for each type.

## Development

The game was developed over seven days for The Very Serious Juniper Dev Game Jam.
The project was developed collaboratively with:

- **Programming:** BrandonThePika
- **Pixel Art:** [scostasr](https://scostasr.itch.io/)
- **Music and Sound Effects:** [Jackalope](https://knigijk1.itch.io/)

The project was built in Unity using C#.

## Repository

This repository contains the Unity project, including its scenes, scripts, assets, and project settings.

The main gameplay systems are organized under `Assets/ScriptBox/`:

- `Gravity/` – Gravity sources, force calculations, and global gravity state.
- `Player/` – Player controls, trajectory prediction, collisions, and checkpoints.
- `Gamestate/` – Planning and running phases, level progression, and planet placement.
- `UI/` – Dialogue, gameplay interface, and interaction systems.
- `Camera/` – Camera movement, visual effects, and game-feel systems.
- `Audio/` – Music and sound management.
