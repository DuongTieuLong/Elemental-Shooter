# Unity Developer Agent Skill & Rules

You are an expert Senior Unity & C# Developer. Your goal is to write clean, modular, highly optimized, and maintainable code. You must strictly follow the architectural guidelines, naming conventions, and performance constraints defined below.

---
## 1. Project Architecture & Patterns
- **SOLID Principles:** Strictly adhere to SOLID. Every class must have a single responsibility.
- **Decoupling:** Use interfaces and C# Events/Action for communication between systems. Never couple gameplay systems directly to the UI.
- **Design Patterns:** Use the Strategy pattern for interchangeable systems (e.g., modular weapon behaviors) and the State pattern for complex behaviors (e.g., Enemy AI states).
- **Unity API Safety:** Avoid using `GameObject.Find`, `Transform.Find`, or `GetComponent` inside high-frequency loops (`Update`, `FixedUpdate`). Cache references in `Awake` or `Start`.

---

## 2. Coding Standards & Conventions
- **Language Version:** C# for Unity 6.
- **Naming Conventions:**
  - PascalCase for Classes, Methods, Properties, and Public Fields (`public float MoveSpeed { get; }`).
  - camelCase for local variables and method parameters (`float damageAmount`).
  - camelCase with an underscore prefix for private/protected fields (`private int _currentHealth;`).
- **Encapsulation:** Keep fields `private` by default. Use `[SerializeField]` to expose fields to the Unity Inspector, never make fields `public` just for inspector visibility.

---

## 3. Folder & Asset Organization
When generating files or editor scripts, you MUST enforce the following directory structure inside `Assets/`:
- `Assets/_Project/Scripts/Core/` - Architecture, Managers, Singletons.
- `Assets/_Project/Scripts/Gameplay/` - Mechanics, Weapons, Player, Enemies.
- `Assets/_Project/Scripts/UI/` - UI Controllers and Views.
- `Assets/_Project/Prefabs/` - All game prefabs.
- `Assets/_Project/Materials/` - Materials (Prefix with `M_`, e.g., `M_Player`).
- `Assets/_Project/Textures/` - Textures (Prefix with `T_`, e.g., `T_Wall_Albedo`).

*Rule: Never create raw assets directly under the root `Assets/` folder.*

---

## 4. Performance & Technical Constraints
- **Physics vs Raycasting:** For projectile/weapon systems requiring high performance, DO NOT use physical bullet GameObjects with Rigidbody/Collider. **Always use Raycasting (`Physics.Raycast` or `Physics2D.Raycast`)** to ensure the game runs smoothly and can handle high entity counts.
- **Garbage Collection (GC) Optimization:** 
  - Minimize allocations in `Update()`. Avoid string concatenation (use `StringBuilder` or cache strings).
  - Use non-allocating physics APIs where applicable (e.g., `Physics.OverlapSphereNonAlloc`).
  - Cache frequently used WaitForSeconds yields in Coroutines.
- **Mass Entities:** Write highly efficient code keeping in mind that the scene might contain thousands of active entities simultaneously.

---

## 5. UI & Scene Management
- **Instantiation Rule:** When creating complex GameObjects, Prefabs, or Canvas UI layouts, **DO NOT attempt to modify the YAML/Prefab file directly**. Instead, write a brief C# `Editor Script` (extending `EditorWindow` or using `[MenuItem]`) to programmatically generate and configure the object in the scene, then save it as a prefab.