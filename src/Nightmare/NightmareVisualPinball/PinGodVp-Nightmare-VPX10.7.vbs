Option Explicit

On Error Resume Next ' Std Controller for DOF etc.
	ExecuteGlobal GetTextFile("controller.vbs")
	If Err Then MsgBox "You need the Controller.vbs file in order to run this table (installed with the VPX package in the scripts folder)"
On Error Goto 0

LoadPinGoVpController 
Sub LoadPinGoVpController ' PinGod.Vp.Controller. Requires modded Core Vbs for different switches csharp
	On Error Resume Next
		If ScriptEngineMajorVersion<5 Then MsgBox "VB Script Engine 5.0 or higher required"
		ExecuteGlobal GetTextFile("PinGod.vbs")
		If Err Then MsgBox "Unable to open " & VBSfile & ". Ensure that it is in the same folder as this table. " & vbNewLine & Err.Description
		Set Controller=CreateObject("PinGod.VP.Controller")		
		If Err Then MsgBox "Failed to initialize PinGod.VP controller, is it registered?" : Exit Sub
	On Error Goto 0
End Sub

'Debug builds
Const IsDebug = True
Const GameDirectory = "..\NightmareGodot"
Const UseSolenoids = 1 ' Check for solenoid states?
Const UsePdbLeds = 0
Const UseLamps = 1  ' Check for lamp states?

Dim B2SController, b2sOn ' For Translites
BallSize = 154  'Ball radius
Dim bsTrough, bsSaucer, dtRBank

Sub Table1_Init	
	With Controller
		.DisplayX			= 1920 - 512
		.DisplayY			= 10
		.DisplayWidth 		= 512 ' 1024 FS
		.DisplayHeight 		= 300 ' 600  FS
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
	
	'Wait for game display to be ready so the VPPlayer doesn't get stuck
	'Using any old game object Timer, usually you will have LeftFlipper but change this to what you please
	LeftFlipper.TimerInterval = 369
	LeftFlipper.TimerEnabled = 1		
End Sub
'Game ready checker from flipper timer
Sub LeftFlipper_Timer
	LeftFlipper.TimerEnabled = 0
	if not Controller.GameRunning Then LeftFlipper.TimerEnabled = 1 : Exit Sub
	InitGame
End Sub

Sub Table1_Exit : Controller.Stop : End Sub ' Closes the display window, sends the quit action
Sub Table1_Paused: Controller.Pause 1 : End Sub
Sub Table1_UnPaused: Controller.Pause 0 : End Sub
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

	LoadingText.Visible = false
	On Error Resume Next
	Set bsTrough = New cvpmTrough
	bsTrough.Size = 4
	bsTrough.Balls = 4
	bsTrough.InitSwitches Array(81,82,83,84) ' trough switches
	bsTrough.InitExit BallRelease, 90, 8	
	bsTrough.CreateEvents "bsTrough",  Drain
	bsTrough.InitEntrySounds "Drain", "", ""
	bsTrough.InitExitSounds "BallRelease", ""
	bsTrough.Reset		

	' Left Saucer
	Set bsLSaucer = new cvpmBallStack
	bsLSaucer.InitSaucer sw25, 27, 200, 8
    'bsLSaucer.InitEntrySnd "Solenoid", "Solenoid"
    bsLSaucer.InitExitSnd "popper_ball", "Solenoid"
	bsLSaucer.CreateEvents "bsLSaucer",  sw25
'
'	' Top Saucer
	Set bsTSaucer = new cvpmBallStack
	bsTSaucer.InitSaucer sw31, 31, 160, 10    
    bsTSaucer.InitExitSnd "popper_ball", "Solenoid"

'	' Top Right Saucer
	Set bsTRSaucer = new cvpmBallStack
	bsTRSaucer.InitSaucer sw32, 32, 280, 30    
    bsTRSaucer.InitExitSnd "popper_ball", "Solenoid"

