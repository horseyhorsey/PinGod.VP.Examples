using Godot;
using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Memory Mapping which COM controller also has access to. Saves events, states of switches, coils, lamps + leds <para/>
/// Mutex=pingod_vp_mutex | MapName=pingod_vp
/// </summary>
public class MemoryMap : IDisposable
{
    internal static System.Threading.Mutex mutex;
    const string MAP_NAME = "pingod_vp";
    const int MAP_SIZE = 2048;
    const string MUTEX_NAME = "pingod_vp_mutex";
    const int WRITE_DELAY = 10;  //low cpu
    private int _offsetLamps;
    private int _offsetLeds;
    private int _offsetSwitches;
    private MemoryMappedViewAccessor _switchMapping;
    private Task _writeStatesTask;
    int COIL_COUNT = 32 * 2;
    int LAMP_COUNT = 64 * 2;
    int LED_COUNT = 64 * 3; //Led 3 = Num, State, Color (ole)
    private MemoryMappedFile mmf;
    bool mutexCreated;
    int SWITCH_COUNT = 64 * 2;
    private readonly bool writeStates;
    private readonly bool readStates;
    private readonly PinGodGame pinGodGame;
    byte[] switchBuffer = new byte[64 * 2];
    private CancellationTokenSource tokenSource;
    private MemoryMappedViewAccessor viewAccessor;

    /// <summary>
    /// Sets up memory mapping and offsets
    /// </summary>
    /// <param name="coilCount"></param>
    /// <param name="lampCount"></param>
    /// <param name="ledCount"></param>
    /// <param name="switchCount"></param>
    public MemoryMap(byte coilCount = 32, byte lampCount = 64, byte ledCount = 64, byte switchCount = 64, bool writeStates = true, bool readStates = true, PinGodGame pinGodGame = null)
    {
        this.COIL_COUNT = coilCount * 2;
        this.LAMP_COUNT = lampCount * 2;
        this.LED_COUNT = ledCount * 3;
        this.SWITCH_COUNT = switchCount;
        this.writeStates = writeStates;
        this.readStates = readStates;
        this.pinGodGame = pinGodGame;
        _offsetLamps = COIL_COUNT;
        _offsetLeds = COIL_COUNT + LAMP_COUNT;
        _offsetSwitches = _offsetLeds + (LED_COUNT * sizeof(int));

        SetUp();
    }

    /// <summary>
    /// Dispose and releases the mutex and stops the read/write states thread <see cref="tokenSource"/>
    /// </summary>
    public void Dispose() => Dispose(true);

    /// <summary>
    /// Starts the <see cref="_writeStatesTask"/>.
    /// </summary>
    /// <param name="writeDelay"></param>
    public void Start(int writeDelay = WRITE_DELAY)
    {
        if (_writeStatesTask != null)
        {
            Logger.LogDebug("memory map already initialized!");
            return;
        }

        //write states to memory
        tokenSource = new CancellationTokenSource();
        _writeStatesTask = Task.Run(async () =>
        {
            Logger.LogDebug("Running Read / Write States...");
            while (!tokenSource.IsCancellationRequested)
            {
                if (writeStates) WriteStates();
                if (readStates) ReadStates();
                await Task.Delay(writeDelay);
            }

            Logger.LogDebug("write states stopped...");
        }, tokenSource.Token);
    }

    /// <summary>
    /// Stops the <see cref="_writeStatesTask"/> thread
    /// </summary>
    public void Stop()
    {
        if (!tokenSource.IsCancellationRequested)
        {
            tokenSource?.Cancel();
        }
    }

