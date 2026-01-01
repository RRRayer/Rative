# Repository Guidelines

## Project Structure & Module Organization
- `Assets/00.Scenes`: Unity scenes such as `Launcher.unity` and room scenes.
- `Assets/01.Scripts`: gameplay and UI C# scripts (e.g., `GameManager.cs`, `Launcher.cs`).
- `Assets/02_Prefabs`: reusable prefab assets.
- `Assets/03_Arts`: materials and art assets.
- `Assets/Photon`, `Assets/StarterAssets`, `Assets/TextMesh Pro`: third-party packages.
- `Packages/` and `ProjectSettings/`: Unity package manifests and editor settings.
- `Docs/`: team documentation and notes.

## Build, Test, and Development Commands
- Run locally: open the project in Unity Hub, open `Assets/00.Scenes/Launcher.unity`, then press Play.
- Build: use Unity Build Profiles (see `Assets/Settings/Build Profiles/Windows.asset`) or `File > Build Settings` in the editor.
- Automated tests: no scripted test command is present; use Unity Test Runner if/when tests are added.

## Coding Style & Naming Conventions
- C# files use 4-space indentation and standard C# conventions.
- Types/methods: `PascalCase`; fields/locals: `camelCase`.
- Keep Unity `.meta` files paired with asset changes (always commit both).
- Avoid hand-editing auto-generated `.sln`/`.csproj` files.

## Testing Guidelines
- No dedicated test assemblies are present in `Assets/`.
- If you add tests, place them under a clear folder (e.g., `Assets/Tests/`) and run them via Unity Test Runner (Edit > Test Runner).

## Commit & Pull Request Guidelines
- Commit messages follow Conventional Commits with Korean summaries (e.g., `feat: 클라이언트 동기화 구현`).
- Keep commits scoped to a feature/fix and include relevant `.meta` files.
- PRs should include: short summary, linked issue/task (if any), and screenshots or GIFs for scene/UI changes.

## Asset & Scene Hygiene
- Prefer editing scenes in `Assets/00.Scenes` and prefabs in `Assets/02_Prefabs` to reduce merge noise.
- When modifying third-party assets (Photon/StarterAssets), document the change in the PR.
