#include <iostream>
#include "UnicornDVCAppLib.h"
#include <sstream>
#include <vector>
#include <process.h>

using namespace std;
ConnectionType connectionType;
BOOL opened=FALSE;
UINT8 received=0;

PROTO_UNICORN_CONNECTED Unicorn_Connected;
PROTO_UNICORN_DISCONNECTED Unicorn_Disconnected;
PROTO_UNICORN_TERMINATED Unicorn_Terminated;
PROTO_UNICORN_ONNEWCHANNELCONNECTION Unicorn_OnNewChannelConnection;
PROTO_UNICORN_ONDATARECEIVED Unicorn_OnDataReceived;
PROTO_UNICORN_ONREADERROR Unicorn_OnReadError;
PROTO_UNICORN_ONCLOSE Unicorn_OnClose;

DWORD WINAPI Unicorn_Connected() {
	wcout<<L"Connected callback called"<<endl;
	return 0;
}

DWORD WINAPI Unicorn_Disconnected(DWORD dwDisconnectCode) {
	wcout<<L"Disconnected callback called with dwDisconnectCode "<<dwDisconnectCode<<endl;
	return 0;
}

DWORD WINAPI Unicorn_Terminated() {
	wcout<<L"Terminated callback called"<<endl;
	return 0;
}

DWORD WINAPI Unicorn_OnNewChannelConnection() {
	wcout<<L"OnNewChannelConnection callback called"<<endl;
	opened=TRUE;
	return 0;
}

DWORD WINAPI Unicorn_OnDataReceived(DWORD cbSize, BYTE* pBuffer) {
	wcout<<L"OnDataReceived called with data '"<<reinterpret_cast<WCHAR*>(pBuffer)<<L"' of size "<<cbSize<<endl;
	received++;
	return 0;
}

DWORD WINAPI Unicorn_OnReadError(DWORD dwErrorCode) {
	wcout<<L"OnReadError callback called with dwErrorCode "<<dwErrorCode<<endl;
	opened=FALSE;
	return 0;
}

DWORD WINAPI Unicorn_OnClose() {
	wcout<<L"OnClose callback called"<<endl;
	opened=FALSE;
	return 0;
}

int wmain(int argc, wchar_t **argv) {
	if (argc<=1) {
		wcout<<L"Error: This application requires a command line argument"<<endl;
		return 1;
	}
	wstringstream arg1(argv[1]);
	UINT iConnectionType;
	if (!(arg1 >> iConnectionType)) {
		wcout<<L"Error: command line argument for connection type must be an integer"<<endl;
		return 1;
	}
	if (!(iConnectionType==connectionTypeServer || iConnectionType==connectionTypeClient)) {
		wcout<<L"Error: command line argument for connection type must be 0 for server and 1 for client mode"<<endl;
		return 1;
	}
	connectionType = static_cast<ConnectionType>(iConnectionType);
	wcout<<L"ACCESS4U UnicornDVC console test application"<<endl;
	Unicorn_SetCallbacks(connectionType,&Unicorn_Connected,&Unicorn_Disconnected,&Unicorn_Terminated,&Unicorn_OnNewChannelConnection,&Unicorn_OnDataReceived,&Unicorn_OnReadError,&Unicorn_OnClose);
	wcout<<L"First, try to initialize the library in ";
	if (connectionType==connectionTypeServer) { 
		wcout<<L"server mode"; 
	} else { 
		wcout<<L"client mode";
	}
	wcout<<endl;
	DWORD res =Unicorn_Initialize(connectionType);
	if (res) {
		wcout<<L"Initialize failed with status code "<<res<<endl;
		return res;
	}
	wcout<<L"Initialize succeeded, now trying to open the virtual channel."<<endl;
	for (UINT8 i=0; i<10; i++) {
		wcout<<L"Attempt "<<i+1<<endl;
		res=Unicorn_Open(connectionType);
		if (res) {
			if (i==9) {
				wcout<<L"Open definately failed with status code "<<res<<endl;
				return res;
			}
			wcout<<L"Open failed with status code "<<res<<endl;
			Sleep(2500);
			continue;
		}
		wcout<<L"Open succeeded"<<endl;
		break;
	}
	if (!opened) {
		wcout<<L"We must wait for the OnNewChannelConnection callback to be called"<<endl;
		for (UINT8 i=0; i<10; i++) {
			if (i==9) {
				wcout<<L"OnNewChannelConnection call took too long"<<endl;
				return 1;
			}
			wcout<<L"Waiting for OnNewChannelConnection call"<<i+1<<L"/10"<<endl;
			Sleep(1000);
			if (opened) {
				break;
			}
		}
	}
	wcout <<L"Sending and receiving pieces of data asynchronously"<<endl;
	vector<wstring> v={
		L"(Do!) doe, a deer, a female deer",
		L"(Re!) ray, a drop of golden sun",
		L"(Mi!) me, a name I call myself",
		L"(Fa!) far, a long, long way to run",
		L"(So!) sew, a needle pulling thread",
		L"(La!) la, a note to follow so",
		L"(Ti!) tea, a drink with jam and bread",
		L"That will bring us back to do oh oh oh"
	};
	for (UINT8 i=0; i<v.size(); i++) {
		wcout<<L"Writing "<<v[i]<<endl;
		res=Unicorn_Write(
			connectionType,
			static_cast<DWORD>((v[i].length()+1)*sizeof(WCHAR)),
			(BYTE*)(v[i].c_str())
		);
		if (res) {
			wcout<<L"Writing "<<v[i]<<L" failed with status code "<<res<<endl;			
			return 1;
		}
	}
	while (received<v.size() && opened) {
		wcout<<L"Waiting for data, "<<received<<L" chunks received"<<endl;
		Sleep(4000);
	}
	wcout<<L"We are ready."<<endl;
	if (connectionType==1) { // Client
		for (UINT8 i=0; i<10; i++) {	
			if (!opened || i==9) {
				break;
			}
			wcout<<L"Waiting for the channel to be closed from the server..."<<endl;
			Sleep(4000);
		}
	}
	if (opened) {
		wcout<<L"closing channel..."<<endl;
		res=Unicorn_Close(connectionType);
		if (res) {
			wcout<<L"Close failed with status code "<<res<<endl;
		}
	}
	wcout<<L"Terminating library..."<<endl;
	res=Unicorn_Terminate(connectionType);
	if (res) {
		wcout<<L"Terminate failed with status code "<<res<<endl;
		return res;
	}
	wcout<<L"All done, have a nice day!"<<endl;
	return 0;
}