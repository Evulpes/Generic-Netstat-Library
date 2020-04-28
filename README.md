# Generic-Netstat-Library
A simple library to return both TCP and UDP netstat -ano entries, and also terminate TCP connections.

# Example Usage
```cs
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Generic_Netstat_Library;
namespace Temp
{
    class Program : NSLib
    {
        static void Main()
        {
            IntPtr tablePtr;
            Tcpmib.MIB_TCPROW_OWNER_PID[] tcpEntries;

            try
            {
                tcpEntries = NSLib.GetExtendedTable(out tablePtr, TableType.TCP);
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Failed on GetExtendedTable, exception details: {ex.Message}");
                return;
            }

            for (int i = 0; i < tcpEntries.Length; i++)
            {
                if (tcpEntries[i].dwRemotePort.ToPort() == 3724)
                {
                    WinError.SeverityCode ntStatus = CloseConnection(tcpEntries[i]);
                    if (ntStatus != WinError.SeverityCode.ERROR_SUCCESS)
                        Debug.WriteLine($"Failed to close connection, error code: {ntStatus}");
                    break;
                }

            }
            Marshal.FreeHGlobal(tablePtr);
        }
    }
}
```
