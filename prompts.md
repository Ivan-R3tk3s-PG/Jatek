# MASTER PROMPT — NEON BREACH

## Your role

You are an expert C# game developer, gameplay programmer, technical artist, and indie game designer.

Your task is to **BUILD A COMPLETE, PLAYABLE, VISUALLY IMPRESSIVE 2D ACTION PLATFORMER** suitable for a school programming project that could realistically be produced in approximately two hours with AI assistance.

The game is called **NEON BREACH**.

The result should feel like a polished miniature indie game, not a generic programming tutorial or a basic prototype with colored rectangles.

**IMPORTANT: I want the actual working game, not just an explanation of how to make it.**

## 1. Technology

- Language: C#
- Framework: Raylib-cs 8.1.0
- Target: .NET 8
- Platform: Windows desktop
- Resolution: 1280 × 720, resizable if straightforward
- Target: 60 FPS
- Controls: Keyboard and mouse
- Development environment: Visual Studio or VS Code

Use conventional, maintainable C# code with proper classes, meaningful names, and understandable logic.

Keep external dependencies to an absolute minimum.

If you have filesystem and terminal access, create the project directly, build it, run tests, and fix any errors.

If you don't have filesystem access, generate the complete source code for every required file, with exact filenames, project structure, and setup commands.

Do not claim to have compiled or tested anything unless you actually did.

## 2. Game concept

Create a 2D side-scrolling sci-fi shooter set inside a dark, abandoned futuristic industrial facility.

The player is a heavily armed operative fighting through increasingly dangerous robotic security forces.

The core experience combines:

- Responsive platforming
- Fluid run-and-gun combat
- Finding and switching weapons
- Enemies with different attack patterns
- Weapon and character upgrades
- Increasing difficulty
- Satisfying audiovisual feedback
- A clear beginning, progression, and ending

The game should take approximately 4–8 minutes to complete in one successful playthrough.

The player should feel powerful, agile, and rewarded for exploration and combat.

## 3. Visual art direction — extremely important

The visual presentation is one of the highest priorities.

Style: **Premium 2D sci-fi action game with dark environments, neon cyan highlights, orange/red hostile effects, and cinematic atmosphere.**

Visual inspiration: modern indie action platformers, cyberpunk industrial environments, and polished arcade shooters.

Make everything visually cohesive.

### Environment

Include:
- Multiple parallax background layers
- Futuristic industrial buildings and machinery
- Pipes, cables, vents, glowing windows, and structural beams
- Platforms with actual visual depth and edge highlights
- Atmospheric haze and distant environmental details
- Glowing indicators and warning lights
- Occasional environmental sparks
- Different-looking areas within the level

### Characters

Avoid plain rectangular characters.

Create an appealing procedural player character with:
- Distinct head, torso, legs, and arms
- A visible firearm
- A futuristic armored appearance
- Animation through articulated parts and transforms
- Facing direction and aiming
- Running, jumping, landing, hurt, and death feedback

Enemies should have distinctive silhouettes and colors so their types are immediately recognizable.

If you can produce original visual assets or obtain appropriately licensed assets, use them only when they can be reliably included in the working project.

Otherwise, produce high-quality procedural vector graphics using layered shapes, silhouettes, highlights, and animation.

Never let unavailable art assets prevent the game from running.

### Effects and polish

Implement as many of the following as possible:
- Muzzle flashes
- Bullet tracers
- Hit sparks
- Enemy destruction particles
- Subtle screen shake
- Weapon recoil
- Animated weapon pickups
- Floating damage numbers
- Impact flashes
- Soft glow or additive light effects
- Smooth camera movement
- Dust particles when landing
- Smooth UI transitions

**Rendering quality, animation, and combat feedback matter more than adding more features.**

Avoid excessive glow that makes the gameplay unreadable.

## 4. Player movement

Create tight, satisfying 2D movement.

Controls:
- A / D: Move horizontally
- Space: Jump
- Mouse: Aim
- Left mouse button: Shoot
- 1 / 2 / 3: Switch available weapons
- E: Interact with upgrades or pickups when applicable
- ESC: Pause/resume
- R: Restart after death

Physics requirements:
- Acceleration and deceleration
- Gravity
- Reliable platform collisions
- Variable jump height if feasible
- Coyote time
- Jump buffering
- Prevent falling through platforms

The player must respond instantly to input without excessive sliding or floaty controls.

The camera should follow the player smoothly and remain within level boundaries.

## 5. Weapon system

Implement three weapons with clearly different gameplay and visual effects.

