# Point-and-Click 101

Point-and-Click 101 is a small learning project built with Godot 4 and C#. It explores the basic systems behind classic point-and-click adventure games such as *Monkey Island* and *Broken Sword*.

The project was created through a **vibe learning** approach: building a simple playable game step by step, asking focused questions, experimenting with Godot, and learning the purpose of each system before moving toward more independent vibe coding.
This is not intended to be a complete or production-ready game. Its main purpose is to serve as a practical reference for beginners learning how common point-and-click mechanics can be implemented.

## Vibe Learning
- The learning phase is done with ChatGPT Sol 5.6
- Basic questions and intents are used as input for ChatGPT
  - Asked just for code snippets
  - Results are re-asked to understand the purpose of each system
- For "Player", https://app.spriterrific.com/ is used both idle and walking animations
- All used arts are generated with ChatGPT and Gemine
  - Mainly with ChatGPT
  - Arts are generated within another session in ChatGPT
- No project folder is shared with ChatGPT in the begining, 
  - The project folder is shared after 2 scene is done.


## What the Project Covers

- **Godot scenes and nodes** — Scenes are used to organize locations, the player, UI elements, hotspots, visual objects, and reusable game components.

- **Click-to-move navigation** — `NavigationRegion2D` and `NavigationAgent2D` allow the player to calculate a path and walk to a clicked position inside a defined walkable area.

- **Player movement and animation** — The player changes between idle and walking animations, faces the current movement direction, and stops when the destination is reached.

- **Perspective-based scaling** — The player's visual scale and movement speed change according to the vertical position in the scene, creating a basic sense of depth.

- **Reusable hotspots** — Interactive objects inherit from a common hotspot structure and expose only the actions supported by that object.

- **Interaction points** — `Marker2D` nodes define where the player should stand before performing an action on a hotspot.

- **Action menu** — Clicking a hotspot opens an icon-based action menu containing options such as Look, Touch, Use, Move, and Talk.

- **Dynamic action availability** — Available actions can change according to previous interactions or game state. 
  - For example, an object can require the Look action before Touch becomes available.

- **Signals** — Signals(GODOT engine) allow the player, hotspots, menus, scene objects, and global systems to communicate without tightly coupling their implementations.

- **Thought bubbles** — Action results and short dialogue lines appear above the player instead of being printed only to the debug console.

- **Thought sequences** — Multiple lines can be displayed in order with configurable delays, allowing simple introductions and character reactions.

- **Viewport-aware UI positioning** — The action menu and thought bubble are kept inside the visible screen area when they appear near an edge.

- **Scene transitions** — Doors, direct exits, transition areas, and map locations can move the player between different game scenes.

- **Scene entry points** — Named `Marker2D` spawn points place the player at the correct position when entering a scene from different directions.

- **Persistent game state** — An autoloaded `GameState` stores information that must survive scene changes, including restored power, collected objects, completed observations, and one-time dialogue.

- **Conditional interactions** — Exits and actions can depend on the current game state. Examples include entering a dark building only after restoring power or leaving an area only after obtaining a map.

- **Environment state changes** — Repairing a cable changes the station's power state, updates the scene visuals, and affects which locations can be entered.

- **Inventory management** — An autoloaded inventory manager stores item identifiers, prevents duplicate items, and announces inventory changes through signals.

- **Inventory UI** — A reusable grid-based panel can be opened from any scene to display collected item icons and tooltips.

- **Collectible objects** — Objects can be inspected, collected, removed from their original scene, and added to the persistent inventory.

- **2D collision and visual ordering** — Collision shapes prevent the player from walking through selected objects, while Y-sorting allows the player to appear in front of or behind scene elements.

- **Simple visual effects** — Godot's particle system can create effects such as smoke and electrical sparks without requiring additional animated images.

- **Debugging and iteration** — Build output, runtime logs, node paths, signal connections, collision visibility, and inspector values are used to identify and fix gameplay problems.

## Current Game Flow

The prototype begins at a spacecraft crash site. The player can move between several outdoor and indoor locations, interact with objects. The Player is the character in the game. He should find a away to go other places in "Nova Map"

The project currently includes these main scenes:

- __Crash Site__
   
   ![Crash Site](Assets/Screen%231.png)
- __Power Offline__

   ![Crash Site](Assets/Screen%232.png)
- __Station Interior__

   ![Crash Site](Assets/Screen%233.png)
- __Nova Map__

   ![Crash Site](Assets/Screen%234.png)
- __Kraker__

   ![Crash Site](Assets/Screen%235.png)

## Controls

- **Left mouse button:** Walk, select hotspots, choose actions...
- **"i" key:** Open or close the inventory.

## Requirements

- Godot 4.7 or a compatible Godot 4 release with .NET support
- A supported .NET SDK
- An optional external C# editor such as JetBrains Rider
  - JetBrains Rider is recommended for its Godot plugin, which provides code completion, debugging, and project management features.

## Running the Project

1. Clone or download the repository.
2. Open `project.godot` using the .NET version of Godot.
3. Allow Godot to import the project assets.
4. Build the C# project if required.
5. Run the main scene.
