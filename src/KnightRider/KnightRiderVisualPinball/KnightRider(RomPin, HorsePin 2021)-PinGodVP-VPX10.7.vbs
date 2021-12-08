Option Explicit
randomize
On Error Resume Next
	ExecuteGlobal GetTextFile("controller.vbs") ' Std Controller for DOF etc.
	If Err Then MsgBox "You need the Controller.vbs file in order to run this table (installed with the VPX package in the scripts folder)"
On Error Goto 0

'*****************************
' Controller scripts
' PinGod.Vp.Controller. Requires modded Core Vbs for different switches csharp
'*****************************
LoadPinGoVpController 
Sub LoadPinGoVpController
	On Error Resume Next
		If ScriptEngineMajorVersion<5 Then MsgBox "VB Script Engine 5.0 or higher required"
		ExecuteGlobal GetTextFile("PinGod.vbs")
		If Err Then MsgBox "Unable to open " & VBSfile & ". Ensure that it is in the same folder as this table. " & vbNewLine & Err.Description
		Set Controller=CreateObject("PinGod.VP.Controller")		
		If Err Then MsgBox "Failed to initialize PinGod.VP controller, is it registered?" : Exit Sub
	On Error Goto 0
End Sub

'Release
'Const IsDebug = False ' set false to load an export
'Const GameDirectory = ".\PinGod.KR.exe" 'exported game

'Debug
Const IsDebug = True
Const GameDirectory = "..\KnightRiderGodot" ' Loads the godot pingod game project
Const UseSolenoids = 1 ' Check for solenoid states?
Const UsePdbLeds = 0  ' use led (color)
Const UseLamps = 1  ' Check for lamp states?
Dim bsTrough, bsSaucer, plungerIM, swSaucer : swSaucer = 27
Dim bsVUK 	'VUK'
Dim bsTSaucer 	'TruckSaucer'
Dim bsVUKScoop 	'Ramp Scoop'
'Dim PlungerIM 'Auto Plunger'
Dim kickbackIM 'Kickback'

'**********************
' VP table display / controller events
'**********************
Sub Table1_Exit : Controller.Stop : End Sub ' Closes the display window, sends the quit action
Sub Table1_Paused: Controller.Pause 1 : End Sub
Sub Table1_UnPaused: Controller.Pause 0 : End Sub

'**********************
' VP init
' Inits the controller then waits for the display to fully load into initial scene.
'**********************
Sub Table1_Init	
	With Controller
		.DisplayX			= 0 ' Position X, Y: If using full screen change this to your other monitor
		.DisplayY			= 0 ' 
		.DisplayWidth 		= 1920 ' 1024 FS
		.DisplayHeight 		= 1080 ' 600  FS
		.DisplayAlwaysOnTop = True
		.DisplayFullScreen 	= False 'Providing the position is on another display it should fullscreen to window
		.DisplayLowDpi 		= False
		.DisplayNoWindow 	= False
	On Error Resume Next
		if isDebug Then '
			.RunDebug GetPlayerHWnd, GameDirectory ' Load game from Godot folder with Godot exe
		else
			.Run GetPlayerHWnd, GameDirectory ' With game exported executable
		end if		
	If Err Then MsgBox Err.Description : Exit Sub
	End With
    On Error Goto 0	

	If Err Then MsgBox Err.Description : Exit Sub
	
	'Wait for game display to be ready for the trough. Using any old game object Timer, usually you will have LeftFlipper but change this to what you please
	LeftFlipper.TimerInterval = 369
	LeftFlipper.TimerEnabled = 1
End Sub
'Game ready checker from flipper timer
Sub LeftFlipper_Timer
	LeftFlipper.TimerEnabled = 0
	if not Controller.GameRunning Then LeftFlipper.TimerEnabled = 1 : Exit Sub
	InitGame
End Sub

