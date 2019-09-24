using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class NetworkCommunicateHelper
{
    public const byte UNKNOW = 0;
    public const byte BYTE = 1;
    public const byte SBYTE = 2;
    public const byte CHAR = 3;
    public const byte BOOL = 4;
    public const byte SHORT = 5;
    public const byte USHORT = 6;
    public const byte INT = 7;
    public const byte UINT = 8;
    public const byte LONG = 9;
    public const byte ULONG = 10;
    public const byte FLOAT = 11;
    public const byte DOUBLE = 12;
    public const byte DECIMAL = 13;
    public const byte STRING = 14;
    public const byte VECTOR2 = 15;
    public const byte VECTOR3 = 16;
    public const byte VECTOR4 = 17;
    public const byte COLOR = 18;
    public const byte COLOR32 = 19;
    public const byte QUATERNION = 20;
    public const byte TRANSFORM = 21;
    public const byte RECT = 22;
    public const byte PLANE = 23;
    public const byte GAMEOBJECT = 24;
    public const byte RAY = 25;
    public const byte MATRIX4X4 = 26;
    public const byte NETWORKHASH128 = 27;
    public const byte NETWORKIDENTITY = 28;
    public const byte MESSAGEBASE = 29;
    public const byte NETWORKINSTANCEID = 30;
    public const byte NETWORKSCENEID = 31;


    public static byte[] ToPack(out int size, params object[] datas)
    {
        NetworkWriter writer = new NetworkWriter();
        byte dataCount = (byte)datas.Length;
        writer.Write(dataCount);

        foreach(object tempData in datas)
        {
            WriteData(writer, tempData);
        }
        size = writer.Position;
        return writer.AsArray();
    }

    private static void WriteData(NetworkWriter writer, object data)
    {
        Type dataType = data.GetType();

        if (dataType.Equals(typeof(byte)))
        {
            writer.Write(BYTE);
            writer.Write((byte)data);
        }
        else if (dataType.Equals(typeof(sbyte)))
        {
            writer.Write(SBYTE);
            writer.Write((sbyte)data);
        }
        else if (dataType.Equals(typeof(char)))
        {
            writer.Write(CHAR);
            writer.Write((char)data);
        }
        else if (dataType.Equals(typeof(bool)))
        {
            writer.Write(BOOL);
            writer.Write((bool)data);
        }
        else if (dataType.Equals(typeof(short)))
        {
            writer.Write(SHORT);
            writer.Write((short)data);
        }
        else if (dataType.Equals(typeof(ushort)))
        {
            writer.Write(USHORT);
            writer.Write((ushort)data);
        }
        else if (dataType.Equals(typeof(int)))
        {
            writer.Write(INT);
            writer.Write((int)data);
        }
        else if (dataType.Equals(typeof(uint)))
        {
            writer.Write(UINT);
            writer.Write((uint)data);
        }
        else if (dataType.Equals(typeof(long)))
        {
            writer.Write(LONG);
            writer.Write((long)data);
        }
        else if (dataType.Equals(typeof(ulong)))
        {
            writer.Write(ULONG);
            writer.Write((ulong)data);
        }
        else if (dataType.Equals(typeof(float)))
        {
            writer.Write(FLOAT);
            writer.Write((float)data);
        }
        else if (dataType.Equals(typeof(double)))
        {
            writer.Write(DOUBLE);
            writer.Write((double)data);
        }
        else if (dataType.Equals(typeof(decimal)))
        {
            writer.Write(DECIMAL);
            writer.Write((decimal)data);
        }
        else if (dataType.Equals(typeof(string)))
        {
            writer.Write(STRING);
            writer.Write((string)data);
        }
        else if (dataType.Equals(typeof(Vector2)))
        {
            writer.Write(VECTOR2);
            writer.Write((Vector2)data);
        }
        else if (dataType.Equals(typeof(Vector3)))
        {
            writer.Write(VECTOR3);
            writer.Write((Vector3)data);
        }
        else if (dataType.Equals(typeof(Vector4)))
        {
            writer.Write(VECTOR4);
            writer.Write((Vector4)data);
        }
        else if (dataType.Equals(typeof(Color)))
        {
            writer.Write(COLOR);
            writer.Write((Color)data);
        }
        else if (dataType.Equals(typeof(Color32)))
        {
            writer.Write(COLOR32);
            writer.Write((Color32)data);
        }
        else if (dataType.Equals(typeof(Quaternion)))
        {
            writer.Write(QUATERNION);
            writer.Write((Quaternion)data);
        }
        else if (dataType.Equals(typeof(Transform)))
        {
            writer.Write(TRANSFORM);
            writer.Write((Transform)data);
        }
        else if (dataType.Equals(typeof(Rect)))
        {
            writer.Write(RECT);
            writer.Write((Rect)data);
        }
        else if (dataType.Equals(typeof(Plane)))
        {
            writer.Write(PLANE);
            writer.Write((Plane)data);
        }
        else if (dataType.Equals(typeof(GameObject)))
        {
            writer.Write(GAMEOBJECT);
            writer.Write((GameObject)data);
        }
        else if (dataType.Equals(typeof(Ray)))
        {
            writer.Write(RAY);
            writer.Write((Ray)data);
        }
        else if (dataType.Equals(typeof(Matrix4x4)))
        {
            writer.Write(MATRIX4X4);
            writer.Write((Matrix4x4)data);
        }
        else if (dataType.Equals(typeof(NetworkHash128)))
        {
            writer.Write(NETWORKHASH128);
            writer.Write((NetworkHash128)data);
        }
        else if (dataType.Equals(typeof(NetworkIdentity)))
        {
            writer.Write(NETWORKIDENTITY);
            writer.Write((NetworkIdentity)data);
        }
        else if (dataType.Equals(typeof(MessageBase)))
        {
            writer.Write(MESSAGEBASE);
            writer.Write((MessageBase)data);
        }
        else if (dataType.Equals(typeof(NetworkInstanceId)))
        {
            writer.Write(NETWORKINSTANCEID);
            writer.Write((NetworkInstanceId)data);
        }
        else if (dataType.Equals(typeof(NetworkSceneId)))
        {
            writer.Write(NETWORKSCENEID);
            writer.Write((NetworkSceneId)data);
        }
    }

    public static object[] UnPack(byte[] buffer)
    {
        object[] datas = null;
        if (buffer != null && buffer.Length > 0)
        {
            NetworkReader reader = new NetworkReader(buffer);
            int dataCount = reader.ReadByte();
            if(dataCount > 0)
            {
                datas = new object[dataCount];

                for(int i = 0; i < dataCount; i++)
                {
                    datas[i] = ReadData(reader);
                }
            }
        }

        return datas;
    }

    private static object ReadData(NetworkReader reader)
    {
        object resultObj = null;

        byte dataType = reader.ReadByte();

        if (dataType == BYTE)
            resultObj = reader.ReadByte();
        else if (dataType == SBYTE)
            resultObj = reader.ReadSByte();
        else if (dataType == CHAR)
            resultObj = reader.ReadChar();
        else if (dataType == BOOL)
            resultObj = reader.ReadBoolean();
        else if (dataType == SHORT)
            resultObj = reader.ReadInt16();
        else if (dataType == USHORT)
            resultObj = reader.ReadUInt16();
        else if (dataType == INT)
            resultObj = reader.ReadInt32();
        else if (dataType == UINT)
            resultObj = reader.ReadPackedUInt32();
        else if (dataType == LONG)
            resultObj = reader.ReadInt64();
        else if (dataType == ULONG)
            resultObj = reader.ReadPackedUInt64();
        else if (dataType == FLOAT)
            resultObj = reader.ReadSingle();
        else if (dataType == DOUBLE)
            resultObj = reader.ReadDouble();
        else if (dataType == DECIMAL)
            resultObj = reader.ReadDecimal();
        else if (dataType == STRING)
            resultObj = reader.ReadString();
        else if (dataType == VECTOR2)
            resultObj = reader.ReadVector2();
        else if (dataType == VECTOR3)
            resultObj = reader.ReadVector3();
        else if (dataType == VECTOR4)
            resultObj = reader.ReadVector4();
        else if (dataType == COLOR)
            resultObj = reader.ReadColor();
        else if (dataType == COLOR32)
            resultObj = reader.ReadColor32();
        else if (dataType == QUATERNION)
            resultObj = reader.ReadQuaternion();
        else if (dataType == TRANSFORM)
            resultObj = reader.ReadTransform();
        else if (dataType == RECT)
            resultObj = reader.ReadRect();
        else if (dataType == PLANE)
            resultObj = reader.ReadPlane();
        else if (dataType == GAMEOBJECT)
            resultObj = reader.ReadGameObject();
        else if (dataType == RAY)
            resultObj = reader.ReadRay();
        else if (dataType == MATRIX4X4)
            resultObj = reader.ReadMatrix4x4();
        else if (dataType == NETWORKHASH128)
            resultObj = reader.ReadNetworkHash128();
        else if (dataType == NETWORKIDENTITY)
            resultObj = reader.ReadNetworkIdentity();
        else if (dataType == MESSAGEBASE)
            //resultObj = reader.ReadMessage<MessageBase>();
            resultObj = null;
        else if (dataType == NETWORKINSTANCEID)
            resultObj = reader.ReadNetworkId();
        else if (dataType == NETWORKSCENEID)
            resultObj = reader.ReadSceneId();
        //Debug.Log(resultObj);
        return resultObj;
    }
}
