
namespace Framework.Utils
{
	public class Singleton<T> where T : class,new()
	{
		protected static T _instance;
		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new T();
				}
				return _instance;
			}
		}

		public virtual void Dispose()
		{
			_instance = null;
		}
	}
}