'**********************
' GAME / VP init
' When the display is ready initialize VPM controller scripts and table objects
'**********************
Dim initialized : initialized = 0
Sub InitGame
	if initialized then exit sub ' prevent any chance of init twice if author decides to use LFlipper Timers
	'init core vbs, vpm
	vpmInit me
	vpmMapLights AllLamps		'Auto lamps collection, lamp id in timerinterval
	vpmCreateEvents AllSwitches 'Auto Switches collection from the swNum in timerInterval

	'Init timers for updates
	pulsetimer.Enabled=1
	PinMAMETimer.Enabled=1		
	
	On Error Resume Next
	Set bsTrough = New cvpmTrough
	bsTrough.Size = 4
	bsTrough.Balls = 4
	bsTrough.InitSwitches Array(81,82,83,84) ' trough switches
	bsTrough.InitExit BallRelease, 90, 8	
	bsTrough.CreateEvents "bsTrough",  Drain
	bsTrough.InitEntrySounds "drainhit", "", ""
	bsTrough.InitExitSounds "solenoidleft", ""
	bsTrough.Reset		

  ' Auto Plunger
    Const IMPowerSetting = 50 ' Plunger Power
    Const IMTime = 0.6        ' Time in seconds for Full Plunge
    Set plungerIM = New cvpmImpulseP
    With plungerIM
        .InitImpulseP swplunger, IMPowerSetting, IMTime
        .Random 0.3        
        .CreateEvents "plungerIM"
		.InitExitSnd "solenoidleft", "plunger"
    End With

	'VUK
	Set bsVUK = New cvpmSaucer : With bsVUK
	  .InitKicker VUKb, 44, 340, 8, 0
	   .InitSounds "saucer_enter", "", "Rscoopout"
	  .CreateEvents "bsVUK", sw31
	End With
	
	' Truck Saucer
	Set bsTSaucer = new cvpmSaucer : With bsTSaucer
		.InitKicker sw42, 34, 80, 5, 0    
		.InitSounds "saucer_enter", "", "saucer_exit"
		.CreateEvents "bsTSaucer", sw42
	End With

	'Scoop Ramp
	Set bsVUKScoop = New cvpmSaucer : With bsVUKScoop
	  .InitKicker sw32b, 40, 190, 8, 0
	  .InitSounds "fx_scoophit", "", "saucer_exit"
	  .CreateEvents "bsVUKScoop", sw32
	End With

  ' Kickback
    Kickback.PullBack

'    ' ### Nudging ###
    vpmNudge.TiltSwitch = swTilt
    vpmNudge.Sensitivity = 0.8
	vpmNudge.TiltObj = Array(LSling,RSling)


	If Err Then MsgBox Err.Description
	initialized = 1
	LoadingText.Visible = false ' Hide the overlay (loading screen)

	ResetTruckRamp
	
	On Error Goto 0	
	
End Sub

'****************************
' Keyboard / Machine
'****************************
Sub Table1_KeyDown(ByVal keycode)

	if Controller.GameRunning = 0 then Exit Sub 'exit because no display is available

	If keycode = PlungerKey Then
		Plunger.PullBack
		PlaySound "plungerpull",0,1,AudioPan(Plunger),0.25,0,0,1,AudioFade(Plunger)
	End If

	If keycode = LeftFlipperKey and FlippersOn Then
		LeftFlipper.RotateToEnd
		if FlippersTopOn then LeftFlipper1.RotateToEnd
		PlaySound SoundFX("flipUpSound",DOFFlippers), 0, .67, AudioPan(LeftFlipper), -0.05,0,0,1,AudioFade(LeftFlipper)
	End If

	If keycode = RightFlipperKey and FlippersOn Then
		RightFlipper.RotateToEnd
		if FlippersTopOn then RightFlipper1.RotateToEnd
		PlaySound SoundFX("flipUpSound",DOFFlippers), 0, .67, AudioPan(RightFlipper), 0.05,0,0,1,AudioFade(RightFlipper)
	End If

	If vpmKeyDown(keycode) Then Exit Sub  ' This will handle machine switches and flippers etc

End Sub

Sub Table1_KeyUp(ByVal keycode)

	if Controller.GameRunning = 0 then Exit Sub 'exit because no display is available

	If keycode = PlungerKey Then
		Plunger.Fire
		PlaySound "plunger",0,1,AudioPan(Plunger),0.25,0,0,1,AudioFade(Plunger)
	End If

	If keycode = LeftFlipperKey and FlippersOn Then
		LeftFlipper.RotateToStart
		if FlippersTopOn then LeftFlipper1.RotateToStart
		PlaySound SoundFX("flipDownSound",DOFFlippers), 0, 1, AudioPan(LeftFlipper), -0.05,0,0,1,AudioFade(LeftFlipper)
	End If

	If keycode = RightFlipperKey and FlippersOn Then
		RightFlipper.RotateToStart
		if FlippersTopOn then RightFlipper1.RotateToStart
		PlaySound SoundFX("flipDownSound",DOFFlippers), 0, 1, AudioPan(RightFlipper), 0.05,0,0,1,AudioFade(RightFlipper)
	End If

	If vpmKeyUp(keycode) Then Exit Sub ' This will handle machine switches and flippers etc
