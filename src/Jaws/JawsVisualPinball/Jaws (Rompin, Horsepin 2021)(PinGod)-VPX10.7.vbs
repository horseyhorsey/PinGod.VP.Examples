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
		ExecuteGlobal GetTextFile("Jaws-Lampshows.vbs") 'load lampshow methods
		If Err Then MsgBox "Unable to open " & VBSfile & ". Ensure that it is in the same folder as this table. " & vbNewLine & Err.Description
		Set Controller=CreateObject("PinGod.VP.Controller")		
		If Err Then MsgBox "Failed to initialize PinGod.VP controller, is it registered?" : Exit Sub
	On Error Goto 0
End Sub

'Release builds
Const IsDebug = False
Const GameDirectory = "PinGod.Jaws.exe"

'Debug builds
'Const IsDebug = True
'Const GameDirectory = "..\JawsGodot"
Const UseSolenoids = 1 ' Check for solenoid states?
Const UsePdbLeds = 0
Const UseLamps = 1  ' Check for lamp states?
Const SCoin="Coin"
Const SSolenoidOn="Solenoid"
Const SSolenoidOff=""
Const SFlipperOn="jawsFlipUp"
Const SFlipperOff="jawsFlipDown"

Dim bsTrough, bsSaucer, plungerIM, swSaucer : swSaucer = 27
Dim bsBarrelStack, bsVUK, bsJawsHole, mMagnet

'**********************
' VP table display / controller events
'**********************
Sub Table1_Exit : Controller.Stop : End Sub ' Closes the display window, sends the quit action
Sub Table1_Paused: Controller.Pause 1 : Controller.Pause 0 : End Sub
Sub Table1_UnPaused: Controller.Pause 0 : End Sub

'**********************
' VP init
' Inits the controller then waits for the display to fully load into initial scene.
'**********************
Sub Table1_Init	
	With Controller
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

'**********************
' GAME / VP init
' When the display is ready initialize VPM controller scripts and table objects
'**********************
Dim initialized : initialized = 0
Sub InitGame

	if initialized then exit sub ' prevent any chance of init twice if author decides to use LFlipper Timers

	'init core vbs, vpm
	vpmInit me
	
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
	
	'Barrel Kicker
     Set bsBarrelStack = New cvpmBallStack
     With bsBarrelStack
         .InitSw 0, 57, 0, 0, 0, 0, 0, 0
         .InitKick bsBarrel2, 210, 10
         .KickZ = 0.4         
         .KickBalls = 4 ' Kick all balls that go into scoop 
		 '.InitEntrySnd "Scoop", ""
		 .InitExitSnd "popper_ball", ""		 
     End With

	'Bruce VUK
    Set bsVUK = New cvpmBallStack
    With bsVUK
    .InitSaucer VUKMid2, 36, 180, 8
	.InitEntrySnd "solenoidleft", "SolenoidOn"
    .InitExitSnd "solenoidVUK", "Solenoid"
	'VUKMid.KickZ 90,90,1.5,0
    End With
	
		'Jaws KickOut
    Set bsJawsHole = New cvpmBallStack
    With bsJawsHole
      .InitSw 0, 37, 0, 0, 0, 0, 0, 0
      .InitKick JawsKickOut, 210, 30
      .Balls = 0
	  .CreateEvents "bsJawsHole",  JawsHole
	  .InitEntrySnd "solenoidleft", "SolenoidOn"
      .InitExitSnd "popper_ball", "SolenoidOn"
    End With
	
	'Orca Magnet
    Set mMagnet=New cvpmMagnet
    With mMagnet
       .InitMagnet OrcaMagnet, 8 '18
       .GrabCenter=True
       .CreateEvents "mMagnet"
    End With	

    ' ### Nudging ###
    vpmNudge.TiltSwitch = swTilt
    vpmNudge.Sensitivity = 0.8
	vpmNudge.TiltObj = Array(LSling,RSling)

	SolOrcaMagnet 1
	SolJaws 1
	Plunger.PullBack
	KickBack.PullBack

	'Drop targets down
	SolJawsTargetA 1 : SolJawsTargetB 1 : SolJawsTargetC 1
    SolJawsTargetD 1 : SolJawsTargetE 1 : SolJawsTargetF 1

	vpmMapLights AllLamps		'Auto lamps collection, lamp id in timerinterval
	vpmCreateEvents AllSwitches 'Auto Switches collection from the swNum in timerInterval

	'Init timers for updates
	pulsetimer.Enabled=1
	PinMAMETimer.Enabled=1		

	If Err Then MsgBox Err.Description

	initialized = 1
	LoadingText.Visible = false
	On Error Goto 0	
	
