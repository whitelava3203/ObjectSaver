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
using System.Threading.Tasks;

namespace ObjectManager
{
    public class ObjectSaver
    {
        public ObjectSaver()
        {
            ClearLoadedTypes();
        }

        const byte _Initializer = 0xFF;
        const byte _ByteInit = 0xFF;
        const byte _DataEnd = 0xFE;

        static readonly byte[] DataEnd = { _Initializer, _DataEnd };

        public Header header = new Header();
        public string path;
        public object BaseObject;
        public bool createHeader = true;

        public class Header
        {
            public CompressionLevel CompressionLevel = CompressionLevel.Fastest;
            public bool Encrypted = false;
            public bool PreLoadedTypes = false;
            public bool MethodInstance = false;
            public bool FullCopyType = false;
        }

        public List<Type> loadedTypes = new List<Type>(32);

        private void ClearLoadedTypes()
        {
            loadedTypes.Clear();
            loadedTypes.Add(typeof(Int16));
            loadedTypes.Add(typeof(UInt16));
            loadedTypes.Add(typeof(Int32));
            loadedTypes.Add(typeof(UInt32));
            loadedTypes.Add(typeof(Int64));
            loadedTypes.Add(typeof(UInt64));
            loadedTypes.Add(typeof(Single));
            loadedTypes.Add(typeof(Double));
            loadedTypes.Add(typeof(Byte));
            loadedTypes.Add(typeof(SByte));
            loadedTypes.Add(typeof(Decimal));
            loadedTypes.Add(typeof(String));
        }
        private IEnumerable RealLoadedTypes
        {
            get
            {
                return loadedTypes.Skip(12);
            }
        }



        public void Save()
        {
            if(BaseObject == null)
            {
                throw new Exception("오브젝트가 설정되지 않았습니다.");
            }
            if(File.Exists(path))
            {
                File.WriteAllBytes(path, new byte[0]);
            }
            FileStream fs = File.OpenWrite(path);

            GetBytes(fs);

            fs.Close();

            return;
        }

        public byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream(4096);
            GetBytes(ms);
            byte[] output = ms.ToArray();
            ms.Close();
            return output;
        }

