using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Runtime.CompilerServices;

namespace ObjectManager
{
    public class ObjectSaver
    {

        /// <summary>
        /// 저장할 경로입니다.
        /// </summary>
        public string path;
        /// <summary>
        /// 저장할 객체입니다.
        /// </summary>
        public object obj;

        
        public byte[] pubKey;
        public byte[] priKey;

        public Header header = new Header();
        /// <summary>
        /// 헤더 추가의 유무입니다.
        /// 추가하지 않으면 불러올때 똑같은 ObjectSaver 객체가 필요합니다.
        /// </summary>
        public bool createHeader = true;
        public class Header
        {
            /// <summary>
            /// 저장하고 불러오기 할때 압축의 유무입니다.
            /// </summary>
            public bool compress = true;
            /// <summary>
            /// 저장하고 불러오기 할때 암호화의 유무입니다.
            /// </summary>
            internal bool encryption = false;
            /// <summary>
            /// 객체의 크기를 나타낼 바이트 수입니다.
            /// 1과 4 사이의 수를 입력해야 합니다.
            /// </summary>
            public ushort filesizelength = 4;
            /// <summary>
            /// 타입 인덱스를 나타낼 바이트 수입니다.
            /// 1과 4 사이의 수를 입력해야 합니다.
            /// </summary>
            public ushort typenamelength = 4;
            internal int typecount;
        }

        


        /// <summary>
        /// 경로 위치에 지정된 설정을 사용해 저장합니다.
        /// </summary>
        public void Save()
        {
            if(path == null)
            {
                throw new Exception("경로가 설정되지 않았습니다.");
            }
            File.WriteAllBytes(path,GetBytes());
        }

