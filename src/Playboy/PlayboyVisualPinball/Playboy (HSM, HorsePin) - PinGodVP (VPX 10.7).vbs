Option Explicit
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
'Const GameDirectory = ".\PinGod.Playboy.exe" 'exported game
'Debug
Const IsDebug = True
Const GameDirectory = "..\PlayboyGodot" ' Loads the godot pingod game project
Const UseSolenoids = 1 ' Check for solenoid states?
Const UsePdbLeds = 0  ' use led (color)
Const UseLamps = 1  ' Check for lamp states?
Dim bsTrough, bsSaucer, dtRBank

'~~~~~~~~~~~~~Switch Constants~~~~~~~~~~~~~~~~~~~~~
Const swTrough1 = 81, swTrough2 = 82, swTrough3 = 83, swTrough4 = 84
Const swShooterLane = 20, swOutlaneL = 21, swOutlaneR = 24, swReturnlaneL = 22, swReturnlaneR = 23
Const swslingL = 25, swslingR = 26
Const swbumpL = 28, swbumpM = 29, swbumpR = 30
Const tL1 = 31, tL2 = 32, tL3 = 33, tL4 = 34, tL5 = 35
Const tR1 = 36, tR2 = 37, tR3 = 38, tR4 = 39, tR5 = 40
Const lane1 = 41, lane2 = 42, lane3 = 43, lane4 = 44
Const rLoop = 45, swSaucer = 27, swXtraBall  = 46, swKeyTarget  = 47
'~~~~~~~~~~~~~End Switch Constants~~~~~~~~~~~~~~~~~~~~~
'**********************
' VP table display / controller events
'**********************
Sub Table1_Exit : Controller.Stop : End Sub ' Closes the display window, sends the quit action
Sub Table1_Paused: Controller.Pause 1 :  Controller.Pause 0 : End Sub
Sub Table1_UnPaused: Controller.Pause 1 : End Sub

'**********************
' VP init
' Inits the controller then waits for the display to fully load into initial scene.
'**********************
Sub Table1_Init	
	With Controller
		.DisplayX			= 0
		.DisplayY			= 0
		.DisplayWidth 		= 1280 ' 1280 FS
		.DisplayHeight 		= 720  ' 720  FS
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
	'vpmCreateEvents AllSwitches 'Auto Switches collection from the swNum in timerInterval

	'Init timers for updates
	pulsetimer.Enabled=1
	PinMAMETimer.Enabled=1		
	
	On Error Resume Next
	Set bsTrough = New cvpmTrough
	bsTrough.Size = 4
	bsTrough.Balls = 4
	bsTrough.InitSwitches Array(swTrough1,swTrough2,swTrough3, swTrough4) ' trough switches
	bsTrough.InitExit BallRelease, 90, 8	
	bsTrough.CreateEvents "bsTrough",  Drain
	bsTrough.InitEntrySounds "Drain", "", ""
	bsTrough.InitExitSounds "BallRelease", ""
	bsTrough.Reset		

	Set bsSaucer=new cvpmBallStack
	  With bsSaucer
		.InitSaucer Kicker1, swSaucer, 0, 26
		.InitExitSnd "Popper_Ball",""
		.KickForceVar = 5
		End With

	Set dtRBank = New cvpmDropTarget
		With dtRBank
		.InitDrop Array (Target7, Target8, Target9, Target10, Target11), array (tR1,tR2,tR3,tR4,tR5)
		.InitSnd "DropTarget", "fx_resetdrop"
		End With

'    ' ### Nudging ###
    vpmNudge.TiltSwitch = swTilt
    vpmNudge.Sensitivity = 0.8
	vpmNudge.TiltObj = Array(LSling,RSling)


	If Err Then MsgBox Err.Description
	initialized = 1
	LoadingText.Visible = false ' Hide the overlay (loading screen)
	On Error Goto 0	
	
End Sub