        public void GetBytes(Stream originstream)
        {
            if(!originstream.CanWrite)
            {
                throw new Exception("쓰기 가능한 스트림이 아닙니다.");
            }


            if(this.createHeader)
            {
                originstream.Write(new byte[] { 0x01, GetHeaderByte() });
            }
            else
            {
                originstream.WriteByte(0x00);
            }

            GZipStream stream = new GZipStream(originstream,this.header.CompressionLevel);

            if(!this.header.PreLoadedTypes)
            {
                AssignAllTypes();
            }

            WriteTypeBytes();

            WriteMiddleBytes();

            stream.Close();

            GC.Collect();

            return;

            byte GetHeaderByte()
            {
                byte b = 0x00;
                switch(this.header.CompressionLevel)
                {
                    case CompressionLevel.NoCompression:
                        b++;
                        b++;
                        break;
                    case CompressionLevel.Fastest:
                        b++;
                        break;
                }
                b += (byte)(this.header.Encrypted ? 4 : 0);
                b += (byte)(this.header.PreLoadedTypes ? 8 : 0);
                b += (byte)(this.header.MethodInstance ? 16 : 0);
                b += (byte)(this.header.FullCopyType ? 32 : 0);
                
                

                return b;
            }

            

            void WriteTypeBytes()
            {
                foreach(Type type in this.RealLoadedTypes)
                {
                    StreamWrite(type.FullName.ToByteArray());
                }
                StreamWriteDataEnd();
            }

            void WriteMiddleBytes()
            {
                WriteEverythingBytes(this.BaseObject,"");
                return;


                void WriteEverythingBytes(object obj, string name, bool writeType = true)
                {
                    Type t = obj.GetType();
                    if (writeType)
                    {
                        WriteTypeIndex(t);
                    }

                    if (name != "")
                    {
                        StreamWrite(name.ToByteArray());
                    }


                    if (IsPrimitive(t))
                    {
                        WritePrimitiveBytes(obj);
                        return;
                    }
                    if(IsString(t))
                    {
                        WriteStringBytes(obj);
                        return;
                    }
                    if (IsAwesome(t))
                    {
                        WriteAwesomeBytes(obj);
                        return;
                    }
                    if (t.IsArray)
                    {
                        WriteArrayBytes(obj);
                        return;
                    }
                    else
                    {
                        WriteObjectBytes(obj);
                        return;
                    }
                    
                }

                void WriteObjectBytes(object obj)
                {

                    Type baseType = obj.GetType();
                    for (int i = 0; baseType.BaseType != null; i++)
                    {
                        WriteObjectFieldBytes(obj, baseType, i);
                        baseType = baseType.BaseType;
                    }

                    StreamWriteDataEnd();
                    return;

                    void WriteObjectFieldBytes(object obj, Type baseType, int upper)
                    {
                        string BaseTypeIndex()
                        {
                            char[] output = new char[upper];
                            for (int i = 0; i < upper; i++)
                            {
                                output[i] = '.'; 
                            }
                            return new string(output);
                        }

                        BindingFlags bindflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                        System.Reflection.FieldInfo[] fieldinfo = baseType.GetFields(bindflags);
                        foreach (System.Reflection.FieldInfo info in fieldinfo)
                        {
                            if (info.GetValue(obj) != null && (!Attribute.IsDefined(info, typeof(IgnoreSave)) || Attribute.IsDefined(info, typeof(ForceSave))))
                            {
                                WriteEverythingBytes(info.GetValue(obj), BaseTypeIndex() + info.Name);
                            }
                        }
                    }
                }

                void WritePrimitiveBytes(object obj, bool dataEnd = false)
                {
                    switch (obj)
                    {
                        case Int16 i:
                            StreamWrite(BitConverter.GetBytes(i).Fix(), dataEnd);
                            return;
                        case UInt16 i:
                            StreamWrite(BitConverter.GetBytes(i).Fix(), dataEnd);
                            return;
                        case Int32 i:
                            StreamWrite(BitConverter.GetBytes(i).Fix(), dataEnd);
                            return;
                        case UInt32 i:
                            StreamWrite(BitConverter.GetBytes(i).Fix(), dataEnd);
                            return;
                        case Int64 i:
                            StreamWrite(BitConverter.GetBytes(i).Fix(), dataEnd);
                            return;
                        case UInt64 i:
                            StreamWrite(BitConverter.GetBytes(i).Fix(), dataEnd);
                            return;
                        case Single i:
                            StreamWrite(BitConverter.GetBytes(i).Fix(), dataEnd);
                            return;
                        case Double i:
                            StreamWrite(BitConverter.GetBytes(i).Fix(), dataEnd);
                            return;
                        case Byte i:
                            StreamWrite(BitConverter.GetBytes(i).Fix(), dataEnd);
                            return;
                        case SByte i:
                            StreamWrite(BitConverter.GetBytes(i).Fix(), dataEnd);
                            return;
                        case Decimal i:
                            StreamWrite(decimal.GetBits(i).Cast<byte>().ToArray().Fix(), dataEnd);
                            return;
                        default:
                            throw new Exception("Primitive가 아님");
                    }
                }
                void WriteStringBytes(object obj)
                {
                    StreamWrite((obj as String).ToByteArray());
                }
                void WriteAwesomeBytes(object obj)
                {
                    throw new NotImplementedException();
                }
                void WriteArrayBytes(object obj)
                {
                    Type t = obj.GetType().GetElementType();

                    if (IsPrimitive(t))//타입명, 크기 생략
                    {
                        WritePrimitiveArrayBytes(obj);
                    }
                    else if(t.IsSealed)//타입명 생략
                    {
                        WriteObjectArrayBytes(obj, false);
                    }
                    else//다써
                    {
                        WriteObjectArrayBytes(obj);
                    }

                    void WritePrimitiveArrayBytes(object obj)
                    {
                        Type elementType = obj.GetType().GetElementType();

                        

                        Array objarr = obj as Array;
                        int length = objarr.Length;

                        StreamWrite(length.ToByteArray().Trim());
                        //배열 시작부분 추가 완료

                        foreach (object o in objarr)
                        {
                            WritePrimitiveBytes(o);
                        }

                        objarr = null;
                        //배열 추가 완료
                        return;
                    }
                    void WriteObjectArrayBytes(object obj, bool writeType = true)
                    {
                        Type elementType = obj.GetType().GetElementType();

                        //배열 시작부분 추가 완료

                        Array objarr = obj as Array;

                        int length = objarr.Length;

                        StreamWrite(length.ToByteArray().Trim());

                        foreach (object o in objarr)
                        {
                            WriteEverythingBytes(o, "", writeType);
                        }

                        objarr = null;
                        //배열 추가 완료
                        return;
                    }
                }
                
                void WriteTypeIndex(Type t)
                {
                    StreamWrite(BitConverter.GetBytes(loadedTypes.IndexOf(t)).Trim());
                    return;
                }

                
            }


            void StreamWrite(byte[] bytearray, bool dataEnd = true)
            {
                if (dataEnd)
                {
                    stream.Write(bytearray.Fix().Concat(DataEnd).ToArray());
                }
                else
                {
                    stream.Write(bytearray);//고정크기는 Fix 안함
                }
            }
            void StreamWriteDataEnd()
            {
                stream.Write(DataEnd);
            }
        }


