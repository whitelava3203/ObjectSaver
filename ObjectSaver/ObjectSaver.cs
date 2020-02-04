using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectSaver
{
    
    public static class ObjectSaver
    {
        public static List<Assembly> LoadedAssemblies = new List<Assembly>();
        private static Type GetType(string str)
        {
            Type t = Type.GetType(str);
            if(t!=null)
            {
                return t;
            }
            else
            {
                foreach(Assembly assembly in LoadedAssemblies)
                {
                    t = assembly.GetType(str);
                    if (t != null)
                        return t;
                }
            }
            return null;
        }

        public static byte[] ToBytes(this object obj, string objname)
        {
            if (IsFirst)
            {
                LoadBase();
                IsFirst = false;
            }
            return ObjectTree.SetTree(new NamedObject(obj, objname)).GetBytes().ToArray();
        }
        public static void Save(this object obj, string objname, string path)
        {
            if (IsFirst)
            {
                LoadBase();
                IsFirst = false;
            }
            File.WriteAllBytes(path, obj.ToBytes("MyObject"));
        }
        public static void Save(this object obj, string path)
        {
            if (IsFirst)
            {
                LoadBase();
                IsFirst = false;
            }
            File.WriteAllBytes(path, obj.ToBytes("MyObject"));
        }
        public static object Load(string path)
        {
            if (IsFirst)
            {
                LoadBase();
                IsFirst = false;
            }
            return ObjectTree.BytesToTree(File.ReadAllBytes(path).ToList()).GetObject().Data;
        }
        public static T Load<T>(string path)
        {
            if (IsFirst)
            {
                LoadBase();
                IsFirst = false;
            }
            return (T)ObjectTree.BytesToTree(File.ReadAllBytes(path).ToList()).GetObject().Data;
        }

        private static bool IsFirst = true;
        private static void LoadBase()
        {
            ObjectInfo.ObjectToInfoFunc.Add(typeof(int), (nameobj) =>
            {
                ObjectInfo info = new ObjectInfo();
                info.Data = BitConverter.GetBytes((int)nameobj.Data);
                info.ObjectType = typeof(int);
                info.ObjectName = nameobj.Name;
                return info;
            });
            ObjectInfo.ObjectToInfoFunc.Add(typeof(uint), (nameobj) =>
            {
                ObjectInfo info = new ObjectInfo();
                info.Data = BitConverter.GetBytes((uint)nameobj.Data);
                info.ObjectType = typeof(uint);
                info.ObjectName = nameobj.Name;
                return info;
            });
            ObjectInfo.ObjectToInfoFunc.Add(typeof(long), (nameobj) =>
            {
                ObjectInfo info = new ObjectInfo();
                info.Data = BitConverter.GetBytes((long)nameobj.Data);
                info.ObjectType = typeof(long);
                info.ObjectName = nameobj.Name;
                return info;
            });
            ObjectInfo.ObjectToInfoFunc.Add(typeof(ulong), (nameobj) =>
            {
                ObjectInfo info = new ObjectInfo();
                info.Data = BitConverter.GetBytes((ulong)nameobj.Data);
                info.ObjectType = typeof(ulong);
                info.ObjectName = nameobj.Name;
                return info;
            });
            ObjectInfo.ObjectToInfoFunc.Add(typeof(short), (nameobj) =>
            {
                ObjectInfo info = new ObjectInfo();
                info.Data = BitConverter.GetBytes((short)nameobj.Data);
                info.ObjectType = typeof(short);
                info.ObjectName = nameobj.Name;
                return info;
            });
            ObjectInfo.ObjectToInfoFunc.Add(typeof(ushort), (nameobj) =>
            {
                ObjectInfo info = new ObjectInfo();
                info.Data = BitConverter.GetBytes((ushort)nameobj.Data);
                info.ObjectType = typeof(ushort);
                info.ObjectName = nameobj.Name;
                return info;
            });
            ObjectInfo.ObjectToInfoFunc.Add(typeof(float), (nameobj) =>
            {
                ObjectInfo info = new ObjectInfo();
                info.Data = BitConverter.GetBytes((float)nameobj.Data);
                info.ObjectType = typeof(float);
                info.ObjectName = nameobj.Name;
                return info;
            });
            ObjectInfo.ObjectToInfoFunc.Add(typeof(double), (nameobj) =>
            {
                ObjectInfo info = new ObjectInfo();
                info.Data = BitConverter.GetBytes((double)nameobj.Data);
                info.ObjectType = typeof(double);
                info.ObjectName = nameobj.Name;
                return info;
            });
            ObjectInfo.ObjectToInfoFunc.Add(typeof(bool), (nameobj) =>
            {
                ObjectInfo info = new ObjectInfo();
                info.Data = BitConverter.GetBytes((bool)nameobj.Data);
                info.ObjectType = typeof(bool);
                info.ObjectName = nameobj.Name;
                return info;
            });
            ObjectInfo.ObjectToInfoFunc.Add(typeof(char), (nameobj) =>
            {
                ObjectInfo info = new ObjectInfo();
                info.Data = BitConverter.GetBytes((char)nameobj.Data);
                info.ObjectType = typeof(char);
                info.ObjectName = nameobj.Name;
                return info;
            });
            ObjectInfo.ObjectToInfoFunc.Add(typeof(string), (nameobj) =>
            {
                ObjectInfo info = new ObjectInfo();
                info.Data = Encoding.UTF8.GetBytes((string)nameobj.Data);
                info.ObjectType = typeof(string);
                info.ObjectName = nameobj.Name;
                return info;
            });
            ObjectInfo.ObjectToInfoFunc.Add(typeof(decimal), (nameobj) =>
            {
                return null;
            });
            ObjectInfo.ObjectToInfoFunc.Add(typeof(byte), (nameobj) =>
            {
                return null;
            });
            ObjectInfo.ObjectToInfoFunc.Add(typeof(sbyte), (nameobj) =>
            {
                return null;
            });
            ObjectInfo.DefaultToInfoFunc = (nameobj) =>
            {
                return null;
            };

            ObjectInfo.InfoToObjectFunc.Add(typeof(int), (info) =>
            {
                return new NamedObject(BitConverter.ToInt32(info.Data, 0), info.ObjectName);
            });
            ObjectInfo.InfoToObjectFunc.Add(typeof(uint), (info) =>
            {
                return new NamedObject(BitConverter.ToUInt32(info.Data, 0), info.ObjectName);
            });
            ObjectInfo.InfoToObjectFunc.Add(typeof(long), (info) =>
            {
                return new NamedObject(BitConverter.ToInt64(info.Data, 0), info.ObjectName);
            });
            ObjectInfo.InfoToObjectFunc.Add(typeof(ulong), (info) =>
            {
                return new NamedObject(BitConverter.ToUInt64(info.Data, 0), info.ObjectName);
            });
            ObjectInfo.InfoToObjectFunc.Add(typeof(short), (info) =>
            {
                return new NamedObject(BitConverter.ToInt16(info.Data, 0), info.ObjectName);
            });
            ObjectInfo.InfoToObjectFunc.Add(typeof(ushort), (info) =>
            {
                return new NamedObject(BitConverter.ToUInt16(info.Data, 0), info.ObjectName);
            });
            ObjectInfo.InfoToObjectFunc.Add(typeof(float), (info) =>
            {
                return new NamedObject(BitConverter.ToSingle(info.Data, 0), info.ObjectName);
            });
            ObjectInfo.InfoToObjectFunc.Add(typeof(double), (info) =>
            {
                return new NamedObject(BitConverter.ToDouble(info.Data, 0), info.ObjectName);
            });
            ObjectInfo.InfoToObjectFunc.Add(typeof(bool), (info) =>
            {
                return new NamedObject(BitConverter.ToBoolean(info.Data, 0), info.ObjectName);
            });
            ObjectInfo.InfoToObjectFunc.Add(typeof(char), (info) =>
            {
                return new NamedObject(BitConverter.ToChar(info.Data, 0), info.ObjectName);
            });
            ObjectInfo.InfoToObjectFunc.Add(typeof(string), (info) =>
            {
                return new NamedObject(Encoding.UTF8.GetString(info.Data), info.ObjectName);
            });
            ObjectInfo.InfoToObjectFunc.Add(typeof(decimal), (info) =>
            {
                return null;
            });
            ObjectInfo.InfoToObjectFunc.Add(typeof(byte), (info) =>
            {
                return null;
            });
            ObjectInfo.InfoToObjectFunc.Add(typeof(sbyte), (info) =>
            {
                return null;
            });
            ObjectInfo.InfoToDefaultFunc = (info) =>
            {
                return null;
            };



            ObjectTree.ObjectToTreeFunc.Add(typeof(Array), (nameobj) =>
            {
                int i = 0;
                ObjectTree tree = new ObjectTree(nameobj.type, nameobj.Name);
                ((IList)nameobj.Data).Cast<object>().ToList().ForEach((obj) => { tree.AddObject(new NamedObject(obj, i++.ToString())); });
                return tree;
            });
            ObjectTree.DefaultToTreeFunc = (nameobj) =>
            {
                if (nameobj.Data.GetType().IsArray)
                {
                    return ObjectTree.ObjectToTreeFunc[typeof(Array)](nameobj);
                }
                ObjectTree objtree = new ObjectTree(nameobj.type, nameobj.Name);
                Type objtype = nameobj.Data.GetType();
                BindingFlags bindflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                System.Reflection.FieldInfo[] fieldinfo =
                   objtype.GetFields(bindflags);
                foreach (System.Reflection.FieldInfo info in fieldinfo)
                {
                    Type infotype = info.FieldType;
                    object infovalue = info.GetValue(nameobj.Data);
                    if (infovalue != null && (!Attribute.IsDefined(info, typeof(IgnoreSave))) || Attribute.IsDefined(info, typeof(ForceSave)))
                    {
                        objtree.AddObject(new NamedObject(infovalue, info.Name));
                    }
                }

                return objtree;

            };

            ObjectTree.TreeToObjectFunc.Add(typeof(Array), (basetree) =>
            {
                //object obj = Activator.CreateInstance(basetree.type);

                ArrayList al = new ArrayList();
                foreach (ObjectInfo info in basetree.InnerDataList)
                {
                    al.Add(info.GetValue());
                }
                foreach (ObjectTree tree in basetree.InnerTreeList)
                {
                    al.Add(tree.GetObject().Data);
                }
                return new NamedObject(al.ToArray(basetree.type.GetElementType()), basetree.ObjectName);
            });
            ObjectTree.TreeToDefaultFunc = (basetree) =>
            {
                if (basetree.type.IsArray)
                {
                    return ObjectTree.TreeToObjectFunc[typeof(Array)](basetree);
                }


                NamedObject nameobj = new NamedObject(Activator.CreateInstance(basetree.type), basetree.ObjectName);
                BindingFlags bindflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                List<System.Reflection.FieldInfo> infolist =
                    basetree.type.GetFields(bindflags).ToList<FieldInfo>();
                foreach (ObjectInfo objinfo in basetree.InnerDataList)
                {
                    foreach (FieldInfo info in infolist)
                    {
                        if (info.Name == objinfo.ObjectName)
                        {
                            info.SetValue(nameobj.Data, objinfo.GetValue());
                            break;
                        }

                    }
                    continue;
                }
                foreach (ObjectTree objtree in basetree.InnerTreeList)
                {
                    foreach (FieldInfo info in infolist)
                    {
                        if (info.Name == objtree.ObjectName)
                        {
                            Func<ObjectTree, NamedObject> function;
                            Type objtype = objtree.type;
                            if (objtype == null)
                                break;
                            if (ObjectTree.TreeToObjectFunc.TryGetValue(objtype, out function))
                            {
                                info.SetValue(nameobj.Data, function(objtree).Data);
                            }
                            else
                            {
                                info.SetValue(nameobj.Data, ObjectTree.TreeToDefaultFunc(objtree).Data);
                            }

                            break;
                        }

                    }
                    continue;



                }

                return nameobj;

            };
        }

        static ObjectSaver()
        {

        }


        public class NamedObject
        {
            public object Data;
            public string Name;
            public NamedObject(object obj, string objname)
            {
                Data = obj;
                Name = objname;
            }
            ~NamedObject()
            {
                Data = null;
                Name = null;
            }
            public Type type
            {
                get
                {
                    return Data.GetType();
                }
            }
        }
        public class ObjectInfo
        {
            public static Dictionary<Type, Func<NamedObject, ObjectInfo>> ObjectToInfoFunc = new Dictionary<Type, Func<NamedObject, ObjectInfo>>();
            public static Func<NamedObject, ObjectInfo> DefaultToInfoFunc;
            public static Dictionary<Type, Func<ObjectInfo, NamedObject>> InfoToObjectFunc = new Dictionary<Type, Func<ObjectInfo, NamedObject>>();
            public static Func<ObjectInfo, NamedObject> InfoToDefaultFunc;

            public Type ObjectType;
            public string ObjectName;
            public Byte[] Data;
            public string GetPath()
            {
                return ObjectType.ToString() + @" " + ObjectName;
            }

            public ObjectInfo()
            {

            }

            public object GetValue()
            {
                if (this.Data != null)
                {
                    Func<ObjectInfo, NamedObject> function;
                    if (ObjectInfo.InfoToObjectFunc.TryGetValue(this.ObjectType, out function))
                    {
                        return function(this).Data;
                    }
                    else
                    {
                        return InfoToDefaultFunc(this).Data;
                    }
                }

                return null;
            }

            public static ObjectInfo GetInfo(NamedObject nameobj)
            {
                if (nameobj != null)
                {
                    if (nameobj.Data != null)
                    {
                        Func<NamedObject, ObjectInfo> function;
                        if (ObjectToInfoFunc.TryGetValue(nameobj.type, out function))
                        {
                            return function(nameobj);
                        }
                    }
                    else
                    {
                        return DefaultToInfoFunc(nameobj);
                    }
                }
                return null;
            }


            public List<byte> GetBytes()
            {
                List<byte> baselist = new List<byte>();
                List<byte> imshilist = new List<byte>();
                baselist.AddRange(this.ObjectType.ToString().ToBytes());
                baselist.Add(32);
                baselist.AddRange(this.ObjectName.ToBytes());
                baselist.Add(32);
                baselist.AddRange(BitConverter.GetBytes(this.Data.Length));
                baselist.AddRange(this.Data);
                return baselist;
            }

            public static ObjectInfo BytesToInfo(List<byte> baselist)// this is ref
            {
                ObjectInfo baseinfo = new ObjectInfo();
                //baselist.CopyTo(bytelist);
                baseinfo.ObjectType = ObjectSaver.GetType(ReadNext(ref baselist));
                baseinfo.ObjectName = ReadNext(ref baselist);
                int length = ReadInt(ref baselist);
                baseinfo.Data = ReadValue(ref baselist, length).ToArray();


                return baseinfo;
            }
        }
        public class ObjectTree
        {
            public static Dictionary<Type, Func<NamedObject, ObjectTree>> ObjectToTreeFunc = new Dictionary<Type, Func<NamedObject, ObjectTree>>();
            public static Func<NamedObject, ObjectTree> DefaultToTreeFunc;
            public static Dictionary<Type, Func<ObjectTree, NamedObject>> TreeToObjectFunc = new Dictionary<Type, Func<ObjectTree, NamedObject>>();
            public static Func<ObjectTree, NamedObject> TreeToDefaultFunc;
            public List<ObjectTree> InnerTreeList = new List<ObjectTree>();
            public List<ObjectInfo> InnerDataList = new List<ObjectInfo>();
            public Type type;
            public string ObjectName;
            public string GetPath()
            {
                return type.ToString() + @" " + ObjectName;
            }
            public ObjectTree()
            {

            }
            public ObjectTree(Type type, string objname)
            {
                this.type = type;
                this.ObjectName = objname;
            }
            public ObjectTree(NamedObject nameobj)
            {
                this.type = nameobj.type;
                this.ObjectName = nameobj.Name;
            }

            public static ObjectTree SetTree(NamedObject nameobj)
            {
                return ObjectTree.DefaultToTreeFunc(nameobj);
            }
            public void AddObject(NamedObject nameobj)
            {

                Type objtype = nameobj.Data.GetType();
                if (objtype == null)
                    return;
                if (IsPrimitive(objtype))
                {
                    Func<NamedObject, ObjectInfo> function;
                    if (ObjectInfo.ObjectToInfoFunc.TryGetValue(objtype, out function))
                    {
                        this.InnerDataList.Add(function(nameobj));
                    }
                    else
                    {
                        this.InnerDataList.Add(ObjectInfo.DefaultToInfoFunc(nameobj));
                    }
                }
                else
                {
                    Func<NamedObject, ObjectTree> function;
                    if (objtype.IsArray)
                    {
                        this.InnerTreeList.Add(ObjectTree.ObjectToTreeFunc[typeof(Array)](nameobj));
                    }
                    if (ObjectTree.ObjectToTreeFunc.TryGetValue(objtype, out function))
                    {
                        this.InnerTreeList.Add(function(nameobj));
                    }
                    else
                    {
                        this.InnerTreeList.Add(DefaultToTreeFunc(nameobj));
                    }
                }


            }

            public List<byte> GetBytes()
            {
                List<byte> baselist = new List<byte>();
                List<byte> imshilist = new List<byte>();
                baselist.AddRange(this.type.ToString().ToBytes());
                baselist.Add(32);
                baselist.AddRange(this.ObjectName.ToBytes());
                baselist.Add(32);



                foreach (ObjectInfo info in this.InnerDataList)
                {
                    imshilist.AddRange(info.GetBytes());
                }
                foreach (ObjectTree tree in this.InnerTreeList)
                {
                    imshilist.AddRange(tree.GetBytes());
                }

                baselist.AddRange(BitConverter.GetBytes(imshilist.Count));
                baselist.AddRange(imshilist);




                return baselist;
            }

            public static ObjectTree BytesToTree(List<byte> baselist)
            {
                ObjectTree basetree = new ObjectTree();
                basetree.type = ObjectSaver.GetType(ReadNext(ref baselist));
                if (basetree.type == null)
                {
                    throw new Exception("타입이 null");
                }
                basetree.ObjectName = ReadNext(ref baselist);
                if (basetree.ObjectName == null)
                {
                    throw new Exception("이름이 null");
                }
                int length = ReadInt(ref baselist);
                List<byte> newlist = ReadValue(ref baselist, length);
                while (newlist.Count > 0)
                {

                    Type type = ObjectSaver.GetType(SafeReadNext(newlist));

                    if (IsPrimitive(type))
                    {
                        basetree.InnerDataList.Add(ObjectInfo.BytesToInfo(newlist));
                    }
                    else
                    {
                        basetree.InnerTreeList.Add(ObjectTree.BytesToTree(newlist));
                    }
                }

                return basetree;
            }


            public void Solidfy(string path)
            {
                Solidfy(new DirectoryInfo(path));
            }
            public void Solidfy(DirectoryInfo basedi)
            {
                //Console.WriteLine("Save Object " + this.type.ToString());
                //nowpath = Path.Combine(di.FullName, this.GetPath());
                DirectoryInfo di = new DirectoryInfo(Path.Combine(basedi.FullName, this.GetPath()));
                string imshi = this.GetPath();
                try
                {
                    di.Create();
                }
                catch (DirectoryNotFoundException ex)
                {
                    throw new DirectoryNotFoundException("폴더 경로가 길어서 생성할수 없음. 경로 : " + di.FullName);
                }
                Console.WriteLine(di.FullName);
                foreach (ObjectInfo info in this.InnerDataList)
                {
                    List<string> teststr = new List<string>();
                    teststr.Add(di.FullName);
                    teststr.Add(info.GetPath());
                    File.WriteAllBytes(Path.Combine(di.FullName, info.GetPath()), info.Data);
                }
                foreach (ObjectTree tree in this.InnerTreeList)
                {
                    tree.Solidfy(di);
                }
            }
            public void Compress(string path)
            {
                string folderpath = Path.Combine(path, this.GetPath());
                this.Solidfy(path);
                ZipFile.CreateFromDirectory(folderpath, Path.Combine(path, this.ObjectName + @".dat"));
                DirectoryInfo di = new DirectoryInfo(folderpath);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
                di.Delete();
            }
            public static ObjectTree DeSolidfy(string path)
            {
                return DeSolidfy(new DirectoryInfo(path));
            }
            public static ObjectTree DeSolidfy(DirectoryInfo basedi)
            {

                Type type;
                string name;
                if (FixName(basedi.Name, out type, out name))
                {

                    DirectoryInfo[] dis;
                    FileInfo[] fis;
                    dis = basedi.GetDirectories("*", SearchOption.TopDirectoryOnly);
                    fis = basedi.GetFiles();
                    ObjectTree basetree = new ObjectTree(type, name);
                    foreach (FileInfo fi in fis)
                    {

                        if (FixName(fi.Name, out type, out name))
                        {
                            ObjectInfo info = new ObjectInfo();
                            info.Data = File.ReadAllBytes(fi.FullName);
                            info.ObjectName = name;
                            info.ObjectType = type;
                            basetree.InnerDataList.Add(info);
                        }
                    }
                    foreach (DirectoryInfo di in dis)
                    {
                        basetree.InnerTreeList.Add(DeSolidfy(di));
                    }
                    return basetree;
                }


                return null;



            }
            private static bool FixName(string dirname, out Type type, out string name)
            {
                string[] strs = dirname.Split(' ');
                if (strs.Length != 2)
                {
                    type = null;
                    name = null;
                    return false;
                }

                type = ObjectSaver.GetType(strs[0]);
                name = strs[1];

                if (type == null || name == null || name == "")
                {
                    type = null;
                    name = null;
                    return false;
                }

                return true;
            }
            public NamedObject GetObject()
            {

                return ObjectTree.TreeToDefaultFunc(this);


            }






        }


        public class IgnoreSave : Attribute
        {

        }
        public class ForceSave : Attribute
        {

        }
        private static string SafeReadNext(List<byte> list)
        {
            int i = 0;
            foreach (byte b in list)
            {
                if (b == (byte)32)
                {
                    byte[] bytes = list.GetRange(0, i).ToArray();
                    return Encoding.UTF8.GetString(bytes);
                }
                i++;
            }
            throw new Exception("더이상 스페이스바가 없음");
        }
        private static string ReadNext(ref List<byte> list)
        {
            int i = 0;
            foreach (byte b in list)
            {
                if (b == (byte)32)
                {
                    byte[] bytes = list.GetRange(0, i).ToArray();
                    list.RemoveRange(0, i + 1);
                    return Encoding.UTF8.GetString(bytes);
                }
                i++;
            }
            throw new Exception("더이상 스페이스바가 없음");
        }
        private static int ReadInt(ref List<byte> list)
        {
            int i = BitConverter.ToInt32(list.GetRange(0, 4).ToArray(), 0);

            list.RemoveRange(0, 4);
            return i;
        }
        private static List<byte> ReadValue(ref List<byte> list, int length)
        {
            List<byte> bytelist = list.GetRange(0, length);
            list.RemoveRange(0, length);
            return bytelist;
        }

        public static byte[] ToBytes(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }









        public static bool IsPrimitive(Type type)
        {
            try
            {
                if (type == typeof(string))
                    return true;
                return IsPrimitive(Activator.CreateInstance(type));
            }
            catch (MissingMethodException ex)
            {
                return false;
            }
        }
        private static bool IsPrimitive(object obj)
        {
            switch (obj)
            {
                case int _:
                case short _:
                case long _:
                case uint _:
                case ushort _:
                case ulong _:
                case char _:
                case decimal _:
                case double _:
                case float _:
                case byte _:
                case bool _:
                case sbyte _:
                case string _:
                    return true;
                default:
                    return false;

            }
        }





    }

}
