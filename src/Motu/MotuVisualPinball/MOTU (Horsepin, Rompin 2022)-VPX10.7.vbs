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
'Const GameDirectory = ".\PinGod.Motu.exe" 'exported game

'Debug
Const IsDebug = True
Const GameDirectory = "..\MotuGodot" ' Loads the godot pingod game project
Const UseSolenoids = 1 ' Check for solenoid states?
Const UsePdbLeds = 0  ' use led (color)
Const UseLamps = 1  ' Check for lamp states?
Dim bsTrough, bsSaucer, plungerIM,  bsRI, swSaucer : swSaucer = 27
Dim turntable,ttDisc1 ' Skeletor Roton'
Set ttDisc1 = New myTurnTable
	ttDisc1.InitTurnTable Disc1Trigger, 8
	ttDisc1.SpinCW = False
	ttDisc1.CreateEvents "ttDisc1"

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
	bsTrough.Size = 8
	bsTrough.Balls = 8
	bsTrough.InitSwitches Array(81,82,83,84,85,86,87,88) ' trough switches
	bsTrough.InitExit BallRelease, 90, 8	
	bsTrough.CreateEvents "bsTrough",  Drain
	bsTrough.InitEntrySounds "Solenoid", "Solenoid", ""
	bsTrough.InitExitSounds "BallRelease", "Solenoid"
	bsTrough.Reset		

  ' Auto Plunger
    Const IMPowerSetting = 36 ' Plunger Power
    Const IMTime = 0.6        ' Time in seconds for Full Plunge
    Set plungerIM = New cvpmImpulseP
    With plungerIM
        .InitImpulseP swplunger, IMPowerSetting, IMTime
        .Random 0.3
        .InitExitSnd "solenoidVUK", "plunger"
        .CreateEvents "plungerIM"
    End With

  ' He Man Scoop
     Set bsRI = New cvpmBallStack
     With bsRI
         .InitSw 0, 36, 0, 0, 0, 0, 0, 0
         .InitKick sw11, 200, 20
         .KickZ = 0.4         
         .KickBalls = 8 ' Kick all balls that go into scoop 
		 .InitEntrySnd "scoop", "Solenoid"
		 .InitExitSnd "popper_ball", "Solenoid"		 
     End With

	' Skillshot uppost
	UpPost.Isdropped=true

'    ' ### Nudging ###
    vpmNudge.TiltSwitch = swTilt
    vpmNudge.Sensitivity = 0.8
	vpmNudge.TiltObj = Array(LSling,RSling)


	If Err Then MsgBox Err.Description
	initialized = 1
	LoadingText.Visible = false ' Hide the overlay (loading screen)
	On Error Goto 0	

	' Set Skele Taggets, bank up
	TBDown=0:Controller.Switch 54, 1:Controller.Switch 55, 0
	TBMove 1
	
End Sub

'****************************
' Keyboard / Machine
'****************************
Sub Table1_KeyDown(ByVal keycode)

	if Controller.GameRunning = 0 then Exit Sub 'exit because no display is available

	If keycode = PlungerKey Then
		Plunger.PullBack
		PlaySoundAt "plungerpull", Plunger
	End If

	If keycode = LeftFlipperKey and FlippersOn Then
		LeftFlipper.RotateToEnd:LeftFlipper1.RotateToEnd
		PlaySoundAt "FlipUp", RightFlipper
	End If

	If keycode = RightFlipperKey and FlippersOn Then
		RightFlipper.RotateToEnd
		PlaySoundAt "FlipUp", RightFlipper
	End If

	If vpmKeyDown(keycode) Then Exit Sub  ' This will handle machine switches and flippers etc

End Sub

Sub Table1_KeyUp(ByVal keycode)

	if Controller.GameRunning = 0 then Exit Sub 'exit because no display is available

	If keycode = PlungerKey Then
		Plunger.Fire
		PlaySoundAt "plunger", Plunger
	End If

	If keycode = LeftFlipperKey and FlippersOn Then
		LeftFlipper.RotateToStart : LeftFlipper1.RotateToStart
		PlaySoundAt "FlipDown", LeftFlipper
	End If

	If keycode = RightFlipperKey and FlippersOn Then
		RightFlipper.RotateToStart
		PlaySoundAt "FlipDown", RightFlipper
	End If

	If vpmKeyUp(keycode) Then Exit Sub ' This will handle machine switches and flippers etc