        public void AssignAllTypes()
        {
            ClearLoadedTypes();
            AssignEverything(BaseObject);
            return;


            void AssignEverything(object obj)
            {
                Type t = obj.GetType();
                
                if (IsPrimitive(t))
                {
                    return;
                }
                if (IsString(t))
                {
                    return;
                }

                AssignType(t);

                if (t.IsArray)
                {
                    AssignArray(obj);
                    return;
                }
                {
                    AssignObject(obj);
                    return;
                }
            }
            void AssignObject(object obj)
            {
                Type baseType = obj.GetType();
                for (int i = 0; baseType.BaseType != null; i++)
                {
                    AssignObjectField(baseType);
                    baseType = baseType.BaseType;
                }


                void AssignObjectField(Type t)
                {
                    BindingFlags bindflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                    System.Reflection.FieldInfo[] fieldinfo = baseType.GetFields(bindflags);
                    foreach (System.Reflection.FieldInfo info in fieldinfo)
                    {
                        if ((!Attribute.IsDefined(info, typeof(IgnoreSave)) || Attribute.IsDefined(info, typeof(ForceSave))))
                        {
                            AssignEverything(info.GetValue(obj));
                        }
                    }
                }
            }
            void AssignArray(object obj)
            {
                Type t = obj.GetType();
                Type elementType = t.GetElementType();
                if (IsPrimitive(elementType))
                {
                    return;
                }
                if (IsString(elementType))
                {
                    return;
                }
                if (elementType.IsSealed)
                {
                    Array arr = obj as Array;
                    foreach (object o in arr)
                    {
                        AssignEverything(o);
                        return;
                        //sealed라 타입이 다 똑같아서 하나만 보고 끝
                    }
                }
                else
                {
                    Array arr = obj as Array;
                    foreach (object o in arr)
                    {
                        AssignEverything(o);
                    }
                    return;
                }
            }
            void AssignType(Type t)
            {
                if (!loadedTypes.Contains(t))
                {
                    loadedTypes.Add(t);
                }
            }
        }

        public object Load()
        {
            if(path == null)
            {
                throw new Exception("경로가 설정되지 않았습니다.");
            }
            if(!File.Exists(path))
            {
                throw new Exception("경로에 파일이 없습니다.");
            }

            FileStream fs = File.OpenRead(path);
            GetObject(fs);

            fs.Close();

            return this.BaseObject;
        }

        public object GetObject(byte[] bytearray)
        {
            return GetObject(new MemoryStream(bytearray));
        }

        public object GetObject(Stream originstream)
        {
            if(!originstream.CanRead)
            {
                throw new Exception("읽기 가능한 스트림이 아닙니다.");
            }


            bool useHeader = Convert.ToBoolean(originstream.ReadByte());



            if (useHeader)
            {
                InitHeaderWithByte(originstream.ReadByte());
            }

            GZipStream stream = new GZipStream(originstream, CompressionMode.Decompress);



            if(this.header.PreLoadedTypes)
            {

            }
            else
            {
                ReadLoadedTypes();
            }

            ReadMiddle();

            GC.Collect();

            return this.BaseObject;




            void InitHeaderWithByte(int headerbyte)
            {

                this.header.CompressionLevel = (CompressionLevel)(headerbyte % 4);
                this.header.Encrypted = (headerbyte & 4) == 0 ? false : true;
                this.header.PreLoadedTypes = (headerbyte & 8) == 0 ? false : true;
                this.header.MethodInstance = (headerbyte & 16) == 0 ? false : true;
                this.header.FullCopyType = (headerbyte & 32) == 0 ? false : true;
                
                
            }

            void ReadLoadedTypes()
            {
                byte[] bytearray;
                while (true)
                {
                    bytearray = StreamRead();
                    if (bytearray.Length == 0)
                    {
                        return;
                    }
                    string typename = bytearray.GetString();
                    loadedTypes.Add(GetTypeFixed(typename));
                }
            }

