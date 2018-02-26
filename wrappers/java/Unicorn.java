package com.access4u.unicorn;

import com.sun.jna.Callback;
import com.sun.jna.Library;
import com.sun.jna.Native;
import com.sun.jna.Memory;
import com.sun.jna.Pointer;

public final class Unicorn {
	public static void main(String[] args) {
		System.out.println("This class has no functionality by itself. Please consult the Java example code for details.");
	}
	public static interface cConnected extends Callback {
		int invoke();
	}
	public static interface cDisconnected extends Callback {
		int invoke(int dwDisconnectCode);
	}
	public static interface cTerminated extends Callback {
		int invoke();
	}
	public static interface cOnNewChannelConnection extends Callback {
		int invoke();
	}
	public static interface cOnDataReceived extends Callback {
		int invoke(int cbSize, Pointer pBuffer);
	}
	public static interface cOnReadError extends Callback {
		int invoke(int dwErrorCode);
	}
	public static interface cOnClose extends Callback {
		int invoke();
	}

	private static interface cLib extends Library {

		int Unicorn_Initialize(int connectionType);
		int Unicorn_SetLicenseKey(Pointer licenseKey, int activate, Pointer errorMessage);
		Pointer Unicorn_GetHardwareId();
		int Unicorn_Open(int connectionType);
		int Unicorn_Write(int connectionType, int cbSize, Pointer pBuffer);
		int Unicorn_Close(int connectionType);
		int Unicorn_Terminate(int connectionType);
		void Unicorn_SetCallbacks(
			int connectionType,
			cConnected _Connected,
			cDisconnected _Disconnected,
			cTerminated _Terminated,
			cOnNewChannelConnection _OnNewChannelConnection,
			cOnDataReceived _OnDataReceived,
			cOnReadError _OnReadError,
			cOnClose _OnClose
		);
	}

	private static cLib lib = (cLib)Native.loadLibrary("UnicornDVCAppLib.dll",cLib.class);

	// Prevent construction
	private Unicorn() {}

	public static int initialize(byte connectionType) { return lib.Unicorn_Initialize(connectionType); }
	public static int setLicenseKey(String licenseKey, boolean activate, Pointer pErrorMessage) {
		Pointer pLicenseKey=new Memory(Native.WCHAR_SIZE * (licenseKey.length() + 1));
		pLicenseKey.setWideString(0,licenseKey);
		return lib.Unicorn_SetLicenseKey(pLicenseKey, (activate) ? 1 : 0, pErrorMessage);
	}
	public static String getHardwareId() {
		Pointer pHardwareId = lib.Unicorn_GetHardwareId();
		return pHardwareId.getWideString(0);
	}
	public static int open(byte connectionType) { return lib.Unicorn_Open(connectionType); }
	public static int write(byte connectionType, int cbSize, byte[] buffer) { 
		Pointer pBuffer=new Memory(buffer.length);
		pBuffer.write(0,buffer,0,buffer.length);
		return lib.Unicorn_Write(connectionType, cbSize, pBuffer); 
	}
	public static int close(byte connectionType) { return lib.Unicorn_Close(connectionType); }
	public static int terminate(byte connectionType) { return lib.Unicorn_Terminate(connectionType); }
	public static void setCallbacks(
		byte connectionType,
		cConnected _Connected,
		cDisconnected _Disconnected,
		cTerminated _Terminated,
		cOnNewChannelConnection _OnNewChannelConnection,
		cOnDataReceived _OnDataReceived,
		cOnReadError _OnReadError,
		cOnClose _OnClose
	) {
		lib.Unicorn_SetCallbacks(connectionType,_Connected,_Disconnected,_Terminated,_OnNewChannelConnection,_OnDataReceived,_OnReadError,_OnClose);
	}

}