End Sub

Sub LeftFlipper_Collide(parm)
	PlaySoundAtBall "fx_rubber_flipper"
End Sub

Sub RightFlipper_Collide(parm)
	PlaySoundAtBall "fx_rubber_flipper"
End Sub


'****************************

'****************************
' Solenoids / Coils / Callbacks
'****************************
SolCallback(0) = "Died"
SolCallback(1) = "bsTrough.solOut" ' This exit is setup in the init
SolCallback(2) = "FlippersEnabled"
SolCallback(3) = "AutoPlunger"
'SolCallback(4) = "bsSaucer.solOut" ' This exit is setup in the init
SolCallback(5) = "TBMove"
SolCallback(6) = "HeManScoop"
SolCallback(7) = "SolRoton"
SolCallback(8) = "SolPost"
SolCallback(9) = "SolHeManToy"
SolCallback(10) = "SolMutant"
SolCallback(11) = "SolRipper"
' FLASHERS ' TODO: all over the shop here
SolCallback(12) = "SolFlashLeft"
SolCallback(13) = "SolFlashRight"
'SolCallback(17) = "SolFlashTopLeft"
SolCallback(15) = "SolFlashTopRight"
SolCallback(16) = "SolFlashSkele"
SolCallback(17) = "SolFlashLeftTop"
SpecialSol = 20
SolCallback(20) = "SpecialSolCallback"

Sub Died(Enabled)
	'on error resume next	
	If not enabled then
		MsgBox "Game window unavailable." : Err.Raise 5
	End if
End Sub

Sub SpecialSolCallback(num)

	Select Case num
		Case 0 : DisableAllLampSequencers
		Case 1 : BlinkGI
		Case 3 : FlashersLeftRight
		Case 4 : BlinkGI : FlashersLeftRight
		Case 5 : ScrewFlashers : BlinkGI
		Case 6 : ScoopShow
		Case 7 : DisableAllLampSequencers
		Case 8 : PowerCombo
		Case 10: MotuMballIntro
		Case 11: BlinkGI : FlashersLeftRight
		Case 12: Attract
		Case 13: Attract
	End Select

End Sub

Sub DisableAllLampSequencers
	LightSeq001.StopPlay : LightSeq002.StopPlay
	LightSeq003.StopPlay : LightSeq004.StopPlay
End Sub

Sub Attract
	LightSeq001.StopPlay
	LightSeq001.UpdateInterval = 100
	LightSeq001.Play SeqHatch1HorizOn,0,1,0' total ms: 5000
	LightSeq001.UpdateInterval = 25
	LightSeq001.Play SeqHatch1HorizOff,0,1,0' total ms: 5000
End Sub

Sub BlinkGI
	LightSeq004.StopPlay
	LightSeq004.UpdateInterval = 1
	LightSeq004.Play SeqRightOn,0,1,0' total ms: 50
	LightSeq004.Play SeqHatch1VertOff,0,1,0' total ms: 50
	LightSeq004.Play SeqRightOn,0,1,0' total ms: 50
	LightSeq004.Play SeqHatch1VertOff,0,1,0' total ms: 50
	LightSeq004.Play SeqRightOn,0,1,0' total ms: 50
	LightSeq004.Play SeqHatch1VertOff,0,1,0' total ms: 50
	LightSeq004.Play SeqRightOn,0,1,0' total ms: 50
	LightSeq004.Play SeqHatch1VertOff,0,1,0' total ms: 50
	LightSeq004.Play SeqRightOn,0,1,0' total ms: 50
	LightSeq004.Play SeqHatch1VertOff,0,1,0' total ms: 50
	LightSeq004.Play SeqRightOn,0,1,0' total ms: 50
	LightSeq004.Play SeqHatch1VertOff,0,1,0' total ms: 50
	LightSeq004.Play SeqRightOn,0,1,0' total ms: 50
	LightSeq004.Play SeqHatch1VertOff,0,1,0' total ms: 50
	LightSeq004.Play SeqRightOn,0,1,0' total ms: 50
	LightSeq004.Play SeqHatch1VertOff,0,1,0' total ms: 50
	LightSeq004.Play SeqRightOn,0,1,0' total ms: 50
	LightSeq004.Play SeqHatch1VertOff,0,1,0' total ms: 50
	LightSeq004.Play SeqRightOn,0,1,0' total ms: 50
	LightSeq004.Play SeqHatch1VertOff,0,1,0' total ms: 50