End Sub

'Game ready checker from flipper timer
Sub LeftFlipper_Timer
	LeftFlipper.TimerEnabled = 0
	if not Controller.GameRunning Then LeftFlipper.TimerEnabled = 1 : Exit Sub
	InitGame
End Sub

'****************************
' Keyboard / Machine
'****************************
Sub Table1_KeyDown(ByVal keycode)

	if Controller.GameRunning = 0 then Exit Sub 'exit because no display is available

	If keycode = PlungerKey Then
		Controller.Switch 28, 1
	End If

	If keycode = LeftFlipperKey and FlippersOn Then
		LeftFlipper.RotateToEnd : PlaySoundAt SFlipperOn, LeftFlipper
	End If

	If keycode = RightFlipperKey and FlippersOn Then
		RightFlipper.RotateToEnd : PlaySoundAt SFlipperOn, RightFlipper
	End If

	If vpmKeyDown(keycode) Then Exit Sub  ' This will handle machine switches and flippers etc

End Sub

Sub Table1_KeyUp(ByVal keycode)

	if Controller.GameRunning = 0 then Exit Sub 'exit because no display is available

	If keycode = PlungerKey Then
		Controller.Switch 28, 0
	End If

	If keycode = LeftFlipperKey and FlippersOn Then
		LeftFlipper.RotateToStart : PlaySoundAt SFlipperOff, LeftFlipper
	End If

	If keycode = RightFlipperKey and FlippersOn Then
		RightFlipper.RotateToStart : PlaySoundAt SFlipperOff, RightFlipper
	End If

	If vpmKeyUp(keycode) Then Exit Sub ' This will handle machine switches and flippers etc
End Sub

'****************************
' Solenoid Callbacks
'****************************
SolCallback(0) = "Died"
SolCallback(1) = "bsTrough.solOut" ' This exit is setup in the init
SolCallback(2) = "FlippersEnabled"
SolCallback(3) = "AutoPlunger"
SolCallback(4)  = "SolDivert"
SolCallback(5)  = "SolJaws"
SolCallback(6)  = "bsJawsHole.solOut"
SolCallback(7)  = "bsVUK.SolOut"
SolCallback(8)  = "SolKickback"
SolCallback(9)  = "SolBoat"
SolCallback(10)  = "SolOrcaMagnet"
SolCallback(11)  = "SolJawsTargetA"
SolCallback(12)  = "SolJawsTargetB"
SolCallback(13)  = "SolJawsTargetC"
SolCallback(14)  = "SolJawsTargetD"
SolCallback(15)  = "SolJawsTargetE"
SolCallback(16)  = "SolJawsTargetF"
SolCallback(17)  = "SolBarrelEject"
SolCallback(18)  = "SolFlashBarrelScoop"
SolCallback(19)  = "SolFlashBarrel"
SolCallback(20)  = "SolFlashJawsSign"
SolCallback(21)  = "SolFlashOrcaL"
SolCallback(22)  = "SolFlashOrcaM"
SolCallback(23)  = "SolFlashOrcaR"
SolCallback(24)  = "SolFlashLockTarget"
SolCallback(25)  = "SolFlashVUK"
SolCallback(26)  = "SolFlashWireRamp"
SolCallback(27)  = "SolFlashSlingL"
SolCallback(28)  = "SolFlashSlingR"

Sub Died(Enabled)
	'on error resume next	
	If not enabled then
		'MsgBox "Game window unavailable." : Err.Raise 5
	End if
End Sub

Sub SolBarrelEject(enabled)
     If Enabled Then
		bsBarrelStack.ExitSol_On
     End If
