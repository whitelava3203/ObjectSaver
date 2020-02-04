# ObjectSaver
ANGGIMODDI
확장메서드 object.Save(string path);
를쓰면 저장할수있고
T ObjectSaver.Load<T>(string path);
를 쓰면 불러올수있다 ㄷㄷㄷ
아그리고 쓰기전에
ObjectSaver.ObjectSaver.LoadedAssemblies.Add(Assembly.GetExecutingAssembly());
이거 해둬야 내가정의한 클래스도 저장됨
다른 dll 참조했다면 다 넣어야함