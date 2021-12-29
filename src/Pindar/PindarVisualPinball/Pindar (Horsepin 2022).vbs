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

'********************************************
'**     Game Specific Code Starts Here     **
'********************************************

'Release
'Const IsDebug = False ' set false to load an export
'Const GameDirectory = ".\PinGod.Pindar.exe" 'exported game

'Debug
Const IsDebug = True
Const GameDirectory = "..\PindarGodot" ' Loads the godot pingod game project
Const UseSolenoids = 1 ' Check for solenoid states?
Const UsePdbLeds = 0  ' use led (color)
Const UseLamps = 1  ' Check for lamp states?
Dim GORtargets,GARtargets,bsLKicker,bsTrough,mtest,mMagnet,BallInPlay,plungerIM,x,ballOut
' Const UseSolenoids=1,UseLamps=1,UseSync=1 ' fuzzel script use sync?

Const SSolenoidOn="solon",SSolenoidOff="soloff",SFlipperOn="FlipperUp",SFlipperOff="FlipperDown",SCoin="quarter"

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
	
	On Error Resume Next
	PinMAMETimer.Interval=PinMAMEInterval:PinMAMETimer.Enabled=1:vpmNudge.TiltSwitch=17:vpmNudge.Sensitivity=4
	vpmNudge.TiltObj=Array(LeftSling,RightSling,TopJet,LeftJet,BottomJet)

	PlaySound "gorgar_feel_my_power"
	
	Set bsTrough = New cvpmTrough
		bsTrough.Size = 1
		bsTrough.Balls = 1
		bsTrough.InitSwitches Array(85) ' trough switches
		bsTrough.InitExit BallRelease, 90, 8	
		bsTrough.CreateEvents "bsTrough",  Drain
		bsTrough.InitEntrySounds "drainhit", "", ""
		bsTrough.InitExitSounds "solenoidleft", ""
		bsTrough.Reset	

	Set GORtargets=New cvpmDropTarget 
		GORtargets.InitDrop Array(Array(sw18),Array(sw19),Array(sw20)),Array(18,44,21)
		GORtargets.InitSnd SoundFX("droptargetdown",DOFContactors),SoundFX("droptargetreset",DOFContactors)
		'GORtargets.AllDownSw=21

	Set GARtargets=New cvpmDropTarget
		GARtargets.InitDrop Array(Array(sw41),Array(sw42),Array(sw43)),Array(41,42,43)
		GARtargets.InitSnd SoundFX("droptargetdown",DOFContactors),SoundFX("droptargetreset",DOFContactors)
		'GARtargets.AllDownSw=44
		
	Set bsLKicker=New cvpmBallStack
		bsLKicker.InitSaucer sw15,15,155,8
		bsLKicker.InitExitSnd SoundFX("popper_ball",DOFContactors),SoundFX("solon",DOFContactors)
		
	Set mMagnet=New cvpmMagnet
 		With mMagnet
 		.InitMagnet sw23, 26
		.GrabCenter=False
 		End With


	If Err Then MsgBox Err.Description
	initialized = 1
	LoadingText.Visible = false ' Hide the overlay (loading screen)
	On Error Goto 0	
	
End Sub

'***********************************
'**     Map Lights Into Array     **
'** Set Unmapped Lamps To Nothing **
'***********************************

