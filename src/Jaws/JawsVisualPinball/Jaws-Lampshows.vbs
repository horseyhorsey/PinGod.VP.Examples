' hack to send in coil with a Value, so instead of 0/1, 0-255
SpecialSol = 29
SolCallback(29) = "SpecialSolCallback"

Sub SpecialSolCallback(num)
	Select Case num
		Case 0 : DisableAllLampSequencers
		Case 1 : Lamps_SwipeLR
		Case 2 : Lamps_SeqStripe
		Case 3 : Lamps_SeqStripe2
		Case 4 : LampsSeqDown
		Case 6 : SuperJackpot
		Case 7 : SuperJackpotStart
	End Select
End Sub

Sub DisableAllLampSequencers
	PfLampsSeq.StopPlay : GiSeq.StopPlay : FlashSeq.StopPlay 
End Sub

Sub Lamps_SwipeLR
	DisableAllLampSequencers
	PfLampsSeq.UpdateInterval = 10
	PfLampsSeq.Play SeqMiddleInHorizOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe2HorizOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqMiddleInHorizOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe2HorizOn,0,1,0' total ms: 5000
	GiSeq.UpdateInterval = 100
    GiSeq.Play SeqBlinking,,3,100
	FlashSeq.UpdateInterval = 50
    FlashSeq.Play SeqBlinking,,3,100
End Sub

Sub Lamps_SeqStripe
	DisableAllLampSequencers
	PfLampsSeq.UpdateInterval = 3
	PfLampsSeq.Play SeqStripe1VertOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe1VertOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqStripe1VertOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe1VertOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqStripe1VertOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe1VertOn,0,1,0' total ms: 5000
	GiSeq.UpdateInterval = 100
    GiSeq.Play SeqBlinking,,4,150
	FlashSeq.UpdateInterval = 50
    FlashSeq.Play SeqBlinking,,4,150
End Sub

Sub Lamps_SeqStripe2
	DisableAllLampSequencers
	PfLampsSeq.UpdateInterval = 3
	PfLampsSeq.Play SeqStripe1VertOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe1VertOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqStripe1VertOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe1VertOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqStripe1VertOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe1VertOn,0,1,0' total ms: 5000
	GiSeq.UpdateInterval = 100
    GiSeq.Play SeqBlinking,,2,150
End Sub

Sub LampsSeqDown
	DisableAllLampSequencers
	PfLampsSeq.StopPlay : PfLampsSeq.UpdateInterval = 4
	PfLampsSeq.Play SeqDownOn,0,1,0' total ms: 200
	PfLampsSeq.Play SeqDownOff,0,1,0' total ms: 200
	PfLampsSeq.Play SeqDownOn,0,1,0' total ms: 200
	PfLampsSeq.Play SeqDownOff,0,1,0' total ms: 200
	PfLampsSeq.Play SeqDownOn,0,1,0' total ms: 200
	PfLampsSeq.Play SeqDownOff,0,1,0' total ms: 200
	PfLampsSeq.Play SeqDownOn,0,1,0' total ms: 200
	PfLampsSeq.Play SeqDownOff,0,1,0' total ms: 200
	PfLampsSeq.Play SeqDownOn,0,1,0' total ms: 200
	PfLampsSeq.Play SeqDownOff,0,1,0' total ms: 200
	PfLampsSeq.Play SeqDownOn,0,1,0' total ms: 200
	PfLampsSeq.Play SeqDownOff,0,1,0' total ms: 200
End Sub

Sub SuperJackpot
	DisableAllLampSequencers
	PfLampsSeq.StopPlay : PfLampsSeq.UpdateInterval = 1000	
	PfLampsSeq.Play SeqDownOff,0,4,0' total ms: 200
	GiSeq.UpdateInterval = 40 : GiSeq.Play SeqBlinking,,8,40
	GiSeq.UpdateInterval = 20 : GiSeq.Play SeqBlinking,,16,20
	FlashSeq.UpdateInterval = 40 : FlashSeq.Play SeqBlinking,,8,40
	FlashSeq.UpdateInterval = 20 : FlashSeq.Play SeqBlinking,,16,20
End Sub

Sub SuperJackpotStart
	DisableAllLampSequencers
	PfLampsSeq.UpdateInterval = 750
	PfLampsSeq.Play SeqMiddleInHorizOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe2HorizOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqMiddleInHorizOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe2HorizOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqMiddleInHorizOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe2HorizOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqMiddleInHorizOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe2HorizOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqMiddleInHorizOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe2HorizOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqMiddleInHorizOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe2HorizOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqMiddleInHorizOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe2HorizOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqMiddleInHorizOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe2HorizOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqMiddleInHorizOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe2HorizOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqMiddleInHorizOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe2HorizOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqMiddleInHorizOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe2HorizOn,0,1,0' total ms: 5000
	PfLampsSeq.Play SeqMiddleInHorizOff,0,1,0' total ms: 3000
	PfLampsSeq.Play SeqStripe2HorizOn,0,1,0' total ms: 5000
End Sub

'Color grade GI todo: start reusing this method
Dim i 
Sub ColGradeGI_Timer
    if i=1 Then 
      ColGradeGI.Enabled = False
    Else
      Table1.ColorGradeImage = "lut_jaws_" & i
      i = i-1
    End If
End Sub

Sub ColGradeGIOff_Timer
    if i=4 Then 
      ColGradeGI.Enabled = False
    Else
      Table1.ColorGradeImage = "lut_jaws_" & i
      i = i+1
    End If      
End Sub