### Weapon 1 — V9 Sidearm

Starting weapon.

- Medium damage
- Moderate fire rate
- Accurate shots
- Unlimited reserve ammunition
- Cyan projectile effects

Reliable in almost any situation.

### Weapon 2 — Viper SMG

Found during level progression.

- Very fast firing
- Lower damage per bullet
- Moderate bullet spread
- Magazine and reload system
- Yellow/orange muzzle flash
- Strong rapid-fire visual feedback

Best for groups of weak enemies.

### Weapon 3 — Ravager Shotgun

Found later in the level.

- Fires multiple pellets
- High close-range damage
- Slow fire rate
- Strong recoil
- Wide projectile spread
- Bright, powerful muzzle flash

Best against heavy enemies.

Each weapon must have separate stats and clearly distinct behavior.

Weapon pickups should glow, animate, and display their name.

The interface should display the equipped weapon and ammunition when applicable.

## 6. Enemy AI

Create three enemy types.

### Scout Drone

- Low health
- Moves or patrols predictably
- Approaches the player or maintains basic attack distance
- Fires small projectiles
- Relatively easy to defeat

### Security Turret

- Stationary
- Detects the player within range
- Tracks and fires toward the player
- Has moderate health
- Creates a different tactical challenge

### Heavy Enforcer

- Larger, imposing robotic enemy
- High health
- Slower movement
- Stronger attacks
- Clearly telegraphed firing pattern
- Larger explosion upon destruction

Enemy behaviors must be genuinely functional, not just visual differences.

Difficulty should increase as the player advances.

Implement sensible attack cooldowns, projectile lifetimes, collision checks, and damage handling.

Avoid unavoidable attacks or overwhelming enemy spam.

## 7. Upgrade system

Throughout the level, the player can collect energy or credits from defeated enemies.

Include at least one upgrade station where players can choose between three upgrades.

### Reinforced Armor
- Increases maximum health
- Restores a portion of health

### Overcharged Rounds
- Increases damage for all weapons

### Combat Accelerator
- Increases movement speed or reduces weapon cooldowns

Display upgrades through an attractive in-game interface.

Upgrades must change actual gameplay values immediately.

Show a clear confirmation after selection.

## 8. Level design and progression

Build one handcrafted or deterministically generated horizontal level.

It must be a true platformer level, not an empty flat arena.

Include:
- Elevated platforms
- Different platform heights
- Jumpable gaps
- Cover and obstacles
- Enemy encounter areas
- Weapon pickup locations
- A mid-level upgrade station
- Checkpoints if time permits
- A recognizable final encounter
- A clear extraction or exit point

Structure progression into four sections:

**Section 1 — Arrival**
Teach movement and basic shooting against Scout Drones.

**Section 2 — Security Wing**
Introduce vertical platforming, turrets, and the SMG pickup.

**Section 3 — Industrial Core**
Introduce tougher enemies, the shotgun, and upgrades.

**Section 4 — Lockdown**
A difficult final encounter containing several enemy types, followed by reaching the exit.

An additional boss with multiple attacks is OPTIONAL and should only be added after everything else is reliable.

The existing Heavy Enforcer can serve as the final major threat.

## 9. Health, damage, and combat feedback

Include:
- Player health
- Enemy health
- Visible damage feedback
- Temporary player invulnerability after taking damage
- Death animation or destruction effect
- Game-over screen
- Restart functionality
- Enemies dropping currency or health occasionally
- Distinct feedback for hitting an enemy versus the environment

Combat must feel satisfying and responsive.

Use frame-rate-independent timing and handle fast-moving projectiles reliably.

## 10. User interface

Create a polished, minimal HUD with:
- Player health bar
- Current weapon name
- Ammo counter when relevant
- Upgrade currency
- Progress indicator if useful

Create:
- Title screen with PLAY and controls
- Pause screen
- Upgrade selection screen
- Game-over screen
- Victory screen

The UI should match the dark cyberpunk aesthetic.

Use readable typography, restrained neon accents, and smooth transitions.

## 11. Audio

If practical, include:
- Different shooting sounds
- Enemy hit effects
- Enemy destruction sounds
- Pickup sounds
- Jump/landing sounds
- UI interaction sounds

Use self-generated audio, original assets, or properly licensed sounds.

Keep the game completely functional if audio cannot be implemented.

Do not spend excessive time on music.

## 12. Code architecture

Use a simple, clean structure, such as:

