using Godot;
using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Threading.Tasks;

public class MemoryMap : IDisposable
{
    const int WRITE_DELAY = 10;  //low cpu
    const int MAP_SIZE = 2048;
    const string MAP_NAME = "pingod_vp";
    const string MUTEX_NAME = "pingod_vp_mutex";

    const int COIL_COUNT = 32 * 2;
    const int LAMP_COUNT = 64 * 2;
    const int LED_COUNT  = 64 * 3; //Led 3 = Num, State, Color (ole)

    internal static System.Threading.Mutex mutex;
    private MemoryMappedFile mmf;
    bool mutexCreated;
    private MemoryMappedViewAccessor viewAccessor;
    private CancellationTokenSource tokenSource;
    private Task _writeStatesTask;

    public MemoryMap()
    {
        if (!Engine.EditorHint)
        {
            mutexCreated = System.Threading.Mutex.TryOpenExisting(MUTEX_NAME, out mutex);
            if (!mutexCreated)
            {
                GD.Print("Couldn't find mutex:", MUTEX_NAME, " creating new");
                mutex = new System.Threading.Mutex(true, MUTEX_NAME, out mutexCreated);
            }
            else
            {
                GD.Print("mutex found:", MAP_NAME);
            }

            mmf = MemoryMappedFile.CreateOrOpen(MAP_NAME, MAP_SIZE);
            viewAccessor = mmf.CreateViewAccessor(0, MAP_SIZE, MemoryMappedFileAccess.ReadWrite);
        }
    }

    public void Start(int writeDelay = WRITE_DELAY)
    {
        if (_writeStatesTask != null)
        {
            GD.Print("memory map already initialized!");
            return;
        }

        //write states to memory
        tokenSource = new CancellationTokenSource();
        _writeStatesTask = Task.Run(async () =>
        {
            GD.Print("Running Write States...");
            await Task.Delay(500);
            while (!tokenSource.IsCancellationRequested)
            {
                WriteStates();
                await Task.Delay(writeDelay);
            }

            GD.Print("write states stopped...");
        }, tokenSource.Token);
    }
    public void Stop()
    {
        if(!tokenSource.IsCancellationRequested)
        {
            tokenSource?.Cancel();
        }
    }

    /// <summary>
    /// Writes coils, lamps and leds
    /// </summary>
    void WriteStates()
    {
        //var start = OS.GetTicksMsec();        
        var coilBytes = Machine.Coils.GetStatesArray(COIL_COUNT);
        var lampsBytes = Machine.Lamps.GetStatesArray(LAMP_COUNT);
        var ledArray = Machine.Leds.GetLedStatesArray(LED_COUNT);

        int offSet = 0;
        viewAccessor.WriteArray(0, coilBytes, 0, coilBytes.Length);
        offSet += coilBytes.Length;
        viewAccessor.WriteArray(offSet, lampsBytes, 0, lampsBytes.Length);
        offSet += lampsBytes.Length;
        viewAccessor.WriteArray(offSet, ledArray, 0, ledArray.Length);
        //Print("states written in:", OS.GetTicksMsec() - start);		
    }

    public void Dispose() => Dispose(true);
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Stop();
            mmf?.Dispose();
            if (mutexCreated)
            {
                try { mutex?.ReleaseMutex(); } catch { }
            }                
        }
        GC.SuppressFinalize(this);
    }
}