End Sub

Sub FlashersLeftRight
	LightSeq003.StopPlay : LightSeq002.StopPlay
	LightSeq003.UpdateInterval = 1 : LightSeq002.UpdateInterval = 1
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
	LightSeq003.Play SeqLeftOn,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqLeftOff,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
	LightSeq003.Play SeqLeftOn,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqLeftOff,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
	LightSeq003.Play SeqLeftOn,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqLeftOff,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
	LightSeq003.Play SeqLeftOn,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqLeftOff,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
End Sub

Sub ScrewFlashers
	LightSeq003.StopPlay : LightSeq002.StopPlay
	LightSeq003.UpdateInterval = 1 : LightSeq002.UpdateInterval = 1
	LightSeq003.Play SeqScrewRightOn,0,1,0 : LightSeq002.Play SeqScrewRightOn,0,1,0
	LightSeq003.Play SeqScrewLeftOff,0,1,0 : LightSeq002.Play SeqScrewLeftOff,0,1,0
	LightSeq003.Play SeqScrewRightOn,0,1,0 : LightSeq002.Play SeqScrewRightOn,0,1,0
	LightSeq003.Play SeqScrewLeftOff,0,1,0 : LightSeq002.Play SeqScrewLeftOff,0,1,0
	LightSeq003.Play SeqScrewRightOn,0,1,0 : LightSeq002.Play SeqScrewRightOn,0,1,0
	LightSeq003.Play SeqScrewLeftOff,0,1,0 : LightSeq002.Play SeqScrewLeftOff,0,1,0
	LightSeq003.Play SeqScrewRightOn,0,1,0 : LightSeq002.Play SeqScrewRightOn,0,1,0
	LightSeq003.Play SeqScrewLeftOff,0,1,0 : LightSeq002.Play SeqScrewLeftOff,0,1,0
	LightSeq003.Play SeqScrewRightOn,0,1,0 : LightSeq002.Play SeqScrewRightOn,0,1,0
	LightSeq003.Play SeqScrewLeftOff,0,1,0 : LightSeq002.Play SeqScrewLeftOff,0,1,0
	LightSeq003.Play SeqScrewRightOn,0,1,0 : LightSeq002.Play SeqScrewRightOn,0,1,0
	LightSeq003.Play SeqScrewLeftOff,0,1,0 : LightSeq002.Play SeqScrewLeftOff,0,1,0
	LightSeq003.Play SeqScrewRightOn,0,1,0 : LightSeq002.Play SeqScrewRightOn,0,1,0
End Sub

Sub ScoopShow
	LightSeq001.UpdateInterval = 5 : LightSeq004.UpdateInterval = 5
	LightSeq001.Play SeqWiperRightOn,0,1,0 : LightSeq004.Play SeqWiperRightOff,0,1,0
	LightSeq001.Play SeqWiperRightOff,0,1,0: LightSeq004.Play SeqWiperRightOff,0,1,0
End Sub


Sub PowerCombo
	LightSeq001.StopPlay : LightSeq002.StopPlay : LightSeq003.StopPlay : LightSeq004.StopPlay 
	LightSeq001.UpdateInterval = 50 : LightSeq001.Play SeqBlinking,0,5,25' total ms: 1200
	LightSeq001.UpdateInterval = 25 : LightSeq001.Play SeqBlinking,0,10,25' total ms: 1200

	LightSeq002.UpdateInterval = 25 : LightSeq002.Play SeqBlinking,0,6,20' total ms: 1200
	LightSeq002.UpdateInterval = 10 : LightSeq002.Play SeqBlinking,0,11,20' total ms: 1200

	LightSeq003.UpdateInterval = 25 : LightSeq003.Play SeqBlinking,0,6,20' total ms: 1200
	LightSeq003.UpdateInterval = 10 : LightSeq003.Play SeqBlinking,0,11,20' total ms: 120

	LightSeq004.UpdateInterval = 25 : LightSeq004.Play SeqBlinking,0,6,20' total ms: 1200
	LightSeq004.UpdateInterval = 10 : LightSeq004.Play SeqBlinking,0,11,20' total ms: 12000
