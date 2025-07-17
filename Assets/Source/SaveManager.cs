using QFSW.QC;
using System.Collections.Generic;
using UnityEngine;

namespace Quinn
{
	public class SaveManager : MonoBehaviour
	{
		private static SaveManager _instance;

		private readonly Dictionary<string, object> _data = new();

		private void Awake()
		{
			_instance = this;
		}

		public static void Save<T>(string path, T data)
		{
			if (_instance._data.ContainsKey(path))
			{
				_instance._data[path] = data;
			}
			else
			{
				_instance._data.Add(path, data);
			}
		}
		public static void Save(string path)
		{
			Save(path, new object());
		}

		public static bool IsSaved(string path)
		{
			return _instance._data.ContainsKey((string)path);
		}

		public static bool Delete(string path)
		{
			if (_instance._data.Remove(path))
			{
				return true;
			}

			return false;
		}

		public static T Load<T>(string path)
		{
			if (_instance._data.TryGetValue(path, out object result))
			{
				return (T)result;
			}

			return default;
		}

		public static bool TryLoad<T>(string path, out T value)
		{
			if (_instance._data.TryGetValue(path, out object result))
			{
				value = (T)result;
				return true;
			}

			value = default;
			return false;
		}

		[Command("save", "Save a key without any payload.")]
		protected void Save_Cmd(string path)
		{
			Save(path, new object());
		}

		[Command("delete", "Save a key.")]
		protected void Delete_Cmd(string path)
		{
			if (!Delete(path))
			{
				Log.Notice("Key doesn't exist!");
			}
		}

		[Command("save.list", "List all save entries.")]
		protected void List_Cmd()
		{
			var builder = new System.Text.StringBuilder();

			builder.AppendLine("Save Data:");

			if (_data.Count > 0)
			{
				foreach (var item in _data)
				{
					builder.AppendLine($"  - {item.Key}: {item.Value}");
				}
			}
			else
			{
				builder.AppendLine("  - No Save Entries");
			}

				Log.Notice(builder);
		}
	}
}
