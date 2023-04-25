using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public enum ClientPackets
{
    welcomeReceived = 1,
    playerMovement
}

public enum ServerPackets
{
    welcome = 1,
    spawnPlayer,
    playerPosition,
    playerRotation
}

public class Packet : IDisposable
{
    private List<byte> buffer;
    private byte[] readableBuffer;
    private int readPos = 0;

    public Packet()
    {
        buffer = new List<byte>();
        readableBuffer = null;
    }

    public Packet(int id)
    {
        buffer = new List<byte>();
        readableBuffer = null;

        Write(id);
    }

    public Packet(byte[] data)
    {
        buffer = new List<byte>();
        readableBuffer = null;

        SetBytes(data);
    }

    public int Length()
    {
        return buffer.Count;
    }

    public int UnreadLength()
    {
        return Length() - readPos;
    }

    public byte[] ToArray()
    {
        readableBuffer = buffer.ToArray();

        return readableBuffer;
    }

    #region write

    public void Reset(bool shouldReset = true)
    {
        if (shouldReset)
        {
            buffer.Clear();
            readableBuffer = null;
            readPos = 0;
        }
        else
        {
            readPos -= 4;
        }
    }

    public void SetBytes(byte[] value)
    {
        Write(value);
        readableBuffer = buffer.ToArray();
    }

    public void InsertInt(int id)
    {
        buffer.InsertRange(0, BitConverter.GetBytes(id));
    }

    public void InsertLength()
    {
        buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
    }

    public void Write(byte value)
    {
        buffer.Add(value);
    }

    public void Write(byte[] value)
    {
        buffer.AddRange(value);
    }

    public void Write(int value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(float value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(short value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(long value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(bool value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(string value)
    {
        Write(value.Length);
        buffer.AddRange(Encoding.UTF8.GetBytes(value));
    }

    public void Write(Vector3 value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
    }

    public void Write(Quaternion value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
        Write(value.w);
    }

    #endregion

    #region read

    public byte ReadByte(bool movePos = true)
    {
        if (buffer.Count <= readPos) throw new Exception("Could not read value of type 'byte'!");

        byte value = readableBuffer[readPos];

        if (movePos) readPos += 1;

        return value;
    }

    public byte[] ReadBytes(int length, bool movePos = true)
    {
        if (buffer.Count <= readPos) throw new Exception("Could not read value of type 'byte[]'!");

        byte[] value = buffer.GetRange(readPos, length).ToArray();

        if (movePos) readPos += length;

        return value;
    }

    public int ReadInt(bool movePos = true)
    {
        if (buffer.Count <= readPos) throw new Exception("Could not read value of type 'int'!");

        int value = BitConverter.ToInt32(readableBuffer, readPos);

        if (movePos) readPos += 4;

        return value;
    }

    public float ReadFloat(bool movePos = true)
    {
        if (buffer.Count <= readPos) throw new Exception("Could not read value of type 'float'!");

        float value = BitConverter.ToSingle(readableBuffer, readPos);

        if (movePos) readPos += 4;

        return value;
    }

    public short ReadShort(bool movePos = true)
    {
        if (buffer.Count <= readPos) throw new Exception("Could not read value of type 'short'!");

        short value = BitConverter.ToInt16(readableBuffer, readPos);

        if (movePos) readPos += 2;

        return value;
    }

    public long ReadLong(bool movePos = true)
    {
        if (buffer.Count <= readPos) throw new Exception("Could not read value of type 'long'!");

        long value = BitConverter.ToInt64(readableBuffer, readPos);

        if (movePos) readPos += 8;

        return value;
    }

    public bool ReadBool(bool movePos = true)
    {
        if (buffer.Count <= readPos) throw new Exception("Could not read value of type 'bool'!");

        bool value = BitConverter.ToBoolean(readableBuffer, readPos);

        if (movePos) readPos += 1;

        return value;
    }

    public string ReadString(bool movePos = true)
    {
        if (buffer.Count <= readPos) throw new Exception("Could not read value of type 'string'!");

        try
        {
            int length = ReadInt();
            string value = Encoding.UTF8.GetString(readableBuffer, readPos, length);

            if (movePos) readPos += length;

            return value;
        }
        catch
        {
            throw new Exception("Could not read value of type 'string'!");
        }
    }

    public Vector3 ReadVector3(bool movePos = true)
    {
        return new Vector3(ReadFloat(movePos), ReadFloat(movePos), ReadFloat(movePos));
    }

    public Quaternion ReadQuaternion(bool movePos = true)
    {
        return new Quaternion(ReadFloat(movePos), ReadFloat(movePos), ReadFloat(movePos), ReadFloat(movePos));
    }

    #endregion

    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;
        if (disposing)
        {
            buffer = null;
            readableBuffer = null;
            readPos = 0;
        }

        disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