End Sub

Sub MotuMballIntro
	LightSeq001.StopPlay
	LightSeq001.UpdateInterval = 7
	LightSeq001.Play SeqMiddleOutHorizOn,0,1,0' total ms: 210
	LightSeq001.Play SeqMiddleInHorizOff,0,1,0' total ms: 210
	LightSeq001.Play SeqHatch2VertOn,0,1,0' total ms: 350
	LightSeq001.Play SeqHatch2VertOff,0,1,0' total ms: 350
	LightSeq001.Play SeqCircleInOn,0,1,0' total ms: 700
	LightSeq001.Play SeqArcTopLeftUpOff,0,1,0' total ms: 630
	LightSeq001.Play SeqStripe2HorizOn,0,1,0' total ms: 350
	LightSeq001.UpdateInterval = 3
	LightSeq001.Play SeqDiagUpRightOff,0,1,0' total ms: 600

	LightSeq001.UpdateInterval = 7
	LightSeq001.Play SeqMiddleOutHorizOn,0,1,0' total ms: 210
	LightSeq001.Play SeqMiddleInHorizOff,0,1,0' total ms: 210
	LightSeq001.Play SeqHatch2VertOn,0,1,0' total ms: 350
	LightSeq001.Play SeqHatch2VertOff,0,1,0' total ms: 350
	LightSeq001.Play SeqCircleInOn,0,1,0' total ms: 700
	LightSeq001.Play SeqArcTopLeftUpOff,0,1,0' total ms: 630
	LightSeq001.Play SeqStripe2HorizOn,0,1,0' total ms: 350
	LightSeq001.UpdateInterval = 3
	LightSeq001.Play SeqDiagUpRightOff,0,1,0' total ms: 600
	LightSeq001.Play SeqMiddleOutHorizOn,0,1,0' total ms: 210
	LightSeq001.Play SeqMiddleInHorizOff,0,1,0' total ms: 210
	LightSeq001.Play SeqArcTopLeftUpOff,0,1,0' total ms: 630
	LightSeq001.Play SeqStripe2HorizOn,0,1,0' total ms: 350
	LightSeq001.Play SeqDiagUpRightOff,0,1,0' total ms: 600
	LightSeq001.Play SeqMiddleOutHorizOn,0,1,0' total ms: 210
	LightSeq001.Play SeqMiddleInHorizOff,0,1,0' total ms: 210
	LightSeq001.Play SeqArcTopLeftUpOff,0,1,0' total ms: 630
	LightSeq001.Play SeqStripe2HorizOn,0,1,0' total ms: 350

	LightSeq004.StopPlay
	LightSeq004.UpdateInterval = 1
	LightSeq004.Play SeqRightOn,0,1,0' total ms: 50
	LightSeq004.Play SeqHatch1VertOff,0,100,0' total ms: 50
	LightSeq004.Play SeqRightOn,0,1,0' total ms: 50

	LightSeq003.StopPlay : LightSeq002.StopPlay
	LightSeq003.UpdateInterval = 1 : LightSeq002.UpdateInterval = 1
	LightSeq003.Play SeqRightOff,0,110,0 :LightSeq002.Play SeqRightOff,0,110,0
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
	LightSeq003.Play SeqLeftOn,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqLeftOff,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
	LightSeq003.Play SeqLeftOn,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqLeftOff,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
	LightSeq003.Play SeqLeftOn,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqLeftOff,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
	LightSeq003.Play SeqLeftOn,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqLeftOff,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
	LightSeq003.Play SeqLeftOff,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
	LightSeq003.Play SeqLeftOn,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqLeftOff,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
	LightSeq003.Play SeqLeftOff,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
	LightSeq003.Play SeqLeftOn,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqLeftOff,0,1,0 : LightSeq002.Play SeqLeftOn,0,1,0
	LightSeq003.Play SeqRightOn,0,1,0 : LightSeq002.Play SeqRightOn,0,1,0
	LightSeq003.Play SeqRightOff,0,1,0 :LightSeq002.Play SeqRightOff,0,1,0
End Sub