- Program.cs
- Game.cs
- Player.cs
- Enemy.cs
- Weapon.cs
- Projectile.cs
- Level.cs
- ParticleSystem.cs
- UI.cs

You may change the structure when appropriate.

Keep classes logically separated but avoid excessive abstraction, dependency injection, complicated patterns, or unnecessary architecture.

This is a two-hour school project, not a commercial game engine.

Prioritize correctness, responsiveness, and visuals.

The code should remain understandable enough for a C# student to explain during a school presentation.

## 13. Performance and reliability

Ensure:
- Delta-time-based updates
- No game-breaking collision bugs
- No unbounded particle or projectile accumulation
- Safe removal of destroyed objects
- Correct player respawn/restart behavior
- No repeated pickups after collection
- No invisible enemies damaging the player
- Stable camera boundaries
- Safe resource disposal
- No missing asset crashes

Do not invent APIs or rely on outdated Raylib-cs signatures.

Prefer straightforward, documented functionality.

## 14. Two-hour development strategy

Time budget: approximately 120 minutes.

Follow these priorities:

**Phase 1 — Minutes 0–25: Playable foundation**
- Initialize project and rendering
- Basic level
- Player movement
- Jumping and collision
- Camera

**Phase 2 — Minutes 25–55: Combat**
- Shooting
- Projectiles
- Enemy AI
- Damage, health, death
- Weapon switching

**Phase 3 — Minutes 55–80: Content**
- Three weapons
- Three enemy types
- Level progression
- Pickups and upgrades
- Victory condition

**Phase 4 — Minutes 80–105: Visual polish**
- Procedural character and enemy art
- Background layers
- Lighting and particles
- Weapon effects
- Refined HUD and menus

**Phase 5 — Minutes 105–120: Quality assurance**
- Build and resolve compilation errors
- Test collision and enemy behavior
- Fix restart and victory state bugs
- Improve glaring gameplay issues
- Prepare project instructions

These are planning targets, not excuses to skip testing during earlier phases.

If time is limited, reduce secondary content rather than sacrificing the playable core.

## 15. Absolutely avoid

- Placeholder rectangles as the final art style
- Completely flat and empty environments
- Overcomplicated architecture
- Requiring a huge amount of manual editor setup
- Dead buttons or fake functionality
- Nonfunctional weapon pickups
- Unreliable physics
- Missing project files
- Pseudocode instead of implementations
- Unsupported claims that the project was tested
- Adding features endlessly without completing the game

## 16. Acceptance criteria

The project is complete when I can:

1. Open and run the game on Windows.
2. Start from an attractive title screen.
3. Move, jump, and aim smoothly.
4. Shoot and damage enemies.
5. Encounter three distinct enemy types.
6. Find and use three distinct weapons.
7. Obtain and apply upgrades.
8. Navigate an actual side-scrolling platforming level.
9. Die and restart correctly.
10. Finish the level and see a victory screen.
11. Enjoy cohesive graphics, satisfying effects, and responsive combat.

## 17. Final deliverables

Provide:
- The complete runnable C# project
- All required source files
- Project and NuGet configuration
- Any original assets used
- A short README
- Exact build and run instructions
- A controls reference
- A brief explanation of the main C# classes
- Known limitations, if any

If you have working filesystem access, create and verify the actual files.

If you are generating text only, present the files in a practical order and include their entire contents. Do not replace necessary code with comments such as "implement later."

If the full output cannot fit into one response, split it into logically complete parts and clearly identify which files remain.

## 18. Final instruction

**START BUILDING THE GAME IMMEDIATELY.**

Make reasonable design decisions independently.

Do not spend the first response discussing endless possibilities or rewriting this specification.

Treat this as a real software project with a strict deadline.

Your objective is to create the most visually polished, technically functional, and enjoyable 2D shooter-platformer that a capable C# developer assisted by AI could plausibly deliver in two hours.

Make it look like something that would impress a teacher who expected an ordinary school assignment.

**Functionality first. Game feel second. Visual polish third. Extra features last.**

Deliver the game, not just a plan.




## NEON BREACH 1.1
Implement NEON BREACH 1.1 in my existing C# Raylib game. Selected improvements: Major visual overhaul — Improve environments, lighting, character animation, and effects; Dash + slide mechanics — Make movement faster and more expressive; Enemy hit reactions — Stagger, knockback and more satisfying destruction; Explosive barrels — Environmental combat and chain explosions; HUD and menu polish — Cleaner presentation, transitions and better readability. Preserve the existing gameplay and deliver a complete updated runnable ZIP project, checking compatibility and reliability.