using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class EnvLoader
{
    private static Dictionary<string, string> envVariables = new Dictionary<string, string>();

    static EnvLoader()
    {
        LoadEnvFile();
    }

    private static void LoadEnvFile()
    {
        string path = Path.Combine(Application.streamingAssetsPath, ".env");

        if (File.Exists(path))
        {
            foreach (string line in File.ReadAllLines(path))
            {
                if (!string.IsNullOrWhiteSpace(line) && line.Contains("="))
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        envVariables[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning(".env file not found!");
        }
    }

    public static string Get(string key)
    {
        return envVariables.ContainsKey(key);
    }
}
