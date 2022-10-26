using System.Collections.Generic;
using System.IO;

namespace Framework.CustomEditor
{
	public enum ResType
	{
		PREFAB,
		SOUND,
		ANIMATION,
	}
	public class ResRefRecorder
	{

		private Dictionary<ResType, HashSet<string>> m_dict = new Dictionary<ResType, HashSet<string>>();

		public ResRefRecorder()
		{
			m_dict.Add(ResType.PREFAB, new HashSet<string>());
			m_dict.Add(ResType.SOUND, new HashSet<string>());
			m_dict.Add(ResType.ANIMATION, new HashSet<string>());
		}

		public void RecordPrefab(string path)
		{
			if (string.IsNullOrEmpty(path)) return;
			m_dict[ResType.PREFAB].Add(path);
		}

		public void RecordSound(string path)
		{
			if (string.IsNullOrEmpty(path)) return;
			m_dict[ResType.SOUND].Add(path);
		}

		public void RecordAnimation(string path)
		{
			if (string.IsNullOrEmpty(path)) return;
			m_dict[ResType.ANIMATION].Add(path);
		}

		public HashSet<string> GetRecords(ResType type)
		{
			return m_dict[type];
		}

		public void Save(string filePath)
		{
			if (File.Exists(filePath)) File.Delete(filePath);

			FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
			StreamWriter writer = new StreamWriter(stream);

			writer.WriteLine("<<PREFAB");
			foreach (string path in m_dict[ResType.PREFAB])
			{
				writer.WriteLine(path);
			}
			writer.WriteLine("<<SOUND");
			foreach (string path in m_dict[ResType.SOUND])
			{
				writer.WriteLine(path);
			}
			writer.WriteLine("<<ANIMATION");
			foreach (string path in m_dict[ResType.ANIMATION])
			{
				writer.WriteLine(path);
			}

			writer.Close();
			stream.Close();
		}
	}
}
