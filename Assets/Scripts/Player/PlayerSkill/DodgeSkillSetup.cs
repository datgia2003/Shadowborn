/*
===============================================================================
                    DODGE SKILL - INPUT SYSTEM VERSION SETUP
===============================================================================

🎯 SETUP INSTRUCTIONS:

1. PLAYER SETUP:
   ✅ Attach DodgeSkill.cs to your Player GameObject
   ✅ Ensure Player has PlayerInput component with PlayerInputActions
   ✅ Make sure "Gameplay" action map is active

2. INPUT ACTIONS CONFIGURATION:
   ✅ PlayerInputActions.inputactions already has "Dodge" action
   ✅ Dodge action is bound to "L" key
   ✅ Move action provides direction input for dodge direction

3. REQUIRED COMPONENTS:
   ✅ PlayerInput (with PlayerInputActions asset)
   ✅ Rigidbody2D 
   ✅ PlayerController
   ✅ PlayerResources
   ✅ SpriteRenderer (for visual effects)
   ✅ AudioSource (optional, for sounds)

4. UI INTEGRATION:
   ✅ Create SkillUISlot with skillName = "Dodge"
   ✅ System automatically connects and shows L key

🎮 HOW IT WORKS:

INPUT SYSTEM CALLBACKS:
- OnDodge(InputValue) → Triggered when L key pressed
- OnMove(InputValue) → Tracks movement for dodge direction
- Uses Unity's Input System message sending

DODGE MECHANICS:
- L key → Perform dodge
- WASD direction → Dodge direction
- No input → Dodge forward/backward based on facing
- Mana cost → Uses TryConsumeMana(10)
- Cooldown → 2 seconds with UI display

PERFECT DODGE:
- Window: 0.2 seconds after damage or near enemies
- Effect: Time slow to 0.3x for 1 second
- Bonus: Restore 5 mana (half cost refund)

🧪 TESTING:
- T: Test damage (trigger perfect dodge)
- Y: Check dodge status
- U: Auto damage test (hold)

⚡ KEY FEATURES:
✅ Input System integration (OnDodge callback)
✅ Movement direction from OnMove callback
✅ Mana system with TryConsumeMana/AddMana
✅ UI cooldown with direct component access
✅ Invincibility frames (0.4s)
✅ Perfect dodge with time slow
✅ Visual trail and perfect dodge effects
✅ Audio support for sounds

🔧 INSPECTOR SETTINGS:
- Dash Distance: 5 units
- Dash Duration: 0.3 seconds
- Cooldown Time: 2 seconds
- Mana Cost: 10
- Perfect Dodge Window: 0.2 seconds
- Time Slow Duration: 1 second (0.3x speed)
- Invincibility Duration: 0.4 seconds

💡 IMPORTANT NOTES:
- Uses Unity Input System callbacks (OnDodge, OnMove)
- Player must have PlayerInput component
- Action map "Gameplay" must be enabled
- Dodge action already bound to L key in InputActions

� TROUBLESHOOTING:
- No dodge response? Check PlayerInput component and action map
- Wrong direction? Verify OnMove callback receives input
- UI not updating? Check SkillUISlot name is "Dodge"
- No effects? Verify AudioSource and sprite components

===============================================================================
*/