using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// int형 뿐만 아니라 다양한 데이터를 그냥 보낼 수 없습니다.
// 데이터 유형을 패킷 클래스로 통해 Byte로 변환하여 송수신 합니다.

// 또한 패킷을 읽어 오면 Byte 데이터를 바로 사용할 수 없습니다.
// 사용하고자 하는 곳에서 적절한 데이터형으로 변환하는 함수를 정의합니다.

public enum ServerPackets // 서버가 전송하는 패킷 종류  
{
    init = 1,
    spawnPlayer,
    playerPosition,
    playerRotation,
    playerDisconnected,
    playerHP,
    playerReSpawned,
    createItemSpawner,
    itemSpawned,
    itemPickedUp,
    spawnProjectile,
    projectilePostion,
    projectileExploded,
    playerCheck,
    playerDieCount,
}

public enum ClientPackets// 클라이언트가 전송하는 패킷 종류
{
    init = 1,
    playerMovement,
    playerShoot,
    playerThrowItem,
}

public class Packet : IDisposable // 패킷에 필요한 변수와 함수
{
    private List<byte> buffer;
    private byte[] readableBuffer;
    private int readPos; // 아래에서 데이터를 쓰거나 읽을 때 데이터 크기에 맞게 크기를 더하거나 빼준다.

    public Packet() // 디폴트 생성자
    {
        buffer = new List<byte>(); // 버퍼 초기화
        readPos = 0; // 읽을 위치를 0으로 설정
    }

    public Packet(int id) // id를 이용해 패킷 생성
    {
        buffer = new List<byte>();
        readPos = 0;

        Write(id); // 버퍼에 id를 저장
    }

    public Packet(byte[] data) // byte 데이터를 이용해 패킷 생성
    {
        buffer = new List<byte>();
        readPos = 0;

        SetBytes(data);
    }

    #region Functions

    public void SetBytes(byte[] data)  // 데이터를 일기위한 버퍼 준비
    {
        Write(data);
        readableBuffer = buffer.ToArray();
    }