        /// <summary>
        /// 지정된 설정으로 바이트 배열을 반환합니다.
        /// </summary>
        public byte[] GetBytes()
        {
            if (obj == null)
            {
                throw new Exception("오브젝트가 null입니다.");
            }

            byte createheader = this.createHeader ? (byte)1 : (byte)0;


            byte[] data = GetDataBodyBytes();

            byte[] header = GetHeaderBytes();

            byte[] typecount = GetTypeCount();

            List<byte> result = new List<byte>(1+header.Length+data.Length+typecount.Length);
            
            result.Add(createheader);
            if (this.createHeader)
            {
                result.AddRange(header);
            }
            result.AddRange(typecount);
            result.AddRange(data);

            return result.ToArray();

            byte[] GetDataBodyBytes()
            {
                List<Type> loadedtypes = new List<Type>(50);

                loadedtypes.Add(typeof(Int16));
                loadedtypes.Add(typeof(UInt16));
                loadedtypes.Add(typeof(Int32));
                loadedtypes.Add(typeof(UInt32));
                loadedtypes.Add(typeof(Int64));
                loadedtypes.Add(typeof(UInt64));
                loadedtypes.Add(typeof(Single));
                loadedtypes.Add(typeof(Double));
                loadedtypes.Add(typeof(Byte));
                loadedtypes.Add(typeof(SByte));
                loadedtypes.Add(typeof(Decimal));
                loadedtypes.Add(typeof(String));

                byte[] bodybytes;
                List<byte> typenamebytelist = new List<byte>(100);


                
                bodybytes = GetEverythingBytes(obj, "Q");


                for (int i = 12; i < loadedtypes.Count; i++)
                {
                    typenamebytelist.AddRange(Encoding.UTF8.GetBytes(loadedtypes[i].FullName));
                    typenamebytelist.Add(Spacebar);
                }
                this.header.typecount = loadedtypes.Count - 12;

                byte[] databytes = typenamebytelist.Concat(bodybytes).ToArray();

                if (this.header.encryption)
                {
                    databytes = Encrypt(databytes);
                }
                if (this.header.compress)
                {
                    databytes = Compress(databytes);
                }

                return databytes;


                byte[] GetEverythingBytes(object value, string name)
                {
                    if(value == null)
                    {
                        return new byte[0];
                    }

                    Type t = value.GetType();
                    if (IsPrimitive(t))
                    {
                        return GetPrimitiveBytes(value, name);
                    }
                    else if (IsAwesome(t))
                    {
                        return GetAwesomeBytes(value, name);
                    }
                    else if (t.IsArray)
                    {
                        return GetArrayBytes(value, name);
                    }
                    else
                    {
                        return GetObjectBytes(value, name);
                    }
                }
                byte[] GetObjectBytes(object obj, string name)
                {
                    List<byte> bytelist = new List<byte>(60);
                    bytelist.AddRange(Trim(GetTypeIndex(obj.GetType()), this.header.typenamelength));//타입인덱스 추가
                    if (name != "")
                    {
                        bytelist.AddRange(Encoding.UTF8.GetBytes(name));//이름 추가
                        bytelist.Add(Spacebar);//스페이스바
                    }

                    List<byte> imshi = new List<byte>();
                  

                    BindingFlags bindflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                    System.Reflection.FieldInfo[] fieldinfo = obj.GetType().GetFields(bindflags);
                    foreach (System.Reflection.FieldInfo info in fieldinfo)
                    {
                        if (info.GetValue(obj) != null &&( !Attribute.IsDefined(info,typeof(IgnoreSave)) || Attribute.IsDefined(info, typeof(ForceSave))))
                        {
                            imshi.AddRange(GetEverythingBytes(info.GetValue(obj), info.Name));
                        }
                    }

                    bytelist.AddRange(Trim(BitConverter.GetBytes(imshi.Count),this.header.filesizelength));
                    bytelist.AddRange(imshi);

                    return bytelist.ToArray();
                }

                byte[] GetPrimitiveBytes(object obj, string name)
                {
                    (byte[] data, byte index) = GetPrimitiveTypeData(obj);
                    return GetRealBytes(data, name, index);

                    byte[] GetRealBytes(byte[] bytes, string name, byte index)
                    {
                        List<byte> bytelist = new List<byte>();
                        bytelist.Add(index);
                        for (int i = 1; i < this.header.typenamelength; i++)
                        {
                            bytelist.Add(0);
                        }
                        if (name != "")
                        {
                            bytelist.AddRange(Encoding.UTF8.GetBytes(name));
                            bytelist.Add(Spacebar);
                        }
                        bytelist.AddRange(bytes);
                        return bytelist.ToArray();
                    }
                }
                (byte[] data, byte index) GetPrimitiveTypeData(object obj)
                {
                    switch (obj)
                    {
                        case Int16 i:
                            return (BitConverter.GetBytes(i), 0);
                        case UInt16 i:
                            return (BitConverter.GetBytes(i), 1);
                        case Int32 i:
                            return (BitConverter.GetBytes(i), 2);
                        case UInt32 i:
                            return (BitConverter.GetBytes(i), 3);
                        case Int64 i:
                            return (BitConverter.GetBytes(i), 4);
                        case UInt64 i:
                            return (BitConverter.GetBytes(i), 5);
                        case Single i:
                            return (BitConverter.GetBytes(i), 6);
                        case Double i:
                            return (BitConverter.GetBytes(i), 7);
                        case Byte i:
                            return (BitConverter.GetBytes(i), 8);
                        case SByte i:
                            return (BitConverter.GetBytes(i), 9);
                        case Decimal i:
                            return (decimal.GetBits(i).Cast<byte>().ToArray(), 10);
                        case String i:
                            byte[] strbyte = Encoding.UTF8.GetBytes(i);
                            byte[] bytelength = Trim(BitConverter.GetBytes(strbyte.Length),this.header.filesizelength);
                            return (bytelength.Concat(strbyte).ToArray(), 11);
                        default:
                            throw new Exception("Primitive가 아님");
                    }
                }
                byte[] GetArrayBytes(object obj, string name)
                {
                    if (IsStaticSize(obj.GetType().GetElementType()))
                    {
                        return GetStaticArrayBytes(obj, name);
                    }
                    else
                    {
                        return GetDynamicArrayBytes(obj, name);
                    }

                    byte[] GetStaticArrayBytes(object obj, string name)
                    {
                        List<byte> bytes = new List<byte>();
                        Type elementType = obj.GetType().GetElementType();
                        
                        byte[] index = GetTypeIndex(obj.GetType()); // 1

                        byte[] fieldname = Encoding.UTF8.GetBytes(name); // 2 + 스페이스바

                        byte[] arraylength = BitConverter.GetBytes((obj as Array).Length); // 3
                        //배열길이는 4바이트

                        bytes.AddRange(index); // 1
                        if (name != "")
                        {
                            bytes.AddRange(fieldname); // 2
                            bytes.Add(Spacebar); // 스페이스바
                        }
                        bytes.AddRange(arraylength); // 3

                        //배열 시작부분 추가 완료

                        Array objarr = obj as Array;

                        foreach(object o in objarr)
                        {
                            bytes.AddRange(GetPrimitiveTypeData(o).data);
                        }

                        //배열 추가 완료

                        return bytes.ToArray();
                    }
                    byte[] GetDynamicArrayBytes(object obj, string name)
                    {
                        List<byte> bytes = new List<byte>();

                        byte[] index = GetTypeIndex(obj.GetType()); // 1
                        

                        byte[] fieldname = Encoding.UTF8.GetBytes(name); // 2 + 스페이스바

                        byte[] arraylength = BitConverter.GetBytes((obj as Array).Length); // 3
                        //배열길이는 4바이트

                        bytes.AddRange(index); // 1
                        if (name != "")
                        {
                            bytes.AddRange(fieldname); // 2
                            bytes.Add(Spacebar); // 스페이스바
                        }
                        bytes.AddRange(arraylength); // 3

                        //배열 시작부분 추가 완료

                        Array objarr = obj as Array;

                        foreach (object o in objarr)
                        {
                            bytes.AddRange(GetEverythingBytes(o,""));
                        }

                        //배열 추가 완료

                        return bytes.ToArray();
                    }
                }

                byte[] GetAwesomeBytes(object obj, string name)
                {
                    return new byte[0];
                }//Func Action IEnumerator 같은거 전용

                byte[] Compress(byte[] bytearray)
                {
                    MemoryStream output = new MemoryStream();
                    using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
                    {
                        dstream.Write(bytearray, 0, bytearray.Length);
                    }
                    return output.ToArray();
                }
                byte[] Encrypt(byte[] bytearray)
                {
                    return new byte[0];
                }

                byte[] GetTypeIndex(Type t)
                {
                    byte[] index;
                    if (!loadedtypes.Contains(t))
                    {
                        loadedtypes.Add(t);
                        index = Trim(BitConverter.GetBytes(loadedtypes.Count-1), this.header.typenamelength);
                    }
                    else
                    {
                        index = Trim(BitConverter.GetBytes(loadedtypes.IndexOf(t)), this.header.typenamelength);
                    }
                    return index;
                }
            }
            byte[] GetHeaderBytes()
            {
                byte i = 0;

                i += (byte)(this.header.compress ? 0 : 1);
                i += (byte)((this.header.encryption ? 0 : 1) * 2);

                i += (byte)((this.header.filesizelength-1) * 4);

                i += (byte)((this.header.typenamelength-1) * 16);


                return new byte[] { i };
            }
            byte[] GetTypeCount()
            {
                return Trim(BitConverter.GetBytes(this.header.typecount), this.header.typenamelength);
            }
        }

