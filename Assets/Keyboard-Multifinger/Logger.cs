using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.IO;
#if WINDOWS_UWP
using Windows.Storage;
#endif

public class Logger : MonoBehaviour
{
    public string filepath = "Assets/Output";
    public string gaze_filepath = "Assets/Output";
    public string filename = "output.csv";
    public string gaze_filename = "gaze-output.csv";
    public string folder = "Output";
    public EyeTracking eyetracker;

    private Queue<string> q = new Queue<string>();
    private Queue<string> gaze_q = new Queue<string>();

    public async void Start()
    {
        filename = DateTime.Now.ToString("yyyy-MM-dd.hh-mm-ss") + "." + filename;
        gaze_filename = DateTime.Now.ToString("yyyy-MM-dd.hh-mm-ss") + "." + gaze_filename;
        await startLog();
    }

    async Task startLog()
    {
#if WINDOWS_UWP
        StorageFolder sessionParentFolder = await KnownFolders.PicturesLibrary
                .CreateFolderAsync(folder,
                CreationCollisionOption.OpenIfExists);
        filepath = sessionParentFolder.Path;
        gaze_filepath = sessionParentFolder.Path;
#endif
    }

    // Logs the user's gaze position
    // hh.mm.ss.FFF, gaze_x, gaze_y, gaze_z
    private void FixedUpdate()
    {
        Vector3 gazePos = eyetracker.getPosition();
        gaze_q.Enqueue(DateTime.Now.ToString("hh.mm.ss.FFF") + "," + gazePos.x + "," + gazePos.y + "," + gazePos.z);
    }

    public void write(string s)
    {
        q.Enqueue(DateTime.Now.ToString("hh.mm.ss.FFF") + "," + s);
    }

    // Logs whenever a key is pressed.
    // hh.mm.ss.FFF, ACTIVE_KEY_PRESS, key_value, finger, finger_x, finger_y, finger_z, gaze_x, gaze_y, gaze_z
    public void write_key(string s, string f)
    {
        Vector3 fingerPos;
        if (GameObject.Find(f) != null)
            fingerPos = GameObject.Find(f).transform.position;
        else
            fingerPos = Vector3.zero;
        Vector3 gazePos = eyetracker.getPosition();

        q.Enqueue(DateTime.Now.ToString("hh.mm.ss.FFF") + ",ACTIVE_KEY_PRESS," + s + "," + f + "," + fingerPos.x + "," + fingerPos.y + "," + fingerPos.z + "," + gazePos.x + "," + gazePos.y + "," + gazePos.z);

    }

    // Logs whenever an inactive key is pressed
    // hh.mm.ss.FFF, INACTIVE_KEY_PRESS, key_value, finger, finger_x, finger_y, finger_z, gaze_x, gaze_y, gaze_z
    public void write_inactive_key(string s, string f)
    {
        Vector3 fingerPos = GameObject.Find(f).transform.position;
        Vector3 gazePos = eyetracker.getPosition();

        q.Enqueue(DateTime.Now.ToString("hh.mm.ss.FFF") + ",INACTIVE_KEY_PRESS," + s + "," + f + "," + fingerPos.x + "," + fingerPos.y + "," + fingerPos.z + "," + gazePos.x + "," + gazePos.y + "," + gazePos.z);

    }

    // Logs whenever the user looks at a different key or away from the keyboard
    // hh.mm.ss.FFF, GAZE_POSITION, key_value
    public void write_gaze(string s)
    {
        q.Enqueue(DateTime.Now.ToString("hh.mm.ss.FFF") + ",GAZE_POSITION," + s);

    }

    // Logs whenever the user is prompted with a new sentence
    // hh.mm.ss.FFF, sentence, {PRACTICE/TEST}, sentence_number
    public void write_sentence(string sentence, string type, int num)
    {
        q.Enqueue(DateTime.Now.ToString("hh.mm.ss.FFF") + ",\"" + sentence + "\"," + type + "," + num);
    }

    // Logs the WPM and error rate of a given sentence, as well as the string that was typed.
    // hh.mm.ss.FFF, SENTENCE_STATS, "typedSentence", WPM, errorRate
    public void write_sentence_stats(string typedSentence, double WPM, double error)
    {
        q.Enqueue(DateTime.Now.ToString("hh.mm.ss.FFF") + ",SENTENCE_STATS,\"" + typedSentence + "\"," + WPM + "," + error);
    }

    // Writes all gathered data
    public void write_all_data()
    {
        string path = Path.Combine(filepath, filename);
        StreamWriter sw = new StreamWriter(path, true);
        while (q.Count != 0)
        {
            sw.WriteLine(q.Dequeue());
        }
        sw.Close();

        string gaze_path = Path.Combine(gaze_filepath, gaze_filename);
        sw = new StreamWriter(gaze_path, true);
        while (gaze_q.Count != 0)
        {
            sw.WriteLine(gaze_q.Dequeue());
        }
        sw.Close();
    }
}