    public void WriteLength() // 버퍼 시작 부분에 패킷 길이를 삽입
    {
        buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); // Insert the byte length of the packet at the very beginning
    }

    public void InsertInt(int value)  // 버퍼의 시작 부분에 주어진 int를 삽입
    {
        buffer.InsertRange(0, BitConverter.GetBytes(value)); // Insert the int at the start of the buffer
    }

    public byte[] ToArray() // 배열 형식으로 패킷을 가져오기
    {
        readableBuffer = buffer.ToArray();
        return readableBuffer;
    }

    public int Length() // 패킷 길이 반환
    {
        return buffer.Count; // Return the length of buffer
    }

    public int UnreadLength() // 아직 읽지 않은 패킷의 길이를 반환
    {
        return Length() - readPos; // Return the remaining length (unread)
    }

    public void Reset(bool shouldReset = true) // 패킷 초기화
    {
        if (shouldReset)
        {
            buffer.Clear(); // Clear buffer
            readableBuffer = null;
            readPos = 0; // Reset readPos
        }
        else
        {
            readPos = 4; // "Unread" the last read int
        }
    }
    #endregion

    #region Write Data // 다양한 데이터의 형식을 버퍼에 저장하는 함수들

    public void Write(byte value)
    {
        buffer.Add(value);
    }

    public void Write(byte[] value) // byte 배열 데이터 형식을 byte로 바꾸어 저장
    {
        buffer.AddRange(value);
    }

    public void Write(short value) // short 데이터 형식을 byte로 바꾸어 저장
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(int value) // int 데이터 형식을 byte로 바꾸어 저장
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(long value) // long 데이터 형식을 byte로 바꾸어 저장
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(float value) // float 데이터 형식을 byte로 바꾸어 저장
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(bool value) // bool 데이터 형식을 byte로 바꾸어 저장
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(string value) // string 데이터 형식을 byte로 바꾸어 저장
    {
        Write(value.Length); // Add the length of the string to the packet
        buffer.AddRange(Encoding.ASCII.GetBytes(value)); // Add the string itself
    }

    public void Write(Vector3 value) // Vector3 데이터 형식을 byte로 바꾸어 저장
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
    }

    public void Write(Quaternion value) // Quaternion 데이터 형식을 byte로 바꾸어 저장
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
        Write(value.w);
    }

    #endregion

    #region Read Data // 패킷의 데이터를 원하는 형식으로 읽는 함수들

    // moveReadPos : 버퍼의 읽기 위치를 이동할지 여부
    // true이면 위치를 이동

    public byte ReadByte(bool moveReadPos = true) // Byte 읽기
    {
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            byte value = readableBuffer[readPos]; // Get the byte at readPos' position
            if (moveReadPos)
            {
                // If moveReadPos is true
                readPos += 1; // Increase readPos by 1
            }
            return value; // Return the byte
        }
        else
        {
            throw new Exception("Could not read value of type 'byte'!");
        }
    }

    // length 바이트 배열의 길이
    public byte[] ReadBytes(int length, bool moveReadPos = true) // 패킷에서 바이트 배열을 읽는 함수
    {
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            byte[] value = buffer.GetRange(readPos, length).ToArray(); // Get the bytes at readPos' position with a range of length
            if (moveReadPos)
            {
                // If moveReadPos is true
                readPos += length; // Increase readPos by length
            }
            return value; // Return the bytes
        }
        else
        {
            throw new Exception("Could not read value of type 'byte[]'!");
        }
    }

    public short ReadShort(bool moveReadPos = true) // 패킷에서 short 데이터 읽는 함수
    {
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            short value = BitConverter.ToInt16(readableBuffer, readPos); // Convert the bytes to a short
            if (moveReadPos)
            {
                // If moveReadPos is true and there are unread bytes
                readPos += 2; // Increase readPos by 2
            }
            return value; // Return the short
        }
        else
        {
            throw new Exception("Could not read value of type 'short'!");
        }
    }

    public int ReadInt(bool moveReadPos = true) // 패킷에서 int 데이터 읽는 함수
    {
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            int value = BitConverter.ToInt32(readableBuffer, readPos); // Convert the bytes to an int
            if (moveReadPos)
            {
                // If moveReadPos is true
                readPos += 4; // Increase readPos by 4
            }
            return value; // Return the int
        }
        else
        {
            throw new Exception("Could not read value of type 'int'!");
        }
    }

    public long ReadLong(bool moveReadPos = true) // 패킷에서 long 데이터 읽는 함수
    {
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            long value = BitConverter.ToInt64(readableBuffer, readPos); // Convert the bytes to a long
            if (moveReadPos)
            {
                // If moveReadPos is true
                readPos += 8; // Increase readPos by 8
            }
            return value; // Return the long
        }
        else
        {
            throw new Exception("Could not read value of type 'long'!");
        }
    }

    public float ReadFloat(bool moveReadPos = true) // 패킷에서 float 데이터 읽는 함수
    {
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            float value = BitConverter.ToSingle(readableBuffer, readPos); // Convert the bytes to a float
            if (moveReadPos)
            {
                // If moveReadPos is true
                readPos += 4; // Increase readPos by 4
            }
            return value; // Return the float
        }
        else
        {
            throw new Exception("Could not read value of type 'float'!");
        }
    }

    public bool ReadBool(bool moveReadPos = true) // 패킷에서 bool 데이터 읽는 함수
    {
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            bool value = BitConverter.ToBoolean(readableBuffer, readPos); // Convert the bytes to a bool
            if (moveReadPos)
            {
                // If moveReadPos is true
                readPos += 1; // Increase readPos by 1
            }
            return value; // Return the bool
        }
        else
        {
            throw new Exception("Could not read value of type 'bool'!");
        }
    }

    public string ReadString(bool moveReadPos = true) // 패킷에서 string 데이터 읽는 함수
    {
        try
        {
            int length = ReadInt(); // Get the length of the string
            string value = Encoding.ASCII.GetString(readableBuffer, readPos, length); // Convert the bytes to a string
            if (moveReadPos && value.Length > 0)
            {
                // If moveReadPos is true string is not empty
                readPos += length; // Increase readPos by the length of the string
            }
            return value; // Return the string
        }
        catch
        {
            throw new Exception("Could not read value of type 'string'!");
        }
    }

    public Vector3 ReadVector3(bool moveReadPos = true) // 패킷에서 Vector3 데이터 읽는 함수
    {
        return new Vector3(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }

    public Quaternion ReadQuaternion(bool moveReadPos = true) // 패킷에서 Quaternion 데이터 읽는 함수
    {
        return new Quaternion(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }

    #endregion

    private bool disposed = false;

    protected virtual void Dispose(bool disposing) // 패킷관련 리소스 해제
    {
        if (!disposed)
        {
            if (disposing)
            {
                buffer = null;
                readableBuffer = null;
                readPos = 0;
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}