'Do it like this as one on/off, will make it faster
Dim FlippersOn : FlippersOn = 0
Sub FlippersEnabled(Enabled)
	'Debug.Print "flippers on coil " & Enabled
	FlippersOn = Enabled
	If not FlippersOn then LeftFlipper.RotateToStart : LeftFlipper1.RotateToStart : RightFlipper.RotateToStart
End Sub

Sub AutoPlunger(Enabled)
  If Enabled Then
    PlungerIM.AutoFire
  End If
End Sub

Sub SolPost(Enabled)
  If Enabled Then
    UpPost.Isdropped=false
  Else
    UpPost.Isdropped=true
  End If
End Sub


' HeManScoop hit 
Dim aBall, aZpos
Dim bBall, bZpos

 Sub sw11_Hit()
     Set bBall = ActiveBall
     'PlaySound "kicker_enter_center"
     bZpos = 35  
     Me.TimerInterval = 2
     Me.TimerEnabled = 1
	 PlaySound "scoop"
 End Sub
 
 Sub sw11_Timer
     bBall.Z = bZpos
     bZpos = bZpos-4
     If bZpos <-30 Then
         Me.TimerEnabled = 0
         Me.DestroyBall
         bsRI.AddBall Me
     End If
 End Sub

Sub HeManScoop(Enabled)
  If Enabled Then 
    bsRI.ExitSol_On
  End If
End Sub

'bumpers
Sub sw30_Hit() : vpmTimer.PulseSw 38 : PlaySoundAtBall "fx_bumper2":BumperFlash1.state = 1:sw30.TimerEnabled=True:End Sub
Sub sw31_Hit() : vpmTimer.PulseSw 39 : PlaySoundAtBall "fx_bumper3":BumperFlash2.state = 1:sw31.TimerEnabled=True:End Sub
Sub sw32_Hit() : vpmTimer.PulseSw 40 : PlaySoundAtBall "fx_bumper4":BumperFlash3.state = 1:sw32.TimerEnabled=True:End Sub

sub sw30_Timer()
  me.TimerEnabled = False
  BumperFlash1.state = 0
End Sub

sub sw31_Timer()
  me.TimerEnabled = False
  BumperFlash2.state = 0
End Sub

sub sw32_Timer()
  me.TimerEnabled = False
  BumperFlash3.state = 0
End Sub

'#################################################
'MODELS
'########################################'
Dim HeManOn
HeManOn=False

Sub SolHeManToy(enabled)
  If enabled Then
    HeManTimer1.Enabled=True
    HeManOn=True
    HeManFlickTimer.Enabled=False
  Else
    HeManOn=False
    HeManTimer1.Enabled=True
  End IF
    
 End Sub

Sub HeManTimer1_Timer()
    If HeManOn = True Then
		HeManModel.RotZ = HeManModel.RotZ + 1
		If HeManModel.RotZ = 260 Then
			HeManTimer1.Enabled=False
		End If
	Else 
		HeManFlickTimer.Interval=2
		HeManModel.RotZ = HeManModel.RotZ - 1
		If HeManModel.RotZ= 200 Then
			HeManTimer1.Enabled=False
			HeManFlickTimer.Enabled=True
		End If
	End if
End Sub

SUb HeManFlickTimer_Timer()
  HeManModel.RotZ = HeManModel.RotZ - 1
  If HeManModel.RotZ = 195  Then
    HeManFlickTimer.Enabled=False
    HeManFlickTimer1.Enabled=True
  End If
End SUb

SUb HeManFlickTimer1_Timer()
  HeManModel.RotZ = HeManModel.RotZ + 1
  If HeManModel.RotZ = 205 Then
    HeManFlickTimer1.Enabled=False
  End If
End SUb

SUb HeManFlickTimer2_Timer()
  HeManModel.RotZ = HeManModel.RotZ - 1
  If HeManModel.RotZ = 200 Then
    HeManFlickTimer1.Enabled=False
  End If
End SUb

''''''''''''''''''''''''
' RipperToy

Dim RipperUp
RipperUp=False


Sub SolRipper(enabled)
	If enabled Then
		RipperUp=True
		RipperTimer.Enabled=True
	else
		RipperUp=False
	end if
 End Sub

