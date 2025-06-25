using System.Collections.Generic;

public static class DictionaryExtensions
{
	public static TKey GetKeyOf<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
	{
		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
		{
			if (EqualityComparer<TValue>.Default.Equals(pair.Value, value))
			{
				return pair.Key;
			}
		}

		return default(TKey);
	}
}
