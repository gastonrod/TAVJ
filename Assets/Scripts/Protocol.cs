using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Protocol
{
    public struct Positions
    {
        public float x1, y1, z1, x2, y2, z2;
    }

    public static byte[] Serialize(Positions positions)
    {
        int size = Marshal.SizeOf(positions);
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(positions, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    public static Positions Deserialize(byte[] serializedPositions)
    {
        Positions str = new Positions();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(serializedPositions, 0, ptr, size);

        str = (Positions)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);

        return str;
    }
    
}
