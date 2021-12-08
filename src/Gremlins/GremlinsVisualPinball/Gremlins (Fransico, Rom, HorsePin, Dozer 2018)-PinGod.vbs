On Error Resume Next
	ExecuteGlobal GetTextFile("controller.vbs") ' Std Controller for DOF etc.
	If Err Then MsgBox "You need the Controller.vbs file in order to run this table (installed with the VPX package in the scripts folder)"
On Error Goto 0
Randomize

'Ball Stacks
Dim bsscoopRight, bsscoopMid, kickbackIM, plungerIM, dscDeagle
'Const BallSize = 49  'Ball radius

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
'Const GameDirectory = ".\PinGod.BasicGame.exe" 'exported game

'Debug
Const IsDebug = True
Const GameDirectory = "..\Gremlins" ' Loads the godot pingod game project
Const UseSolenoids = 1 ' Check for solenoid states?
Const UsePdbLeds = 0  ' use led (color)
Const UseLamps = 1  ' Check for lamp states?
Dim bsTrough, bsSaucer, swSaucer : swSaucer = 27

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
		.DisplayX			= 1920
		.DisplayY			= 10
		.DisplayWidth 		= 1920 ' 1920 FS
		.DisplayHeight 		= 1080 ' 1080  FS
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
	bsTrough.InitEntrySounds "Drain", "", ""
	bsTrough.InitExitSounds "BallRelease", ""
	bsTrough.Reset		

	Set bsscoopRight=New cvpmBallStack : With bsscoopRight
	.InitSw 0, 40, 0, 0, 0, 0, 0, 0
	.InitKick sw45, 165, 10
	.KickZ = 0.3
	.KickForceVar = 1.5
	.InitExitSnd "Solenoid", "Solenoid"
	.CreateEvents "bsscoopRight", sw45
	End With

	Set bsscoopMid=New cvpmBallStack : With bsscoopMid
	.InitSw 0, 41, 0, 0, 0, 0, 0, 0
	.InitKick sw46, 160, 20
	.KickZ = 0.3
	.KickForceVar = 1.5
	.InitExitSnd "Solenoid", "Solenoid"
	.CreateEvents "bsscoopMid", sw46
	End With

	Set dscDeagle = New cvpmTurnTable
		dscDeagle.InitTurnTable DeagleTrigger, 20
		dscDeagle.SpinCW = True
		dscDeagle.maxSpeed = 40
			dscDeagle.spinUp = 20
		dscDeagle.spinDown = 5
		dscDeagle.CreateEvents "dscDeagle"

  ' Auto Plunger
    Const IMPowerSetting = 50 ' Plunger Power
    Const IMTime = 0.6        ' Time in seconds for Full Plunge
    Set plungerIM = New cvpmImpulseP
    With plungerIM
        .InitImpulseP swplunger, IMPowerSetting, IMTime
        .Random 0.3
        .InitExitSnd "plunger2", "plunger"
        .CreateEvents "plungerIM"
    End With

  ' Kickback
    Const kickbackPowerSetting = 38 ' Plunger Power
    Const kickbackIMTime = 0       ' Time in seconds for Full Plunge
    Set kickbackIM = New cvpmImpulseP
    With kickbackIM
        .InitImpulseP swkickback, kickbackPowerSetting, kickbackIMTime
        .Random 0.3
        .InitExitSnd "plunger2", "plunger"
        .CreateEvents "kickbackIM"
    End With

	SolspiderStop 0
	SolrampStop 0


	If Err Then MsgBox Err.Description
	initialized = 1
	LoadingText.Visible = false ' Hide the overlay (loading screen)
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

