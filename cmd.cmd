--------------------------------------file cmd.cmd{

[command]
name = "skill_1"
command = ~D, DF, F, a
time = 50
[command]
name = "skill_2"
command = ~D, DB, B, a
time = 50
[command]
name = "skill_3"
command = ~D, DF, F, b
time = 50
[command]
name = "skill_4"
command = ~D, DB, B, b
time = 50
[command]
name = "skill_5"
command = ~D, DF, F, c
time = 50
[command]
name = "skill_6"
command = ~D, DB, B, c
time = 50
[command]
name = "skill_7"
command = ~D, a
time = 50
[command]
name = "skill_8"
command = ~D, b
time = 50
[command]
name = "skill_9"
command = ~D, c
time = 50
[command]
name = "Truth: Multilate"
command = ~x
time = 50
[command]
name = "Arise Iris"
command = ~D,x
time = 50
[command]
name = "Bellion & Greed"
command = ~y
time = 50
[command]
name = "Dodge"
command = ~D,y
time = 50
[command]
name = "Kaisel"
command = ~z
time = 50
[command]
name = "FF"
command = F, F
time = 50
[command]
name = "BB"
command = B, B
time = 50
[command]
name = "recovery"
command = x+y
time = 1
[command]
name = "recovery"
command = y+z
time = 1
[command]
name = "recovery"
command = x+z
time = 1
[command]
name = "recovery"
command = a+b
time = 1
[command]
name = "recovery"
command = b+c
time = 1
[command]
name = "recovery"
command = a+c
time = 1
[command]
name = "back_x"
command = /$B, x
time = 1
[command]
name = "back_y"
command = /$B, y
time = 1
[command]
name = "back_z"
command = /$B, z
time = 1
[command]
name = "down_x"
command = /$D, x
time = 1
[command]
name = "down_y"
command = /$D, y
time = 1
[command]
name = "down_z"
command = /$D, z
time = 1
[command]
name = "fwd_x"
command = /$F, x
time = 1
[command]
name = "fwd_y"
command = /$F, y
time = 1
[command]
name = "fwd_z"
command = /$F, z
time = 1
[command]
name = "up_x"
command = /$U, x
time = 1
[command]
name = "up_y"
command = /$U, y
time = 1
[command]
name = "up_z"
command = /$U, z
time = 1
[command]
name = "back_a"
command = /$B, a
time = 1
[command]
name = "back_b"
command = /$B, b
time = 1
[command]
name = "back_c"
command = /$B, c
time = 1
[command]
name = "down_a"
command = /$D, a
time = 1
[command]
name = "down_b"
command = /$D, b
time = 1
[command]
name = "down_c"
command = /$D, c
time = 1
[command]
name = "fwd_a"
command = /$F, a
time = 1
[command]
name = "fwd_b"
command = /$F, b
time = 1
[command]
name = "fwd_c"
command = /$F, c
time = 1
[command]
name = "up_a"
command = /$U, a
time = 1
[command]
name = "up_b"
command = /$U, b
time = 1
[command]
name = "up_c"
command = /$U, c
time = 1
[command]
name = "a"
command = a
time = 1
[command]
name = "b"
command = b
time = 1
[command]
name = "c"
command = c
time = 1
[command]
name = "x"
command = x
time = 1
[command]
name = "y"
command = y
time = 1
[command]
name = "z"
command = z
time = 1
[command]
name = "s"
command = s
time = 1
[command]
name = "fwd"
command = $F
time = 1
[command]
name = "downfwd"
command = $DF
time = 1
[command]
name = "down"
command = $D
time = 1
[command]
name = "downback"
command = $DB
time = 1
[command]
name = "back"
command = $B
time = 1
[command]
name = "upback"
command = $UB
time = 1
[command]
name = "up"
command = $U
time = 1
[command]
name = "upfwd"
command = $UF
time = 1
[command]
name = "hold_x"
command = /x
time = 1
[command]
name = "hold_y"
command = /y
time = 1
[command]
name = "hold_z"
command = /z
time = 1
[command]
name = "hold_a"
command = /a
time = 1
[command]
name = "hold_b"
command = /b
time = 1
[command]
name = "hold_c"
command = /c
time = 1
[command]
name = "hold_s"
command = /s
time = 1
[command]
name = "holdfwd"
command = /$F
time = 1
[command]
name = "holddownfwd"
command = /$DF
time = 1
[command]
name = "holddown"
command = /$D
time = 1
[command]
name = "holddownback"
command = /$DB
time = 1
[command]
name = "holdback"
command = /$B
time = 1
[command]
name = "holdupback"
command = /$UB
time = 1
[command]
name = "holdup"
command = /$U
time = 1
[command]
name = "holdupfwd"
command = /$UF
time = 1

[statedef -1]

[State 0]
type = ChangeState
triggerall = stateno != 70
triggerall = command = "FF"
triggerall = ailevel = 0
trigger1 = ctrl
trigger2 = stateno = [60,75]
value = ifelse(statetype = A,70,60)

[State 0]
type = ChangeState
triggerall = stateno != 75
triggerall = command = "BB"
triggerall = ailevel = 0
trigger1 = ctrl
trigger2 = stateno = [60,65]
value = ifelse(statetype = A,75,65)

[State 0]
type = ChangeState
triggerall = command = "s"
triggerall = ailevel = 0
trigger1 = ctrl
trigger2 = stateno = [60,65]
Triggerall = statetype != A
value = 500

[State 0]
type = ChangeState
triggerall = command = "skill_1"
triggerall = statetype != A
triggerall = ailevel = 0
triggerall = power >= 1000
trigger1 = ctrl
trigger2 = stateno = [60,65]
value = 1000

[State 0]
type = ChangeState
;triggerall = statetype != A
triggerall = command = "skill_2"
triggerall = ailevel = 0
triggerall = power >= 1000
trigger1 = ctrl
trigger2 = stateno = [60,75]
value = 1100

[State 0]
type = ChangeState
triggerall = numhelper(1210) = 0
triggerall = numhelper(2200) = 0
triggerall = numhelper(2300) = 0
triggerall = statetype != A
triggerall = command = "skill_3"
triggerall = ailevel = 0
triggerall = power >= 1000
trigger1 = ctrl
trigger2 = stateno = [60,75]
value = 1200

[State 0]
type = ChangeState
triggerall = numhelper(1310) = 0
triggerall = statetype != A
triggerall = command = "skill_4"
triggerall = ailevel = 0
triggerall = power >= 1000
trigger1 = ctrl
trigger2 = stateno = [60,75]
value = 1300

[State 0]
type = ChangeState
triggerall = numhelper(1410) = 0
triggerall = command = "skill_5"
triggerall = ailevel = 0
triggerall = power >= 1500
trigger1 = ctrl
trigger2 = stateno = [60,75]
value = 1400

[State 0]
type = ChangeState
triggerall = numhelper(1510) = 0
triggerall = statetype != A
triggerall = command = "skill_6"
triggerall = ailevel = 0
triggerall = power >= 2000
trigger1 = ctrl
trigger2 = stateno = [60,75]
value = 1500

[State 0]
type = ChangeState
triggerall = statetype != A
triggerall = command = "skill_7"
triggerall = ailevel = 0
triggerall = power >= 1000
trigger1 = ctrl
trigger2 = stateno = [60,75]
value = 1600

[State 0]
type = ChangeState
triggerall = statetype != A
triggerall = command = "skill_8"
triggerall = ailevel = 0
triggerall = power >= 1000
trigger1 = ctrl
value = 1700

[State 0]
type = ChangeState
triggerall = numhelper(1810) = 0
triggerall = statetype != A
triggerall = command = "skill_9"
triggerall = ailevel = 0
triggerall = power >= 1000
trigger1 = ctrl
value = 1800

[State 0]
type = ChangeState
triggerall = statetype != A
triggerall = command = "Dodge"
triggerall = ailevel = 0
triggerall = power >= 1000
trigger1 = ctrl
value = 1900

[State 0]
type = ChangeState
triggerall = numhelper(2010) = 0
triggerall = numhelper(2120) = 0
triggerall = statetype != A
triggerall = command = "Bellion & Greed"
triggerall = ailevel = 0
triggerall = power >= 1500
trigger1 = ctrl
value = 2000

[State 0]
type = ChangeState
triggerall = numhelper(2210) = 1
triggerall = numhelper(2275) = 0
triggerall = command = "Arise Iris"
trigger1 = ctrl
value = 2270
[State 0]
type = ChangeState
;triggerall = helper(30990),fvar(8) = 1
triggerall = numhelper(2200) = 0
triggerall = numhelper(2300) = 0
triggerall = power >= 1500
triggerall = command = "Arise Iris"
Triggerall = statetype != A
trigger1 = ctrl
value = 2200

[State 0]
type = ChangeState
triggerall = numhelper(2410) = 0
triggerall = command = "Kaisel"
triggerall = ailevel = 0
triggerall = power >= 1500
trigger1 = ctrl
value = 2400

[State 0]
type = ChangeState
triggerall = numhelper(1810) = 0
triggerall = numhelper(2300) = 0
triggerall = statetype != A
triggerall = command = "Truth: Multilate"
triggerall = ailevel = 0
triggerall = power >= 2000
triggerall = helper(30990),fvar(8) = 1
trigger1 = ctrl
trigger2 = stateno = [60,75]
value = 3000

[State 0]
type = ChangeState
triggerall = command = "a"
triggerall = statetype != A
triggerall = ailevel = 0
trigger1 = ctrl
trigger2 = stateno = [60,65]
value = 200

[State 0]
type = ChangeState
triggerall = command = "b"
triggerall = statetype != A
triggerall = ailevel = 0
trigger1 = ctrl
trigger2 = stateno = [60,65]
value = 300

[State 0]
type = ChangeState
triggerall = command = "c"
triggerall = statetype != A
triggerall = ailevel = 0
triggerall = power >= 500
trigger1 = ctrl
trigger2 = stateno = [60,65]
value = 400

[State 0]
type = ChangeState
triggerall = command = "a"
triggerall = statetype = A
triggerall = ailevel = 0
trigger1 = ctrl
trigger2 = stateno = [60,65]
value = 600

[State 0]
type = ChangeState
triggerall = stateno != 340
triggerall = stateno != 82744
triggerall = stateno != 82745
triggerall = command = "b"
triggerall = statetype = A
triggerall = ailevel = 0
trigger1 = ctrl
trigger2 = stateno = [60,65]
value = 340

[State 0]
type = ChangeState
triggerall = command = "c"
triggerall = statetype = A
triggerall = ailevel = 0
triggerall = power >= 500
trigger1 = ctrl
trigger2 = stateno = [60,65]
value = 620
}



