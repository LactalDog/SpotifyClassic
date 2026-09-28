using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Phone.Globalization;

namespace SpotifyClassic.Data
{
    public class AlphaKeyGroup<T> : List<T>
    {
        public delegate string GetKeyDelegate(T item);

        public string Key { get; private set; }

        public AlphaKeyGroup(string key)
        {
            Key = key;
        }

        public static List<AlphaKeyGroup<T>> CreateGroups(IEnumerable<T> items, CultureInfo ci, GetKeyDelegate getKey, bool sort)
        {
            SortedLocaleGrouping slg = new SortedLocaleGrouping(ci);
            List<AlphaKeyGroup<T>> list = CreateDefaultGroups(slg);

            foreach (T item in items)
            {
                string keyVal = getKey(item);
                if (string.IsNullOrEmpty(keyVal))
                {
                    keyVal = "#";
                }

                int groupIndex = slg.GetGroupIndex(keyVal);
                if (groupIndex >= 0 && groupIndex < slg.GroupDisplayNames.Count)
                {
                    string label = slg.GroupDisplayNames[groupIndex];
                    int targetIndex = list.FindIndex(g => g.Key.Equals(label, StringComparison.InvariantCultureIgnoreCase));
                    if (targetIndex >= 0 && targetIndex < list.Count)
                    {
                        list[targetIndex].Add(item);
                    }
                }
            }

            if (sort)
            {
                foreach (AlphaKeyGroup<T> group in list)
                {
                    group.Sort((c0, c1) => ci.CompareInfo.Compare(getKey(c0), getKey(c1)));
                }
            }

            return list;
        }

        private static List<AlphaKeyGroup<T>> CreateDefaultGroups(SortedLocaleGrouping slg)
        {
            List<AlphaKeyGroup<T>> list = new List<AlphaKeyGroup<T>>();
            foreach (string str in slg.GroupDisplayNames)
            {
                list.Add(new AlphaKeyGroup<T>(str));
            }
            return list;
        }
    }
}