'    ' ### Nudging ###
    vpmNudge.TiltSwitch = swTilt
    vpmNudge.Sensitivity = 0.8
	vpmNudge.TiltObj = Array(LSling,RSling)

    Plunger.PullBack
	Plunger.Fire


	If Err Then MsgBox Err.Description

	initialized = 1
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
		PlaySound SoundFX("fx_flipperup",DOFFlippers), 0, .67, AudioPan(LeftFlipper), 0.05,0,0,1,AudioFade(LeftFlipper)
	End If

	If keycode = RightFlipperKey and FlippersOn Then
		RightFlipper.RotateToEnd
		PlaySound SoundFX("fx_flipperup",DOFFlippers), 0, .67, AudioPan(RightFlipper), 0.05,0,0,1,AudioFade(RightFlipper)
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
		PlaySound SoundFX("fx_flipperdown",DOFFlippers), 0, 1, AudioPan(LeftFlipper), 0.05,0,0,1,AudioFade(LeftFlipper)
	End If

	If keycode = RightFlipperKey and FlippersOn Then
		RightFlipper.RotateToStart
		PlaySound SoundFX("fx_flipperdown",DOFFlippers), 0, 1, AudioPan(RightFlipper), 0.05,0,0,1,AudioFade(RightFlipper)
	End If

	If vpmKeyUp(keycode) Then Exit Sub ' This will handle machine switches and flippers etc
End Sub

 'Dim bsTrough 'The trough object'
Dim bsLSaucer,bsTSaucer, bsTRSaucer ' Saucers
'****************************
' Solenoids / Coils / Callbacks
'****************************
SolCallback(0) = "Died"
SolCallback(1) = "bsTrough.solOut" ' This exit is setup in the init
SolCallback(2) = "FlippersEnabled"
'SolCallback(3) = "AutoPlunger"
SolCallback(4) = "bsLSaucer.solOut" ' This exit is setup in the init
SolCallback(5) = "Solsaucer_top"
SolCallback(6) = "Solsaucer_top_right"
SolCallback(7) = "Solsaucer_left_tunnel"
SolCallback(8) = "Solsaucer_topright_tunnel"
'SolCallback(9) = "Solsaucer_topright_tunnel"
SolCallback(10) = "SolLeftFlash"
SolCallback(11) = "SolRightFlash"
SolCallback(12) = "SolLeftMidFlash"
SolCallback(13) = "SolTopRightFlash"
SolCallback(14) = "SolTopLeftFlash"
SolCallback(15) = "SolRightFlashMid"
'SolCallback(16) = "SolRightFlash"
'SolCallback(15) = "SolslingL"
'SolCallback(16) = "SolslingR"

Sub Died(Enabled)
	'on error resume next	
	If not enabled then
		MsgBox "Game window unavailable." : Err.Raise 5
	End if
End Sub

'Do it like this as one on/off, will make it faster
Dim FlippersOn : FlippersOn = 0
Sub FlippersEnabled(Enabled)
	'Debug.Print "flippers on coil " & Enabled
	FlippersOn = Enabled
	If not FlippersOn then LeftFlipper.RotateToStart : RightFlipper.RotateToStart
End Sub

'################
'# Flippers
'###############

Sub SolLFlipper(Enabled)    
	If flippersEnabled Then
		If Enabled Then
			PlaySound "flipUpSound", 0, 1, -0.1, 0.25
			LeftFlipper.RotateToEnd
		Else        
			PlaySound "flipDownSound", 0, 1, -0.1, 0.25
			LeftFlipper.RotateToStart
		End If
	End If
End Sub

Sub SolRFlipper(Enabled)
	If flippersEnabled Then
		If Enabled Then
			PlaySound "flipUpSound", 0, 1, 0.1, 0.25
			RightFlipper.RotateToEnd
		Else        
			PlaySound "flipDownSound", 0, 1, 0.1, 0.25
			RightFlipper.RotateToStart
		End If
	End If
End Sub

'###############
' GAME SWITCHES
'###############

' saucerTop hit 
Sub sw31_Hit()
	bsTSaucer.AddBall 0
End Sub

