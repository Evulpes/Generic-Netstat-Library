using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace Generic_Netstat_Library
{
    public class NativeMethods
    {
        protected class Iphlpapi : WinError
        {
            [DllImport("iphlpapi.dll", SetLastError = true)]
            public static extern SeverityCode GetExtendedTcpTable(IntPtr pTcpTable, out int pdwSize, bool bOrder, Winsock.AddressFamily ulAf, Iprtrmib.TCP_TABLE_CLASS TableClass, int Reserved = 0);
            [DllImport("iphlpapi.dll", SetLastError = true)]
            public static extern SeverityCode GetExtendedUdpTable(IntPtr pUdpTable, out int pdwSize, bool bOrder, Winsock.AddressFamily ulAf, Iprtrmib.UDP_TABLE_CLASS TableClass, int Reserved = 0);
            [DllImport("iphlpapi.dll", SetLastError = true)]
            public static extern SeverityCode SetTcpEntry(IntPtr pTcprow);
        }
        public static class Iprtrmib
        {
            public enum TCP_TABLE_CLASS
            {
                TCP_TABLE_BASIC_LISTENER,
                TCP_TABLE_BASIC_CONNECTIONS,
                TCP_TABLE_BASIC_ALL,
                TCP_TABLE_OWNER_PID_LISTENER,
                TCP_TABLE_OWNER_PID_CONNECTIONS,
                TCP_TABLE_OWNER_PID_ALL,
                TCP_TABLE_OWNER_MODULE_LISTENER,
                TCP_TABLE_OWNER_MODULE_CONNECTIONS,
                TCP_TABLE_OWNER_MODULE_ALL
            }
            public enum UDP_TABLE_CLASS
            {
                UDP_TABLE_BASIC,
                UDP_TABLE_OWNER_PID,
                UDP_TABLE_OWNER_MODULE
            }
        }
        public static class Tcpmib
        {

            public struct MIB_TCPTABLE_OWNER_PID
            {
                public int dwNumEntries;
                [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 1)]
                public MIB_TCPROW_OWNER_PID[] table;
            }
            public struct MIB_TCPROW_OWNER_PID
            {
                public Tcpmib.TCPState dwState;
                public int dwLocalAddr;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] dwLocalPort;
                public int dwRemoteAddr;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] dwRemotePort;
                public int dwOwningPid;
            }
            public enum TCPState
            {
                MIB_TCP_STATE_CLOSED = 1,
                MIB_TCP_STATE_LISTEN = 2,
                MIB_TCP_STATE_SYN_SENT = 3,
                MIB_TCP_STATE_SYN_RCVD = 4,
                MIB_TCP_STATE_ESTAB = 5,
                MIB_TCP_STATE_FIN_WAIT1 = 6,
                MIB_TCP_STATE_FIN_WAIT2 = 7,
                MIB_TCP_STATE_CLOSE_WAIT = 8,
                MIB_TCP_STATE_CLOSING = 9,
                MIB_TCP_STATE_LAST_ACK = 10,
                MIB_TCP_STATE_TIME_WAIT = 11,
                MIB_TCP_STATE_DELETE_TCB = 12,
                MIB_TCP_STATE_RESERVED = 100
            }
        }
        public static class Udpmib
        {
            public struct MIB_UDPTABLE_OWNER_PID
            {
                public uint dwNumEntries;
                [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 1)]
                public MIB_UDPROW_OWNER_PID[] table;
            }
            public struct MIB_UDPROW_OWNER_PID
            {
                public uint dwLocalAddr;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] dwLocalPort;
                public int dwOwningPid; //uint?
            }
        }
        public class WinError
        {
            public enum SeverityCode : uint
            {
                ERROR_SUCCESS = 0x0,
                NO_ERROR = 0x0,
                ERROR_NOT_SUPPORTED = 0x32,
                ERROR_INVALID_PARAMETER = 0x57,
                ERROR_INSUFFICIENT_BUFFER = 0x7A,
            }
        }
        public static class Winsock
        {
            public enum AddressFamily
            {
                AF_INET = 2,
            }
        }
    }
}
