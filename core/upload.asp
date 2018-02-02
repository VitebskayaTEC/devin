<%
	Public objStream1

	Class FileUploader
		Public  Files
		Private mcolFormElem

		Private Sub Class_Initialize()
			Set Files = Server.CreateObject("Scripting.Dictionary")
			Set mcolFormElem = Server.CreateObject("Scripting.Dictionary")
		End Sub

		Private Sub Class_Terminate()
			If IsObject(Files) Then
				Files.RemoveAll()
				Set Files = Nothing
			End If
			If IsObject(mcolFormElem) Then
				mcolFormElem.RemoveAll()
				Set mcolFormElem = Nothing
			End If
		End Sub

		Public Property Get Form(sIndex)
			Form = ""
			If mcolFormElem.Exists(LCase(sIndex)) Then Form = mcolFormElem.Item(LCase(sIndex))
		End Property

		Public Default Sub Upload()
			Dim biData, sInputName
			Dim nPosBegin, nPosEnd, nPos, vDataBounds, nDataBoundPos
			Dim nPosFile, nPosBound

			biData = Request.BinaryRead(Request.TotalBytes)
			Set objStream1 = Server.CreateObject("ADODB.Stream")
			objStream1.Type = 1
			objStream1.Open
			objStream1.Write biData

			nPosBegin = 1
			nPosEnd = InstrB(nPosBegin, biData, CByteString(Chr(13)))

			If (nPosEnd-nPosBegin) <= 0 Then Exit Sub

			vDataBounds = MidB(biData, nPosBegin, nPosEnd-nPosBegin)
			nDataBoundPos = InstrB(1, biData, vDataBounds)

			Do Until nDataBoundPos = InstrB(biData, vDataBounds & CByteString("--"))

				nPos = InstrB(nDataBoundPos, biData, CByteString("Content-Disposition"))
				nPos = InstrB(nPos, biData, CByteString("name="))
				nPosBegin = nPos + 6
				nPosEnd = InstrB(nPosBegin,biData, CByteString(Chr(34)))
				sInputName = CWideString(MidB(biData, nPosBegin, nPosEnd - nPosBegin))
				nPosFile = InstrB(nDataBoundPos, biData, CByteString("filename="))
				nPosBound = InstrB(nPosEnd, biData, vDataBounds)

				If nPosFile <> 0 And nPosFile < nPosBound Then
					Dim oUploadFile, sFileName
					Set oUploadFile = New UploadedFile

					nPosBegin = nPosFile + 10
					nPosEnd =  InstrB(nPosBegin, biData, CByteString(Chr(34)))
					sFileName = CWideString(MidB(biData, nPosBegin, nPosEnd - nPosBegin))
					oUploadFile.FileName = Right(sFileName, Len(sFileName) - InStrRev(sFileName, "\"))

					nPos = InstrB(nPosEnd, biData, CByteString("Content-Type:"))
					nPosBegin = nPos + 14
					nPosEnd = InstrB(nPosBegin, biData, CByteString(Chr(13)))

					oUploadFile.ContentType = CWideString(MidB(biData, nPosBegin, nPosEnd - nPosBegin))

					nPosBegin = nPosEnd + 4
					nPosEnd = InstrB(nPosBegin, biData, vDataBounds) - 2

					oUploadFile.FileData = MidB(biData, nPosBegin, nPosEnd - nPosBegin)

					oUploadFile.nPosBegin= nPosBegin
					oUploadFile.nPosEnd = nPosEnd

					If oUploadFile.FileSize > 0 Then Files.Add LCase(sInputName), oUploadFile
				Else
					nPos = InstrB(nPos, biData, CByteString(Chr(13)))
					nPosBegin = nPos + 4
					nPosEnd = InstrB(nPosBegin, biData, vDataBounds) - 2
					If Not mcolFormElem.Exists(LCase(sInputName)) Then mcolFormElem.Add LCase(sInputName), CWideString(MidB(biData, nPosBegin, nPosEnd-nPosBegin))
				End If

				nDataBoundPos = InstrB(nDataBoundPos + LenB(vDataBounds), biData, vDataBounds)
			Loop
		End Sub

		'String to byte string conversion
		Private Function CByteString(sString)
			Dim nIndex
			For nIndex = 1 to Len(sString)
				CByteString = CByteString & ChrB(AscB(Mid(sString,nIndex,1)))
			Next
		End Function

		'Byte string to string conversion
		Private Function CWideString(bsString)
			Dim nIndex
			CWideString =""
			For nIndex = 1 to LenB(bsString)
				CWideString = CWideString & Chr(AscB(MidB(bsString,nIndex,1)))
			Next
		End Function

	End Class


	Class UploadedFile

		Public ContentType
		Public FileName
		Public FileData
		Public nPosBegin
		Public nPosEnd

		Public Property Get FileSize()
			FileSize = LenB(FileData)
		End Property

		Public Sub SaveToDisk(sPath)
			Dim oFS, oFile, sss, j
			Dim nIndex
			If sPath = "" Or FileName = "" Then Exit Sub
			If Mid(sPath, Len(sPath)) <> "\" Then sPath = sPath & "\"
			Set oFS = Server.CreateObject("Scripting.FileSystemObject")
			If Not oFS.FolderExists(sPath) Then Exit Sub
			Dim oStream
			Set oStream = Server.CreateObject("ADODB.Stream")
			oStream.Type = 1
			oStream.Open
			'Response.Write("nPosBegin=" & nPosBegin & "  nPosEnd=" & nPosEnd)
			objStream1.Position = nPosBegin - 1
			oStream.Write objStream1.Read(nPosEnd - nPosBegin) 'записываем в поток2 из потока1
			oFile=sPath & FileName
			'Response.Write("nPosBegin=" & nPosBegin &"  nPosEnd=" & nPosEnd & " oFile=" & oFile)
			oStream.SaveToFile oFile, 2
			oStream.Close
			Set oStream = Nothing
		End Sub

	End Class
%>