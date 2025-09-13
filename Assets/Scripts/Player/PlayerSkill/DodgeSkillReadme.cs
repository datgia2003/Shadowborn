/*
===============================================================================
                            DODGE SKILL SYSTEM SETUP GUIDE
===============================================================================

🎯 OVERVIEW:
- Complete dodge/dash skill system with invincibility frames
- Perfect dodge mechanics with time slow effect
- UI integration with skill cooldown display
- Visual and audio effects support

📦 WHAT'S INCLUDED:
1. DodgeSkill.cs - Main dodge skill logic
2. DodgeSkillTest.cs - Testing script with GUI
3. DashTrailEffect.cs - Visual trail effect
4. PerfectDodgeEffect.cs - Perfect dodge visual effect
5. Updated PlayerResources.cs - Damage integration

🔧 SETUP INSTRUCTIONS:

1. ADD TO PLAYER:
   - Attach DodgeSkill.cs to your Player GameObject
   - Attach DodgeSkillTest.cs for testing (optional)
   - Ensure Player has: Rigidbody2D, PlayerController, PlayerResources

2. INPUT SETUP:
   - Make sure PlayerInputActions has "Dodge" action
   - Default key: Left Shift (can be changed in Input Actions)

3. UI SETUP:
   - Create a SkillUISlot with skillName = "Dodge" or "Dash"
   - The script will automatically find and connect to it

4. AUDIO & EFFECTS (Optional):
   - dashSound: Sound for normal dodge
   - perfectDodgeSound: Sound for perfect dodge
   - dashTrailEffect: Trail prefab (auto-created if null)
   - perfectDodgeEffect: Perfect dodge effect prefab (auto-created if null)

🎮 CONTROLS:
- Left Shift (or configured key): Perform dodge
- Movement direction determines dodge direction
- No input = dodge forward/backward based on facing

🧪 TESTING:
- T: Test damage (for perfect dodge)
- Y: Check invincibility status
- U: Auto damage test (hold)

⚡ FEATURES:

DODGE MECHANICS:
✅ Dash in movement direction
✅ Invincibility frames during dodge
✅ Cooldown system with UI integration
✅ Mana cost system

PERFECT DODGE:
✅ Perfect dodge detection window
✅ Time slow effect on perfect dodge
✅ Mana refund on perfect dodge
✅ Enhanced visual effects

INTEGRATION:
✅ PlayerResources damage blocking
✅ UI cooldown display
✅ Audio and visual effects
✅ Debug logging and testing

📊 DEFAULT SETTINGS:
- Dash Distance: 5 units
- Dash Duration: 0.3 seconds  
- Cooldown: 2 seconds
- Mana Cost: 10
- Perfect Dodge Window: 0.2 seconds
- Time Slow Duration: 1 second
- Time Slow Scale: 0.3x
- Invincibility Duration: 0.4 seconds

🔧 CUSTOMIZATION:
All settings can be adjusted in the inspector:
- Distance, duration, cooldown
- Perfect dodge timing window
- Time slow effects
- Visual and audio effects
- Mana costs

🐛 TROUBLESHOOTING:
- No dodge response? Check Input Actions setup
- No UI updates? Ensure SkillUISlot name matches
- No effects? Check AudioSource component
- Time slow not working? Check Time.timeScale usage in other scripts

===============================================================================
*/