    /// <summary>
    /// Disposes of the memory map and mutex
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {            
            mmf?.Dispose();
            if (mutexCreated)
            {
                try { mutex?.ReleaseMutex(); } catch { }
            }

            Stop();
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Reads the buffer size <see cref="SWITCH_COUNT"/> switch states from the memory map buffer at position 0. Acts on any new switch events found. <para/>
    /// Emits a signal on VpCommand found. Switch 0 changed will process game states, switch zero useed with GameSyncState.
    /// </summary>
    private void ReadStates()
    {
        byte[] buffer = new byte[SWITCH_COUNT];
        _switchMapping.ReadArray(0, buffer, 0, buffer.Length);

        if (switchBuffer != buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] != switchBuffer[i])
                {                   
                    //override sending switch if this is a visual pinball command
                    if(pinGodGame?.GameSettings?.VpCommandSwitchId > 0 && pinGodGame?.GameSettings?.VpCommandSwitchId == i)
                    {
                        pinGodGame?.EmitSignal("VpCommand", buffer[i]);
                    }
                    else if (i > 0)
                    {
                        bool actionState = (bool)GD.Convert(buffer[i], Variant.Type.Bool);
                        var ev = new InputEventAction() { Action = $"sw{i}", Pressed = actionState };
                        Input.ParseInputEvent(ev);
                    }
                    else // Use Switch 0 for game GameSyncState
                    {                        
                        var syncState = (GameSyncState)buffer[i];
                        var action = ProcessGameState(syncState);
                        //GD.Print("zero switched action, ", action);                        
                    }
                }
            }
        }

        switchBuffer = buffer;
    }

    private string ProcessGameState(GameSyncState syncState)
    {
        var ev = new InputEventAction() { Action = "", Pressed = true };
        switch (syncState)
        {
            case GameSyncState.quit:
                ev.Action = GameSyncState.quit.ToString();
                break;
            case GameSyncState.pause: //pause / resume on a toggle, not held down
            case GameSyncState.resume:
                ev.Action = GameSyncState.pause.ToString();
                ev.Pressed = true;
                break;
            case GameSyncState.None:
            default:
                break;
        }

        if (!string.IsNullOrWhiteSpace(ev.Action))
        {
            Input.ParseInputEvent(ev);
        }        
        return ev.Action;
    }

    /// <summary>
    /// Creates a mutex and opens the memory map. Creates the <see cref="viewAccessor"/> and <see cref="_switchMapping"/>. 
    /// </summary>
    private void SetUp()
    {
        if (!Engine.EditorHint)
        {
            if (this.writeStates || this.readStates)
            {
                mutexCreated = System.Threading.Mutex.TryOpenExisting(MUTEX_NAME, out mutex);
                if (!mutexCreated)
                {
                    Logger.LogDebug("couldn't find mutex:", MUTEX_NAME, " creating new");
                    mutex = new System.Threading.Mutex(true, MUTEX_NAME, out mutexCreated);
                }
                else
                {
                    Logger.LogDebug("mutex found:", MAP_NAME);
                }

                mmf = MemoryMappedFile.CreateOrOpen(MAP_NAME, MAP_SIZE);
                viewAccessor = mmf.CreateViewAccessor(0, MAP_SIZE, MemoryMappedFileAccess.ReadWrite);
                _switchMapping = mmf.CreateViewAccessor(_offsetSwitches, LAMP_COUNT * 2, MemoryMappedFileAccess.Read);
                //GD.Print("offset for switches: ", _offsetSwitches);
            }
            else
            {
                Logger.LogDebug("mem_map: read/write states disabled");
            }
        }
    }

    /// <summary>
    /// Writes coils, lamps and leds
    /// </summary>
    void WriteStates()
    {
        //var start = OS.GetTicksMsec();        
        //GD.Print("write states");

        //get game machine states
        var coilBytes = Machine.Coils.GetStatesArray(COIL_COUNT);
        var lampsBytes = Machine.Lamps.GetStatesArray(LAMP_COUNT);
        var ledArray = Machine.Leds.GetLedStatesArray(LED_COUNT);

        //write states
        viewAccessor.WriteArray(0, coilBytes, 0, coilBytes.Length);
        viewAccessor.WriteArray(_offsetLamps, lampsBytes, 0, lampsBytes.Length);
        viewAccessor.WriteArray(_offsetLeds, ledArray, 0, ledArray.Length);

        //Print("states written in:", OS.GetTicksMsec() - start);
    }
}