End Sub

SUb bsBarrel_Hit
	PlaySoundAt "Scoop", bsBarrel
    bsBarrelStack.AddBall Me
End Sub

'Do it like this as one on/off, will make it faster
Dim FlippersOn : FlippersOn = 0
Sub FlippersEnabled(Enabled)
	'Debug.Print "flippers on coil " & Enabled
	FlippersOn = Enabled
	If not FlippersOn then LeftFlipper.RotateToStart : RightFlipper.RotateToStart
End Sub

Sub AutoPlunger(enabled)
	If enabled Then 
		Plunger.Fire
		PlaySoundAt "solenoidVUK", Plunger
		DOF 113,2
	Else 
		Plunger.PullBack
	End If	
End Sub

Sub VUKMid_Hit()
	PlaySoundAtBall "kicker_enter_center"
	bsVUK.AddBall Me
End SUb

Sub SolDivert(enabled) : DiverterWall.IsDropped=enabled : End Sub

Sub SolKickBack(enabled)
	If enabled Then 
		KickBack.Fire
	Else
		KickBack.PullBack
	End If
End Sub

Sub SolJawsTargetA(enabled) : sw36.IsDropped = not enabled : End Sub
Sub SolJawsTargetB(enabled) : sw37.IsDropped = not enabled : End Sub
Sub SolJawsTargetC(enabled) : sw38.IsDropped = not enabled : End Sub
Sub SolJawsTargetD(enabled) : sw41.IsDropped = not enabled : End Sub
Sub SolJawsTargetE(enabled) : sw42.IsDropped = not enabled : End Sub
Sub SolJawsTargetF(enabled) : sw43.IsDropped = not enabled : End Sub

'##################################
' Jaws Toy
'##################################
Sub SolJaws(enabled)
	DOF 104, 2
	PlaySoundAt "motor-short", bottom_jaw
	If enabled Then
		JawsMouthWall.IsDropped=False
		JawsDown.enabled=False
		JawsUp.enabled = True
	Else
		JawsMouthWall.IsDropped=True
		JawsUp.enabled = False
		JawsDown.enabled = True		
	End If
End Sub

Sub JawsUp_Timer()
	If bottom_jaw.RotZ < 52 Then
		bottom_jaw.RotZ = bottom_jaw.RotZ +1
		bottom_jaw.TransY = bottom_jaw.TransY -1
		bottom_jaw.TransX = bottom_jaw.TransX +1
	Else
		JawsUp.enabled=False
	ENd If
End Sub

Sub JawsDown_Timer()
	If bottom_jaw.RotZ > 0 Then
		bottom_jaw.RotZ = bottom_jaw.RotZ -1
		bottom_jaw.TransX = bottom_jaw.TransX -1
		bottom_jaw.TransY = bottom_jaw.TransY +1		
	Else
		JawsDown.enabled=False
	End If
End Sub

Sub SolOrcaMagnet(enabled)
	If enabled Then
		mMagnet.MagnetOn = False
		OrcaWall.IsDropped=True		
	Else
		mMagnet.MagnetOn = True
		OrcaWall.IsDropped=False	
	End If
End Sub

'#####################################
' Orca Boat
'####################################
Sub SolBoat(enabled)
	If enabled Then
		BoatUp.enabled=True
	Else
		BoatDown.enabled=False
		BoatUp.enabled=False
		BoatReset.enabled=True
	End If
End Sub

Sub BoatUp_Timer()
	If Boat.ObjRotY <8 Then
		Boat.ObjRotY = Boat.ObjRotY + 1
		Boat.TransZ = Boat.TransZ - 3
	Else
		BoatDown.enabled = True
	End If
	
End Sub

Sub BoatDown_Timer()
	BoatUp.enabled = False
	If Boat.ObjRotY > 0 Then
		Boat.ObjRotY = Boat.ObjRotY - 1
		Boat.TransZ = Boat.TransZ + 3
	Else
		BoatDown.enabled = False
		BoatUp.enabled = True
	End If
End Sub

