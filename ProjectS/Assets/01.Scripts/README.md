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

Player Structure Expansion (feat #2, feat #3)
- feat(#2): Player structure expansion baseline
  - Added module scaffolding and asmdefs for AI/Classes/Core/Data/Gameplay/Networking/Progression/Systems/UI.
  - Introduced class system foundations (ClassDefinition, PlayerClassState, TestClassFactory).
  - Introduced skill system foundations (SkillSlot, ISkillExecutor, PlayerSkillExecutor).
  - Extended PlayerManager to integrate class/skill setup, firing state, and health sync.
  - Added TestLee scene plus test prefabs (Player, Beam) for local validation.
  - Updated input actions and StarterAssetsInputs to include combat/skill inputs.
  - Updated project tags/layers used by the player flow.
- feat(#3): Player structure expansion follow-up
  - Added first-person controller implementations (ProjectS variant and Starter Assets variant).
  - Updated PlayerManager to accommodate first-person controller use and camera targeting.
  - Added Starter Assets - First Person package assets, prefabs, and sample scene.
  - Updated TestLee scene and player prefabs to align with first-person setup.
  - Updated StarterAssets input actions to include FPS-friendly bindings.

.cs File Breakdown (feat #2, feat #3)
- feat(#2)
  - Assets/01.Scripts/Classes/PlayerClassState.cs: player class state holder and assignment API.
  - Assets/01.Scripts/Classes/TestClassFactory.cs: test class definition provider for defaults.
  - Assets/01.Scripts/Gameplay/Skills/PlayerSkillExecutor.cs: execute skill slots, cooldown tracking.
  - Assets/01.Scripts/PlayerManager.cs: player lifecycle, health sync, attack/skill input integration.
- feat(#3)
  - Assets/01.Scripts/FirstPersonController.cs: custom FPS controller (ProjectS variant).
  - Assets/StarterAssets/FirstPersonController/Scripts/FirstPersonController.cs: Starter Assets FPS controller.
  - Assets/01.Scripts/PlayerManager.cs: camera target assignment and FPS controller compatibility.
  - Assets/StarterAssets/InputSystem/StarterAssetsInputs.cs: added attack/skill input flags and handlers.
