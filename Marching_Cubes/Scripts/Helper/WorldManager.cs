using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

public class WorldManager : Singleton<WorldManager>
{
    [SerializeField] public string world = "default"; //World selected by the manager
    public const string WORLDS_DIRECTORY = "/worlds"; //Directory worlds (save folder, that contains the worlds folders)

    public static byte[] RawSerializeEx(object anything)
    {
        int rawsize = Marshal.SizeOf(anything);
        byte[] rawdatas = new byte[rawsize];
        GCHandle handle = GCHandle.Alloc(rawdatas, GCHandleType.Pinned);
        System.IntPtr buffer = handle.AddrOfPinnedObject();
        Marshal.StructureToPtr(anything, buffer, false);
        handle.Free();
        return rawdatas;
    }
    void resourcesToPersistent()
    {

        Directory.CreateDirectory(Application.persistentDataPath + WORLDS_DIRECTORY + "/" + world);
        Debug.Log("resourcesToPersistent");
        var txt = Resources.Load<TextAsset>("worlds/" + "4" + "/0.0").bytes;
        File.WriteAllBytes(Application.persistentDataPath + "/worlds/" + world + "/0.0.reg", (txt));
        for (int i = 0; i <= 4; i++)
        {

            txt = Resources.Load<TextAsset>("worlds/" + i + "/0.0").bytes;
            File.WriteAllBytes(Application.persistentDataPath + "/worlds/" + world + "/0.0.reg", (txt));

            txt = Resources.Load<TextAsset>("worlds/" + i + "0.-1").bytes;
            File.WriteAllBytes(Application.persistentDataPath + "/worlds/" + world + "/0.-1.reg", (txt));

            txt = Resources.Load<TextAsset>("worlds/" + i + "-1.0").bytes;
            File.WriteAllBytes(Application.persistentDataPath + "/worlds/" + world + "/-1.0.reg", (txt));

            txt = Resources.Load<TextAsset>("worlds/" + i + "-1.-1").bytes;
            File.WriteAllBytes(Application.persistentDataPath + "/worlds/" + world + "/-1.-1.reg", (txt));
        }
    }
    private void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);

            if (!Directory.Exists(Application.persistentDataPath + WORLDS_DIRECTORY))//in case worlds directory not created, create the "worlds" directory 
                Directory.CreateDirectory(Application.persistentDataPath + WORLDS_DIRECTORY);

            if (!Directory.Exists(Application.persistentDataPath + WORLDS_DIRECTORY + "/" + world))//in case world not created, create the world (generate folder)
            {
                // resourcesToPersistent();
                Directory.CreateDirectory(Application.persistentDataPath + WORLDS_DIRECTORY + "/" + world);   //original

            }
        }
    }

    /// <summary>
    /// Create and select a new world (save/load folder), a worldConfig can be passed as second optional parameter for being used by the Noisemanager (or empty for default one).
    /// </summary>
    public static bool CreateWorld(string worldName, NoiseManager.WorldConfig newWorldConfig = null)
    {
        if (!Directory.Exists(Application.persistentDataPath + WORLDS_DIRECTORY + '/' + worldName))
        {
            Directory.CreateDirectory(Application.persistentDataPath + WORLDS_DIRECTORY + '/' + worldName);
            Instance.world = worldName;
            if (newWorldConfig != null)//Use the WorldConfig passed as parameter
            {
                string worldConfig = JsonUtility.ToJson(newWorldConfig);
                File.WriteAllText(Application.persistentDataPath + WORLDS_DIRECTORY + '/' + worldName + "/worldConfig.json", worldConfig);
            }
            else//Use the default world config
            {
                newWorldConfig = new NoiseManager.WorldConfig();
                newWorldConfig.worldSeed = Random.Range(int.MinValue, int.MaxValue);
                string worldConfig = JsonUtility.ToJson(newWorldConfig);
                File.WriteAllText(Application.persistentDataPath + WORLDS_DIRECTORY + '/' + worldName + "/worldConfig.json", worldConfig);
            }
            return true;
        }
        else
        {
            Debug.LogError("folder already exists");
            return false;
        }
    }

    /// <summary>
    /// Delete a world (save/load folder) and remove all related files.
    /// </summary>
    public static bool DeleteWorld(string worldName)
    {
        if (Directory.Exists(Application.persistentDataPath + WORLDS_DIRECTORY + '/' + worldName))
        {
            Directory.Delete(Application.persistentDataPath + WORLDS_DIRECTORY + '/' + worldName, true);
            return true;
        }
        else
        {
            Debug.LogError("folder not exists");
            return false;
        }
    }

    /// <summary>
    /// Select a world (save/load folder) which will load next time by the ChunkSystem.
    /// </summary>
    public static bool SelectWorld(string worldName)
    {

        if (Directory.Exists(Application.persistentDataPath + WORLDS_DIRECTORY + '/' + worldName))
        {
            Instance.world = worldName;
            return true;
        }
        else
        {
            Debug.LogError("world (folder) not exists");
            return false;
        }
    }

    /// <summary>
    /// Return the name of the selected world.
    /// </summary>
    public static string GetSelectedWorldName()
    {
        return Instance.world;
    }

    /// <summary>
    /// Return the path of the selected world.
    /// </summary>
    public static string GetSelectedWorldDir()
    {
        return Application.persistentDataPath + WORLDS_DIRECTORY + "/" + Instance.world;
    }

    /// <summary>
    /// Return WorldConfig of the selected world.
    /// </summary>
    public static NoiseManager.WorldConfig GetSelectedWorldConfig()
    {
        string selectedWorld = GetSelectedWorldName();
        if (File.Exists(Application.persistentDataPath + WORLDS_DIRECTORY + '/' + selectedWorld + "/worldConfig.json"))
        {
            string json = File.ReadAllText(Application.persistentDataPath + WORLDS_DIRECTORY + '/' + selectedWorld + "/worldConfig.json");
            return JsonUtility.FromJson<NoiseManager.WorldConfig>(json);
        }
        else
        {
            Debug.Log("No worldConfig.json exist, generating a new one, using the default parameters.");
            NoiseManager.WorldConfig newWorldConfig = new NoiseManager.WorldConfig();
            newWorldConfig.worldSeed = NoiseManager.Instance.worldConfig.worldSeed;
            string worldConfig = JsonUtility.ToJson(newWorldConfig);
            //  File.WriteAllText(Application.persistentDataPath + WORLDS_DIRECTORY + '/' + selectedWorld + "/worldConfig.json", worldConfig);
            return newWorldConfig;
        }

    }

    /// <summary>
    /// Return all the worlds as a string[].
    /// </summary>
    public static string[] GetAllWorlds()
    {
        return Directory.GetDirectories(Application.persistentDataPath + WORLDS_DIRECTORY).Select(Path.GetFileName).ToArray();

    }

    /// <summary>
    /// Return size of a world.
    /// </summary>
    public static long worldSize(string worldName)
    {
        if (Directory.Exists(Application.persistentDataPath + WORLDS_DIRECTORY + '/' + worldName))
            return new DirectoryInfo(Application.persistentDataPath + WORLDS_DIRECTORY + '/' + worldName).GetFiles("*.*", SearchOption.TopDirectoryOnly).Sum(file => file.Length);
        else
            return 0;
    }
}