'****************************
' Solenoids / Coils / Callbacks
'****************************
SolCallback(0) = "Died"
SolCallback(1) = "bsTrough.SolOut"
SolCallback(2) = "FlippersEnabled"
SolCallback(3) = "AutoPlunger"
SolCallback(4) = "bsscoopRight.SolOut"
SolCallback(5) = "bsscoopMid.SolOut"
SolCallback(6) = "SolresetDrop"
SolCallback(7) = "SolrampStop"
SolCallback(8) = "SolspiderStop"
SolCallback(9) = "SolgizmoToy"
SolCallback(10) = "SolKickBack"
SolCallback(11) = "SolDiscMotor"
SolCallback(12) = "SolflasherLeft"
SolCallback(13) = "SolflasherRight"
SolCallback(14) = "SolflasherLeftLwr"
'SolCallback(15) = "SolflasherLeftMid"
SolCallback(15) = "SolflasherLeftTop"
SolCallback(16) = "SolflasherRightTop"
SolCallback(17) = "SolFlasherRightMid"
SolCallback(18) = "SolflasherSpiderLeft"
SolCallback(19) = "SolflasherSpiderRight"

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
Sub AutoPlunger(Enabled)
  If Enabled Then
    PlungerIM.AutoFire
  End If
End Sub

' ** VP SWITCH EVENTS **  
' startButton hit 
'Sub sw13_Hit():Controller.Switch(13) = 1 :End Sub
'Sub sw13_UnHit():Controller.Switch(13) = 0 :End Sub
'' outlaneL hit 
'Sub sw21_Hit():Controller.Switch(21) = 1 :End Sub
'Sub sw21_UnHit():Controller.Switch(21) = 0 :End Sub
'' outlaneR hit 
'Sub sw22_Hit():Controller.Switch(22) = 1 :End Sub
'Sub sw22_UnHit():Controller.Switch(22) = 0 :End Sub
'' inlaneL hit 
'Sub sw23_Hit():Controller.Switch(23) = 1 :End Sub
'Sub sw23_UnHit():Controller.Switch(23) = 0 :End Sub
'' inlaneR hit 
'Sub sw24_Hit():Controller.Switch(24) = 1 :End Sub
'Sub sw24_UnHit():Controller.Switch(24) = 0 :End Sub
'
'' bumper1 hit 
'Sub sw25_Hit():vpmTimer.PulseSw 25:PlayBumperSounds 1:End Sub
'' bumper2 hit 
'Sub sw26_Hit():vpmTimer.PulseSw 26:PlayBumperSounds 2:End Sub
'' bumper3 hit 
'Sub sw27_Hit():vpmTimer.PulseSw 27:PlayBumperSounds 3:End Sub
'
'' leftRamp hit 
'Sub sw31_Hit():Controller.Switch(31) = 1 :End Sub
'Sub sw31_UnHit():Controller.Switch(31) = 0 :End Sub
'' deagle1 hit 
'Sub sw32_Hit():vpmTimer.PulseSw 32:End Sub
'' deagle2 hit 
'Sub sw33_Hit():vpmTimer.PulseSw 33:End Sub
'' deagle3 hit 
'Sub sw34_Hit():vpmTimer.PulseSw 34:End Sub
'' deagle4 hit 
'Sub sw35_Hit():vpmTimer.PulseSw 35:End Sub
'' deagle5 hit 
'Sub sw36_Hit():vpmTimer.PulseSw 36:End Sub
'' rightRamp hit 
'Sub sw37_Hit():Controller.Switch(37) = 1 :End Sub
'Sub sw37_UnHit():Controller.Switch(37) = 0 :End Sub
'' shooter_lane hit 
'Sub sw41_Hit():Controller.Switch(41) = 1 :End Sub
'Sub sw41_UnHit():Controller.Switch(41) = 0 :End Sub
'' orbitL hit 
'Sub sw42_Hit():Controller.Switch(42) = 1 :sldir = 1:Spider_Left.enabled = 1:Spider_Right.enabled = 0:End Sub
'Sub sw42_UnHit():Controller.Switch(42) = 0 :End Sub
'' gizmoLane hit 
'Sub sw43_Hit():Controller.Switch(43) = 1 :sldir = 1:Spider_Right.enabled = 1:Spider_Left.enabled = 0:End Sub
'Sub sw43_UnHit():Controller.Switch(43) = 0 :End Sub
'' lockLane hit 
'Sub sw44_Hit():Controller.Switch(44) = 1 :End Sub
'Sub sw44_UnHit():Controller.Switch(44) = 0 :End Sub
'' scoopRight hit 
'Sub sw45_Hit():Controller.Switch(45) = 1 :End Sub
'Sub sw45_UnHit():Controller.Switch(45) = 0 :End Sub
'' scoopMid hit 
'Sub sw46_Hit():Controller.Switch(46) = 1 :End Sub
'Sub sw46_UnHit():Controller.Switch(46) = 0 :End Sub
'' slingL hit 
'Sub sw51_Hit():vpmTimer.PulseSw 51:End Sub
'' slingR hit 
'Sub sw52_Hit():vpmTimer.PulseSw 52:End Sub
'' drop1 hit 
'Sub sw53_Hit():vpmTimer.PulseSw 53:End Sub
'' drop2 hit 
'Sub sw54_Hit():vpmTimer.PulseSw 54:End Sub
'' drop3 hit 
'Sub sw55_Hit():vpmTimer.PulseSw 55:End Sub
'' spiderTarget hit 
'Sub sw56_Hit():vpmTimer.PulseSw 56:End Sub