Sub RipperTimer_Timer()
		if RipperUp=True Then
			If ManAtArms.ObjRotY <=20 Then				
				ManAtArms.ObjRotY =ManAtArms.ObjRotY + 1
			End If

			Wheel.ObjRotY = Wheel.ObjRotY + 1			
			if Wheel.ObjRotY = 360 Then Wheel.ObjRotY = 0: End If
		else
			If ManAtArms.ObjRotY >=1 Then
				ManAtArms.ObjRotY =ManAtArms.ObjRotY - 1
			else
				RipperTimer.Enabled=False
			End If
		end if
End Sub

''''''''''''''''''''''''
' Roton
'
Dim RotonUp
RotonUp=False

Sub SolRoton(enabled)
	If enabled Then
		RotonUp=True
		RotonStartTimer.Enabled=True
	Else
		RotonUp=False
	End If
 End Sub

Sub RotonStartTimer_Timer()

		If RotonUp=True THen
			If Skeletor.Z <= 110 Then
				Roton.TransY = Roton.TransY  + 1
				Skeletor.Z= Skeletor.Z  + 1
			End If
				
		Else
			If Skeletor.Z >= 65 Then
				Roton.TransY = Roton.TransY  - 1
				Skeletor.Z= Skeletor.Z  - 1
			End if
			If Skeletor.Z =65 Then
				RotonTimer.Enabled = False
				RotonStartTimer.Enabled = False
				RotonTimer.Enabled = False
			End If
		eND IF

		Roton.ObjRotZ = Roton.ObjRotZ -25
	End Sub

Sub RotonTimer_Timer()
		Roton.TransY = Roton.TransY  - 1
		Roton.ObjRotZ = Roton.ObjRotZ +25
		Skeletor.Z= Skeletor.Z  - 1
		If Skeletor.Z <= 65 Then
			RotonTimer.Enabled = False
		eND IF
End Sub

''''''''''''''''''''''''
' Sol Motlok
'
Dim MutantUp
MutantUp=False


Sub SolMutant(enabled)
	If enabled Then
		MutantUp=True
		Mutant.ObjRotZ = Mutant.ObjRotZ +5
		Mutant.ObjRotY = Mutant.ObjRotY +2
		'MutantTimer.Enabled=True
	else
		MutantUp=False
		Mutant.ObjRotZ = Mutant.ObjRotZ -5
		Mutant.ObjRotY = Mutant.ObjRotY -2
	end if
 End Sub

'Mutant
Sub MutantTimer_Timer()
	If MutantUp=True then
		Mutant.ObjRotZ = Mutant.ObjRotZ +5
	Else		
		Mutant.ObjRotZ = Mutant.ObjRotZ -5
		MutantTimer.Enabled = False
	End If
	End Sub

Sub MutantTimer1_Timer()
	RotonStartTimer.Enabled = False
	RotonTimer.Interval = 5
	Roton.ObjRotZ = Roton.ObjRotZ +25
End Sub