End Sub
'****************************

dim lamp
For each lamp in GILow
	lamp.State=1
next

'****************************
' Solenoids / Coils / Callbacks
'****************************

SolCallback(0) = "Died"
SolCallback(1) = "bsTrough.SolOut"
SolCallback(2) = "FlippersEnabled"
SolCallback(3) = "AutoPlunger"
SolCallback(4) = "bsTSaucer.SolOut"
SolCallback(6) = "bsVUKScoop.SolOut"
SolCallback(7) = "bsVUK.SolOut"
SolCallback(8) = "SolKickBack"
SolCallback(9) = "SolTruckRamp"
SolCallback(10) = "SoltruckLockDiverter"
SolCallback(11) = "SoltruckTargetLockDiverter"
SolCallback(12) = "SolLockFillerKicker"
SolCallback(13) = "SolKittRampToy"
SolCallback(14) = "FlippersTopEnabled"
SolCallback(15) = "SolFlasherLeft"
SolCallback(16) = "SolFlasherRight"
SolCallback(17) = "LampshowRandom"
SolCallback(18) = "LampshowPursuit"
SolCallback(19) = "SolFlasherLeftMid"
SolCallback(20) = "SolFlasherRightMid"
SolCallback(21) = "SolFlasherLeftUp"
SolCallback(22) = "SolFlasherRightUp"
SolCallback(23) = "LampshowFlash"
SolCallback(24) = "LampshowAttract"

Sub Died(Enabled)
	'on error resume next	
	If not enabled then
		LoadingText.Visible = false
		MsgBox "Game window unavailable." ' : Err.Raise 5
	End if
End Sub

'Do it like this as one on/off, will make it faster
Dim FlippersOn, FlippersTopOn : FlippersOn = 0 : FlippersTopOn = 0
Sub FlippersEnabled(Enabled)
	'Debug.Print "flippers on coil " & Enabled
	FlippersOn = Enabled
	If not FlippersOn then LeftFlipper.RotateToStart : RightFlipper.RotateToStart
	If not FlippersTopOn then LeftFlipper1.RotateToStart : RightFlipper1.RotateToStart
End Sub

Sub FlippersTopEnabled(Enabled)
	'Debug.Print "flippers on coil " & Enabled
	FlippersTopOn = Enabled
	If not FlippersTopOn then LeftFlipper1.RotateToStart : RightFlipper1.RotateToStart
End Sub

Sub AutoPlunger(Enabled)
  If Enabled Then
    PlungerIM.AutoFire
  End If
End Sub

Sub SolKickBack(enabled)
	If enabled Then 
		KickBack.Fire
	Else
		KickBack.PullBack
	End If
End Sub

Sub SolLockFillerKicker(enabled)
	if enabled Then
		bsTrough.Balls = bsTrough.Balls - 1
		LockFillerKicker.CreateBall
		LockFillerKicker.Kick 30, 2
		bsTrough.Reset()
	End If
End Sub

'#############
'KITT

'KitBody

Sub SolKittRampToy(enabled)
 If enabled Then
	'PlaySound "Rampopens"
	KittUpDownTimer.enabled=True
End If
End Sub

Dim KittDown
KittDown = True

Sub KittUpDownTimer_TImer()
	
If not KittDown Then
	KittGrillTop.TransY = KittGrillTop.TransY -1
	KittGrillTop.TransZ = KittGrillTop.TransZ -1
	KittGrillBottom.TransY = KittGrillBottom.TransY -1
	KittSpoiler.TransZ = KittSpoiler.TransZ -2
	KittLeft.TransX = KittLeft.TransX -1

	KittRight.TransX = KittRight.TransX +1

	If KittGrillBottom.TransY = 0 Then		
		KittUpDownTimer.enabled=False		
		KittDown=True						
	End IF
Else
	KittGrillBottom.TransY = KittGrillBottom.TransY +1
	KittGrillTop.TransY = KittGrillTop.TransY +1
	KittGrillTop.TransZ = KittGrillTop.TransZ +1
	KittSpoiler.TransZ = KittSpoiler.TransZ +2
	KittLeft.TransX = KittLeft.TransX +1

	KittRight.TransX = KittRight.TransX -1
	
	If KittGrillBottom.TransY = 15 Then
		KittUpDownTimer.enabled=False
		KittDown=False				
	End IF