'Switch under duffy to trigger shake  (Will also shake with nudges).
Sub duffy_the_nuffy_hit:DuffyShake:End Sub

Sub SolresetDrop(Enabled)
	If Enabled Then
		sw53.IsDropped = 0
		sw54.IsDropped = 0
		sw55.IsDropped = 0		
	End If
End Sub

Sub SolrampStop(Enabled)
If Enabled Then
	StopPost1.IsDropped = False
	StopPost2.IsDropped = False
Else
	StopPost1.IsDropped = True
	StopPost2.IsDropped = True
End If
End Sub

Sub SolspiderStop(Enabled)
If Enabled Then
	StopPost3.IsDropped = False
	StopPost4.IsDropped = False
Else
	StopPost3.IsDropped = True
	StopPost4.IsDropped = True
End If
End Sub

Sub SolgizmoToy(Enabled)
	If Enabled Then
		gdir = 1:Gizmo.enabled = 1
	Else
		gdir = 2:Gizmo.enabled = 1
	End If
End Sub

Sub Solautoplunge(Enabled)
  If Enabled Then
    PlungerIM.AutoFire
  End If
End Sub

Sub SolKickBack(Enabled)
  If Enabled Then
    kickbackIM.AutoFire
  End If
End Sub

'MODEL ANIMATIONS - DOZER

'GIZMO
Dim gdir
gdir = 1

Sub Gizmo_Timer()
Select Case gdir
	Case 1:
		If GizmoBox1.Z => 150 Then
			me.enabled = 0
		End If

		GizmoBox1.Z = GizmoBox1.Z + 3
	Case 2:
		If GizmoBox1.Z <=120 Then
			me.enabled = 0
			GizmoBox1.Z = 120
		End If

		GizmoBox1.Z = GizmoBox1.Z - 3
		End Select
End Sub

'SPIDER LEFT
Dim sldir
sldir = 1

Sub Spider_Left_Timer()
Select Case sldir
Case 1:
If Spider.ObjRotZ => 50 Then
sldir = 2
End If
Spider.ObjRotZ = Spider.ObjRotz + 1
Case 2:
If Spider.ObjRotZ <=0 Then
me.enabled = 0
Spider.ObjRotZ = 0
End If
Spider.ObjRotZ = Spider.ObjRotz - 1
End Select
End Sub

'SPIDER RIGHT
Dim srdir
srdir = 1

Sub Spider_Right_Timer()
Select Case srdir
Case 1:
If Spider.ObjRotZ <= -50 Then
srdir = 2
End If
Spider.ObjRotZ = Spider.ObjRotz - 1
Case 2:
If Spider.ObjRotZ => 0 Then
me.enabled = 0
Spider.ObjRotz = 0
End If
Spider.ObjRotZ = Spider.ObjRotz + 1
End Select
End Sub

'DUFFY THE NUFFY INNIT
Dim cBall

Sub Duffy_Init
Set cBall = ckicker.createball
ckicker.Kick 0, 0
End Sub

Sub DuffyShake
cball.velx = 1 
cball.vely = -2
End Sub

Sub DuffyShaker_Timer
Dim a, b, c
a = 90-(ckicker.y - cball.y)
b = (ckicker.y - cball.y) / 2
c = cball.x - ckicker.x
'Duffy.rotx = a
'Duffy.transz = b
Duffy.rotz = c - 40
End Sub


