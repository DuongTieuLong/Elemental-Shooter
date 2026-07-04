# Survival Game Project

![Gameplay Placeholder](https://via.placeholder.com/800x400?text=Gameplay+Screenshot/GIF)

## 📖 Introduction
This is a Top-Down Survival/Roguelike game developed in **Unity**. The game focuses on intense wave-based combat, strategic upgrades, and dynamic player-enemy interactions.

This project showcases clean code architecture, scalable design patterns, and solid gameplay mechanics. It is designed with modularity in mind, making it easy to extend with new enemies, weapons, and abilities.

## ✨ Features
- **Dynamic Wave System:** Enemies spawn in procedurally or pre-configured waves with increasing difficulty.
- **Roguelike Upgrade System:** Upon leveling up, players can choose from a randomized pool of upgrades (Stat boosts, new abilities, etc.).
- **Robust Combat Mechanics:**
  - Multiple weapons and firing strategies.
  - Directional aiming and hit detection.
  - Elemental damage and reaction system.
- **Modular Stat System:** Dynamic calculation of player and enemy stats using modifiers and base values.
- **Loot System:** Enemies drop experience gems and loot that players can collect.

## 🏗️ Architecture & Code Design
This project was built focusing on best practices in Game Programming:
- **Design Patterns:**
  - **Singleton / Manager Pattern:** For core systems like `GameManager`, `UIManager`, `PoolManager`, and `AudioManager`.
  - **Strategy Pattern:** Used for flexible weapon firing logic (`IFireStrategy`).
  - **Component-Based Architecture:** Entities (Player, Enemies) are built using decoupled components (`Health`, `Movement`, `HitBox`) communicating via interfaces (`IDamageable`, `IStatusReceiver`).
  - **Object Pooling:** Efficient management of frequently spawned objects like bullets, floating text, and VFX via `PoolManager`.
- **Data-Driven Design:** Extensive use of `ScriptableObjects` (`EnemyData`, `WeaponData`, `WaveConfig`, `UpgradeData`) to decouple data from logic, allowing designers to tweak balance without changing code.
- **Event-Driven UI:** UI updates are handled through Action/Events to keep UI code completely decoupled from gameplay logic.

## 📁 Project Structure (Key Folders)
- `Scripts/Core/`: Core managers and critical systems.
- `Scripts/Component/`: Modular behaviors attached to GameObjects (Player, Enemy, Environment, Stats).
- `Scripts/Data/ & DataSO/`: ScriptableObject configurations.
- `Scripts/UI/`: User Interface controllers and managers.

## 🚀 How to Run
1. Ensure you have **Unity 60000.3.14f1 LTS** (or compatible newer version) installed.
2. Clone this repository:
   ```bash
   git clone https://github.com/yourusername/SurvivalGame.git
   ```
3. Open the project in Unity Hub (`Survival Game Project A` folder).
4. Open the main scene located in `Assets/Scenes/`.
5. Press **Play** in the editor.

## 🛠️ Technologies
- **Engine:** Unity
- **Language:** C#
- **Rendering Pipeline:** URP (Universal Render Pipeline)

## 📝 To-Do / Future Updates
- [ ] Add Boss Encounters.
- [ ] Implement varied biomes and maps.
- [ ] Add more weapon types and elemental combos.

---
*Created by [Your Name] - [Link to Portfolio/LinkedIn]*