            void ReadMiddle()
            {
                this.BaseObject = GetEverything(false).data;
                return;

                (object data, string name) GetEverything(bool readName = true, Type preLoadedType = null)
                {
                    int typeindex;
                    Type type;
                    if (preLoadedType == null)
                    {
                        byte[] imshi = StreamRead();
                        if (imshi.Length == 0)
                        {
                            return (null, "");
                        }
                        typeindex = BitConverter.ToInt32(imshi.Enrich());
                        type = loadedTypes[typeindex];
                    }
                    else
                    {
                        type = preLoadedType;
                        typeindex = loadedTypes.IndexOf(type);
                    }

                    string name = "";

                    if (readName)
                    {
                        byte[] imshi = StreamRead();
                        if (imshi.Length == 0)
                        {
                            return (null, "");
                        }
                        name = imshi.GetString();
                    }
                    
                    object data = null;

                    if (typeindex < 12)//String까지
                    {
                        if (typeindex == 11)//String만 빼고
                        {
                            data =  StreamReadString();
                        }
                        else
                        {
                            data = GetPrimitive(typeindex);
                        }
                    }
                    else if (type.IsArray)
                    {
                        GetArray(type);
                    }
                    else if (IsAwesome(type))
                    {
                        data = GetAwesome(type);
                    }
                    else if (IsIteratorBlock(type))
                    {
                        data = GetObject(type, Activator.CreateInstance(type, 0));
                    }
                    else//IsObject
                    {
                        data = GetObject(type);
                    }
                    return (data, name);
                }
                object GetObject(Type t, object baseObj = null)
                {
                    
                    object o = baseObj == null ? Activator.CreateInstance(t) : baseObj;
                    List<Type> baseTypes = GetBaseTypes(t);
                    List<FieldInfo[]> fieldInfos = GetBaseFieldInfos(baseTypes);

                    BindingFlags bindflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                    System.Reflection.FieldInfo[] fieldinfos = t.GetFields(bindflags);
                    while (true)
                    {
                        (object fieldobj, string fieldname) = GetEverything();
                        if(fieldobj == null)
                        {
                            break;
                        }
                        SetFieldFixed(fieldobj, fieldname);
                    }
                    return o;

                    List<Type> GetBaseTypes(Type baseType)
                    {
                        List<Type> baseTypeList = new List<Type>();
                        baseTypeList.Add(baseType);
                        Type t = baseType;
                        while (t.BaseType != null)
                        {
                            baseTypeList.Add(t.BaseType);
                            t = t.BaseType;
                        }

                        return baseTypeList;
                    }
                    List<FieldInfo[]> GetBaseFieldInfos(List<Type> types)
                    {
                        BindingFlags bindflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                        List<FieldInfo[]> fieldinfos = new List<FieldInfo[]>();
                        foreach (Type type in types)
                        {
                            fieldinfos.Add(type.GetFields(bindflags));
                        }
                        return fieldinfos;
                    }
                    void SetFieldFixed(object fieldobj, string fieldname)
                    {
                        int i = 0;
                        while(true)
                        {
                            if(fieldname[0] == '.')
                            {
                                    i++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        string realfieldname = new string(fieldname.Skip(i).ToArray());
                        FieldInfo[] realInfos = fieldInfos[i];
                        foreach (FieldInfo info in realInfos)
                        {
                            if (info.Name == fieldname)
                            {
                                info.SetValue(o, fieldobj);
                                return;
                            }
                        }
                        throw new Exception("값이 설정되지 못했습니다.");
                    }
                }
                Array GetArray(Type t)
                {
                    Type elementType = t.GetElementType();
                    if (IsPrimitive(elementType))//타입명, 크기 생략
                    {
                        return GetPrimitiveArray(t);
                    }
                    else if (t.IsSealed)//타입명 생략
                    {
                        return GetDynamicArray(t, false);
                    }
                    else//다써
                    {
                        return GetDynamicArray(t);
                    }

                    Array GetPrimitiveArray(Type t)
                    {
                        int length = BitConverter.ToInt32(StreamRead().Enrich());//배열 갯수
                        Type elementtype = t.GetElementType();
                        Array array = Array.CreateInstance(elementtype,length);
                        int elementtypeindex = loadedTypes.IndexOf(elementtype);
                        for(int i=0;i<length;i++)
                        {
                            array.SetValue(GetPrimitive(elementtypeindex), i);
                        }

                        return array;
                    }
                    Array GetDynamicArray(Type t, bool readElementType = true)
                    {
                        int length = BitConverter.ToInt32(StreamRead().Enrich());//배열 갯수
                        Type elementtype = t.GetElementType();
                        Array array = Array.CreateInstance(elementtype, length);

                        for(int i=0;i<length;i++)
                        {
                            array.SetValue(GetEverything(readElementType), i);
                        }

                        return array;
                    }
                }

                object GetPrimitive(int typeindex)
                {
                    object imshi;
                    switch (typeindex)
                    {
                        case 0:
                            imshi = BitConverter.ToInt16(StreamReadSized(2));
                            break;
                        case 1:
                            imshi = BitConverter.ToUInt16(StreamReadSized(2));
                            break;
                        case 2:
                            imshi = BitConverter.ToInt32(StreamReadSized(4));
                            break;
                        case 3:
                            imshi = BitConverter.ToUInt32(StreamReadSized(4));
                            break;
                        case 4:
                            imshi = BitConverter.ToInt64(StreamReadSized(8));
                            break;
                        case 5:
                            imshi = BitConverter.ToUInt64(StreamReadSized(8));
                            break;
                        case 6:
                            imshi = BitConverter.ToSingle(StreamReadSized(4));
                            break;
                        case 7:
                            imshi = BitConverter.ToDouble(StreamReadSized(8));
                            break;
                        case 8:
                            throw new NotImplementedException();
                        case 9:
                            throw new NotImplementedException();
                        case 10:
                            imshi = ByteArrayToDecimal(StreamReadSized(16));
                            break;
                        default:
                            throw new Exception("오류");



                    }
                    return imshi;


                    const byte DecimalSignBit = 128;
                    decimal ByteArrayToDecimal(byte[] src, int offset = 0)
                    {
                        return new decimal(
                            BitConverter.ToInt32(src, offset),
                            BitConverter.ToInt32(src, offset + 4),
                            BitConverter.ToInt32(src, offset + 8),
                            src[offset + 15] == DecimalSignBit,
                            src[offset + 14]);
                    }
                }
                object GetAwesome(Type t)
                {
                    object data;
                    using (var ms = new MemoryStream(StreamRead()))
                    {
                        data = new BinaryFormatter().Deserialize(ms);
                    }
                    return data;
                }
            }

            
            byte[] StreamRead()
            {
                List<byte> bytelist = new List<byte>(32);
                byte b;
                byte b2;
                while(true)
                {
                    b = Convert.ToByte(stream.ReadByte());
                    if(b == _Initializer)
                    {
                        b2 = Convert.ToByte(stream.ReadByte());
                        switch(b2)
                        {
                            case _DataEnd:
                                if(bytelist.Count == 0)
                                {
                                    return new byte[0];
                                }
                                return bytelist.ToArray();
                            default:
                                bytelist.Add(_Initializer);
                                bytelist.Add(b2);
                                break;
                        }
                    }
                    else
                    {
                        bytelist.Add(b);
                    }

                }
            }
            byte[] StreamReadSized(int count)
            {
                byte[] bytearray = new byte[count];
                stream.Read(bytearray, 0, count);
                return bytearray;
            }
            string StreamReadString()
            {
                return Encoding.UTF8.GetString(StreamRead());
            }
            
        }


        bool IsPrimitive(Type t)
        {
            if (t.IsPrimitive || t == typeof(Decimal))
            {
                return true;
            }
            return false;
        }
        bool IsString(Type t)
        {
            if(t == typeof(String))
            {
                return true;
            }
            return false;
        }
        bool IsAwesome(Type t)
        {
            if (IsCapsuledMethod(t))
            {
                return true;
            }
            return false;
        }
        bool IsIteratorBlock(Type t)
        {
            if (
                Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute))
                &&
                t.IsSealed
                &&
                t.GetInterfaces().Contains(typeof(IEnumerator))
            )
            {
                return true;
            }