' saucerTopRight hit 
Sub sw32_Hit()
	bsTRSaucer.AddBall 0
End Sub

Sub Solsaucer_top(Enabled)
If Enabled Then
	bsTSaucer.ExitSol_On
End If
End Sub

Sub Solsaucer_top_right(Enabled)
If Enabled Then
	bsTRSaucer.ExitSol_On
End If
End Sub

Sub Solsaucer_left_tunnel(Enabled)
	If Enabled Then
		sw25.DestroyBall
		Controller.Switch 25,0
		'bsLSaucer.ExitSol_On		
		Plunger.CreateBall
		sw41_Hit()
		Controller.Switch 41,0
	End If
End Sub

Sub Solsaucer_topright_tunnel(Enabled)
	If Enabled Then
		sw32.DestroyBall
		Controller.Switch 32,0
		'bsTRSaucer.ExitSol_On	
		Plunger.CreateBall		
		sw41_Hit()		
		Controller.Switch 41,0
	End If
End Sub

'#############
' SWITCHES FROM MANAGER
'##############
' shooterGate hit 
'Sub sw14_Hit():vpmTimer.PulseSw 14:End Sub 'TODO: DG

'Sounds
Dim snds_LSling,snds_RSling,snds_flips, flipperVol, snds_bumpers
Dim snds_AutoPlunge,snds_Drain,snds_BallRelease, snds_Scoop, snds_VUK
Dim snds_Saucer, snds_Kicker, snds_DropTargets,snds_DropTargetReset, snds_Targets

'Table Pyhsics Enabled - Table physics are directly loaded into the tables variable
Dim tbl_enabled

'Flipper settings
Dim flp_enabled, flp_strength, flp_friction, flp_mass, flp_elastic, flp_elasticFallOff

'******************************
' LAMPS & GI - 
' ADD ALL LAMPS TO A COLLECTION CALLED AllLamps
' In the Lamps TimerInterval box enter the lamp number from the machine.yaml
'******************************

Dim slider, xx
slider = (18 - Table1.nightday)/2
for each xx in GILow: xx.intensity = xx.intensity + slider: next
'for each xx in GIMid: xx.intensity = xx.intensity + slider: next
'for each xx in GITop: xx.intensity = xx.intensity + slider: next

Dim GIon
vpmMapLights AllLamps
Set LampCallback    = GetRef("UpdateMultipleLamps")
Sub UpdateMultipleLamps : End Sub

Set GICallback    = GetRef("UpdateGI")
Sub UpdateGI(giNo, stat)
  Select Case giNo
    Case 0 GI_low(abs(stat))
    Case 1 GI_up(abs(stat))
    Case 2 GI_mid(abs(stat)) ' GI_mid
  End Select
End Sub

Sub SolGI(enabled)
End Sub

Dim lamp
Sub GI_low(enabled)
   For each lamp in GILow
    lamp.State= enabled
   Next
    If enabled Then
		'ColGradeGI.enabled=True:ColGradeGIOff.enabled=False:i=5
		If b2sOn Then B2SController.B2ssetdata 100,1
    Else
		'ColGradeGI.enabled=False:ColGradeGIOff.enabled=True:i=1  
		If b2sOn Then B2SController.B2ssetdata 100,0
    End If

End Sub

Sub GI_up(enabled)
    For each lamp in GITop
    lamp.State=enabled
   Next
    If enabled Then
        If b2sOn Then B2SController.B2ssetdata 101,1
    Else
       If b2sOn Then B2SController.B2ssetdata 101,0
    End If

End Sub

Sub GI_mid(enabled)
   For each lamp in GIMid
       lamp.State=enabled
   Next
    If enabled Then
        If b2sOn Then B2SController.B2ssetdata 102,1
   Else
       If b2sOn Then B2SController.B2ssetdata 102,0
    End If
End Sub

Sub SolLeftFlash(Enabled)
  vpmSolToggleObj left_flash1, Nothing, "", Enabled
  vpmSolToggleObj left_flash2, Nothing, "", Enabled
  vpmSolToggleObj left_flash3, Nothing, "", Enabled
  vpmSolToggleObj left_flash4, Nothing, "", Enabled

