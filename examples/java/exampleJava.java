import com.access4u.unicorn.Unicorn;
import com.sun.jna.Pointer;
import java.nio.charset.StandardCharsets;

public class ExampleJava {
	static byte connectionType;
	static boolean opened=false;
	static int received=0;

	static Unicorn.cConnected Connected = new Unicorn.cConnected() {
		public int invoke() {
			System.out.println("Connected callback called");
			return 0;
		}
	};

	static Unicorn.cDisconnected Disconnected= new Unicorn.cDisconnected() {
		public int invoke(int dwDisconnectCode) {
			System.out.println("Disconnected callback called with dwDisconnectCode "+dwDisconnectCode);
			return 0;
		}
	};

	static Unicorn.cTerminated Terminated = new Unicorn.cTerminated() {
		public int invoke() {
			System.out.println("Terminated callback called");
			return 0;
		}
	};

	static Unicorn.cOnNewChannelConnection OnNewChannelConnection = new Unicorn.cOnNewChannelConnection() {
		public int invoke() {
			System.out.println("OnNewChannelConnection callback called");
			opened=true;
			return 0;
		}
	};

	static Unicorn.cOnDataReceived OnDataReceived = new Unicorn.cOnDataReceived() {
		public int invoke(int cbSize, Pointer pBuffer) {
			String buffer=pBuffer.getWideString(0);
			System.out.println("OnDataReceived called with data '"+buffer+"' of size "+cbSize);
			received++;
			return 0;
		}
	};

	static Unicorn.cOnReadError OnReadError = new Unicorn.cOnReadError() {
		public int invoke(int dwErrorCode) {
			System.out.println("OnReadError callback called with dwErrorCode "+dwErrorCode);
			opened=false;
			return 0;
		}
	};

	static Unicorn.cOnClose OnClose = new Unicorn.cOnClose() {
		public int invoke() {
			System.out.println("OnClose callback called");
			opened=false;
			return 0;
		}
	};

	public static void main(String[] args) {
		if (args.length<2) {
			System.out.println("Error: This application requires two command line arguments");
			return;
		}
		connectionType=Byte.parseByte(args[0]);
		if (!(connectionType==0 || connectionType==1)) {
			System.out.println("Error: command line argument for connection type must be 0 for server and 1 for client mode");
			return;
		}
		System.out.println("ACCESS4U UnicornDVC console test application");
		Unicorn.setCallbacks(connectionType,Connected,Disconnected,Terminated,OnNewChannelConnection,OnDataReceived,OnReadError,OnClose);
		System.out.println("First, try to initialize the library in "+(connectionType==1 ? "client" : "server")+" mode");
		int res = Unicorn.initialize(connectionType);
		if (res!=0) {
			System.out.println("Initialize failed with status code "+res);
			return;
		}
		System.out.println("Initialize succeeded, now trying to open the virtual channel.");
		for (int i=0; i<10; i++) {
			System.out.println("Attempt "+(i+1));
			res=Unicorn.open(connectionType);
			if (res!=0) {
				if (i==9) {
					System.out.println("Open definitely failed with status code "+res);
					return;
				}
				System.out.println("Open failed with status code "+res);
				try {
				Thread.sleep(2500);
				} catch(InterruptedException e) {
					System.out.println(e);
				}
				continue;
			}
			System.out.println("Open succeeded");
			break;
		}
		if (!opened) {
			System.out.println("We must wait for the OnNewChannelConnection callback to be called");
			for (int i=0; i<10; i++) {
				if (i==9) {
					System.out.println("OnNewChannelConnection call took too long");
					return;
				}
				System.out.println("Waiting for OnNewChannelConnection call "+(i+1)+"/10");
				try {
					Thread.sleep(1000);
				} catch(InterruptedException e) {
					System.out.println(e);
				}
				if (opened) {
					break;
				}
			}
		}
		System.out.println("Sending and receiving pieces of data asynchronously");
		String[] strings={
			"(Do!) doe, a deer, a female deer",
			"(Re!) ray, a drop of golden sun",
			"(Mi!) me, a name I call myself",
			"(Fa!) far, a long, long way to run",
			"(So!) sew, a needle pulling thread",
			"(La!) la, a note to follow so",
			"(Ti!) tea, a drink with jam and bread",
			"That will bring us back to do oh oh oh"
		};
		for (int i=0; i<strings.length; i++) {
			System.out.println("Writing '"+strings[i]+"'");
			byte[] buffer=strings[i].getBytes(StandardCharsets.UTF_8);
			res=Unicorn.write(connectionType,buffer.length,buffer);
			if (res!=0) {
				System.out.println("Writing "+strings[i]+" failed with status code "+res);
				return;
			}
		}
		while (received<strings.length && opened) {
			System.out.println("Waiting for data, "+received+" chunks received");
			try {
				Thread.sleep(4000);
			} catch(InterruptedException e) {
				System.out.println(e);
			}
		}
		System.out.println("We are ready.");
		if (connectionType==1) { // Client
			for (int i=0; i<10; i++) {	
				if (!opened || i==9) {
					break;
				}
				System.out.println("Waiting for the channel to be closed from the server...");
				try {
					Thread.sleep(4000);
				} catch(InterruptedException e) {
					System.out.println(e);
				}
			}
		}
		if (opened) {
			System.out.println("closing channel...");
			res=Unicorn.close(connectionType);
			if (res!=0) {
				System.out.println("Close failed with status code "+res);
			}
		}
		System.out.println("Terminating library...");
		res=Unicorn.terminate(connectionType);
		if (res!=0) {
			System.out.println("Terminate failed with status code "+res);
			return;
		}
		System.out.println("All done, have a nice day!");
	}

}