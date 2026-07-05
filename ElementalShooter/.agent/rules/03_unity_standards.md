---
trigger: always_on
---

# UNITY STANDARDS AND OPTIMIZATION
- Scale: The project is designed to handle a massive number of entities (e.g., thousands of enemies on screen, similar to "Vampire Survivors"-style shooters).
- Update Loop: Strictly avoid overusing `MonoBehaviour.Update()` unless absolutely necessary. Favor event-driven approaches over polling.
- Collision & Projectiles: Prioritize **Raycasting** over instantiating physics objects (`Rigidbody`/`Collider`) for fast-moving projectiles to save on physics computation resources.
- Memory: Object Pooling is mandatory for all frequently spawned entities (monsters, projectiles, VFX). The use of `Instantiate()` and `Destroy()` within the gameplay loop is prohibited.