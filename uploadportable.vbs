If WScript.Arguments.Count < 1 Then: WScript.Quit
strBaseLocalFolder = WScript.Arguments(0)

Set oFTP = New FTP
Set oFS = New FileSystem

oFS.SetCurrentDir strBaseLocalFolder

UbiquitousFiles = Array("*.dll", _ 
						"ubiquitous2.pdb", _
						"dotIRC.pdb", _
						"Content\*.*", _
						"web\*.*", _
						"Ubiquitous2.application", _
						"Ubiquitous2.exe.*", _
						"Ubiquitous2.exe", _
						"web\*.*", _
						"database\*.*")

Set oWinRar = new WinRAR

For Each file in UbiquitousFiles
	oWinRar.AddFile file
Next
oWinRar.Archive "ubiquitous2.zip"
						
oFTP.Connect InputBox("User name:"), InputBox("Password:"), "just24.justhost.com"
oFTP.Copy "ubiquitous2.zip", "/public_html/apps/"
oFTP.Copy "..\..\..\Ubiquitous2PluginInstaller\bin\Release\Ubiquitous2PluginInstaller.exe", "/public_html/apps/ubiquitous2obs/"

MsgBox "Press OK when upload will finish"

Class FTP	
	Dim oApp, oFTP, oFSO
	Dim strUser, strPassword, strHost
	Dim isConnected
	Private Sub Class_Initialize
		Set oApp = CreateObject("Shell.Application")
		Set oFSO = CreateObject("Scripting.FileSystemObject")
		isConnected = False
	End Sub
	Private Sub Class_Terminate
		Set oFSO = Nothing
		Set oApp = Nothing
	End Sub
	
	Public Sub Connect( user, password, host)		
		strUser = user
		strPassword = password
		strHost = host
		isConnected = True
	End Sub
	
	Public Sub Copy( localPath, ftpFolder )
		Set oFTP = oApp.NameSpace( "ftp://" & strUser & ":" & strPassword & "@" & strHost & ftpFolder)
		If Not oFSO.FileExists(localPath) Then: Exit Sub
		
		Set oLocalFile = oFSO.getFile(localPath)
		strFolder = oLocalFile.ParentFolder
		Set oLocalFolder = oApp.NameSpace(strFolder)
		Set oLocalItem = oLocalFolder.ParseName(oLocalFile.Name )
		oFTP.CopyHere oLocalItem, 16
	End Sub
End Class