Sub BoatReset_Timer()
	If Boat.ObjRotY > 0 Then
	   Boat.ObjRotY = Boat.ObjRotY - 1
	   Boat.TransZ = Boat.TransZ + 3
		If Boat.ObjRotY = 0 Then
			BoatReset.enabled=False
		End If
	End If
End Sub

Sub JawsHole_Hit()
	bsJawsHole.AddBall Me
	PlaySoundAt "solenoidleft", JawsHole
End Sub

'#####################################
' END Solenoid
'####################################

'************************
'Bumpers
'************************
Sub Bumper1_Hit
	PlaySoundAt "bumper_1", Bumper1
	DOF 166,2
End Sub

Sub Bumper2_Hit
	PlaySoundAt "bumper_2", Bumper2	
	DOF 167,2
End Sub
Sub Bumper3_Hit
	PlaySoundAt "bumper_3", Bumper3
	DOF 168,2
End Sub

Sub Lampshow1(Enabled)
	if Enabled then : LightSeq001.StopPlay : LightSeq001.Play SeqUpOff, 10, 2 : Debug.print "lampshow1"	
End Sub

Sub Lampshow2(Enabled)		
	if Enabled then : LightSeq001.StopPlay : LightSeq001.Play SeqDownOff, 10, 2 : Debug.print "lampshow2"	
End Sub

Sub DisableLampShows(Enabled)		
	if Enabled then : LightSeq001.StopPlay : Debug.print "stopping lampshows"	
End Sub
'##############
'# Flasher Coils
'##############
'Sling Flash L
Sub SolFlashSlingL(enabled)	
	FlashSlingLampL.TimerEnabled = Enabled
	If not Enabled Then 
		vpmSolToggleObj Array(FlashSlingLampL),Nothing,"",0
		FlashSlingL.Visible=0
	End if
End Sub
Sub FlashSlingLampL_Timer		
	FlashSlingL.Visible = not FlashSlingL.Visible
	vpmSolToggleObj Array(FlashSlingLampL),Nothing,"", not FlashSlingL.Visible	
	FlashSlingLampL.TimerEnabled = 1
End Sub
'Sling Flash R
Sub SolFlashSlingR(enabled)	
	If Enabled Then 
		FlashSlingLampR.TimerEnabled = 1		
    else	
		FlashSlingLampR.TimerEnabled = 0		
		vpmSolToggleObj Array(FlashSlingLampR),Nothing,"",0
		FlashSlingR.Visible=0
	End if
End Sub
Sub FlashSlingLampR_Timer		
	FlashSlingR.Visible = not FlashSlingR.Visible
	vpmSolToggleObj Array(FlashSlingLampR),Nothing,"", not FlashSlingR.Visible	
	FlashSlingLampR.TimerEnabled = 1
End Sub
'Barrel Flash
Sub SolFlashBarrel(enabled)
	FlashBarrelLamp.TimerEnabled = Enabled
	If not Enabled Then 	
		vpmSolToggleObj Array(FlashBarrelLamp),Nothing,"",0
		FlashSlingR.Visible=0
	End if
End Sub
Sub FlashBarrelLamp_Timer		
	FlashBarrel.Visible = not FlashBarrel.Visible
	vpmSolToggleObj Array(FlashBarrelLamp),Nothing,"", not FlashBarrel.Visible	
	FlashBarrelLamp.TimerEnabled = 1
End Sub
'Barrel Scoop Flash
Sub SolFlashBarrelScoop(enabled)
	FlashBarrelScoopLamp.TimerEnabled = Enabled
	If not Enabled Then 	
		vpmSolToggleObj Array(FlashBarrelScoopLamp),Nothing,"",0
		FlashBarrelScoop.Visible=0
	End if
End Sub
Sub FlashBarrelScoopLamp_Timer		
	FlashBarrelScoop.Visible = not FlashBarrelScoop.Visible
	vpmSolToggleObj Array(FlashBarrelScoopLamp),Nothing,"", not FlashBarrelScoop.Visible	
	FlashBarrelScoopLamp.TimerEnabled = 1
End Sub
'Jaws Sign Flash
Sub SolFlashJawsSign(enabled)
	FlashJawsLamp.TimerEnabled = Enabled
	If not Enabled Then 	
		vpmSolToggleObj Array(FlashJawsLamp),Nothing,"",0
		FlashJawsSign.Visible=0
	End if