'flippers 0 - 3
SolCallback(0) = "Died"
SolCallback(1) = "bsTrough.solOut" ' This exit is setup in the init
SolCallback(2) = "FlippersEnabled"
'SolCallback(5)     		= "SlingL"
'SolCallback(6) 		= "SlingR"
SolCallback(4)     		= "bsSaucer.SolOut"
SolCallback(5) 		= "dtRBank.SolDropUp"
'SolCallback(sLRFlipper) = "SolRFlipper"
'SolCallback(sLLFlipper) = "SolLFlipper"
'SolCallback(2)     		= "vpmSolSound Soundfx(""Knocker"",DOFKnocker)" 

Sub Died(Enabled)
	'on error resume next	
	If not enabled then
		'MsgBox "Game window unavailable." : Err.Raise 5
	End if
End Sub

'Do it like this as one on/off, will make it faster
Dim FlippersOn : FlippersOn = 0
Sub FlippersEnabled(Enabled)
	'Debug.Print "flippers on coil " & Enabled
	FlippersOn = Enabled
	If not FlippersOn then LeftFlipper.RotateToStart : RightFlipper.RotateToStart
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
		LeftFlipper.RotateToEnd : PlaySoundAt "FlipperUp", LeftFlipper
	End If

	If keycode = RightFlipperKey and FlippersOn Then
		RightFlipper.RotateToEnd : PlaySoundAt "FlipperUp", RightFlipper
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
		LeftFlipper.RotateToStart : PlaySoundAt "FlipperDown", LeftFlipper
	End If

	If keycode = RightFlipperKey and FlippersOn Then
		RightFlipper.RotateToStart : PlaySoundAt "FlipperDown", RightFlipper		
	End If

	If vpmKeyUp(keycode) Then Exit Sub ' This will handle machine switches and flippers etc
End Sub
'~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

'~~~~~~~~Switch events~~~~~~~~~~~
'std
Sub shooter_Hit:Controller.Switch swShooterLane, 1:End Sub
Sub shooter_UnHit:Controller.Switch swShooterLane, 0:End Sub
Sub LeftOutLane_Hit:Controller.Switch swOutlaneL, 1:End Sub
Sub LeftOutLane_UnHit:Controller.Switch swOutlaneL, 0:End Sub
Sub LeftInLane_Hit:Controller.Switch swReturnlaneL, 1: PlaySound "WireRight", 0, 1, AudioPan(LeftInLane), 0.05,0,0,1,AudioFade(LeftInLane):End Sub
Sub LeftInLane_UnHit:Controller.Switch swReturnlaneL, 0:End Sub
Sub RightInLane_Hit:Controller.Switch swReturnlaneR, 1:PlaySound "WireRight", 0, 1, AudioPan(RightInLane), 0.05,0,0,1,AudioFade(RightInLane):End Sub
Sub RightInLane_UnHit:Controller.Switch swReturnlaneR, 0:End Sub
Sub RightOutLane_Hit:Controller.Switch swOutlaneR, 1:End Sub
Sub RightOutLane_UnHit:Controller.Switch swOutlaneR, 0:End Sub
'left targets
Sub Target1_Hit:vpmTimer.PulseSw tL1:End Sub
Sub Target2_Hit:vpmTimer.PulseSw tL2:End Sub
Sub Target3_Hit:vpmTimer.PulseSw tL3:End Sub
Sub Target4_Hit:vpmTimer.PulseSw tL4:End Sub
Sub Target5_Hit:vpmTimer.PulseSw tL5:End Sub
'extra ball
Sub Trigger2_Hit:Controller.Switch rLoop, 1:End Sub
Sub Trigger2_UnHit:Controller.Switch rLoop, 0:End Sub
'top lanes
Sub Trigger6_Hit:Controller.Switch lane1, 1:End Sub
Sub Trigger6_UnHit:Controller.Switch lane1, 0:End Sub
Sub Trigger5_Hit:Controller.Switch lane2, 1:End Sub
Sub Trigger5_UnHit:Controller.Switch lane2, 0:End Sub
Sub Trigger4_Hit:Controller.Switch lane3, 1:End Sub
Sub Trigger4_UnHit:Controller.Switch lane3, 0:End Sub
Sub Trigger3_Hit:Controller.Switch lane4, 1:End Sub
Sub Trigger3_UnHit:Controller.Switch lane4, 0:End Sub
Sub Target6_Hit : vpmTimer.pulseSw swKeyTarget: End Sub
'loop
Sub Trigger8_Hit:Controller.Switch rLoop, 1:End Sub
Sub Trigger8_UnHit:Controller.Switch rLoop, 0:End Sub
'bumpers
Sub Bumper1_Hit:  vpmTimer.pulseSw swbumpL: RandomSoundBumper:End Sub
Sub Bumper2_Hit:  vpmTimer.pulseSw swbumpM: RandomSoundBumper:End Sub
Sub Bumper3_Hit:  vpmTimer.pulseSw swbumpR: RandomSoundBumper:End Sub