Set Lights(1)=Light1 'Same Player Shoots Again (Playfield)
Set Lights(2)=Light2 'Left Special
Set Lights(3)=Light3 'Right special
Set Lights(4)=Light4 '2X
Set Lights(5)=Light5 '3X
Set Lights(6)=Light6 'Star 1
Set Lights(7)=Light7 'Star 2
Set Lights(8)=Light8 '1,000 Bonus
Set Lights(9)=Light9 '2.000 Bonus
Set Lights(10)=Light10 '3,000 Bonus
Set Lights(11)=Light11 '4,000 Bonus
Set Lights(12)=Light12 '5,000 Bonus
Set Lights(13)=Light13 '6,000 Bonus
Set Lights(14)=Light14 '7,000 Bonus
Set Lights(15)=Light15 '8,000 Bonus
Set Lights(16)=Light16 '9,000 Bonus
'Light17 NOT USED
Set Lights(18)=Light18 '10,000 Bonus
Set Lights(19)=Light19 '20,000 Bonus
Set Lights(20)=Light20 'A
Set Lights(21)=Light21 'B
Set Lights(22)=Light22 'C
Set Lights(23)=Light23 'D
Set Lights(24)=Light24 'E
Set Lights(25)=Light25 '1 Target
Set Lights(26)=Light26 '2 Target
Set Lights(27)=Light27 '3 Target
Set Lights(28)=Light28 '4 Target
Set Lights(29)=Light29 '1 Target Arrow
Set Lights(30)=Light30 '2 Target Arrow
Set Lights(31)=Light31 '3 Target Arrow
Set Lights(32)=Light32 '4 Target Arrow
Set Lights(33)=Light33 'Magnet 5,000
Set Lights(34)=Light34 'Magnet 10,000
Set Lights(35)=Light35 'Magnet 20,000
Set Lights(36)=Light36 'Magnet 30,000
Set Lights(37)=Light37 'Magnet 50,000
Set Lights(38)=Light70  'Top Jet Bumper
Set Lights(39)=Light71 'Left Jet Bumper
Set Lights(40)=Light69 'Bottom Jet Bumper
Set Lights(41)=Light41 'GAR 5,000 When Lit
Set Lights(42)=Light42 'GOR
Set Lights(43)=Light43 'GAR
Set Lights(44)=Light44 'Eject Hole 10,000
Set Lights(45)=Light45 'Eject Hole 15,000
Set Lights(46)=Light46 'Eject Hole Extra Ball
'Light47 NOT USED
Set Lights(48)=Light48 'Spinner 1,000 When Lit 

'**************************************
'**     Bind Events To Solenoids     **
'**************************************

SolCallback(0) = "Died"
SolCallback(1)	= "bsTrough.SolOut"
SolCallBack(2)	= "bsLKicker.SolOut"
SolCallback(3)	= "GARtargets.SolDropUp"
SolCallback(4)	= "GORtargets.SolDropUp"
SolCallback(5)	= "mMagnet.MagnetOn="
SolCallback(6)  = "MagnetFlash"
SolCallback(7) = "FlippersEnabled"
SolCallback(14)	= "vpmSolSound SoundFX(""knocker"",DOFKnocker),"
'SolCallback(17)	= "vpmSolSound SoundFX(""bumper1"",DOFContactors),"  
'SolCallback(18)	= "vpmSolSound SoundFX(""bumper2"",DOFContactors),"  
'SolCallback(19)	= "vpmSolSound SoundFX(""bumper3"",DOFContactors),"  
'SolCallback(20)	= "vpmSolSound SoundFX(""sling"",DOFContactors),"
'SolCallback(21)	= "vpmSolSound SoundFX(""sling"",DOFContactors),"
SolCallback(23) = "vpmNudge.SolGameOn"
SpecialSol = 20
SolCallback(20) = "SpecialSolCallback"

Sub rubber_Hit(idx):PlaySound "rubber":End Sub

Sub Died(Enabled)
	'on error resume next	
	If not enabled then
		MsgBox "Game window unavailable." : Err.Raise 5
	End if
End Sub


Dim FlippersOn : FlippersOn = 0
Sub FlippersEnabled(Enabled)
	FlippersOn = Enabled
	If not FlippersOn then LeftFlipper.RotateToStart : RightFlipper.RotateToStart
End Sub

Sub SpecialSolCallback(num)
	Select Case num
		Case 0 : DisableAllLampSequencers
		Case 1 : BlinkGISlow
		Case 2 : MagnetShow
	End Select
End Sub

Sub DisableAllLampSequencers
	LightSeq002.StopPlay
	magnetlight001.state=0:magnetlight002.state=0
End Sub

