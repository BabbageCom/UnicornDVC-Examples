using System;
using System.Runtime.InteropServices;

namespace Access4u {

	public sealed class Unicorn {
		public delegate int D_Connected();
		public delegate int D_Disconnected(int dwDisconnectCode);
		public delegate int D_Terminated();
		public delegate int D_OnNewChannelConnection();
		public delegate int D_OnDataReceived(int cbSize, IntPtr pBuffer);
		public delegate int D_OnReadError(int dwErrorCode);
		public delegate int D_OnClose();
		public enum ConnectionType { server=0, client=1 };

		[DllImport("UnicornDVCAppLib.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
			private static extern int Unicorn_Initialize(
				ConnectionType connectionType
			);
		[DllImport("UnicornDVCAppLib.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
			private static extern int Unicorn_SetLicenseKey(
				[MarshalAs(UnmanagedType.LPWStr)]String licenseKey,
				[MarshalAs(UnmanagedType.Bool)]bool activate,
				[MarshalAs(UnmanagedType.LPWStr)]String errorMessage
			);
		[DllImport("UnicornDVCAppLib.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
			private static extern IntPtr Unicorn_GetHardwareId();
		[DllImport("UnicornDVCAppLib.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
			private static extern int Unicorn_Open(
				ConnectionType connectionType
			);
		[DllImport("UnicornDVCAppLib.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
			private static extern int Unicorn_Write(
				ConnectionType connectionType,
				int cbSize, 
				IntPtr pBuffer);
		[DllImport("UnicornDVCAppLib.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
			private static extern int Unicorn_Close(
				ConnectionType connectionType
			);
		[DllImport("UnicornDVCAppLib.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
			private static extern int Unicorn_Terminate(
				ConnectionType connectionType
			);
		[DllImport("UnicornDVCAppLib.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
			private static extern void Unicorn_SetCallbacks(
				ConnectionType connectionType,
				D_Connected _Connected,
				D_Disconnected _Disconnected,
				D_Terminated _Terminated,
				D_OnNewChannelConnection _OnNewChannelConnection,
				D_OnDataReceived _OnDataReceived,
				D_OnReadError _OnReadError,
				D_OnClose _OnClose
			);

		// Prevent construction
		private Unicorn() {}

		public static int Initialize(ConnectionType connectionType) { 
			return Unicorn_Initialize(connectionType); 
		}
		public static int SetLicenseKey(string licenseKey, bool activate, string errorMessage) { 
			return Unicorn_SetLicenseKey(licenseKey, activate, errorMessage); 
		}
		public static string GetHardwareId() { 
			return Marshal.PtrToStringUni(Unicorn_GetHardwareId()); 
		}
		public static int Open(ConnectionType connectionType) { 
			return Unicorn_Open(connectionType); 
		}
		public static int Write(ConnectionType connectionType, int cbSize, byte[] buffer) {
			IntPtr pBuffer=Marshal.AllocHGlobal(cbSize);
			Marshal.Copy(buffer, 0, pBuffer, cbSize);
			int res = Unicorn_Write(connectionType, cbSize, pBuffer);
			Marshal.FreeHGlobal(pBuffer);
			return res;
		}
		public static int Close(ConnectionType connectionType) { 
			return Unicorn_Close(connectionType); 
		}
		public static int Terminate(ConnectionType connectionType) { 
			return Unicorn_Terminate(connectionType);
		}
		public static void SetCallbacks(
			ConnectionType connectionType,
			D_Connected _Connected,
			D_Disconnected _Disconnected,
			D_Terminated _Terminated,
			D_OnNewChannelConnection _OnNewChannelConnection,
			D_OnDataReceived _OnDataReceived,
			D_OnReadError _OnReadError,
			D_OnClose _OnClose
		) {
			Unicorn_SetCallbacks(connectionType,_Connected,_Disconnected,_Terminated,_OnNewChannelConnection,_OnDataReceived,_OnReadError,_OnClose);
		}

	}
}