Class myTurnTable
	Private mX, mY, mSize, mMotorOn, mDir, mBalls, mTrigger
	Public MaxSpeed, SpinDown, Speed

	Private Sub Class_Initialize
		mMotorOn = False : Speed = 0 : mDir = 1 : SpinDown = 15
		Set mBalls = New cvpmDictionary
	End Sub

	Public Sub InitTurntable(aTrigger, aMaxSpeed)
		mX = aTrigger.X : mY = aTrigger.Y : mSize = aTrigger.Radius
		MaxSpeed = aMaxSpeed : Set mTrigger = aTrigger
	End Sub

	Public Sub CreateEvents(aName)
		If vpmCheckEvent(aName, Me) Then
			vpmBuildEvent mTrigger, "Hit", aName & ".AddBall ActiveBall"
			vpmBuildEvent mTrigger, "UnHit", aName & ".RemoveBall ActiveBall"
			vpmBuildEvent mTrigger, "Timer", aName & ".Update"
		End If
	End Sub

	Public Sub SolMotorState(aCW, aEnabled)
		mMotorOn = aEnabled
		If aEnabled Then If aCW Then mDir = 1 Else mDir = -1
		NeedUpdate = True
	End Sub

	Public Property Let MotorOn(aEnabled)
		mMotorOn = aEnabled
		NeedUpdate = (mBalls.Count > 0) Or (SpinDown > 0)
	End Property
	Public Property Get MotorOn
		MotorOn = mMotorOn
	End Property

	Public Sub AddBall(aBall)
		On Error Resume Next
		mBalls.Add aBall,0
		NeedUpdate = True
	End Sub
	Public Sub RemoveBall(aBall)
		On Error Resume Next
		mBalls.Remove aBall
		NeedUpdate = (mBalls.Count > 0) Or (SpinDown > 0)
	End Sub

	Public Property Let SpinCW(aCW)
		If aCW Then mDir = 1 Else mDir = -1
		NeedUpdate = True
	End Property
	Public Property Get SpinCW
		SpinCW = (mDir = 1)
	End Property

	Public Sub Update
		If mMotorOn Then
			Speed = MaxSpeed
			NeedUpdate = mBalls.Count
		Else
			Speed = Speed - SpinDown*MaxSpeed/3000 '100
			If Speed < 0 Then 
				Speed = 0
				'msgbox "off"
				NeedUpdate = mBalls.Count
			End If
		End If
		If Speed > 0 Then
			Dim obj
			On Error Resume Next
			For Each obj In mBalls.Keys
				If obj.X < 0 Or Err Then RemoveBall obj Else AffectBall obj
			Next
			On Error Goto 0
		End If
	End Sub

	Public Sub AffectBall(aBall)
		Dim dX, dY, dist
		dX = aBall.X - mX : dY = aBall.Y - mY : dist = Sqr(dX*dX + dY*dY)
		If dist > mSize Or dist < 1 Or Speed = 0 Then Exit Sub
		aBall.VelX = aBall.VelX - (dY * mDir * Speed / 1000)
		aBall.VelY = aBall.VelY + (dX * mDir * Speed / 1000)
	End Sub

	Private Property Let NeedUpdate(aEnabled)
		If mTrigger.TimerEnabled <> aEnabled Then
			mTrigger.TimerInterval = 10
			mTrigger.TimerEnabled = aEnabled
		End If
	End Property
End Class

'**'SoundTriggers
Sub SndTrigger1_hit():PlaySound "PlasticRampStretched":End Sub
Sub SndTrigger2_hit():PlaySound "PlasticRamp_1_sec_b":End Sub
Sub SndTrigger3_hit():PlaySound "PlasticRampStretched":End Sub
Sub leftdrop_hit():PlaySound "BallDrop2_long":End Sub
Sub leftdrop1_hit():PlaySound "BallDrop2_long":End Sub

'**********Sling Shot Animations
' Rstep and Lstep  are the variables that increment the animation
'****************
Dim RStep, Lstep

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

'*********************************************************************
'* TARGETBANK MOVEMENT Taken from AFM written by Groni ***************
'*********************************************************************

Dim TBPos, TBDown

Sub TBMove (enabled)
	if enabled then
		TBTimer.Enabled=1
		PlaySound SoundFX("TargetBank",DOFContactors)
	End If
End Sub