        /// <summary>
        /// 파일에서 객체를 불러옵니다.
        /// </summary>
        public void Load()
        {
            if (path == null)
            {
                throw new Exception("경로가 설정되지 않았습니다.");
            }
            if(!File.Exists(path))
            {
                throw new Exception("경로에 파일이 없습니다,");
            }
            SetBytes(File.ReadAllBytes(path));
        }

        /// <summary>
        /// 바이트 배열에서 객체를 불러옵니다.
        /// </summary>
        /// <param name="bytes"></param>
        public void SetBytes(byte[] bytes)
        {
            if(bytes == null)
            {
                throw new Exception("입력 배열이 null");
            }

            
            List<Type> loadedtypes = new List<Type>(50);
            loadedtypes.Add(typeof(Int16));
            loadedtypes.Add(typeof(UInt16));
            loadedtypes.Add(typeof(Int32));
            loadedtypes.Add(typeof(UInt32));
            loadedtypes.Add(typeof(Int64));
            loadedtypes.Add(typeof(UInt64));
            loadedtypes.Add(typeof(Single));
            loadedtypes.Add(typeof(Double));
            loadedtypes.Add(typeof(Byte));
            loadedtypes.Add(typeof(SByte));
            loadedtypes.Add(typeof(Decimal));
            loadedtypes.Add(typeof(String));

            byte[] mainbytes;
            if (bytes[0] == 1)//헤더 읽기 시작
            {
                SetHeader(new byte[]{bytes[1]});
                mainbytes = bytes[1..];
            }
            else
            {
                mainbytes = bytes;
            }

           
            if (this.header.compress)
            {
                mainbytes = Decompress(mainbytes[1..]);
            }
            else
            {
                mainbytes = mainbytes[1..];
            }
            if(this.header.encryption)
            {
                mainbytes = Decrypt(mainbytes);
            }
            else
            {
                //그대로
            }

            int current = 0;

            int typelength = ReadInt(this.header.typenamelength);

            for (int i=0;i<typelength;i++)
            {
                loadedtypes.Add(GetTypeFixed(ReadNameString()));
            }
            
            this.obj = GetEverything().data;

            //Inner 안붙은것 시작위치
            //Everything : 타입 인덱스
            //나머지 : 이름

            //Inner 붙은것 시작위치
            //Everything : 타입 인덱스
            //나머지 : 크기 또는 값

            //Inner 붙어있는건 이름 안가져 온다는 의미


            (object data,string name) GetEverything()
            {
                int typeindex = ReadInt(this.header.typenamelength);
                Type type() => loadedtypes[typeindex];

                if (typeindex < 12)//String까지
                {
                    if (typeindex < 11)//String만 빼고
                    {
                        return GetPrimitive(typeindex);
                    }
                    else
                    {
                        return GetValueString();
                    }
                }
                else if (type().IsArray)
                {
                    if(IsStaticSize(type().GetElementType()))
                    {
                        return GetStaticArray(type());
                    }
                    else
                    {
                        return GetDynamicArray(type());
                    }
                }
                else if (IsAwesome(type()))
                {
                    
                    return GetAwesome(type());
                }
                else//IsObject
                {
                    return GetObject(type());
                }
            }
            object GetInnerEverything()
            {
                int typeindex = ReadInt(this.header.typenamelength);
                Type type() => loadedtypes[typeindex];

                if (typeindex < 12)//String까지
                {
                    if (typeindex < 11)//String만 빼고
                    {
                        return GetInnerPrimitive(typeindex);
                    }
                    else
                    {
                        return GetInnerValueString();
                    }
                }
                else if (type().IsArray)
                {
                    if (IsStaticSize(type().GetElementType()))
                    {
                        return GetInnerStaticArray(type());
                    }
                    else
                    {
                        return GetInnerDynamicArray(type());
                    }
                }
                else if (IsAwesome(type()))
                {
                    return GetInnerAwesome(type());
                }
                else//IsObject
                {
                    return GetInnerObject(type());
                }
            }
            (object data, string name) GetObject(Type t)
            {
                string name = ReadNameString();

                return (GetInnerObject(t),name);
            }
            object GetInnerObject(Type t)
            {
                object o = Activator.CreateInstance(t);
                int endindex = ReadInt(this.header.filesizelength) + current;
                BindingFlags bindflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                System.Reflection.FieldInfo[] fieldinfos = t.GetFields(bindflags);
                while (current < endindex)
                {
                    (object fieldobj, string fieldname) = GetEverything();

                    bool IsSet = false;
                    foreach (FieldInfo info in fieldinfos)
                    {
                        if (info.Name == fieldname)
                        {
                            info.SetValue(o, fieldobj);
                            IsSet = true;
                            break;
                        }
                    }
                    if (!IsSet)
                    {
                        throw new Exception("값이 설정되지 못했습니다.");
                    }



                }
                return o;
            }
            (object data,string name) GetStaticArray(Type t)
            {
                string name = ReadNameString();
                return (GetInnerStaticArray(t), name);
            }
            object GetInnerStaticArray(Type t)
            {
                int count = ReadInt(4);
                ArrayList arrayList = new ArrayList(count);

                int typeindex = loadedtypes.IndexOf(t);


                int elementtypeindex = loadedtypes.IndexOf(t.GetElementType());
                for (int i = 0; i < count; i++)
                {
                    arrayList.Add(GetInnerPrimitive(elementtypeindex));
                }

                return arrayList.ToArray(t.GetElementType());
            }
            (object data, string name) GetDynamicArray(Type t)
            {
                string name = ReadNameString();
                return (GetInnerDynamicArray(t),name);
            }
            object GetInnerDynamicArray(Type t)
            {
                int count = ReadInt(4);
                ArrayList arrayList = new ArrayList(count);

                int typeindex = loadedtypes.IndexOf(t);


                int elementtypeindex = loadedtypes.IndexOf(t.GetElementType());
                for (int i = 0; i < count; i++)
                {
                    arrayList.Add(GetInnerEverything());
                }

                return arrayList.ToArray(t.GetElementType());
            }

            (object data, string name) GetPrimitive(int typeindex)
            {
                string name = ReadNameString();
                return (GetInnerPrimitive(typeindex), name);
            }
            object GetInnerPrimitive(int typeindex)
            {
                switch (typeindex)
                {
                    case 0:
                        current += 2;
                        return BitConverter.ToInt16(mainbytes, current - 2);
                    case 1:
                        current += 2;
                        return BitConverter.ToUInt16(mainbytes, current - 2);
                    case 2:
                        current += 4;
                        return BitConverter.ToInt32(mainbytes, current - 4);
                    case 3:
                        current += 4;
                        return BitConverter.ToUInt32(mainbytes, current - 4);
                    case 4:
                        current += 8;
                        return BitConverter.ToInt32(mainbytes, current - 8);
                    case 5:
                        current += 8;
                        return BitConverter.ToUInt32(mainbytes, current - 8);
                    case 6:
                        current += 4;
                        return BitConverter.ToSingle(mainbytes, current - 4);
                    case 7:
                        current += 8;
                        return BitConverter.ToDouble(mainbytes, current - 8);
                    case 8:
                        current += 1;
                        return BitConverter.ToSingle(mainbytes, current - 1);
                    case 9:
                        current += 1;
                        return BitConverter.ToDouble(mainbytes, current - 1);
                    case 10:
                        current += 24;
                        return ByteArrayToDecimal(mainbytes, current - 24);
                    default:
                        throw new Exception("오류");

                    
                    
                }


                const byte DecimalSignBit = 128;
                decimal ByteArrayToDecimal(byte[] src, int offset)
                {
                    return new decimal(
                        BitConverter.ToInt32(src, offset),
                        BitConverter.ToInt32(src, offset + 4),
                        BitConverter.ToInt32(src, offset + 8),
                        src[offset + 15] == DecimalSignBit,
                        src[offset + 14]);
                }
            }
            (string data, string name) GetValueString()
            {
                string name = ReadNameString();
                return (GetInnerValueString(), name);
            }
            string GetInnerValueString()
            {
                int size = ReadInt(this.header.filesizelength);
                byte[] bytearray = new byte[size];
                for (int i = 0; i < size; i++)
                {
                    bytearray[i] = mainbytes[current + i];
                }
                current += size;
                return Encoding.UTF8.GetString(bytearray);
            }
            (object data, string name) GetAwesome(Type t)
            {
                throw new NotImplementedException();
            }
            object GetInnerAwesome(Type t)
            {
                throw new NotImplementedException();
            }
            void SetHeader(byte[] bytearray)
            {
                this.header.compress = (bytearray[0] % 2) == 0 ? true : false;
                this.header.encryption = (bytearray[0] % 4 / 2) == 0 ? true : false;
                this.header.filesizelength = (UInt16)(((bytearray[0] % 16) / 4) + 1);
                this.header.typenamelength = (UInt16)(((bytearray[0] % 64) / 16) + 1);
            }

            int ReadInt(int length)
            {
                byte[] bytearray = new byte[length];
                for(int i=0;i<length;i++)
                {
                    bytearray[i] = mainbytes[current + i];
                }
                current += length;
                return BitConverter.ToInt32(bytearray);
            }

            string ReadNameString()
            {
                int startindex = current;
                while(mainbytes[current] != Spacebar)
                {
                    current++;
                }
                return Encoding.UTF8.GetString(mainbytes[startindex..current++]);
            }

            byte[] Decompress(byte[] data)
            {
                MemoryStream input = new MemoryStream(data);
                MemoryStream output = new MemoryStream();
                using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                {
                    dstream.CopyTo(output);
                }
                return output.ToArray();
            }
            byte[] Decrypt(byte[] data)
            {
                throw new NotImplementedException();
            }
            Type GetTypeFixed(string str)
            {
                Type t = Type.GetType(str);
                if (t != null)
                {
                    return t;
                }
                else
                {
                    foreach (Assembly assembly in LoadedAssemblies)
                    {
                        t = assembly.GetType(str);
                        if (t != null)
                            return t;
                    }
                }
                return null;
            }
            
        }