End If

End Sub

'#################################
'TRUCK
'#################################

Dim DoorOpen
DoorOpen = False

Sub ResetTruckRamp
	Controller.Switch 55, 0 : Controller.Switch 56, 1
	TruckRamp.Collidable=False
End Sub

Sub TruckSoundTrigger_Hit
	PlaySoundAt "WireRamp_1_sec", TruckSoundTrigger
End Sub

' truckTarget hit 
Sub sw38_Hit()
	vpmTimer.PulseSw 35
	'TruckDivert.ObjRotZ = 220
	TruckDivertWall.IsDropped=False
End Sub

Sub swTruckDivertWall_hit()
	TruckDivertWall.IsDropped=True
End Sub

Sub SolTruckRamp(enabled)
 If enabled Then
	PlaySound "KR_TruckOpen"
	TruckTimer.enabled=True
 Else
	TruckTimer.enabled=True
End If
End Sub

Sub TruckTimer_TImer()
	
If not DoorOpen Then
	TruckDoor.RotZ = TruckDoor.RotZ -1
	If TruckDoor.RotZ = 0 Then
		TruckTimer.enabled=False
		DoorOpen=True		
		TruckRamp.Collidable=True
		'TruckDivert.ObjRotZ = 285
		'TruckDivertWall.IsDropped=True
		Controller.Switch 55, 1 : Controller.Switch 56, 0
	End IF
Else	
	If TruckDoor.RotZ = 115 Then
		TruckTimer.enabled=False
		DoorOpen=False
		ResetTruckRamp
	Else
		TruckDoor.RotZ = TruckDoor.RotZ +1
	End IF
End If

End Sub

' This holds the balls on the wire ramp
Sub SoltruckLockDiverter(enabled)
	if enabled Then
		LockDiverter.IsDropped=True
	Else	
		LockDiverter.IsDropped=False
	End If
End Sub

Sub SoltruckTargetLockDiverter(enabled)
	if enabled Then
		TruckDivertWall.IsDropped=False
	Else	
		TruckDivertWall.IsDropped=True
	End If
End Sub

'#################################
'END TRUCK
'#################################

'Flashers
Sub SolFlasherLeft(Enabled)
  vpmSolToggleObj flash_l_1, Nothing, "", Enabled
  vpmSolToggleObj flash_l_2, Nothing, "", Enabled
  vpmSolToggleObj flash_l_3, Nothing, "", Enabled
  vpmSolToggleObj flash_l_4, Nothing, "", Enabled
End SUb

Sub SolFlasherRight(Enabled)
  vpmSolToggleObj flash_r_1, Nothing, "", Enabled
  vpmSolToggleObj flash_r_2, Nothing, "", Enabled
  vpmSolToggleObj flash_r_3, Nothing, "", Enabled
  vpmSolToggleObj flash_r_4, Nothing, "", Enabled
End SUb

Sub SolFlasherLeftMid(Enabled)
  vpmSolToggleObj flash_mid_l_1, Nothing, "", Enabled
  vpmSolToggleObj flash_mid_l_2, Nothing, "", Enabled
  vpmSolToggleObj flash_mid_l_3, Nothing, "", Enabled
  vpmSolToggleObj flash_mid_l_4, Nothing, "", Enabled
End SUb

Sub SolFlasherRightMid(Enabled)
  vpmSolToggleObj flash_mid_r_1, Nothing, "", Enabled
  vpmSolToggleObj flash_mid_r_1, Nothing, "", Enabled
  vpmSolToggleObj flash_mid_r_1, Nothing, "", Enabled
  vpmSolToggleObj flash_mid_r_1, Nothing, "", Enabled
End SUb

Sub SolFlasherLeftUp(Enabled)
  vpmSolToggleObj flash_top_l_1, Nothing, "", Enabled
  vpmSolToggleObj flash_top_l_2, Nothing, "", Enabled
  vpmSolToggleObj flash_top_l_3, Nothing, "", Enabled
  vpmSolToggleObj flash_top_l_4, Nothing, "", Enabled
End SUb

Sub SolFlasherRightUp(Enabled)
  vpmSolToggleObj flash_top_r_1, Nothing, "", Enabled
  vpmSolToggleObj flash_top_r_2, Nothing, "", Enabled
  vpmSolToggleObj flash_top_r_3, Nothing, "", Enabled
  vpmSolToggleObj flash_top_r_4, Nothing, "", Enabled