Sub TBTimer_Timer() 
Select Case TBPos
Case 0: MotorBank.Z=-20:SW49P.Z=-20:SW50P.Z=-20:SW51P.Z=-20:TBPos=0:TBDown=0:TBTimer.Enabled=0:Controller.Switch 54, 1:Controller.Switch 55, 0::SW49.isdropped=0:SW50.isdropped=0:SW51.isdropped=0:DPWall.isdropped=0:DPWall1.isdropped=1
Case 1: MotorBank.Z=-22:SW49P.Z=-22:SW50P.Z=-22:SW51P.Z=-22
Case 2: MotorBank.Z=-24:SW49P.Z=-24:SW50P.Z=-24:SW51P.Z=-24
Case 3: MotorBank.Z=-26:SW49P.Z=-26:SW50P.Z=-26:SW51P.Z=-26
Case 4: MotorBank.Z=-28:SW49P.Z=-28:SW50P.Z=-28:SW51P.Z=-28
Case 5: MotorBank.Z=-30:SW49P.Z=-30:SW50P.Z=-30:SW51P.Z=-30
Case 6: MotorBank.Z=-32:SW49P.Z=-32:SW50P.Z=-32:SW51P.Z=-32
Case 7: MotorBank.Z=-34:SW49P.Z=-34:SW50P.Z=-34:SW51P.Z=-34
Case 8: MotorBank.Z=-36:SW49P.Z=-36:SW50P.Z=-36:SW51P.Z=-36
Case 9: MotorBank.Z=-38:SW49P.Z=-38:SW50P.Z=-38:SW51P.Z=-38
Case 10: MotorBank.Z=-40:SW49P.Z=-40:SW50P.Z=-40:SW51P.Z=-40
Case 11: MotorBank.Z=-42:SW49P.Z=-42:SW50P.Z=-42:SW51P.Z=-42
Case 12: MotorBank.Z=-44:SW49P.Z=-44:SW50P.Z=-44:SW51P.Z=-44:
Case 13: MotorBank.Z=-46:SW49P.Z=-46:SW50P.Z=-46:SW51P.Z=-46:
Case 14: MotorBank.Z=-48:SW49P.Z=-48:SW50P.Z=-48:SW51P.Z=-48
Case 15: MotorBank.Z=-50:SW49P.Z=-50:SW50P.Z=-50:SW51P.Z=-50
Case 16: MotorBank.Z=-52:SW49P.Z=-52:SW50P.Z=-52:SW51P.Z=-52
Case 17: MotorBank.Z=-54:SW49P.Z=-54:SW50P.Z=-54:SW51P.Z=-54
Case 18: MotorBank.Z=-56:SW49P.Z=-56:SW50P.Z=-56:SW51P.Z=-56
Case 19: MotorBank.Z=-58:SW49P.Z=-58:SW50P.Z=-58:SW51P.Z=-58
Case 20: MotorBank.Z=-60:SW49P.Z=-60:SW50P.Z=-60:SW51P.Z=-60
Case 21: MotorBank.Z=-62:SW49P.Z=-62:SW50P.Z=-62:SW51P.Z=-62
Case 22: MotorBank.Z=-64:SW49P.Z=-64:SW50P.Z=-64:SW51P.Z=-64
Case 23: MotorBank.Z=-66:SW49P.Z=-66:SW50P.Z=-66:SW51P.Z=-66
Case 24: MotorBank.Z=-68:SW49P.Z=-68:SW50P.Z=-68:SW51P.Z=-68
Case 25: MotorBank.Z=-70:SW49P.Z=-70:SW50P.Z=-70:SW51P.Z=-70
Case 26: MotorBank.Z=-72:SW49P.Z=-72:SW50P.Z=-72:SW51P.Z=-72
Case 27: MotorBank.Z=-74:SW49P.Z=-74:SW50P.Z=-74:SW51P.Z=-74
Case 28: MotorBank.Z=-76:SW49P.Z=-76:SW50P.Z=-76:SW51P.Z=-76:SW49.isdropped=1:SW50.isdropped=1:SW51.isdropped=1:DPWALL.isdropped=1
Case 29: TBTimer.Enabled=0:TBDown=1:Controller.Switch 54, 0: Controller.Switch 55, 1
End Select

If TBDown=0 then TBPos=TBPos+1 
If TBDown=1 then TBPos=TBPos-1
End Sub

'*************
' Spinners
'************
Sub RLS_Timer()
	'RampGate1.RotZ = -(Spinner4.currentangle)
	'RampGate2.RotZ = -(Spinner1.currentangle)
	'RampGate3.RotZ = -(Spinner3.currentangle)
	'RampGate4.RotZ = -(Spinner2.currentangle)
	SpinnerT4.RotZ = -(sw44.currentangle)
	SpinnerT1.RotZ = -(sw36.currentangle)
End Sub

'*********************************************
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


Sub Pins_Hit (idx): PlaySoundAtBall "pinhit_low" : End Sub
Sub Targets_Hit (idx): PlaySoundAtBall "target": End Sub
Sub Metals_Thin_Hit (idx) : PlaySoundAtBall "metalhit_thin"	: End Sub
Sub Metals_Medium_Hit (idx) :PlaySoundAtBall "metalhit_medium": End Sub
Sub Metals2_Hit (idx):PlaySoundAtBall "metal2hit" :End Sub
Sub Gates_Hit (idx) :PlaySoundAtBall "gate" :End Sub
Sub Spinner_Spin : PlaySoundAtBall "fx_spinner": End Sub
Sub Rubbers_Hit(idx):PlaySoundAtBall "rubber_hit_2":End Sub

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