'####################
' FLASHERS
'####################
Sub SolflasherLeft(Enabled)
  vpmSolToggleObj left_flash12, Nothing, "", Enabled
  vpmSolToggleObj FlashSlingL, Nothing, "", Enabled
End Sub

Sub SolflasherRight(Enabled)
  vpmSolToggleObj left_flash7, Nothing, "", Enabled
  vpmSolToggleObj FlashSlingR, Nothing, "", Enabled
End Sub

Sub SolflasherLeftLwr(Enabled)
  vpmSolToggleObj left_flash3, Nothing, "", Enabled
  vpmSolToggleObj FlashLwrL, Nothing, "", Enabled
End Sub

Sub SolflasherLeftMid(Enabled)
  vpmSolToggleObj left_flash4, Nothing, "", Enabled
  vpmSolToggleObj FlashLMid, Nothing, "", Enabled	
End Sub

Sub SolFlasherRightMid(enabled)
  'The right Sideflash
	vpmSolToggleObj left_flash13, Nothing, "", Enabled
	vpmSolToggleObj FlashRMid, Nothing, "", Enabled		
End Sub

Sub SolflasherLeftTop(Enabled)
	vpmSolToggleObj left_flash1, Nothing, "", Enabled
	vpmSolToggleObj left_flash28, Nothing, "", Enabled
	vpmSolToggleObj FlashLTop, Nothing, "", Enabled
End Sub

Sub SolflasherRightTop(Enabled)
	vpmSolToggleObj left_flash25, Nothing, "", Enabled
    vpmSolToggleObj FlashRTop, Nothing, "", Enabled
End Sub

Sub SolflasherSpiderLeft(Enabled)
	vpmSolToggleObj left_flash2, Nothing, "", Enabled
	vpmSolToggleObj left_flash19, Nothing, "", Enabled
    vpmSolToggleObj FlashSpidL, Nothing, "", Enabled
End Sub

Sub SolflasherSpiderRight(Enabled)
	vpmSolToggleObj left_flash23, Nothing, "", Enabled
	vpmSolToggleObj FlashSpidR, Nothing, "", Enabled
End Sub


Sub SolDiscMotor(Enabled)
	dscDeagle.MotorOn = Enabled
End Sub

' ===============================================================================================
' spinning discs by Herweh
' ===============================================================================================

Dim discAngle, stepAngle, stopDiscs, discsAreRunning

InitDiscs()

Sub InitDiscs()
	discAngle 			= 0
	discsAreRunning		= False
End Sub

Sub SolDiscMotor(Enabled)
	dscDeagle.MotorOn = Enabled

	If Enabled Then
		stepAngle			= 20.0
		discsAreRunning		= True
		stopDiscs			= False
		DiscsTimer.Interval = 10
		DiscsTimer.Enabled 	= True
	Else
		stopDiscs			= True
		discsAreRunning		= True
	End If
End Sub

Sub DiscsTimer_Timer()
	' calc angle
	discAngle = discAngle + stepAngle
	If discAngle >= 360 Then
		discAngle = discAngle - 360
	End If
	' rotate discs
	DeagleDisc.RotAndTra2 = 360 - discAngle
	If stopDiscs Then
		stepAngle = stepAngle - 0.1
		If stepAngle <= 0 Then
			DiscsTimer.Enabled 	= False
		End If
	End If
End Sub


'######################################################################
' Lamps & GI:
' Add all lamps to a VP Collection called AllLamps. (Events are set on this collection updating the states)
' Each Lamp must have it's lamp number entered in the TimerInterval box
' Create at least one GI collection called GI_low. Add all GI to this collection. 
' From skeleton game the lamp gi01 - gi05 will control these
'######################################################################

Dim GIon
Set LampCallback    = GetRef("UpdateMultipleLamps")
Sub UpdateMultipleLamps
	'Mode lamps
	L61a.Visible = L61.State
	L62a.Visible = L63.State
	L63a.Visible = L62.State
	L64a.Visible = L64.State
	L65a.Visible = L65.State
	L66a.Visible = L66.State
	L67a.Visible = L67.State