            return false;
        }
        bool IsCapsuledMethod(Type t)
        {
            if (
                IsAction(t)
                ||
                IsFunc(t)
            )
            {
                return true;
            }
            return false;
        }
        bool IsAction(Type t)
        {
            if (t == typeof(System.Action)) return true;
            Type generic = null;
            if (t.IsGenericTypeDefinition) generic = t;
            else if (t.IsGenericType) generic = t.GetGenericTypeDefinition();

            if (generic == null) return false;
            if (generic == typeof(System.Action<>)) return true;
            if (generic == typeof(System.Action<,>)) return true;
            if (generic == typeof(System.Action<,,>)) return true;
            if (generic == typeof(System.Action<,,,>)) return true;
            if (generic == typeof(System.Action<,,,,>)) return true;
            if (generic == typeof(System.Action<,,,,,>)) return true;
            if (generic == typeof(System.Action<,,,,,,>)) return true;
            if (generic == typeof(System.Action<,,,,,,,>)) return true;
            if (generic == typeof(System.Action<,,,,,,,,>)) return true;
            if (generic == typeof(System.Action<,,,,,,,,,>)) return true;
            if (generic == typeof(System.Action<,,,,,,,,,,>)) return true;
            if (generic == typeof(System.Action<,,,,,,,,,,,>)) return true;
            if (generic == typeof(System.Action<,,,,,,,,,,,,>)) return true;
            if (generic == typeof(System.Action<,,,,,,,,,,,,,>)) return true;
            if (generic == typeof(System.Action<,,,,,,,,,,,,,,>)) return true;
            if (generic == typeof(System.Action<,,,,,,,,,,,,,,,>)) return true;

            return false;
        }
        bool IsFunc(Type t)
        {
            Type generic = null;
            if (t.IsGenericTypeDefinition) generic = t;
            else if (t.IsGenericType) generic = t.GetGenericTypeDefinition();

            if (generic == null) return false;
            if (generic == typeof(System.Func<>)) return true;
            if (generic == typeof(System.Func<,>)) return true;
            if (generic == typeof(System.Func<,,>)) return true;
            if (generic == typeof(System.Func<,,,>)) return true;
            if (generic == typeof(System.Func<,,,,>)) return true;
            if (generic == typeof(System.Func<,,,,,>)) return true;
            if (generic == typeof(System.Func<,,,,,,>)) return true;
            if (generic == typeof(System.Func<,,,,,,,>)) return true;
            if (generic == typeof(System.Func<,,,,,,,,>)) return true;
            if (generic == typeof(System.Func<,,,,,,,,,>)) return true;
            if (generic == typeof(System.Func<,,,,,,,,,,>)) return true;
            if (generic == typeof(System.Func<,,,,,,,,,,,>)) return true;
            if (generic == typeof(System.Func<,,,,,,,,,,,,>)) return true;
            if (generic == typeof(System.Func<,,,,,,,,,,,,,>)) return true;
            if (generic == typeof(System.Func<,,,,,,,,,,,,,,>)) return true;
            if (generic == typeof(System.Func<,,,,,,,,,,,,,,,>)) return true;
            if (generic == typeof(System.Func<,,,,,,,,,,,,,,,,>)) return true;

            return false;
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
        static Assembly[] LoadedAssemblies;

        static ObjectSaver()
        {
            LoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        }


    }
    internal static class ExtensionClass
    {
        const byte _Initializer = 0xFF;
        const byte _ByteInit = 0xFF;
        const byte _DataEnd = 0xFE;