End SUb

'+++++++++++++++++++++++++


'**********Sling Shot Animations
' Rstep and Lstep  are the variables that increment the animation
'****************
Dim RStep, Lstep, LstepTop, RstepTop

Sub RightSlingShot_Slingshot
	vpmTimer.PulseSw 26 ' pulse switch same Controller.Switch 26, 1/0
    PlaySound SoundFX("right_slingshot",DOFContactors), 0,1, 0.05,0.05 '0,1, AudioPan(RightSlingShot), 0.05,0,0,1,AudioFade(RightSlingShot)
    RSling.Visible = 0
    RSling1.Visible = 1
    sling1.rotx = 20
    RStep = 0
    RightSlingShot.TimerEnabled = 1
End Sub

Sub RightSlingShot_Timer
    Select Case RStep
        Case 3:RSLing1.Visible = 0:RSLing2.Visible = 1:sling1.rotx = 10
        Case 4:RSLing2.Visible = 0:RSLing.Visible = 1:sling1.rotx = 0:RightSlingShot.TimerEnabled = 0
    End Select
    RStep = RStep + 1
End Sub

Sub LeftSlingShot_Slingshot
	vpmTimer.PulseSw 25
    PlaySound SoundFX("left_slingshot",DOFContactors), 0,1, -0.05,0.05 '0,1, AudioPan(LeftSlingShot), 0.05,0,0,1,AudioFade(LeftSlingShot)
    LSling.Visible = 0
    LSling1.Visible = 1
    sling2.rotx = 20
    LStep = 0
    LeftSlingShot.TimerEnabled = 1
End Sub

Sub LeftSlingShot_Timer
    Select Case LStep
        Case 3:LSLing1.Visible = 0:LSLing2.Visible = 1:sling2.rotx = 10
        Case 4:LSLing2.Visible = 0:LSLing.Visible = 1:sling2.rotx = 0:LeftSlingShot.TimerEnabled = 0
    End Select
    LStep = LStep + 1
End Sub

Sub TopLeftSlingShot_Slingshot
    'PlaySound SoundFX("left_slingshot",DOFContactors), 0,1, -0.05,0.05 '0,1, AudioPan(LeftSlingShot), 0.05,0,0,1,AudioFade(LeftSlingShot)
    TopLeftSling.Visible = 0
    TopLeftSling1.Visible = 1
    SLING2TOP.TransZ = -20
    LstepTop = 0
    TopLeftSlingShot.TimerEnabled = 1	
	vpmTimer.PulseSw 53
End Sub

Sub TopLeftSlingShot_Timer
    Select Case LstepTop
        Case 3:TopLeftSling1.Visible = 0:TopLeftSling2.Visible = 1:SLING2TOP.TransZ = -10
        Case 4:TopLeftSling2.Visible = 0:TopLeftSling.Visible = 1:SLING2TOP.TransZ = 0:TopLeftSlingShot.TimerEnabled = 0
    End Select
    LstepTop = LstepTop + 1
End Sub

Sub TopRightSlingShot_Slingshot
    'PlaySound SoundFX("left_slingshot",DOFContactors), 0,1, -0.05,0.05 '0,1, AudioPan(LeftSlingShot), 0.05,0,0,1,AudioFade(LeftSlingShot)
    TopRightSling.Visible = 0
    TopRightSling1.Visible = 1
    SLING2TOP1.TransZ = -20
    RstepTop = 0
    TopRightSlingShot.TimerEnabled = 1	
	vpmTimer.PulseSw 54
End Sub

Sub TopRightSlingShot_Timer
    Select Case RstepTop
        Case 3:TopRightSling1.Visible = 0:TopRightSling2.Visible = 1:SLING2TOP1.TransZ = -10
        Case 4:TopRightSling2.Visible = 0:TopRightSling.Visible = 1:SLING2TOP1.TransZ = 0:TopRightSlingShot.TimerEnabled = 0
    End Select
    RstepTop = RstepTop + 1
End Sub

' PINGOD END
'*********************************************

'Table Example scripts
Dim EnableBallControl
EnableBallControl = false 'Change to true to enable manual ball control (or press C in-game) via the arrow keys and B (boost movement) keys

'*********************************************************************
'                 Positional Sound Playback Functions
'*********************************************************************

