using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Access4u;

public class ExampleCSharp {
	private static Unicorn.ConnectionType connectionType;
	private static bool opened=false;
	private static byte received=0;

	private static int Connected() {
		Console.WriteLine("Connected callback called");
		return 0;
	}

	private static int Disconnected(int dwDisconnectCode) {
		Console.WriteLine("Disconnected callback called with dwDisconnectCode {0}",dwDisconnectCode);
		return 0;
	}

	private static int Terminated() {
		Console.WriteLine("Terminated callback called");
		return 0;
	}

	private static int OnNewChannelConnection() {
		Console.WriteLine("OnNewChannelConnection callback called");
		opened=true;
		return 0;
	}

	private static int OnDataReceived(int cbSize, IntPtr pBuffer) {
		string buffer=Marshal.PtrToStringUni(pBuffer,cbSize);
		Console.WriteLine("OnDataReceived called with data '{0}' of size {1}",buffer,cbSize);
		received++;
		return 0;
	}

	private static int OnReadError(int dwErrorCode) {
		Console.WriteLine("OnReadError callback called with dwErrorCode {0}",dwErrorCode);
		opened=false;
		return 0;
	}

	private static int OnClose() {
		Console.WriteLine("OnClose callback called");
		opened=false;
		return 0;
	}

	public static int Main(string[] args) {
		if (args.Length<1) {
			Console.WriteLine("Error: This application requires a command line argument");
			return 1;
		}
		if (!Unicorn.ConnectionType.TryParse(args[0], out connectionType)) {
			Console.WriteLine("Error: command line argument for connection type must either be 0 for server and 1 for client mode");
			return 1;
		}
		Console.WriteLine("ACCESS4U UnicornDVC console test application");
		Unicorn.D_Connected _Connected=new Unicorn.D_Connected(Connected);
		Unicorn.D_Disconnected _Disconnected=new Unicorn.D_Disconnected(Disconnected);
		Unicorn.D_Terminated _Terminated=new Unicorn.D_Terminated(Terminated);
		Unicorn.D_OnNewChannelConnection _OnNewChannelConnection=new Unicorn.D_OnNewChannelConnection(OnNewChannelConnection);
		Unicorn.D_OnDataReceived _OnDataReceived=new Unicorn.D_OnDataReceived(OnDataReceived);
		Unicorn.D_OnReadError _OnReadError=new Unicorn.D_OnReadError(OnReadError);
		Unicorn.D_OnClose _OnClose=new Unicorn.D_OnClose(OnClose);


		Unicorn.SetCallbacks(connectionType, Connected,Disconnected,Terminated,OnNewChannelConnection,OnDataReceived,OnReadError,OnClose);
		Console.WriteLine("First, try to initialize the library in {0} mode", connectionType);
		int res =Unicorn.Initialize(connectionType);
		if (res!=0) {
			Console.WriteLine("Initialize failed with status code {0}",res);
			return res;
		}
		Console.WriteLine("Initialize succeeded, now trying to open the virtual channel.");
		for (byte i=0; i<10; i++) {
			Console.WriteLine("Attempt {0}",i+1);
			res=Unicorn.Open(connectionType);
			if (res!=0) {
				if (i==9) {
					Console.WriteLine("Open definitely failed with status code {0}",res);
					return res;
				}
				Console.WriteLine("Open failed with status code {0}",res);
				Thread.Sleep(2500);
				continue;
			}
			Console.WriteLine("Open succeeded");
			break;
		}
		if (!opened) {
			Console.WriteLine("We must wait for the OnNewChannelConnection callback to be called");
			for (byte i=0; i<10; i++) {
				if (i==9) {
					Console.WriteLine("OnNewChannelConnection call took too long");
					return 1;
				}
				Console.WriteLine("Waiting for OnNewChannelConnection call {0}/10",i+1);
				Thread.Sleep(1000);
				if (opened) {
					break;
				}
			}
		}
		Console.WriteLine("Sending and receiving pieces of data asynchronously");
		string[] strings={
			"(Do!) doe, a deer, a female deer",
			"(Re!) ray, a drop of golden sun",
			"(Mi!) me, a name I call myself",
			"(Fa!) far, a long, long way to run",
			"(So!) sew, a needle pulling thread",
			"(La!) la, a note to follow so",
			"(Ti!) tea, a drink with jam and bread",
			"That will bring us back to do oh oh oh"
		};
		for (byte i=0; i<strings.Length; i++) {
			Console.WriteLine("Writing '{0}'",strings[i]);
			byte[] buffer=Encoding.Unicode.GetBytes(strings[i]);
			res=Unicorn.Write(connectionType,buffer.Length,buffer);
			if (res!=0) {
				Console.WriteLine("Writing {0} failed with status code {1}",strings[i],res);			
				return 1;
			}
		}
		while (received<strings.Length && opened) {
			Console.WriteLine("Waiting for data, {0} chunks received",received);
			Thread.Sleep(4000);
		}
		Console.WriteLine("We are ready.");
		if (connectionType==Unicorn.ConnectionType.client) { // Client
			for (byte i=0; i<10; i++) {	
				if (!opened || i==9) {
					break;
				}
				Console.WriteLine("Waiting for the channel to be closed from the server...");
				Thread.Sleep(4000);
			}
		}
		if (opened) {
			Console.WriteLine("closing channel...");
			res=Unicorn.Close(connectionType);
			if (res!=0) {
				Console.WriteLine("Close failed with status code {0}",res);
			}
		}
		Console.WriteLine("Terminating library...");
		res=Unicorn.Terminate(connectionType);
		if (res!=0) {
			Console.WriteLine("Terminate failed with status code {0}",res);
			return res;
		}
		Console.WriteLine("All done, have a nice day!");
		return 0;
	}

}