Sub Kicker1_Hit: bsSaucer.AddBall Me: End Sub

''**********Sling Shot Animations
'' Rstep and Lstep  are the variables that increment the animation
''****************
Dim RStep, Lstep

Sub RightSlingShot_Slingshot
	PlaySound "Sling",0,1, 0.05,0.05
    RSling.Visible = 0
    RSling1.Visible = 1
    sling1.TransZ = -20
    RStep = 0
    RightSlingShot.TimerEnabled = 1
	vpmTimer.PulseSw swslingR
End Sub

Sub RightSlingShot_Timer
    Select Case RStep
        Case 3:RSLing1.Visible = 0:RSLing2.Visible = 1:sling1.TransZ = -10
        Case 4:RSLing2.Visible = 0:RSLing.Visible = 1:sling1.TransZ = 0:RightSlingShot.TimerEnabled = 0:gi1.State = 1:Gi2.State = 1
    End Select
    RStep = RStep + 1
End Sub

Sub LeftSlingShot_Slingshot	
	PlaySound "Sling", 0,1, -0.05,0.05
    LSling.Visible = 0
    LSling1.Visible = 1
    sling2.TransZ = -20
    LStep = 0
    LeftSlingShot.TimerEnabled = 1
	vpmTimer.PulseSw swslingL
End Sub

Sub LeftSlingShot_Timer
    Select Case LStep
        Case 3:LSLing1.Visible = 0:LSLing2.Visible = 1:sling2.TransZ = -10
        Case 4:LSLing2.Visible = 0:LSLing.Visible = 1:sling2.TransZ = 0:LeftSlingShot.TimerEnabled = 0:gi3.State = 1:Gi4.State = 1
    End Select
    LStep = LStep + 1
End Sub

'~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

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
Dim BallShadow,Ballsize
Ballsize	=	50
BallShadow 	= 	Array (BallShadow1,BallShadow2,BallShadow3,BallShadow4,BallShadow5)

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
'***********************************************************

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

Sub Rubbers_Hit(idx)
 	dim finalspeed
  	finalspeed=SQR(activeball.velx * activeball.velx + activeball.vely * activeball.vely)
 	If finalspeed > 20 then 
		PlaySound "RubberTopRight", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
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
		Case 1 : PlaySound "FlipperCollide", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
		Case 2 : PlaySound "FlipperCollide", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
		Case 3 : PlaySound "FlipperCollide", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
	End Select
End Sub

Sub RandomSoundBumper()
	Select Case Int(Rnd*3)+1
		Case 1 : PlaySound "Bumper"
		Case 2 : PlaySound "Bumper"
		Case 3 : PlaySound "Bumper"
	End Select
End Sub

''*********************************************************************
''                 Positional Sound Playback Functions
''*********************************************************************

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