' Play a sound, depending on the X,Y position of the table element (especially cool for surround speaker setups, otherwise stereo panning only)
' parameters (defaults): loopcount (1), volume (1), randompitch (0), pitch (0), useexisting (0), restart (1))
' Note that this will not work (currently) for walls/slingshots as these do not feature a simple, single X,Y position
Sub PlayXYSound(soundname, tableobj, loopcount, volume, randompitch, pitch, useexisting, restart)
	PlaySound soundname, loopcount, volume, AudioPan(tableobj), randompitch, pitch, useexisting, restart, AudioFade(tableobj)
End Sub

' Similar subroutines that are less complicated to use (e.g. simply use standard parameters for the PlaySound call)
Sub PlaySoundAt(soundname, tableobj)
    PlaySound soundname, 1, 1, AudioPan(tableobj), 0,0,0, 1, AudioFade(tableobj)
End Sub

Sub PlaySoundAtBall(soundname)
    PlaySoundAt soundname, ActiveBall
End Sub


'*********************************************************************
'                     Supporting Ball & Sound Functions
'*********************************************************************

Function AudioFade(tableobj) ' Fades between front and back of the table (for surround systems or 2x2 speakers, etc), depending on the Y position on the table. "table1" is the name of the table
	Dim tmp
    tmp = tableobj.y * 2 / table1.height-1
    If tmp > 0 Then
		AudioFade = Csng(tmp ^10)
    Else
        AudioFade = Csng(-((- tmp) ^10) )
    End If
End Function

Function AudioPan(tableobj) ' Calculates the pan for a tableobj based on the X position on the table. "table1" is the name of the table
    Dim tmp
    tmp = tableobj.x * 2 / table1.width-1
    If tmp > 0 Then
        AudioPan = Csng(tmp ^10)
    Else
        AudioPan = Csng(-((- tmp) ^10) )
    End If
End Function

Function Vol(ball) ' Calculates the Volume of the sound based on the ball speed
    Vol = Csng(BallVel(ball) ^2 / 2000)
End Function

Function Pitch(ball) ' Calculates the pitch of the sound based on the ball speed
    Pitch = BallVel(ball) * 20
End Function

Function BallVel(ball) 'Calculates the ball speed
    BallVel = INT(SQR((ball.VelX ^2) + (ball.VelY ^2) ) )
End Function


