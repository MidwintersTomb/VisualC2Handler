Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading

Module VC2Handler

    ' Import the FlashWindow function from the User32.dll library
    <DllImport("User32.dll")>
    Public Function FlashWindow(ByVal hWnd As IntPtr, ByVal bInvert As Boolean) As Boolean
    End Function

    Dim sessions As New List(Of String)() From {"Connection Log"} ' List of sessions
    Dim currentSession As String = "Connection Log" ' Default session
    Dim selectedSessionIndex As Integer = 0 ' Selected session index
    Dim isSessionMenuActive As Boolean = False ' Flag to track session menu state
    Dim sessionBuffers As New Dictionary(Of String, List(Of String))() ' Buffer to store inputs for each session
    Dim listener As TcpListener ' TCP listener for accepting connections
    Dim clientSessions As New Dictionary(Of TcpClient, String)() ' Mapping of clients to sessions
    Dim sessionNameColors As New Dictionary(Of String, ConsoleColor)() ' Dictionary to store session name colors
    Dim sessionDirectory As New Dictionary(Of String, String)() ' Dictionary to map session name to display name


    Sub AddToConnectionLog(message As String)
        Dim connectionLogSession As String = "Connection Log"
        If Not sessionBuffers.ContainsKey(connectionLogSession) Then
            sessionBuffers.Add(connectionLogSession, New List(Of String)())
        End If
        sessionBuffers(connectionLogSession).Add(message)
    End Sub

    Sub FlashConsoleWindow()
        Dim hWnd As IntPtr = Process.GetCurrentProcess().MainWindowHandle
        FlashWindow(hWnd, True)
    End Sub

    Sub Main(args As String())
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.Green
        Console.Write(" __      ___                 _ ")
        Console.ForegroundColor = ConsoleColor.Red
        Console.Write(" _____ ___   ")
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.Write(" _    _                 _ _           ")
        Console.WriteLine()
        Console.ForegroundColor = ConsoleColor.Green
        Console.Write(" \ \    / (_)               | |")
        Console.ForegroundColor = ConsoleColor.Red
        Console.Write("/ ____|__ \  ")
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.Write("| |  | |               | | |          ")
        Console.WriteLine()
        Console.ForegroundColor = ConsoleColor.Green
        Console.Write("  \ \  / / _ ___ _   _  __ _| |")
        Console.ForegroundColor = ConsoleColor.Red
        Console.Write(" |       ) | ")
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.Write("| |__| | __ _ _ __   __| | | ___ _ __ ")
        Console.WriteLine()
        Console.ForegroundColor = ConsoleColor.Green
        Console.Write("   \ \/ / | / __| | | |/ _` | |")
        Console.ForegroundColor = ConsoleColor.Red
        Console.Write(" |      / /  ")
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.Write("|  __  |/ _` | '_ \ / _` | |/ _ \ '__|")
        Console.WriteLine()
        Console.ForegroundColor = ConsoleColor.Green
        Console.Write("    \  /  | \__ \ |_| | (_| | |")
        Console.ForegroundColor = ConsoleColor.Red
        Console.Write(" |____ / /_  ")
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.Write("| |  | | (_| | | | | (_| | |  __/ |   ")
        Console.WriteLine()
        Console.ForegroundColor = ConsoleColor.Green
        Console.Write("     \/   |_|___/\__,_|\__,_|_|")
        Console.ForegroundColor = ConsoleColor.Red
        Console.Write("\_____|____| ")
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.Write("|_|  |_|\__,_|_| |_|\__,_|_|\___|_|   ")
        Console.WriteLine()
        Console.ForegroundColor = ConsoleColor.White
        Console.WriteLine("==================================================================================")
        Console.ForegroundColor = ConsoleColor.Gray
        Console.WriteLine("         Coded entirely in VisUwU :3asic.Net (Are you happy now, Astrid?)")
        Console.ForegroundColor = ConsoleColor.White
        Console.WriteLine("==================================================================================")
        Console.WriteLine("Usage:")
        Console.WriteLine("VisualC2.exe -l <port>")
        Console.WriteLine()
        Console.WriteLine("              Change session:  vc2.sessions")
        Console.WriteLine("       Rename active session:  vc2.rename ""new session name""")
        Console.WriteLine(" Save active session to disk:  vc2.save")
        Console.WriteLine("        Close active session:  vc2.close")
        Console.WriteLine("               Quit VisualC2:  vc2.exit")
        Console.WriteLine()
        Console.WriteLine("    Why did the window flash?  Client connect/disconnect notification")
        Console.Write("      Why is a ")
        Console.ForegroundColor = ConsoleColor.Green
        Console.Write("session green")
        Console.ForegroundColor = ConsoleColor.White
        Console.Write("?  The session is new")
        Console.WriteLine()
        Console.Write("       Why Is a ")
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.Write("session cyan")
        Console.ForegroundColor = ConsoleColor.White
        Console.Write("?  The session has data waiting for you")
        Console.WriteLine()
        Console.Write("        Why Is a ")
        Console.ForegroundColor = ConsoleColor.Red
        Console.Write("session red")
        Console.ForegroundColor = ConsoleColor.White
        Console.Write("?  The session is disconnected")
        Console.WriteLine()
        Console.WriteLine("==================================================================================")
        Console.WriteLine()
        Console.ForegroundColor = ConsoleColor.Gray
        Console.WriteLine("Press any key To Continue...")
        Console.ReadKey()

        Console.Clear()

        Dim listenPort As Integer = GetListenPortFromArgs(args)
        If listenPort = 0 Then
            Console.WriteLine("Invalid port number. Exiting...")
            Return
        End If
        If listenPort < 0 Then
            Console.WriteLine("Invalid port number. Exiting...")
            Return
        End If
        If listenPort > 65535 Then
            Console.WriteLine("Invalid port number. Exiting...")
            Return
        End If

        ' Initialize the session name colors
        For Each session As String In sessions
            sessionNameColors.Add(session, ConsoleColor.Gray)
        Next
        sessionNameColors("Connection Log") = ConsoleColor.Gray ' Set the color of the Connection Log session to gray
        sessionNameColors("Menu") = ConsoleColor.Gray ' Set the color of the Menu session to gray

        ListenForConnections(listenPort)

        While True
            ' Check if the session menu is active
            If isSessionMenuActive Then
                currentSession = "Menu"
                If Not sessionDirectory.ContainsKey("Menu") Then
                    sessionDirectory.Add("Menu", "Menu")
                End If
                ' Set the Window title
                Console.Title = "Select A SESSION"
                ' Read a key from the console without displaying it
                Dim keyInfo As ConsoleKeyInfo = Console.ReadKey(True)

                Select Case keyInfo.Key
                    Case ConsoleKey.UpArrow
                        ' Move the selected session index up if it's not at the first session
                        If selectedSessionIndex > 0 Then
                            selectedSessionIndex -= 1
                            Console.SetCursorPosition(0, Console.CursorTop - 1) ' Move the cursor to the previous line
                            PrintSessionList() ' Update and print the session list
                        End If

                    Case ConsoleKey.DownArrow
                        ' Move the selected session index down if it's not at the last session
                        If selectedSessionIndex < sessions.Count - 1 Then
                            selectedSessionIndex += 1
                            Console.SetCursorPosition(0, Console.CursorTop - 1) ' Move the cursor to the previous line
                            PrintSessionList() ' Update and print the session list
                        End If

                    Case ConsoleKey.Enter
                        ' Update the current session with the selected session
                        currentSession = sessions(selectedSessionIndex)
                        Console.Title = currentSession.ToUpper() ' Set the console title to match the selected session
                        Console.Clear()
                        isSessionMenuActive = False ' Deactivate the session menu
                        PrintSessionBuffer(currentSession) ' Print the buffer of the selected session

                End Select
            Else
                ' Read the input from the console when the session menu is not active
                Dim input As String = Console.ReadLine()

                If input.StartsWith("vc2.rename", StringComparison.OrdinalIgnoreCase) AndAlso currentSession <> "Connection Log" Then
                    Dim startIndex As Integer = input.IndexOf("""") + 1
                    Dim endIndex As Integer = input.LastIndexOf("""")
                    If startIndex >= 0 AndAlso endIndex >= 0 AndAlso endIndex > startIndex Then
                        Dim newName As String = input.Substring(startIndex, endIndex - startIndex)

                        ' Get key value for color settings
                        Dim value As String = sessionNameColors(currentSession)

                        If sessionDirectory.ContainsKey(currentSession) Then
                            sessionDirectory(currentSession) = newName
                        End If

                        ' Update the console title with the new name
                        Console.Title = newName.ToUpper()
                    End If

                ElseIf input.Equals("vc2.sessions", StringComparison.OrdinalIgnoreCase) Then
                    currentSession = "Menu"
                    isSessionMenuActive = True ' Activate the session menu
                    Console.Clear()
                    PrintSessionList()
                ElseIf input.Equals("vc2.close", StringComparison.OrdinalIgnoreCase) Then
                    If currentSession <> "Connection Log" Then
                        Dim client As TcpClient = GetClientBySession(currentSession)
                        If client Is Nothing Then
                            ' Doing nothing here as if client is null then it is already disconnected
                        Else
                            Dim stream As NetworkStream = client.GetStream()
                            Dim data As Byte() = Encoding.ASCII.GetBytes("exit" & vbLf)
                            stream.Write(data, 0, data.Length)
                            Thread.Sleep(100)
                            client.Close()
                            Thread.Sleep(100)
                        End If
                        sessionBuffers.Remove(currentSession) ' Remove the buffer for the client's session
                        sessions.Remove(currentSession) ' Remove the session from the list of sessions
                        sessionDirectory.Remove(currentSession)
                        isSessionMenuActive = True ' Activate the session menu
                        Console.Clear()
                        selectedSessionIndex = 0
                        PrintSessionList()
                    End If
                ElseIf input.Equals("vc2.save", StringComparison.OrdinalIgnoreCase) Then
                    ' Save current session buffer to disk as a text file
                    SaveSessionBuffer(currentSession)
                ElseIf input.Equals("vc2.Exit", StringComparison.OrdinalIgnoreCase) Then
                    ' Send "Exit" to all active connections and gracefully close them
                    SendExitToAllConnections()
                    Exit While ' Exit the program
                ElseIf currentSession = "Connection Log" Then
                    ' Do nothing here so that entered commands do not store to the Connection Log buffer
                Else
                    ' Store the input in the buffer for the current session
                    If Not sessionBuffers.ContainsKey(currentSession) Then
                        sessionBuffers.Add(currentSession, New List(Of String)())
                    End If
                    sessionBuffers(currentSession).Add(input)

                    ' Send the input to the client associated with the current session
                    Dim client As TcpClient = GetClientBySession(currentSession)
                    If client IsNot Nothing Then
                        SendMessageToClient(client, input)
                    End If
                End If
            End If
        End While
    End Sub

    Function GetListenPortFromArgs(args As String()) As Integer
        If args.Length > 0 AndAlso args(0) = "-l" AndAlso args.Length > 1 Then
            If Integer.TryParse(args(1), GetListenPortFromArgs) Then
                Return GetListenPortFromArgs
            End If
        End If
        Return 0
    End Function

    Sub PrintSessionList()
        Console.Clear()
        Console.WriteLine("Available Sessions:")
        For i As Integer = 0 To sessions.Count - 1
            Dim sessionIndex As String = (i + 1).ToString() ' Get the session number
            Dim client As TcpClient = GetClientBySession(sessions(i))
            Dim ipAddress As String = If(client IsNot Nothing, DirectCast(client.Client.RemoteEndPoint, IPEndPoint).Address.ToString(), "Connection Log")
            Dim sessionName As String = sessionIndex & ": " & sessions(i) ' Combine session number and session name
            Dim sessionColor As ConsoleColor = sessionNameColors(sessions(i)) ' Get the color for the session name
            If i = selectedSessionIndex Then
                If i = 0 Then
                    Console.WriteLine("> " & sessionDirectory(sessions(i))) ' Prefix the selected session with ">"
                Else
                    Console.ForegroundColor = ConsoleColor.Gray
                    Console.Write("> ")
                    Console.ForegroundColor = sessionColor ' Set the session name color
                    Console.Write(sessionDirectory(sessions(i))) ' Prefix the selected session with ">"
                    Console.ForegroundColor = ConsoleColor.Gray
                    Console.WriteLine()
                End If
            Else
                If i = 0 Then
                    Console.WriteLine("  " & sessionDirectory(sessions(i))) ' Prefix the selected session with ">"
                Else
                    Console.ForegroundColor = ConsoleColor.Gray
                    Console.Write("  ")
                    Console.ForegroundColor = sessionColor ' Set the session name color
                    Console.Write(sessionDirectory(sessions(i))) ' Prefix the selected session with ">"
                    Console.ForegroundColor = ConsoleColor.Gray
                    Console.WriteLine()
                End If
            End If
        Next
    End Sub

    Sub PrintSessionBuffer(session As String)
        If sessionBuffers.ContainsKey(session) Then
            Dim clientSession As String = "Session " & (sessions.Count - 1).ToString() ' Assign a new session to the client
            Dim value As String = sessionNameColors(currentSession)
            If value = 12 Then
                Dim buffer As List(Of String) = sessionBuffers(session)
                For Each item As String In buffer
                    Console.WriteLine(item)
                Next
            Else
                sessionNameColors.Remove(currentSession)
                sessionNameColors.Add(currentSession, ConsoleColor.Gray) ' Set the color of the new session to gray
                Dim buffer As List(Of String) = sessionBuffers(session)
                For Each item As String In buffer
                    Console.WriteLine(item)
                Next
            End If
        Else
            sessionNameColors.Remove(currentSession)
            sessionNameColors.Add(currentSession, ConsoleColor.Gray) ' Set the color of the new session to gray
        End If
    End Sub

    Sub SaveSessionBuffer(session As String)
        If sessionBuffers.ContainsKey(session) Then
            Dim filename As String = sessionDirectory(currentSession)
            Dim pattern As String = "[\\/:*?""<>|]"
            Dim replacement As String = "-"
            Dim output As String = Regex.Replace(filename, pattern, replacement)
            ' Create a StreamWriter instance to write to a file
            Dim writer As New StreamWriter(output & ".txt")

            ' Write the variable contents to the file
            Dim buffer As List(Of String) = sessionBuffers(session)
            For Each item As String In buffer
                writer.Write(item & vbLf)
            Next

            ' Close the StreamWriter to release resources
            writer.Close()
        Else
            Dim filename As String = sessionDirectory(currentSession)
            Dim pattern As String = "[\\/:*?""<>|]"
            Dim replacement As String = "-"
            Dim output As String = Regex.Replace(filename, pattern, replacement)
            ' Create a StreamWriter instance to write to a file
            Dim writer As New StreamWriter(output & ".txt")

            ' Write the variable contents to the file
            Dim buffer As String = ""
            writer.Write(buffer)
            ' Close the StreamWriter to release resources
            writer.Close()
        End If
    End Sub

    Sub ListenForConnections(port As Integer)
        listener = New TcpListener(IPAddress.Any, port)
        listener.Start()
        currentSession = "Connection Log"
        sessionDirectory.Add("Connection Log", "Connection Log")
        Console.Title = "CONNECTION LOG"
        Console.WriteLine("Listening for connections on port " & port)

        Dim acceptThread As New Thread(AddressOf AcceptConnections)
        acceptThread.Start()
    End Sub

    Sub AcceptConnections()
        While True
            Dim client As TcpClient = listener.AcceptTcpClient()
            Dim clientSession As String = "Session " & (sessions.Count - 1).ToString() ' Assign a new session to the client

            ' Add connection log message to the Connection Log session
            Dim connectionLogMessage As String = "Client connected: " & client.Client.RemoteEndPoint.ToString()
            AddToConnectionLog(connectionLogMessage)

            If currentSession = "Connection Log" Then
                Console.WriteLine("Client connected: " & client.Client.RemoteEndPoint.ToString())
                sessionNameColors.Remove(clientSession)
                sessionNameColors.Add(clientSession, ConsoleColor.Green) ' Set the color of the new session to green
            End If

            If clientSession <> "Connection Log" Then
                sessions.Insert(1, clientSession) ' Add the new session after the Connection Log session
                clientSessions.Add(client, clientSession) ' Associate the client with the session
                sessionDirectory.Add(clientSession, clientSession) ' Add the client session and and default name of the session
            Else
                clientSessions.Add(client, currentSession) ' Associate the client with the current session
                sessionNameColors.Remove(clientSession)
                sessionNameColors.Add(clientSession, ConsoleColor.Green) ' Set the color of the new session to green
                sessionDirectory.Add(clientSession, clientSession) ' Add the client session and and default name of the session
            End If

            If isSessionMenuActive = True Then
                sessionNameColors.Remove(clientSession)
                sessionNameColors.Add(clientSession, ConsoleColor.Green) ' Set the color of the new session to green
                Console.Clear()
                PrintSessionList()
            End If

            FlashConsoleWindow()

            Dim clientThread As New Thread(AddressOf HandleClientCommunication)
            clientThread.Start(client)
        End While
    End Sub

    Sub HandleClientCommunication(client As Object)
        Dim tcpClient As TcpClient = DirectCast(client, TcpClient)
        Dim clientEndPoint As String = tcpClient.Client.RemoteEndPoint.ToString()
        Dim clientSession As String = clientSessions(tcpClient)
        Dim buffer(1024) As Byte
        Dim bytesRead As Integer

        While True
            Dim stream As NetworkStream = tcpClient.GetStream()

            Try
                bytesRead = stream.Read(buffer, 0, buffer.Length)

                If bytesRead = 0 Then
                    If currentSession = "Connection Log" Then
                        Console.WriteLine("Client disconnected: " & clientEndPoint)
                        sessionNameColors.Remove(clientSession)
                        sessionNameColors.Add(clientSession, ConsoleColor.Red) ' Set the color of the new session to red
                    End If
                    If isSessionMenuActive = True Then
                        sessionNameColors.Remove(clientSession)
                        sessionNameColors.Add(clientSession, ConsoleColor.Red) ' Set the color of the new session to red
                        Console.Clear()
                        PrintSessionList()
                    End If
                    If sessionBuffers.ContainsKey(clientSession) Then
                        sessionBuffers(clientSession).Add("Client disconnected: " & clientEndPoint)
                        sessionNameColors.Remove(clientSession)
                        sessionNameColors.Add(clientSession, ConsoleColor.Red) ' Set the color of the new session to red
                    End If
                    If clientSession = currentSession Then
                        If isSessionMenuActive = True Then
                            ' Do nothing
                        Else
                            Console.WriteLine("Client disconnected: " & clientEndPoint)
                            sessionNameColors.Remove(clientSession)
                            sessionNameColors.Add(clientSession, ConsoleColor.Red) ' Set the color of the new session to red
                        End If
                    End If
                    FlashConsoleWindow()
                    ' Add disconnection log message to the Connection Log session
                    Dim disconnectionLogMessage As String = "Client disconnected: " & clientEndPoint
                    AddToConnectionLog(disconnectionLogMessage)
                    clientSessions.Remove(tcpClient)
                    Exit While
                End If

                Dim data As String = Encoding.ASCII.GetString(buffer, 0, bytesRead)
                ' Process and print the received output
                Dim processedOutput As String = data.ToString()
                ' Remove "]0;" pattern
                processedOutput = Regex.Replace(processedOutput, "]0;", "")
                ' Remove "[\d+;\d+m.*?[\d+m\$" pattern
                processedOutput = Regex.Replace(processedOutput, "\[\d+;\d+m.*?\[\d+m\$", "")
                ' Store the data in the buffer for the client's session
                If Not sessionBuffers.ContainsKey(clientSession) Then
                    sessionBuffers.Add(clientSession, New List(Of String)())
                    sessionNameColors.Remove(clientSession)
                    If clientSession = currentSession Then
                        sessionNameColors.Add(clientSession, ConsoleColor.Gray) ' Set the color of the new session to cyan
                    Else
                        sessionNameColors.Add(clientSession, ConsoleColor.Cyan) ' Set the color of the new session to cyan
                    End If
                End If
                If currentSession = "Menu" Then
                    sessionBuffers(clientSession).Add(processedOutput)
                    sessionNameColors.Remove(clientSession)
                    sessionNameColors.Add(clientSession, ConsoleColor.Cyan) ' Set the color of the new session to cyan
                    Console.Clear()
                    PrintSessionList()
                End If
                sessionBuffers(clientSession).Add(processedOutput)
                If clientSession = currentSession Then
                    If Not isSessionMenuActive Then
                        Console.WriteLine(processedOutput)
                    End If
                End If
            Catch ex As IOException
                If currentSession = "Connection Log" Then
                    ' Handle the IOException (including the SocketException) here
                    Console.WriteLine("Client disconnected: " & clientEndPoint)
                    sessionNameColors.Remove(clientSession)
                    sessionNameColors.Add(clientSession, ConsoleColor.Red) ' Set the color of the new session to red
                End If
                If isSessionMenuActive = True Then
                    sessionNameColors.Remove(clientSession)
                    sessionNameColors.Add(clientSession, ConsoleColor.Red) ' Set the color of the new session to red
                    Console.Clear()
                    PrintSessionList()
                End If
                If sessionBuffers.ContainsKey(clientSession) Then
                    sessionBuffers(clientSession).Add("Client disconnected: " & clientEndPoint)
                    sessionNameColors.Remove(clientSession)
                    sessionNameColors.Add(clientSession, ConsoleColor.Red) ' Set the color of the new session to red
                End If
                If clientSession = currentSession Then
                    Console.WriteLine("Client disconnected: " & clientEndPoint)
                    sessionNameColors.Remove(clientSession)
                    sessionNameColors.Add(clientSession, ConsoleColor.Red) ' Set the color of the new session to red
                End If
                FlashConsoleWindow()
                ' Add disconnection log message to the Connection Log session
                Dim disconnectionLogMessage As String = "Client disconnected: " & clientEndPoint
                ' Add disconnection log message to the Connection Log session
                AddToConnectionLog(disconnectionLogMessage)
                clientSessions.Remove(tcpClient)
                Exit While
            End Try
        End While
    End Sub

    Function GetClientBySession(session As String) As TcpClient
        For Each kvp In clientSessions
            If kvp.Value = session Then
                Return kvp.Key
            End If
        Next
        Return Nothing
    End Function

    Sub SendMessageToClient(client As TcpClient, message As String)
        If client Is Nothing Then
            ' Do nothing, as if client is null then client is disconnected
        Else
            Dim stream As NetworkStream = client.GetStream()
            Dim data As Byte() = Encoding.ASCII.GetBytes(message & vbLf)
            stream.Write(data, 0, data.Length)
        End If
    End Sub

    Sub SendExitToAllConnections()
        For Each client As TcpClient In clientSessions.Keys
            SendMessageToClient(client, "exit" & vbLf)
        Next
        Environment.Exit(0)
    End Sub
End Module