End Sub
Sub FlashJawsLamp_Timer		
	FlashJawsSign.Visible = not FlashJawsSign.Visible
	vpmSolToggleObj Array(FlashJawsLamp),Nothing,"", not FlashJawsSign.Visible	
	FlashJawsLamp.TimerEnabled = 1
End Sub
'Orca Boat Flash L
Sub SolFlashOrcaL(enabled)	
	FlashOrcaL.Visible=enabled
	FlashOrcaL.TimerEnabled = Enabled	
End Sub
Sub FlashOrcaL_Timer		
	FlashOrcaL.Visible = not FlashOrcaL.Visible
	FlashOrcaL.TimerEnabled = 1
End Sub
'Orca Boat Flash M
Sub SolFlashOrcaM(enabled)	
	FlashOrcaM.Visible=enabled
	FlashOrcaM.TimerEnabled = Enabled	
End Sub
Sub FlashOrcaM_Timer		
	FlashOrcaM.Visible = not FlashOrcaM.Visible
	FlashOrcaM.TimerEnabled = 1
End Sub
'Orca Boat Flash R
Sub SolFlashOrcaR(enabled)	
	FlashOrcaR.Visible=enabled
	FlashOrcaR.TimerEnabled = Enabled	
End Sub
Sub FlashOrcaR_Timer		
	FlashOrcaR.Visible = not FlashOrcaR.Visible
	FlashOrcaR.TimerEnabled = 1
End Sub
'flash vuk
Sub SolFlashVUK(enabled)
	FlashVUK.Visible=enabled
	FlashVUK.TimerEnabled = Enabled	
	if not Enabled then
		vpmSolToggleObj Array(FlashVUKLamp, FlashVUKLamp1),Nothing,"",Enabled
	end if	
End Sub
Sub FlashVUK_Timer		
	FlashVUK.Visible = not FlashVUK.Visible
	vpmSolToggleObj Array(FlashVUKLamp, FlashVUKLamp1),Nothing,"", FlashVUK.Visible
	FlashVUK.TimerEnabled = 1
End Sub
'flash shark
Sub SolFlashShark(enabled)
	FlashTopLeft1.Visible=enabled
	FlashTopLeft2.Visible=enabled
	FlashTopLeft1.TimerEnabled = enabled
End Sub
Sub FlashTopLeft1_Timer		
	FlashTopLeft1.Visible = not FlashTopLeft1.Visible
	FlashTopLeft2.Visible = FlashTopLeft1.Visible
	FlashTopLeft1.TimerEnabled = 1
End Sub
'Flash lock
Sub SolFlashLockTarget(enabled)
	FlashLockTarget.Visible=enabled
	FlashLockTarget.TimerEnabled = Enabled	
	if not Enabled then
		vpmSolToggleObj Array(FlashLockTargetLamp),Nothing,"",Enabled
	end if	
End Sub
Sub FlashLockTarget_Timer		
	FlashLockTarget.Visible = not FlashLockTarget.Visible
	vpmSolToggleObj Array(FlashLockTargetLamp),Nothing,"", FlashLockTarget.Visible
	FlashLockTarget.TimerEnabled = 1
End Sub
'Flash wire
Sub SolFlashWireRamp(enabled)
	FlashShooter1.Visible=enabled : FlashShooter2.Visible=enabled
	FlashShooter1.TimerEnabled = Enabled
	if not Enabled then
		vpmSolToggleObj Array(FlashShooterLamp2),Nothing,"",Enabled
	end if	
End Sub
Sub FlashShooter1_Timer		
	FlashShooter1.Visible=not FlashShooter1.Visible : FlashShooter2.Visible=not FlashShooter1.Visible
	vpmSolToggleObj Array(FlashShooterLamp2),Nothing,"",FlashShooter1.Visible
	FlashShooter1.TimerEnabled = 1
End Sub

'**********Sling Shot Animations
' Rstep and Lstep  are the variables that increment the animation
'****************
Dim RStep, Lstep

