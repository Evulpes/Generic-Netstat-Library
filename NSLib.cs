using System;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace Generic_Netstat_Library
{
    public class NSLib : NativeMethods
    {
        private delegate WinError.SeverityCode GetTableMethod(out IntPtr tableType);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tablePtr">A pointer to the unmanaged memory where the table details reside.</param>
        /// <param name="tableType">The type of table to Get (TCP/UDP).</param>
        /// <returns>An array of MIB_UDPROW_OWNER_PID or MIB_TCPROW_OWNER_PID depending on the tableType parameter.</returns>
        public static dynamic GetExtendedTable(out IntPtr tablePtr, TableType tableType = TableType.TCP)
        {
            //Create an empty delegate for the TCP, or UDP, query.
            GetTableMethod GetTable;
            dynamic tabelRowArray;
            dynamic tableInfo;

            //If the tableType is UDP, initialize the dynamic values and delegate to UDP values.
            if (tableType == TableType.UDP)
            {
                GetTable = new GetTableMethod(ConnectionTables.GetExtendedUdpTable);
                tableInfo = new Udpmib.MIB_UDPTABLE_OWNER_PID();
                tabelRowArray = new Udpmib.MIB_UDPROW_OWNER_PID();
            }
            else
            {
                GetTable = new GetTableMethod(ConnectionTables.GetExtendedTcpTable);
                tableInfo = new Tcpmib.MIB_TCPTABLE_OWNER_PID();
                tabelRowArray = new Tcpmib.MIB_TCPROW_OWNER_PID();
            }

            WinError.SeverityCode status = GetTable(out tablePtr);
            if (status != WinError.SeverityCode.NO_ERROR)
                throw new Exception($"Failed on GetExtendedTable ({tableType}). Error Code: {status}.");

            //Marshals the table pointer to either a TCP or UDP table struct.
            tableInfo = Convert.ChangeType(Marshal.PtrToStructure(tablePtr, (Type)tableInfo.GetType()), tableInfo.GetType());

            //Initialize a new array of either a TCP or UDP row array.
            tabelRowArray = Array.CreateInstance(tabelRowArray.GetType(), tableInfo.dwNumEntries);

            //Gets a pointer to the first row in the table.
            IntPtr rowPtr = (IntPtr)(tablePtr + Marshal.SizeOf(tableInfo.dwNumEntries));


            for (int i = 0; i < tabelRowArray.Length; i++)
            {
                //Marshals the index of the table row array to the content of row struct.
                tabelRowArray[i] = Convert.ChangeType(Marshal.PtrToStructure(rowPtr, (Type)tabelRowArray[i].GetType()), tabelRowArray[i].GetType());

                //Increments the pointer to the row by the size of the struct.
                rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(tabelRowArray[i]));
            }
            return tabelRowArray;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tcpTableRow">The TCP Row which contains the connection to close.</param>
        /// <returns>An NTSTATUS code.</returns>
        public static WinError.SeverityCode CloseConnection(Tcpmib.MIB_TCPROW_OWNER_PID tcpTableRow)
        {
            //Sets the state value to delete.
            tcpTableRow.dwState = Tcpmib.TCPState.MIB_TCP_STATE_DELETE_TCB;

            //Allocate a region for the row details to reside.
            IntPtr tcpRowPtr = Marshal.AllocHGlobal(Marshal.SizeOf(tcpTableRow));

            //Marshal the array into memory.
            Marshal.StructureToPtr(tcpTableRow, tcpRowPtr, false);

            //Overwrite the connection data.
            WinError.SeverityCode ntStatus = Iphlpapi.SetTcpEntry(tcpRowPtr);

            //Free the memory region.
            Marshal.FreeHGlobal(tcpRowPtr);

            
            return ntStatus;
        }
        public enum TableType
        {
            /// <summary>
            /// If the type of query required is TCP.
            /// </summary>
            TCP,
            /// <summary>
            /// If the type of query required is UDP.
            /// </summary>
            UDP,
        }

    }
    class ConnectionTables : NativeMethods
    {
        internal static WinError.SeverityCode GetExtendedTcpTable(out IntPtr tcpTable)
        {
            tcpTable = IntPtr.Zero;
            //The initial status will return as ERROR_INSUFFICIENT_BUFFER, and pdwSize will contain the correct buffer size.
            WinError.SeverityCode status = Iphlpapi.GetExtendedTcpTable(IntPtr.Zero, out int pdwSize, true, Winsock.AddressFamily.AF_INET, Iprtrmib.TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);

            //The buffer may be slightly higher after the previous statement, so check.
            while (status == WinError.SeverityCode.ERROR_INSUFFICIENT_BUFFER)
            {
                if (tcpTable != IntPtr.Zero)
                    Marshal.FreeHGlobal(tcpTable);
                tcpTable = Marshal.AllocHGlobal(pdwSize);
                status = Iphlpapi.GetExtendedTcpTable(tcpTable, out pdwSize, true, Winsock.AddressFamily.AF_INET, Iprtrmib.TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);
            }
            return status;
        }
        internal static WinError.SeverityCode GetExtendedUdpTable(out IntPtr udpTable)
        {
            udpTable = IntPtr.Zero;
            WinError.SeverityCode status = Iphlpapi.GetExtendedUdpTable(IntPtr.Zero, out int pdwSize, true, Winsock.AddressFamily.AF_INET, Iprtrmib.UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID);
            while (status == WinError.SeverityCode.ERROR_INSUFFICIENT_BUFFER)
            {
                if (udpTable != IntPtr.Zero)
                    Marshal.FreeHGlobal(udpTable);

                udpTable = Marshal.AllocHGlobal(pdwSize);
                status = Iphlpapi.GetExtendedUdpTable(udpTable, out pdwSize, true, Winsock.AddressFamily.AF_INET, Iprtrmib.UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID);
            }
            return status;
        }
    }
    public static class Extensions
    {
        /// <summary>
        /// Swaps the first two bytes of an array.
        /// </summary>
        /// <param name="array">The array on which to perform the swap.</param>
        /// <returns>A new byte[].</returns>
        public static int ToPort(this byte[] array) => array.Length < 0 ? 0 : BitConverter.ToUInt16(new byte[2] { array[1], array[0], }, 0);
    }

}