'********************************************************************
'      JP's VP10 Rolling Sounds (+rothbauerw's Dropping Sounds)
'********************************************************************

Const tnob = 5 ' total number of balls
ReDim rolling(tnob)
InitRolling

Sub InitRolling
    Dim i
    For i = 0 to tnob
        rolling(i) = False
    Next
End Sub

Sub RollingTimer_Timer()
    Dim BOT, b
    BOT = GetBalls

    ' stop the sound of deleted balls
    For b = UBound(BOT) + 1 to tnob
        rolling(b) = False
        StopSound("fx_ballrolling" & b)
    Next

    ' exit the sub if no balls on the table
    If UBound(BOT) = -1 Then Exit Sub

    For b = 0 to UBound(BOT)
        ' play the rolling sound for each ball
        If BallVel(BOT(b) ) > 1 AND BOT(b).z < 30 Then
            rolling(b) = True
            PlaySound("fx_ballrolling" & b), -1, Vol(BOT(b)), AudioPan(BOT(b)), 0, Pitch(BOT(b)), 1, 0, AudioFade(BOT(b))
        Else
            If rolling(b) = True Then
                StopSound("fx_ballrolling" & b)
                rolling(b) = False
            End If
        End If

        ' play ball drop sounds
        If BOT(b).VelZ < -1 and BOT(b).z < 55 and BOT(b).z > 27 Then 'height adjust for ball drop sounds
            PlaySound "fx_ball_drop" & b, 0, ABS(BOT(b).velz)/17, AudioPan(BOT(b)), 0, Pitch(BOT(b)), 1, 0, AudioFade(BOT(b))
        End If
    Next
End Sub

'**********************
' Ball Collision Sound
'**********************

Sub OnBallBallCollision(ball1, ball2, velocity)
	PlaySound("fx_collide"), 0, Csng(velocity) ^2 / 2000, AudioPan(ball1), 0, Pitch(ball1), 0, 0, AudioFade(ball1)
End Sub

Sub RampWire_Hit (idx)
	PlaySound "Rwireramp", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 0, 0
End Sub

Sub BallToWood_Hit (idx)
	PlaySound "Rballfall", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 0, 0
End Sub

Sub Pins_Hit (idx)
	PlaySound "pinhit_low", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 0, 0, AudioFade(ActiveBall)
End Sub

Sub Targets_Hit (idx)
	PlaySound "target", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 0, 0, AudioFade(ActiveBall)
End Sub

Sub Metals_Thin_Hit (idx)
	PlaySound "metalhit_thin", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
End Sub

Sub Metals_Medium_Hit (idx)
	PlaySound "metalhit_medium", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
End Sub

Sub Metals2_Hit (idx)
	PlaySound "metalhit2", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
End Sub

Sub Gates_Hit (idx)
	PlaySound "gate4", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
End Sub

Sub Spinner_Spin
	PlaySound "fx_spinner", 0, .25, AudioPan(Spinner), 0.25, 0, 0, 1, AudioFade(Spinner)
End Sub

Sub Bumpers_Hit (idx)
	PlaySound "bumperA", 0, 0.8, AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
End Sub


Sub Rubbers_Hit(idx)
 	dim finalspeed
  	finalspeed=SQR(activeball.velx * activeball.velx + activeball.vely * activeball.vely)
 	If finalspeed > 20 then 
		PlaySound "fx_rubber2", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
	End if
	If finalspeed >= 6 AND finalspeed <= 20 then
 		RandomSoundRubber()
 	End If
End Sub

Sub Posts_Hit(idx)
 	dim finalspeed
  	finalspeed=SQR(activeball.velx * activeball.velx + activeball.vely * activeball.vely)
 	If finalspeed > 16 then 
		PlaySound "fx_rubber2", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
	End if
	If finalspeed >= 6 AND finalspeed <= 16 then
 		RandomSoundRubber()
 	End If
End Sub

Sub RandomSoundRubber()
	Select Case Int(Rnd*3)+1
		Case 1 : PlaySound "rubber_hit_1", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
		Case 2 : PlaySound "rubber_hit_2", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
		Case 3 : PlaySound "rubber_hit_3", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
	End Select
End Sub

Sub LeftFlipper_Collide(parm)
 	RandomSoundFlipper()
End Sub

Sub RightFlipper_Collide(parm)
 	RandomSoundFlipper()
End Sub

Sub RandomSoundFlipper()
	Select Case Int(Rnd*3)+1
		Case 1 : PlaySound "flip_hit_1", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
		Case 2 : PlaySound "flip_hit_2", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
		Case 3 : PlaySound "flip_hit_3", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
	End Select
End Sub


'+++++++++++++++++++++++++
' LAMPSHOWS
'+++++++++++++++++++++++++

Sub LampshowRandom(Enabled)
	if Enabled Then	
		LightSeq001.StopPlay
		Dim randInt
		randInt = Int((2 - 0 + 1) * Rnd + 0)
		Select Case randInt
			Case 0: LampshowRandom_0
			Case 1: LampshowRandom_1
			Case 2: LampshowRandom_2
		End Select
	End if
End Sub

Sub LampShowRandom_0
	LightSeq001.UpdateInterval = 5
	LightSeq001.Play SeqArcBottomLeftUpOn,10,1,0 ' total ms: 500
	LightSeq001.Play SeqArcBottomRightUpOn,10,1,0 ' total ms: 500
End Sub

Sub LampShowRandom_1
	LightSeq001.UpdateInterval = 3
	LightSeq001.Play SeqFanLeftUpOn,5,1,0 ' total ms: 555
	LightSeq001.Play SeqFanRightDownOn,5,1,0 ' total ms: 555
End Sub

Sub LampShowRandom_2
	LightSeq001.UpdateInterval = 5
	LightSeq001.Play SeqArcBottomLeftUpOff,5,1,0 ' total ms: 475
	LightSeq001.Play SeqArcBottomLeftDownOff,5,1,0 ' total ms: 475
End Sub

Sub LampShowFlash(Enabled)
	if Enabled Then	
		LightSeqFlash.StopPlay : LightSeqGI.StopPlay
		Dim randInt
		randInt = Int((2 - 0 + 1) * Rnd + 0)
		Select Case randInt
			Case 0:LampshowFlash_0
			Case 1:LampshowFlash_1
			Case 2:LampshowFlash_2
		End Select
	End if
End Sub

Sub LampShowFlash_0
	LightSeqFlash.UpdateInterval = 3
	LightSeqFlash.Play SeqMiddleOutHorizOn,1,1,0 ' total ms: 90
	LightSeqFlash.UpdateInterval = 3
	LightSeqFlash.Play SeqStripe1HorizOff,0,1,100 ' total ms: 250
	
	LightSeqGI.UpdateInterval = 3
	LightSeqGI.Play SeqMiddleOutHorizOn,1,1,0 ' total ms: 90
	LightSeqGI.UpdateInterval = 3
	LightSeqGI.Play SeqStripe1HorizOff,0,1,100 ' total ms: 250
End Sub

Sub LampShowFlash_1
	LightSeqFlash.UpdateInterval = 10
	LightSeqFlash.Play SeqBlinking,0,6,50 ' total ms: 262
	
	LightSeqGI.UpdateInterval = 1
	LightSeqGI.Play SeqScrewLeftOn,0,1,2 ' total ms: 182
	LightSeqGI.UpdateInterval = 1
	LightSeqGI.Play SeqClockLeftOff,0,1,0 ' total ms: 360
	LightSeqGI.Play SeqBlinking,0,2,50 ' total ms: 262
End Sub

Sub LampShowFlash_2
	LightSeqFlash.UpdateInterval = 1
	LightSeqFlash.Play SeqScrewLeftOn,0,1,2 ' total ms: 182
	LightSeqFlash.UpdateInterval = 1
	LightSeqFlash.Play SeqClockLeftOff,0,1,0 ' total ms: 360
	LightSeqFlash.Play SeqBlinking,0,2,50 ' total ms: 262
	
	LightSeqGI.UpdateInterval = 10
	LightSeqGI.Play SeqBlinking,0,6,50 ' total ms: 262
End Sub

Sub LampshowPursuit (Enabled)
	'Start multiball
	if Enabled Then
		LightSeq001.StopPlay
		LightSeq001.UpdateInterval = 10
		LightSeq001.Play SeqStripe2VertOn,0,1,0 ' total ms: 1000
		LightSeq001.UpdateInterval = 10
		LightSeq001.Play SeqStripe2VertOff,0,1,0 ' total ms: 1000
		LightSeq001.UpdateInterval = 10
		LightSeq001.Play SeqCircleOutOn,0,1,0 ' total ms: 1000
		LightSeq001.UpdateInterval = 3
		LightSeq001.Play SeqClockRightOff,0,1,0 ' total ms: 1080
		LightSeq001.UpdateInterval = 10
		LightSeq001.Play SeqUpOn,0,1,0 ' total ms: 1000
		LightSeq001.UpdateInterval = 10
		LightSeq001.Play SeqDownOff,0,1,0 ' total ms: 1000
		LightSeq001.UpdateInterval = 100
		LightSeq001.Play SeqBlinking,0,8,100 ' total ms: 1000
		LightSeq001.UpdateInterval = 50
		LightSeq001.Play SeqBlinking,0,18,50 ' total ms: 1000
		LightSeq001.UpdateInterval = 25
		LightSeq001.Play SeqBlinking,0,36,25 ' total ms: 1850

		LightSeqFlash.StopPlay
		LightSeqFlash.UpdateInterval = 1
		LightSeqFlash.Play SeqStripe1VertOff,0,90,0 ' total ms: 6000
		LightSeqFlash.Play SeqBlinking,0,63,25 ' total ms: 1850

		LightSeqGI.StopPlay
		LightSeqGI.UpdateInterval = 1
		LightSeqGI.Play SeqStripe1VertOff,0,90,0 ' total ms: 6000
		LightSeqGI.Play SeqBlinking,0,63,25 ' total ms: 1850
	Else
		LightSeq001.StopPlay
	End If
End Sub


Sub LampshowAttract(Enabled)
	if Enabled Then
		LightSeqAttract.UpdateInterval = 10
		LightSeqAttract.Play SeqScrewLeftOn,90,1,0 ' total ms: 2700
		LightSeqAttract.UpdateInterval = 10
		LightSeqAttract.Play SeqScrewRightOff,90,1,0 ' total ms: 2700
		LightSeqAttract.UpdateInterval = 10
		LightSeqAttract.Play SeqRightOn,180,1,0 ' total ms: 1400
		LightSeqAttract.UpdateInterval = 10
		LightSeqAttract.Play SeqLeftOff,180,1,0 ' total ms: 1400
	Else
		LightSeqAttract.StopPlay
	End If
End Sub


'Restart the attract to loop it
Sub LightSeqAttract_PlayDone()
	LampshowAttract 1
End Sub