End SUb

Sub SolLeftMidFlash(Enabled)
  vpmSolToggleObj left_flash_mid1, Nothing, "", Enabled
  vpmSolToggleObj left_flash_mid2, Nothing, "", Enabled
  vpmSolToggleObj left_flash_mid3, Nothing, "", Enabled
vpmSolToggleObj left_flash_mid4, Nothing, "", Enabled
End SUb

Sub SolTopLeftFlash(Enabled)
  vpmSolToggleObj top_left_flash1, Nothing, "", Enabled
  vpmSolToggleObj top_left_flash2, Nothing, "", Enabled
  vpmSolToggleObj top_left_flash3, Nothing, "", Enabled
vpmSolToggleObj top_left_flash4, Nothing, "", Enabled
vpmSolToggleObj top_left_flash5, Nothing, "", Enabled


End SUb

Sub SolRightFlash(Enabled)
  vpmSolToggleObj right_flash1, Nothing, "", Enabled
  vpmSolToggleObj right_flash2, Nothing, "", Enabled
  vpmSolToggleObj right_flash3, Nothing, "", Enabled
  vpmSolToggleObj right_flash7, Nothing, "", Enabled

End SUb

Sub SolRightFlashMid(Enabled)
  vpmSolToggleObj right_flash4, Nothing, "", Enabled
  vpmSolToggleObj right_flash5, Nothing, "", Enabled
  vpmSolToggleObj right_flash6, Nothing, "", Enabled
  vpmSolToggleObj right_flash8, Nothing, "", Enabled


End SUb

Sub SolTopRightFlash(Enabled)
  vpmSolToggleObj top_right_flash1, Nothing, "", Enabled
  vpmSolToggleObj top_right_flash2, Nothing, "", Enabled
  vpmSolToggleObj top_right_flash3, Nothing, "", Enabled
vpmSolToggleObj top_right_flash4, Nothing, "", Enabled

End SUb





'**********Sling Shot Animations
' Rstep and Lstep  are the variables that increment the animation
'****************
Dim RStep, Lstep

Sub RightSlingShot_Slingshot
    'PlaySound SoundFX("right_slingshot",DOFContactors), 0,1, 0.05,0.05 '0,1, AudioPan(RightSlingShot), 0.05,0,0,1,AudioFade(RightSlingShot)
    RSling.Visible = 0
    RSling1.Visible = 1
    sling1.TransZ = -20
    RStep = 0
    RightSlingShot.TimerEnabled = 1
	vpmTimer.PulseSw 26
End Sub

Sub RightSlingShot_Timer
    Select Case RStep
        Case 3:RSLing1.Visible = 0:RSLing2.Visible = 1:sling1.TransZ = -10
        Case 4:RSLing2.Visible = 0:RSLing.Visible = 1:sling1.TransZ = 0:RightSlingShot.TimerEnabled = 0
    End Select
    RStep = RStep + 1
End Sub

Sub LeftSlingShot_Slingshot
    'PlaySound SoundFX("left_slingshot",DOFContactors), 0,1, -0.05,0.05 '0,1, AudioPan(LeftSlingShot), 0.05,0,0,1,AudioFade(LeftSlingShot)
    LSling.Visible = 0
    LSling1.Visible = 1
    sling2.TransZ = -20
    LStep = 0
    LeftSlingShot.TimerEnabled = 1	
	vpmTimer.PulseSw 25
End Sub

Sub LeftSlingShot_Timer
    Select Case LStep
        Case 3:LSLing1.Visible = 0:LSLing2.Visible = 1:sling2.TransZ = -10
        Case 4:LSLing2.Visible = 0:LSLing.Visible = 1:sling2.TransZ = 0:LeftSlingShot.TimerEnabled = 0
    End Select
    LStep = LStep + 1
End Sub


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


'*****************************************
'   rothbauerw's Manual Ball Control
'*****************************************