End Sub

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
		ColGradeGI.enabled=True:ColGradeGIOff.enabled=False:i=5
		If b2sOn Then B2SController.B2ssetdata 100,1
    Else
		ColGradeGI.enabled=False:ColGradeGIOff.enabled=True:i=1  
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

'Color grade GI
Dim i 
Sub ColGradeGI_Timer
    if i=1 Then 
      ColGradeGI.Enabled = False
    Else
      Table1.ColorGradeImage = "Grem_" & i
      i = i-1
    End If
End Sub

Sub ColGradeGIOff_Timer
    if i=5 Then 
      ColGradeGI.Enabled = False
    Else
      Table1.ColorGradeImage = "Grem_" & i
      i = i+1
    End If      
End Sub


'#######################################################################
' VPROC Controller
' Loads settings through the controller from the game which are set inside the service menu to change VP variables  
'#######################################################################

'Sounds
Dim snds_LSling,snds_RSling,snds_flips, flipperVol, snds_bumpers
Dim snds_AutoPlunge,snds_Drain,snds_BallRelease, snds_Scoop, snds_VUK
Dim snds_Saucer, snds_Kicker, snds_DropTargets,snds_DropTargetReset, snds_Targets

'Table Pyhsics Enabled - Table physics are directly loaded into the tables variable
Dim tbl_enabled

'#######################################################################
' FUNCTIONS THAT COME WITH A DEFAULT VP SCRIPT
'#######################################################################

'**********Sling Shot Animations
' Rstep and Lstep  are the variables that increment the animation
'****************
Dim RStep, Lstep, LstepTop, RstepTop

Sub RightSlingShot_Slingshot    
    RSling.Visible = 0
    RSling1.Visible = 1
    sling1.TransZ = -20
    RStep = 0
    RightSlingShot.TimerEnabled = 1	
	if snds_RSling Then PlaySound "right_slingshot":End If	
	vpmTimer.PulseSw 52	

End Sub

Sub RightSlingShot_Timer
    Select Case RStep
        Case 3:RSLing1.Visible = 0:RSLing2.Visible = 1:sling1.TransZ = -10
        Case 4:RSLing2.Visible = 0:RSLing.Visible = 1:sling1.TransZ = 0:RightSlingShot.TimerEnabled = 0
    End Select
    RStep = RStep + 1
End Sub

Sub LeftSlingShot_Slingshot    
    LSling.Visible = 0
    LSling1.Visible = 1
    sling2.TransZ = -20
    LStep = 0
    LeftSlingShot.TimerEnabled = 1	
	if snds_LSling Then PlaySound "left_slingshot":End If
	vpmTimer.PulseSw 51
	
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


'************************************
' What you need to add to your table
'************************************

' a timer called RollingTimer. With a fast interval, like 10
' one collision sound, in this script is called fx_collide
' as many sound files as max number of balls, with names ending with 0, 1, 2, 3, etc
' for ex. as used in this script: fx_ballrolling0, fx_ballrolling1, fx_ballrolling2, fx_ballrolling3, etc


'******************************************
' Explanation of the rolling sound routine
'******************************************

' sounds are played based on the ball speed and position

' the routine checks first for deleted balls and stops the rolling sound.

' The For loop goes through all the balls on the table and checks for the ball speed and 
' if the ball is on the table (height lower than 30) then then it plays the sound
' otherwise the sound is stopped, like when the ball has stopped or is on a ramp or flying.

' The sound is played using the VOL, AUDIOPAN, AUDIOFADE and PITCH functions, so the volume and pitch of the sound
' will change according to the ball speed, and the AUDIOPAN & AUDIOFADE functions will change the stereo position
' according to the position of the ball on the table.


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

Sub RampWire_Hit (idx)
	PlaySound "Rwireramp", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 0, 0
End Sub

Sub BallToWood_Hit (idx)
	PlaySound "Rballfall", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 0, 0
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

Sub PlayBumperSounds(num)
	if snds_bumpers Then
		PlaySound "fx_bumper" & num
	End If
End Sub

Sub rampwire_trigger_Hit()
	PlaySound "WireRamp1", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 0, 0
End Sub