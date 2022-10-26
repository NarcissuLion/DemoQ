using Framework.Log;

namespace Framework.Utils
{
	public class LazySingleton<T> where T : class,new()
	{
		protected static T _instance;
		public static T Instance
		{
			get
			{
				return _instance;
			}
		}

		public static void CreateInstance()
		{
			if (_instance != null)
			{
				Logger.LogError("Duplicate Lazy Singleton Instance: " + typeof(T).Name);
				return;
			}
			_instance = new T();
		}

		public static void CreateInstance<T2>() where T2 : T, new()
		{
			if (_instance != null)
			{
				Logger.LogError("Duplicate Lazy Singleton Instance: " + typeof(T2).Name);
				return;
			}
			_instance = new T2();
		}

		public virtual void Dispose()
		{
			_instance = null;
		}
	}
}