Dim BCup, BCdown, BCleft, BCright
Dim ControlBallInPlay, ControlActiveBall
Dim BCvel, BCyveloffset, BCboostmulti, BCboost

BCboost = 1				'Do Not Change - default setting
BCvel = 4				'Controls the speed of the ball movement
BCyveloffset = -0.01 	'Offsets the force of gravity to keep the ball from drifting vertically on the table, should be negative
BCboostmulti = 3		'Boost multiplier to ball veloctiy (toggled with the B key) 

ControlBallInPlay = false

Sub StartBallControl_Hit()
	Set ControlActiveBall = ActiveBall
	ControlBallInPlay = true
End Sub

Sub StopBallControl_Hit()
	ControlBallInPlay = false
End Sub	

Sub BallControlTimer_Timer()
	If EnableBallControl and ControlBallInPlay then
		If BCright = 1 Then
			ControlActiveBall.velx =  BCvel*BCboost
		ElseIf BCleft = 1 Then
			ControlActiveBall.velx = -BCvel*BCboost
		Else
			ControlActiveBall.velx = 0
		End If

		If BCup = 1 Then
			ControlActiveBall.vely = -BCvel*BCboost
		ElseIf BCdown = 1 Then
			ControlActiveBall.vely =  BCvel*BCboost
		Else
			ControlActiveBall.vely = bcyveloffset
		End If
	End If
End Sub


'*****************************************
'      JP's VP10 Rolling Sounds
'*****************************************

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

	' play the rolling sound for each ball
    For b = 0 to UBound(BOT)
        If BallVel(BOT(b) ) > 1 AND BOT(b).z < 30 Then
            rolling(b) = True
            PlaySound("fx_ballrolling" & b), -1, Vol(BOT(b)), AudioPan(BOT(b)), 0, Pitch(BOT(b)), 1, 0, AudioFade(BOT(b))
        Else
            If rolling(b) = True Then
                StopSound("fx_ballrolling" & b)
                rolling(b) = False
            End If
        End If
    Next
End Sub

'**********************
' Ball Collision Sound
'**********************

Sub OnBallBallCollision(ball1, ball2, velocity)
	PlaySound("fx_collide"), 0, Csng(velocity) ^2 / 2000, AudioPan(ball1), 0, Pitch(ball1), 0, 0, AudioFade(ball1)
End Sub


'*****************************************
'	ninuzzu's	FLIPPER SHADOWS
'*****************************************

sub FlipperTimer_Timer()
	FlipperLSh.RotZ = LeftFlipper.currentangle
	FlipperRSh.RotZ = RightFlipper.currentangle

End Sub

'*****************************************
'	ninuzzu's	BALL SHADOW
'*****************************************
Dim BallShadow
BallShadow = Array (BallShadow1,BallShadow2,BallShadow3,BallShadow4,BallShadow5)

Sub BallShadowUpdate_timer()
    Dim BOT, b
    BOT = GetBalls
    ' hide shadow of deleted balls
    If UBound(BOT)<(tnob-1) Then
        For b = (UBound(BOT) + 1) to (tnob-1)
            BallShadow(b).visible = 0
        Next
    End If
    ' exit the Sub if no balls on the table
    If UBound(BOT) = -1 Then Exit Sub
    ' render the shadow for each ball
    For b = 0 to UBound(BOT)
        If BOT(b).X < Table1.Width/2 Then
            BallShadow(b).X = ((BOT(b).X) - (Ballsize/6) + ((BOT(b).X - (Table1.Width/2))/7)) + 6
        Else
            BallShadow(b).X = ((BOT(b).X) + (Ballsize/6) + ((BOT(b).X - (Table1.Width/2))/7)) - 6
        End If
        ballShadow(b).Y = BOT(b).Y + 12
        If BOT(b).Z > 20 Then
            BallShadow(b).visible = 1
        Else
            BallShadow(b).visible = 0
        End If
    Next
End Sub


'**************************************
' Explanation of the collision routine
'**************************************


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