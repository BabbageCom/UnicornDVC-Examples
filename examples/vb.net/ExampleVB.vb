Imports System.Text
Imports System.Threading
Imports System.Runtime.InteropServices
Imports Access4u

Public Class ExampleCSharp
	Private Shared connectionType As Unicorn.ConnectionType
	Private Shared opened As Boolean = False
	Private Shared received As UInteger = 0

	Private Shared Function Connected() As Integer
		System.Console.WriteLine("Connected callback called")
		Return 0
	End Function

	Private Shared Function Disconnected(dwDisconnectCode As Integer) As Integer
		System.Console.WriteLine("Disconnected callback called with dwDisconnectCode {0}", dwDisconnectCode)
		Return 0
	End Function

	Private Shared Function Terminated() As Integer
		System.Console.WriteLine("Terminated callback called")
		Return 0
	End Function

	Private Shared Function OnNewChannelConnection() As Integer
		System.Console.WriteLine("OnNewChannelConnection callback called")
		opened = True
		Return 0
	End Function

	Private Shared Function OnDataReceived(cbSize As Integer, pBuffer As System.IntPtr) As Integer
		Dim buffer As String = Marshal.PtrToStringUni(pBuffer, cbSize)
		System.Console.WriteLine("OnDataReceived callsed with data '{0}' of size {1}", buffer, cbSize)
		received += 1
		Return 0
	End Function

	Private Shared Function OnReadError(dwErrorCode As Integer) As Integer
		System.Console.WriteLine("OnReadError callback called with dwErrorCode {0}", dwErrorCode)
		opened = False
		Return 0
	End Function

	Private Shared Function OnClose() As Integer
		System.Console.WriteLine("OnClose callback called")
		opened = False
		Return 0
	End Function

	Public Shared Function Main(args As String()) As Integer
		If args.Length < 1 Then
			System.Console.WriteLine("Error: This application requires a command line argument")
			Return 1
		End If
		If Not Unicorn.ConnectionType.TryParse(args(0), connectionType) Then
			System.Console.WriteLine("Error: command line argument for connection type must either be 0 for server and 1 for client mode")
			Return 1
		End If
		System.Console.WriteLine("ACCESS4U UnicornDVC console test application")
		Dim _Connected As New Unicorn.D_Connected(AddressOf Connected)
		Dim _Disconnected As New Unicorn.D_Disconnected(AddressOf Disconnected)
		Dim _Terminated As New Unicorn.D_Terminated(AddressOf Terminated)
		Dim _OnNewChannelConnection As New Unicorn.D_OnNewChannelConnection(AddressOf OnNewChannelConnection)
		Dim _OnDataReceived As New Unicorn.D_OnDataReceived(AddressOf OnDataReceived)
		Dim _OnReadError As New Unicorn.D_OnReadError(AddressOf OnReadError)
		Dim _OnClose As New Unicorn.D_OnClose(AddressOf OnClose)
		Unicorn.SetCallbacks(connectionType, AddressOf Connected, AddressOf Disconnected, AddressOf Terminated, AddressOf OnNewChannelConnection, AddressOf OnDataReceived, AddressOf OnReadError, _
			AddressOf OnClose)
		System.Console.WriteLine("First, try to initialize the library in {0} mode", (If(connectionType = 1, "client", "server")))
		Dim res As Integer = Unicorn.Initialize(connectionType)
		If res <> 0 Then
			System.Console.WriteLine("Initialize failed with status code {0}", res)
			Return res
		End If
		System.Console.WriteLine("Initialize succeeded, now trying to open the virtual channel.")
		For i As Byte = 0 To 9
			System.Console.WriteLine("Attempt {0}", i + 1)
			res = Unicorn.Open(connectionType)
			If res <> 0 Then
				If i = 9 Then
					System.Console.WriteLine("Open definitely failed with status code {0}", res)
					Return res
				End If
				System.Console.WriteLine("Open failed with status code {0}", res)
				Thread.Sleep(2500)
				Continue For
			End If
			System.Console.WriteLine("Open succeeded")
			Exit For
		Next
		If Not opened Then
			System.Console.WriteLine("We must wait for the OnNewChannelConnection callback to be called")
			For i As Byte = 0 To 9
				If i = 9 Then
					System.Console.WriteLine("OnNewChannelConnection call took too long")
					Return 1
				End If
				System.Console.WriteLine("Waiting for OnNewChannelConnection call {0}/10", i + 1)
				Thread.Sleep(1000)
				If opened Then
					Exit For
				End If
			Next
		End If
		System.Console.WriteLine("Sending and receiving pieces of data asynchronously")
		Dim strings As String() = {"(Do!) doe, a deer, a female deer", "(Re!) ray, a drop of golden sun", "(Mi!) me, a name I call myself", "(Fa!) far, a long, long way to run", "(So!) sew, a needle pulling thread", "(La!) la, a note to follow so", _
			"(Ti!) tea, a drink with jam and bread", "That will bring us back to do oh oh oh"}
		For i As Byte = 0 To strings.Length - 1
			System.Console.WriteLine("Writing '{0}'", strings(i))
			Dim buffer As Byte() = Encoding.Unicode.GetBytes(strings(i))
			res = Unicorn.Write(connectionType, buffer.Length, buffer)
			If res <> 0 Then
				System.Console.WriteLine("Writing {0} failed with status code {1}", strings(i), res)
				Return 1
			End If
		Next
		While received < strings.Length AndAlso opened
			System.Console.WriteLine("Waiting for data, {0} chunks received", received)
			Thread.Sleep(4000)
		End While
		System.Console.WriteLine("We are ready.")
		If connectionType = Unicorn.ConnectionType.Client Then
			For i As Byte = 0 To 9
				If Not opened OrElse i = 9 Then
					Exit For
				End If
				System.Console.WriteLine("Waiting for the channel to be closed from the server...")
				Thread.Sleep(4000)
			Next
		End If
		If opened Then
			System.Console.WriteLine("closing channel...")
			res = Unicorn.Close(connectionType)
			If res <> 0 Then
				System.Console.WriteLine("Close failed with status code {0}", res)
			End If
		End If
		System.Console.WriteLine("Terminating library...")
		res = Unicorn.Terminate(connectionType)
		If res <> 0 Then
			System.Console.WriteLine("Terminate failed with status code {0}", res)
			Return res
		End If
		System.Console.WriteLine("All done, have a nice day!")
		Return 0
	End Function

End Class