        internal static byte[] Enrich(this byte[] bytearray)
        {
            byte[] output = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            if(bytearray.Length > 4)
            {
                throw new Exception();
            }
            for(int i=0;i<bytearray.Length;i++)
            {
                output[i] = bytearray[i];
            }

            return output;
        }
        internal static byte[] ToByteArray(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        internal static byte[] ToByteArray(this int i)
        {
            return BitConverter.GetBytes(i).Trim();
        }

        internal static string GetString(this byte[] bytearray)
        {
            return Encoding.UTF8.GetString(bytearray);
        }
        internal static string GetString(this List<byte> bytearray)
        {
            return Encoding.UTF8.GetString(bytearray.ToArray());
        }

        internal static byte[] Fix(this byte[] bytearray)
        {
            List<byte> newlist = new List<byte>(bytearray.Length * 2);
            foreach(byte b in bytearray)
            {
                if(b == _Initializer)
                {
                    newlist.Add(_Initializer);
                    newlist.Add(_ByteInit);
                }
                else
                {
                    newlist.Add(b);
                }
            }
            return newlist.ToArray();
        }

        internal static byte[] Trim(this byte[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == 0x00)
                {
                    return input.Take(i).ToArray();
                }
            }
            return input;
        }
    }
    //헤더
    //사용할 타입들
    //내용

    //타입 + 이름 + 

    public class IgnoreSave : Attribute
    {

    }
    public class ForceSave : Attribute
    {

    }
    public class SaveTypeItself : Attribute
    {

    }
    public class SaveMethodAsInstance : Attribute
    {

    }
}
