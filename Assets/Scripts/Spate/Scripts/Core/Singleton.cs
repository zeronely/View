using System;

/// <summary>
/// 所有要成为Singleton都必须保存默认构造方法，否则Activator创建实例会失败
/// </summary>
public class Singleton<T> where T : ISingleton, new()
{
    private static T _inst;

    public static T Inst
    {
        get
        {
            if (Singleton<T>._inst == null)
            {
                Singleton<T>._inst = Activator.CreateInstance<T>();
            }
            return Singleton<T>._inst;
        }
    }
}