        const byte Spacebar = 0xFF;

        //n : 헤더에 정해진 크기
        //자유 : 자유로운 크기, 끝날때 스페이스바
        //첫 바이트 : 헤더 사용 유무
        //헤더 사용시 그다음 2바이트 : 헤더
        //그다음 n바이트 : 사용할 타입 개수
        //그다음 바이트부터 사용할 타입명 + 스페이스바 넣음
        //그다음 n바이트 : 시작 객체 타입 인덱스
        //그다음 바이트부터
        //타입번호(n)+필드이름(자유)+스페이스바+크기(n)+데이터(크기)  :  객체나 Func등등 일때 string도 여기에 들어감
        //타입번호(n)+필드이름(자유)+스페이스바+데이터(설정된값)  :  Primitive일때
        //타입번호(n)+필드이름(자유)+스페이스바+배열갯수(4)+<데이터의 데이터(설정된값)> : Primitive의 배열일때
        //타입번호(n)+필드이름(자유)+스페이스바+배열갯수(4)+<타입번호(n)+크기(n)+데이터의 데이터(설정된값)> : 객체의 배열일때

        //암호화가 되있다면 헤더 이후 바이트가 암호화됨
        //압축이 걸려있다면 헤더 이후 바이트가 압축됨
        //암호화 먼저 하고 압축함

        byte[] Trim(byte[] bytearray, int size)
        {
            byte[] newarray = new byte[size];
            int i;
            for (i = 0; i < this.header.filesizelength; i++)
            {
                newarray[i] = bytearray[i];
            }
            for (; i < bytearray.Length; i++)
            {
                if (bytearray[i] != 0)
                {
                    throw new Exception("바이트 수 부족");
                }
            }
            return newarray;
        }

        bool IsPrimitive(Type t)
        {
            if (t.IsPrimitive || t == typeof(Decimal) || t == typeof(String))
            {
                return true;
            }
            return false;
        }
        bool IsAwesome(Type t)
        {
            return false;
        }
        bool IsStaticSize(Type t)
        {
            if (t.IsPrimitive || t == typeof(Decimal))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 표시한 필드를 저장하지 않습니다.
        /// </summary>


        static Assembly[] LoadedAssemblies;

        static ObjectSaver()
        {
            LoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

    }

    public class IgnoreSave : Attribute
    {

    }
    public class ForceSave : Attribute
    {

    }
}
