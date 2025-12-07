using System.IO;
using UnityEngine;

// This static class handles logging episode rewards to a CSV file in the project root directory
public static class EpisodeLogger
{
    private static StreamWriter _writer;
    private static int _episodeCounter = 0;
    private static bool _initialized = false;

    // Ensures the StreamWriter is initialized
    private static void EnsureWriter()
    {
        if (_initialized) return;

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string path = Path.Combine(projectRoot, "episode_rewards.csv");

        _writer = new StreamWriter(path, append: false); // Overwrite existing file
        _writer.WriteLine("episode,reward");
        _writer.Flush();

        Debug.Log("[LOGGING] Rewards: " + path);
        _initialized = true;
    }

    // Logs the reward for an episode
    public static void LogEpisode(float reward)
    {
        EnsureWriter();
        _episodeCounter++;

        _writer.WriteLine($"{_episodeCounter},{reward}");
        _writer.Flush();
    }

    // Closes the writer
    public static void Close()
    {
        if (_writer != null)
        {
            _writer.Flush();
            _writer.Close();
            _writer = null;
            _initialized = false;
        }
    }
}