Sub BlinkGISlow
	LightSeq002.StopPlay: LightSeq002.UpdateInterval = 1
	LightSeq002.Play SeqCircleOutOn,0,1,0' total ms: 100
	LightSeq002.Play SeqCircleOutOff,0,3,0' total ms: 100
	LightSeq002.Play SeqCircleOutOn,0,1,0' total ms: 100
	LightSeq002.Play SeqCircleOutOff,0,3,0' total ms: 100
	LightSeq002.Play SeqCircleOutOn,0,1,0' total ms: 100
	LightSeq002.Play SeqCircleOutOff,0,3,0' total ms: 100
	LightSeq002.Play SeqCircleOutOn,0,1,0' total ms: 100
	LightSeq002.Play SeqCircleOutOff,0,3,0' total ms: 100
End Sub

Sub MagnetShow
	magnetlight001.state=2:magnetlight002.state=2
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
		LeftFlipper.RotateToEnd
		PlaySoundAt "fx_flipperup", LeftFlipper
	End If

	If keycode = RightFlipperKey and FlippersOn Then
		RightFlipper.RotateToEnd
		PlaySoundAt "fx_flipperup", RightFlipper
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
		PlaySoundAt "fx_flipperdown", LeftFlipper
	End If

	If keycode = RightFlipperKey and FlippersOn Then
		RightFlipper.RotateToStart
		PlaySoundAt "fx_flipperdown", RightFlipper
	End If

	If vpmKeyUp(keycode) Then Exit Sub ' This will handle machine switches and flippers etc
End Sub


 

'*****************************
'**     Switch Handling     ** 
'*****************************
 
Sub Drain_Hit : PlaySound "Drain" : bsTrough.AddBall Me : End Sub
Sub BallRelease_UnHit : Set BallInPlay = ActiveBall : End Sub				
Sub LeftSling_Slingshot   : vpmTimer.PulseSw 12 : PlaySound SoundFX("left_slingshot",DOFContactors), 0, 1, 0.05, 0.05 : End Sub 'switch 12
Sub RightSling_Slingshot   : vpmTimer.PulseSw 36 : PlaySound SoundFX("right_slingshot",DOFContactors),0,1,-0.05,0.05 : End Sub 'switch 12
Sub sw13_Slingshot   : vpmTimer.PulseSwitch 50, 0, 0 : End Sub
Sub sw16_Slingshot   : vpmTimer.PulseSwitch 51, 0, 0 : End Sub
Sub sw17_Slingshot   : vpmTimer.PulseSwitch 52, 0, 0 : End Sub
Sub sw22_Slingshot   : vpmTimer.PulseSwitch 53, 0, 0 : End Sub
Sub sw24_Slingshot   : vpmTimer.PulseSwitch 54, 0, 0 : End Sub
Sub sw25_Slingshot   : vpmTimer.PulseSwitch 55, 0, 0 : End Sub
Sub sw40_Slingshot   : vpmTimer.PulseSwitch 56, 0, 0 : End Sub
Sub sw30_Spin : vpmTimer.PulseSwitch 30, 0, 0 :PlaySound "Spinner",0,.25,0,0.25: End Sub

'plunger lane
Sub sw001_Hit : Controller.Switch 20, 1 : End Sub
Sub sw001_UnHit : Controller.Switch 20, 0 : End Sub


Sub sw15_Hit
	bsLKicker.AddBall 0
End Sub 				'switch 15