Sub RightSlingShot_Slingshot
	vpmTimer.PulseSw 26 ' pulse switch same Controller.Switch 26, 1/0
    PlaySound SoundFX("sling3",DOFContactors), 0,1, 0.05,0.05 '0,1, AudioPan(RightSlingShot), 0.05,0,0,1,AudioFade(RightSlingShot)
    RSling.Visible = 0
    RSling1.Visible = 1
    sling1.rotx = 20
    RStep = 0
    RightSlingShot.TimerEnabled = 1
	gi1.State = 0:Gi2.State = 0
End Sub

Sub RightSlingShot_Timer
    Select Case RStep
        Case 3:RSLing1.Visible = 0:RSLing2.Visible = 1:sling1.rotx = 10
        Case 4:RSLing2.Visible = 0:RSLing.Visible = 1:sling1.rotx = 0:RightSlingShot.TimerEnabled = 0:gi1.State = 1:Gi2.State = 1
    End Select
    RStep = RStep + 1
End Sub

Sub LeftSlingShot_Slingshot
	vpmTimer.PulseSw 25
    PlaySound SoundFX("sling3",DOFContactors), 0,1, -0.05,0.05 '0,1, AudioPan(LeftSlingShot), 0.05,0,0,1,AudioFade(LeftSlingShot)
    LSling.Visible = 0
    LSling1.Visible = 1
    sling2.rotx = 20
    LStep = 0
    LeftSlingShot.TimerEnabled = 1
	gi3.State = 0:Gi4.State = 0
End Sub

Sub LeftSlingShot_Timer
    Select Case LStep
        Case 3:LSLing1.Visible = 0:LSLing2.Visible = 1:sling2.rotx = 10
        Case 4:LSLing2.Visible = 0:LSLing.Visible = 1:sling2.rotx = 0:LeftSlingShot.TimerEnabled = 0:gi3.State = 1:Gi4.State = 1
    End Select
    LStep = LStep + 1
End Sub

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

'Jaws table sounds
Sub WireRamp_Hit(idx)
 	PlaySoundAtBall "WireRamp_1_sec_b"
End Sub

Sub PlasticRamp_Hit(idx)
 	PlaySoundAtBall "PlasticRampStretched"
End Sub

Sub RampEnter_Hit()
 	PlaySoundAtBall "WireUp"
End Sub

Sub RampDrop_Hit(idx)
 	PlaySoundAtBall "BallDrop2"
End Sub

Sub SharkRampDrop_Hit()
 	PlaySoundAtBall "BallDrop3"
End Sub

Sub Bumpers_Hit (idx)
	Select Case Int(Rnd*3)+1
		Case 1 : PlaySoundAt "fx_bumperacreat1", Bumper1
		Case 2 : PlaySoundAt "fx_bumperacreat1", Bumper2
		Case 3 : PlaySoundAt "fx_bumperacreat1", Bumper3
	End Select
End Sub


'**************************************
' Explanation of the collision routine
'**************************************

' The collision is built in VP.
' You only need to add a Sub OnBallBallCollision(ball1, ball2, velocity) and when two balls collide they 
' will call this routine. What you add in the sub is up to you. As an example is a simple Playsound with volume and paning
' depending of the speed of the collision.

Sub Pins_Hit (idx)
	PlaySound "pinhit_low", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 0, 0, AudioFade(ActiveBall)
End Sub

Sub DropTargets_Hit (idx)
	PlaySound "DropTargetDropped", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 0, 0, AudioFade(ActiveBall)
End Sub

Sub Triggers_Hit (idx)
	PlaySound "sensor", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 0, 0, AudioFade(ActiveBall)
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
		PlaySound "fx_rubber2", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0
	End if
	If finalspeed >= 1 AND finalspeed <= 20 then
 		RandomSoundRubber()
 	End If
End Sub

Sub Posts_Hit(idx)
 	dim finalspeed
  	finalspeed=SQR(activeball.velx * activeball.velx + activeball.vely * activeball.vely)
 	If finalspeed > 20 then 
		PlaySound "fx_rubber2", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0, AudioFade(ActiveBall)
	End if
	If finalspeed >= 6 AND finalspeed <= 20 then
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