Class WinRAR
	Dim tmpFile, fileList, move_,oShell, archiveFilePath_, objFS
	Private Sub Class_Initialize
	    Set oShell = CreateObject("WScript.Shell")
		Set tmpFile = New TempFile
		Set fileList = New BinaryFile
		Set objFS = New FileSystem
		archiveFilePath_ = ""
		fileList.Charset = "ascii"
	End Sub
	Private Sub Class_Terminate
		Set oShell = Nothing
		Set tmpFile = Nothing
		Set fileList = Nothing
		If archiveFilePath_ <> "" Then: objFS.DeleteFile archiveFilePath_
		Set objFS = Nothing
		Set tmpFile = Nothing
	End Sub
	Public Property Let Move(value)
		move_ = value
	End Property
	Public Property Get Move()
		Move = move_
	End Property
	Public Function AddFile( filePath )
		AddFile = False
		'On Error Resume Next
		fileList.WriteLine tmpFile.FullPath, filePath
		If Err.Number<>0 Then: Exit Function
		AddFile = True
	End Function
	Public Function Archive( archivePath )
		Archive = False
		'On Error Resume Next
		archiveFilePath_ = archivePath
		fileList.Close
		If move_ Then
			result = oShell.Run( "cmd /c start """" /w winrar m -m5 -IBCK """ & archivePath & """ @" & tmpFile.FullPath, 0, 1 )
		Else 
			result = oShell.Run( "cmd /c start """" /w winrar a -m5 -IBCK """ & archivePath & """ @" & tmpFile.FullPath, 0, 1 )	
		End if
		
		If Err.Number <> 0 Or result <> 0 Then: Exit Function
		Archive = True
	End Function
End Class

Class TempFile
	Private objFS_
	Private objApp_
	Private filename_
	Private fullpath_
	Private Sub Class_Initialize
		Set objFS_ = New FileSystem
		Set objApp_ = New Application
		Set fso_ = objFS_.fso
		filename_ = "tmp_" & fso_.GetTempName()
		fullpath_ = objApp_.Env("TEMP")  & "\" & filename_
	End Sub
	Private Sub Class_Terminate
		objFS_.DeleteFile fullpath_
		Set fso_ = Nothing
		Set objFS_ = Nothing
		Set objApp_ = Nothing
	End Sub
	Public  Property Get FullPath()
		FullPath = fullpath_
	End Property
	Public  Property Get FileName()
		FileName = filename_
	End Property
End Class

Const ForReading = 1, ForWriting = 2, ForAppending = 8
Const adModeRead = 1, adModeWrite = 2, adModeReadWrite = 3, adTypeText = 2, adTypeBinary = 1
Const adCmdStoredProc = 4, adParamInput = 1, adExecuteNoRecords = 128, adVarChar = 200, adLongVarBinary = 205
Class BinaryFile

   Private opened_
   Private filename_
   Private file_
   Private binfile_
   Private mode_
   Private fso_
   Private objFS_
   Private charset_
   Private lineSeparator_
   
   Private Sub Class_Initialize
	  Set objFS_ = New FileSystem
	  Set fso_ = objFS_.fso
	  charset_ = "ascii"
	  Set file_ = CreateObject("ADODB.Stream")
	  Set binfile_ = CreateObject("ADODB.Stream")
	  opened_ = False
	  lineSeparator_ = 10
   End Sub
   
   Private Sub Class_Terminate
      If Opened() Then:	Close
      Set fso_ = Nothing
	  Set file_ = Nothing
	  Set binfile_ = Nothing
	  Set objFS_ = Nothing
   End Sub
   Public Property Let LineSeparator( value )
		lineSeparator_ = value
   End Property
   Public Property Get Opened()
      Opened = opened_
   End Property

   Public Property Let Charset(value)
		charset_ = value
   End Property
   
   Public Function ReadText(filename,charset)
      ReadText = ""
	  If fso_.FileExists( filename ) Then
		  If Not opened_ or filename <> filename_ Then: Open filename, adModeRead,charset
		  If opened_ Then
			 file_.LoadFromFile filename
			 ReadText = file_.ReadText()
		  End If
	  End If
   End Function
   
   Public Function ReadBinary(filename)
		ReadBinary = ""	
		If fso_.FileExists( filename ) Then	
			If Not opened_ or filename <> filename_ Then: OpenBinary filename, adModeRead
			If opened_ Then
				file_.LoadFromFile filename
				ReadBinary = file_.Read(-1)
			End If
		End If
   End Function
   
   Public Function ReadLine(filename,charset)
      ReadLine = ""
	  If fso_.FileExists( filename ) Then
		  If Not opened_ or filename <> filename_ Then: Open filename, adModeRead,charset
		  If opened_ Then
			 file_.LoadFromFile filename
			 file_.LineSeparator = lineSeparator_
			 ReadLine = file_.ReadText(-2)
		  End If
	  End If
   End Function
   
   Public Function Close
	  'On Error Resume Next
	  Err.Clear
	  Close = False
	  Err.Source = "Close binary file" & filename_
	  If opened_ Then 
		If mode_ = adModeWrite Then
			objFS_.DeleteFile filename_
			binfile_.Mode = adModeReadWrite
			binfile_.Type = adTypeBinary
			binfile_.Open
			file_.Position = 0
			If charset_ = "utf-8" Then: file_.Position = 3
			file_.CopyTo binfile_
			binfile_.SaveToFile filename_
			binfile_.Close
		End If
		file_.Close
        opened_ = False
      End If
	  If Err.Number <> 0 Then: Exit Function
	  Close = True
   End Function 
   Private Function Open(filename,mode,charset)
		If filename <> filename_ Then: Close
		Open = True
		If Not opened_ Then
			charset_ = charset
			file_.Mode = adModeReadWrite
			file_.Type = adTypeText
			file_.Charset = charset_
			file_.Open
			mode_ = mode
		End If
		If Err.Number<>0 Then
			Open = False
			opened_ = False            
		Else
			opened_ = True
			filename_ = filename
		End If
   End Function

   Private Function OpenBinary(filename,mode)
		If filename <> filename_ Then: Close
		OpenBinary = True
		If Not opened_ Then
			file_.Mode = adModeReadWrite
			file_.Type = adTypeBinary
			file_.Open
			mode_ = mode
		End If
		If Err.Number<>0 Then
			OpenBinary = False
			opened_ = False            
		Else
			opened_ = True
			filename_ = filename
		End If
   End Function
   
   Public Sub WriteLine( filename, line )
      If Not opened_ Or filename <> filename_ Then: Open filename, adModeWrite, charset_
      If opened_ Then: file_.WriteText line & vbCrLf
   End Sub

   Public Sub WriteBinary( filename, content )
      If Not opened_ Or filename <> filename_ Then: OpenBinary filename, adModeRead
	  If opened_ Then: file_.Write content
	  file_.Flush
	  file_.SaveToFile filename
	  file_.Close
	  opened_ = false
   End Sub
   
   Public Function Size()
		If opened_ Then
			Size = file_.Size
		Else
			Size = 0
		End If
   End Function
   
End Class
Class Application
	Private WshNetwork
	Private oShell
	Private objWMIService
	Private Sub Class_Initialize
		Set WshNetwork = CreateObject("WScript.Network")
		Set objWMIService = GetObject("winmgmts:\\.\root\cimv2")
		Set oShell = CreateObject("WScript.Shell")
	End Sub
	Private Sub Class_Terminate
		Set WshNetwork = Nothing
		Set objWMIService = Nothing
		Set oShell = Nothing
	End Sub
	Public Function isRunning()
		wscrCount = FindProcess( "%wscript%" & WScript.ScriptName & "%" )
		If  wscrCount > 1 Then
			isRunning = True
		Else
			isRunning = False
		End If
	End Function
	Public Function FindProcess(likestr)
		Set objWMIService = GetObject("winmgmts:\\.\root\CIMV2")
		Set colItems = objWMIService.ExecQuery("SELECT Name,CommandLine FROM Win32_Process WHERE CommandLine Like '" & likestr & "'")
		FindProcess = colItems.Count
	End Function   
	Public Function RunAsUser
		RunAsUser = WshNetwork.UserName
	End Function
	Public Function LoggedInUser
		Set colItems = objWMIService.ExecQuery("Select * from Win32_ComputerSystem",,48)
		LoggedInUser = ""
		For Each objItem in colItems
			LoggedInUser = objItem.UserName
			Exit Function
		Next	  
	End Function
	Public Function GetSystemID()
		Set OpSysSet = GetObject("winmgmts:\root\cimv2").ExecQuery("select * from Win32_ComputerSystem")
		For each i in OpSysSet
			GetSystemID = i.Name
		Next
	End Function
	Public Function Ping(sComputer)
		errorCode = oShell.Run("ping -n 1 -w 1500 " & sComputer, 0, True)
		If errorCode <> 0 Then
			Ping = False
		Else
			Ping = True
		End If
	End Function
	Public Function Run( cmd,visible,waitonreturn )
		'On Error Resume Next
		Run = false
		result = oShell.Run( cmd,visible,waitonreturn )
		If result = 0 Then Run = true
	End Function
	Public Function Env( name )
		Env = ""
		If isObject(WScript) Then: Env = oShell.ExpandEnvironmentStrings("%" + name + "%")
	End Function

End Class

Class FileSystem
	Private fso_, WshShell_
	Private Sub Class_Initialize
		Set fso_ = CreateObject("Scripting.FileSystemObject")
		Set WshShell_ = CreateObject("Wscript.Shell")
	End Sub
	Private Sub Class_Terminate
		Set fso_ = Nothing
		Set WShShell_ = Nothing
	End Sub
	Public Property Get fso
		Set fso = fso_
	End Property
	Public Sub SetCurrentDir( folder )
		WshShell_.CurrentDirectory = folder
	End Sub
	Public Function GetParentFolder( filename )
		GetParentFolder = fso_.GetParentFolderName(filename)
	End Function
	Public Function GetFileName( path )
		GetFileName = fso_.GetFileName(path)
	End Function
	Public Function GetWorkDir
		GetWorkDir = ".\"
		If isObject(WScript) Then: GetWorkDir = GetParentFolder(WScript.ScriptFullName)
	End Function
	Public Function GetLogFileName
		GetLogFileName = "pumatools.log"
		If isObject(WScript) Then: GetLogFileName = Left(WScript.ScriptName,InStr(WScript.ScriptName,".")-1) & ".log"
	End Function
	Public Function Run( cmd, show,wait )
		Run = WshShell_.Run(cmd,show,wait)
	End Function   
	Public Function FilesExist ( folder, extension )
		Set f = fso_.GetFolder(folder)
		Set fc = f.Files
		FilesExist = False
		For Each file In fc
			If LCase(Right(file,Len(extension))) = LCase(extension) Then
			FilesExist = True
			Exit For
			End If
		Next
	End Function
	Public Function FileSize( filename )
		If fso_.FileExists( filename ) Then
			Set file = fso_.GetFile( filename )
			FileSize = file.Size
		Else
			FileSize = 0
		End If
	End Function
	Public Function CopyFile( src, dst )
		'On Error Resume next
		CopyFile = false
		Err.Source = "FILESYSTEM.CopyFile"
		If fso_.FileExists( dst ) Then
			fso_.DeleteFile dst
		End If
		fso_.CopyFile src, dst		
		If Err.Number = 0 Then: CopyFile = true
	End Function
	Public Function FileList(folderspec,ext)
		Dim fileList_
		ReDim fileList_(0)
		If Not fso.FolderExists(folderspec) Then
			Exit Function
		End If
		Set folder = fso.GetFolder(folderspec)
		Set files = folder.Files
		For Each fn In files
			If LCase(Right(fn,Len(ext))) = LCase(ext) Then
				fileList_(UBound(fileList_)) = fn
				ReDim Preserve fileList_(UBound(fileList_)+1)
			End If
		Next
		ReDim Preserve fileList_(UBound(fileList_)-1)
		FileList = fileList_
	End Function
	Public Function FileList2(folderspec,prefix)
		Dim fileList_
		ReDim fileList_(0)
		If Not fso.FolderExists(folderspec) Then
			Exit Function
		End If
		Set folder = fso.GetFolder(folderspec)
		Set files = folder.Files
		For Each fn In files
			If LCase(Mid(fn,Len(folderspec)+1,Len(prefix))) = LCase(prefix) Then
				fileList_(UBound(fileList_)) = fn
				ReDim Preserve fileList_(UBound(fileList_)+1)
			End If
		Next
		ReDim Preserve fileList_(UBound(fileList_)-1)
		FileList2 = fileList_
	End Function
	Public Function LinesCount( filename )
		LinesCount = -1
		If fso_.FileExists(filename) Then
			Set file = fso_.OpenTextFile( filename, ForReading )
			file.Skip fso_.GetFile(filename).Size
			LinesCount = file.Line
			file.Close
		End If
	End Function
	Public Function DeleteFile( filename )
		Err.Source = "FILESYSTEM.DeleteFile"
		If fso_.FileExists(filename) Then
			fso_.DeleteFile filename
		End If
	End Function
	Public Function MoveFile( filename_, dstfolder )
		If fso_.FileExists( filename_ ) Then
			dstfilename_ = fso_.GetFile(filename_).Name
		If Not fso_.FileExists( dstfolder & "\" & dstfilename_ ) Then
			If fso_.FolderExists( dstfolder ) Then
				fso_.MoveFile filename_, dstfolder
			End If
			Else
				fso_.DeleteFile filename_
			End If
		End If
	End Function
End Class