Sub sw18_Hit:GORtargets.Hit 1:End Sub                         'switch 18
Sub sw19_Hit:GORtargets.Hit 2:End Sub                         'switch 44
Sub sw20_Hit:GORtargets.Hit 3:End Sub                         'switch 20
Sub sw23_Hit:Controller.Switch 23, 1:mMagnet.AddBall ActiveBall:End Sub
Sub sw23_UnHit:Controller.Switch 23, 0:mMagnet.RemoveBall ActiveBall:End Sub
Sub LeftJet_Hit : vpmTimer.PulseSwitch 37, 0, 0 : PlaySound SoundFX("fx_bumper3",DOFContactors): End Sub
Sub TopJet_Hit : vpmTimer.PulseSwitch 38, 0, 0 : PlaySound SoundFX("fx_bumper3",DOFContactors): End Sub
Sub BottomJet_Hit : vpmTimer.PulseSwitch 39, 0, 0 : PlaySound SoundFX("fx_bumper3",DOFContactors) :End Sub
Sub sw41_Hit:GARtargets.Hit 1:End Sub                        'switch 41
Sub sw42_Hit:GARtargets.Hit 2:End Sub                        'switch 42
Sub sw43_Hit:GARtargets.Hit 3:End Sub                        'switch 43


Sub MagnetFlash(Enabled)

End Sub

  
'***********************************
'**     Map Lights Into Array     **
'** Set Unmapped Lamps To Nothing **
'***********************************

Set Lights(1)=Light1 'Same Player Shoots Again (Playfield)
Set Lights(2)=Light2 'Left Special
Set Lights(3)=Light3 'Right special
Set Lights(4)=Light4 '2X
Set Lights(5)=Light5 '3X
Set Lights(6)=Light6 'Star 1
Set Lights(7)=Light7 'Star 2
Set Lights(8)=Light8 '1,000 Bonus
Set Lights(9)=Light9 '2.000 Bonus
Set Lights(10)=Light10 '3,000 Bonus
Set Lights(11)=Light11 '4,000 Bonus
Set Lights(12)=Light12 '5,000 Bonus
Set Lights(13)=Light13 '6,000 Bonus
Set Lights(14)=Light14 '7,000 Bonus
Set Lights(15)=Light15 '8,000 Bonus
Set Lights(16)=Light16 '9,000 Bonus
'Light17 NOT USED
Set Lights(18)=Light18 '10,000 Bonus
Set Lights(19)=Light19 '20,000 Bonus
Set Lights(20)=Light20 'A
Set Lights(21)=Light21 'B
Set Lights(22)=Light22 'C
Set Lights(23)=Light23 'D
Set Lights(24)=Light24 'E
Set Lights(25)=Light25 '1 Target
Set Lights(26)=Light26 '2 Target
Set Lights(27)=Light27 '3 Target
Set Lights(28)=Light28 '4 Target
Set Lights(29)=Light29 '1 Target Arrow
Set Lights(30)=Light30 '2 Target Arrow
Set Lights(31)=Light31 '3 Target Arrow
Set Lights(32)=Light32 '4 Target Arrow
Set Lights(33)=Light33 'Magnet 5,000
Set Lights(34)=Light34 'Magnet 10,000
Set Lights(35)=Light35 'Magnet 20,000
Set Lights(36)=Light36 'Magnet 30,000
Set Lights(37)=Light37 'Magnet 50,000
Set Lights(38)=Light70  'Top Jet Bumper
Set Lights(39)=Light71 'Left Jet Bumper
Set Lights(40)=Light69 'Bottom Jet Bumper
Set Lights(41)=Light41 'GAR 5,000 When Lit
Set Lights(42)=Light42 'GOR
Set Lights(43)=Light43 'GAR
Set Lights(44)=Light44 'Eject Hole 10,000
Set Lights(45)=Light45 'Eject Hole 15,000
Set Lights(46)=Light46 'Eject Hole Extra Ball
'Light47 NOT USED
Set Lights(48)=Light48 'Spinner 1,000 When Lit 



'**********************************
'**     Table-Specific Stuff     **
'**********************************

Sub sw31_Hit:vpmTimer.PulseSw 31:PlaySound SoundFX("droptargetR",DOFContactors):End Sub
Sub sw32_Hit:vpmTimer.PulseSw 32:PlaySound SoundFX("droptargetR",DOFContactors):End Sub
Sub sw33_Hit:vpmTimer.PulseSw 33:PlaySound SoundFX("droptargetR",DOFContactors):End Sub
Sub sw14_Hit:vpmTimer.PulseSw 14:PlaySound SoundFX("droptargetL",DOFContactors):End Sub


