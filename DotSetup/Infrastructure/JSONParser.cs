// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;



namespace DotSetup
{
    public static class JSONParser
    {
        public static string ObjToJSON(object obj)
        {
            string json = (obj != null) ? obj.ToString() : "";
            if (string.IsNullOrEmpty(json) || (json[0] != '{'))
                json = "{" + json + "}";
            json = json.Replace(" =", "\":").Replace(", ", ",\"").Replace("{ ", "{\"");
            return json;
        }

        public static string DictionaryToJson(Dictionary<string, string> dict)
        {
            string[] entries = dict.Select(d => string.Format("\"{0}\":{1}", d.Key, d.Value[0] == '{' ? d.Value : "\"" + d.Value + "\"")).ToArray();
            return "{" + string.Join(",", entries) + "}";
        }

        public static string Escape(string json)
        {
            return json.Replace("\b", @"\b").Replace("\f", @"\f").Replace("\n", @"\n").Replace("\r", @"\r").Replace("\t", @"\t").Replace("\"", "\\\"");
        }

        // From https://stackoverflow.com/questions/1207731/how-can-i-deserialize-json-to-a-simple-dictionarystring-string-in-asp-net
        public static Dictionary<string, object> JsonToDictionary(string jsonString)
        {
            return ParseJSON(jsonString, 0, out _);
        }

        private static Dictionary<string, object> ParseJSON(string json, int start, out int end)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            bool escbegin = false;
            bool escend = false;
            bool inquotes = false;
            string key = null;
            StringBuilder sb = new StringBuilder();
            List<object> arraylist = null;
            Regex regex = new Regex(@"\\u([0-9a-z]{4})", RegexOptions.IgnoreCase);
            int autoKey = 0;
            for (int i = start; i < json.Length; i++)
            {
                char c = json[i];
                if (c == '\\')
                    escbegin = !escbegin;
                if (!escbegin)
                {
                    if (c == '"')
                    {
                        inquotes = !inquotes;
                        if (!inquotes && arraylist != null)
                        {
                            arraylist.Add(DecodeString(regex, sb.ToString()));
                            sb.Length = 0;
                        }
                        continue;
                    }
                    if (!inquotes)
                    {
                        switch (c)
                        {
                            case '{':
                                if (i != start)
                                {
                                    var child = ParseJSON(json, i, out var cend);
                                    if (arraylist != null)
                                        arraylist.Add(child);
                                    else
                                    {
                                        dict.Add(key, child);
                                        key = null;
                                    }
                                    i = cend;
                                }
                                continue;
                            case '}':
                                end = i;
                                if (key != null)
                                {
                                    if (arraylist != null)
                                        dict.Add(key, arraylist);
                                    else
                                        dict.Add(key, DecodeString(regex, sb.ToString()));
                                }
                                return dict;
                            case '[':
                                arraylist = new List<object>();
                                continue;
                            case ']':
                                if (key == null)
                                {
                                    key = "array" + autoKey.ToString();
                                    autoKey++;
                                }
                                if (arraylist != null && sb.Length > 0)
                                {
                                    arraylist.Add(sb.ToString());
                                    sb.Length = 0;
                                }
                                dict.Add(key, arraylist);
                                arraylist = null;
                                key = null;
                                continue;
                            case ',':
                                if (arraylist == null && key != null)
                                {
                                    dict.Add(key, DecodeString(regex, sb.ToString()));
                                    key = null;
                                    sb.Length = 0;
                                }
                                if (arraylist != null && sb.Length > 0)
                                {
                                    arraylist.Add(sb.ToString());
                                    sb.Length = 0;
                                }
                                continue;
                            case ':':
                                key = DecodeString(regex, sb.ToString()).Trim();
                                sb.Length = 0;
                                continue;
                        }
                    }
                }
                sb.Append(c);
                if (escend)
                    escbegin = false;
                if (escbegin)
                    escend = true;
                else
                    escend = false;
            }
            end = json.Length - 1;
            return dict; //theoretically shouldn't ever get here
        }
        private static string DecodeString(Regex regex, string str)
        {
            return Regex.Unescape(regex.Replace(str, match => char.ConvertFromUtf32(int.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber))));
        }
    }
}
