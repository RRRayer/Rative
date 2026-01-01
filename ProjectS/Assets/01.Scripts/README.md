# ProjectS Code Layout

This folder is organized by module with assembly definitions per module.

Modules
- Core: shared interfaces, small data structs
- Data: ScriptableObject definitions
- Networking: network context and transport wrappers
- Gameplay: combat/skills/projectiles/status effects
- AI: enemy behavior and wave spawning
- Progression: XP, leveling, unlocks
- Classes: class loadouts and passives
- UI: HUD, menus, and presentation
- Systems: game flow and stage rules

Dependency Rules
- Core and Data are dependency roots
- Gameplay/AI/Progression/Classes/Systems depend on Core/Data
- Networking depends on Core only
- UI depends on Core/Data (+ Gameplay for read-only display)
- Editor references all runtime assemblies