Sub sw10_Hit:   Controller.Switch 10,1 : End Sub
Sub sw10_unHit: Controller.Switch 10,0 : End Sub
Sub sw11_Hit:   Controller.Switch 49,1 : End Sub
Sub sw11_unHit: Controller.Switch 49,0 : End Sub
Sub sw34_Hit:   Controller.Switch 34,1 : End Sub
Sub sw34_unHit: Controller.Switch 34,0 : End Sub
Sub sw35_Hit:   Controller.Switch 35,1 : End Sub
Sub sw35_unHit: Controller.Switch 35,0 : End Sub
Sub sw24_Hit:   Controller.Switch 24,1 : End Sub
Sub sw24_unHit: Controller.Switch 24,0 : End Sub
Sub sw27_Hit:   Controller.Switch 27,1 : End Sub
Sub sw27_unHit: Controller.Switch 27,0 : End Sub
Sub sw28_Hit:   Controller.Switch 28,1 : End Sub
Sub sw28_unHit: Controller.Switch 28,0 : End Sub

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
	PlaySound("fx_collide"), 0, Csng(velocity) ^2 / 2000, AudioPan(ball1), 0, Pitch(ball1), 0, 0
End Sub

'*****************************************
'			FLIPPER SHADOWS
'*****************************************

sub FlipperTimer_Timer()
	FlipperLSh.RotZ = LeftFlipper.currentangle
	FlipperRSh.RotZ = RightFlipper.currentangle

End Sub

'*****************************************
'			BALL SHADOW
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
        If BOT(b).X < table1.Width/2 Then
            BallShadow(b).X = ((BOT(b).X) - (Ballsize/6) + ((BOT(b).X - (table1.Width/2))/7)) + 5
        Else
            BallShadow(b).X = ((BOT(b).X) + (Ballsize/6) + ((BOT(b).X - (table1.Width/2))/7)) - 5
        End If
        ballShadow(b).Y = BOT(b).Y + 10
        If BOT(b).Z > 20 Then
            BallShadow(b).visible = 1
        Else
            BallShadow(b).visible = 0
        End If
    Next
End Sub


Sub Pins_Hit (idx)
	PlaySound "pinhit_low", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 0, 0
End Sub

Sub Targets_Hit (idx)
	PlaySound "target", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 0, 0
End Sub

Sub Metals_Hit (idx)
	PlaySound "metalhit_medium", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0
End Sub

Sub Gates_Hit (idx)
	PlaySound "gate4", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0
End Sub

Sub Rubbers_Hit(idx)
 	dim finalspeed
  	finalspeed=SQR(activeball.velx * activeball.velx + activeball.vely * activeball.vely)
 	If finalspeed > 20 then 
		PlaySound "fx_rubber2", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0
	End if
	If finalspeed >= 6 AND finalspeed <= 20 then
 		RandomSoundRubber()
 	End If
End Sub

Sub Posts_Hit(idx)
 	dim finalspeed
  	finalspeed=SQR(activeball.velx * activeball.velx + activeball.vely * activeball.vely)
 	If finalspeed > 16 then 
		PlaySound "fx_rubber2", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0
	End if
	If finalspeed >= 6 AND finalspeed <= 16 then
 		RandomSoundRubber()
 	End If
End Sub

Sub RandomSoundRubber()
	Select Case Int(Rnd*3)+1
		Case 1 : PlaySound "rubber_hit_1", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0
		Case 2 : PlaySound "rubber_hit_2", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0
		Case 3 : PlaySound "rubber_hit_3", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0
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
		Case 1 : PlaySound "flip_hit_1", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0
		Case 2 : PlaySound "flip_hit_2", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0
		Case 3 : PlaySound "flip_hit_3", 0, Vol(ActiveBall), AudioPan(ActiveBall), 0, Pitch(ActiveBall), 1, 0
	End Select
End Sub



