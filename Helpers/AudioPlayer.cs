using NAudio.Wave;
using System.IO;
using System.Media;

namespace ODEliteTracker.Helpers
{
    internal class AudioPlayer
    {
        internal static Task PlayFile(string? file, float volume = 1f)
        {
            return Task.Factory.StartNew(() =>
            {
                if (string.IsNullOrEmpty(file) == false
                    && File.Exists(file))
                {
                    try
                    {
                        using var audioFile = new AudioFileReader(file);
                        using var outputDevice = new WaveOutEvent();
                        outputDevice.Init(audioFile);
                        outputDevice.Volume = volume;
                        outputDevice.Play();
                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        { }
                        return;
                    }
                    catch
                    {
                        SystemSounds.Beep.Play();
                        return;
                    }
                }

                SystemSounds.Beep.Play();
            });
        }
    }
}
