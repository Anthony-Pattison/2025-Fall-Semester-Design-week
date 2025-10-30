using System.IO;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
public class ArduinoInput : MonoBehaviour
{
    Thread IOthread = new Thread(Datathread);

     static bool LeftDive;
     static bool RightDive;
     static float WingFlap;
     static string[] _values;

    public bool LFTDive;
    public bool RIGDive;
    public float flap;
    private static void Datathread()
    {
        SerialPort serialData = new SerialPort("COM7", 115200);
        serialData.Open();

        while (true)
        {
            string ard = serialData.ReadLine();
            _values = ard.Split(',');
            WingFlap = float.Parse(_values[0]);
            LeftDive = System.Convert.ToBoolean(int.Parse(_values[1]));
            RightDive = System.Convert.ToBoolean(int.Parse(_values[2]));
            Thread.Sleep(200);
        }

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       // serialData.ReadTimeout = 500;
       IOthread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        LFTDive = LeftDive;
        RIGDive = RightDive;
        flap = WingFlap;
    }

    
}
