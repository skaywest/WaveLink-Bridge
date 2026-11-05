using System.Net.Http.Json;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace BeatriceBridge;

internal static class Program
{
    private const uint WmHotkey = 0x0312, WmQuit = 0x0012;
    private const uint ModAlt = 0x0001, ModControl = 0x0002, ModShift = 0x0004, ModNoRepeat = 0x4000;
    private static readonly HttpClient Http = new() { BaseAddress = new Uri("http://127.0.0.1:5005/"), Timeout = TimeSpan.FromSeconds(3) };
    private static readonly SemaphoreSlim ApiLock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly List<uint> RegisteredIds = new();
    private static readonly List<HotkeyAction> RegisteredActions = new();
    private static readonly Dictionary<string, int> ExpectedSpeakerCounts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["jvs"] = 100,
        ["official_1"] = 4,
        ["old_tts"] = 8
    };

    [STAThread]
    private static async Task<int> Main()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "bridge.json");
        if (!File.Exists(configPath))
        {
            var examplePath = Path.Combine(AppContext.BaseDirectory, "bridge.example.json");
            if (!File.Exists(examplePath))
            {
                Console.Error.WriteLine($"Missing configuration. Expected bridge.json or bridge.example.json in {AppContext.BaseDirectory}");
                return 2;
            }
            configPath = examplePath;
            Console.WriteLine("Using bridge.example.json. Copy it to bridge.json to customize hotkeys.");
        }
        BridgeConfig config;
        try
        {
            config = JsonSerializer.Deserialize<BridgeConfig>(await File.ReadAllTextAsync(configPath), JsonOptions)
                     ?? throw new InvalidDataException("Configuration is empty.");
            ValidateConfig(config);
        }
        catch (Exception ex) { Console.Error.WriteLine($"Invalid bridge.json: {ex.Message}"); return 2; }

        try
        {
            var status = await GetStatusAsync();
            Console.WriteLine($"Connected to Beatrice. Model={status.CurrentModel}; voices={status.SpeakerCount}; bypass={status.Bypass}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Beatrice API unavailable at 127.0.0.1:5005: {ex.Message}");
            Console.Error.WriteLine("Start the Beatrice 112-voice Windows app first.");
            return 3;
        }

        uint id = 1;
        foreach (var item in config.Hotkeys)
        {
            var (mods, key) = ParseHotkey(item.Key);
            if (!RegisterHotKey(IntPtr.Zero, id, mods | ModNoRepeat, key))
            {
                Console.Error.WriteLine($"Could not register {item.Key}; another app may use it.");
                UnregisterAll(); return 4;
            }
            RegisteredIds.Add(id++);
            RegisteredActions.Add(item.Value);
            Console.WriteLine($"{item.Key} -> {Describe(item.Value)}");
        }

        Console.WriteLine("Bridge active. Ctrl+C to exit.");
        var actionQueue = Channel.CreateBounded<HotkeyAction>(new BoundedChannelOptions(16)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true
        });
        using var shutdown = new CancellationTokenSource();
        var actionWorker = ProcessActionQueueAsync(actionQueue.Reader, shutdown.Token);
        uint mainThreadId = GetCurrentThreadId();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            if (!PostThreadMessage(mainThreadId, WmQuit, IntPtr.Zero, IntPtr.Zero))
                Console.Error.WriteLine($"Could not post shutdown message: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
        };
        try
        {
            int messageResult;
            while ((messageResult = GetMessage(out var message, IntPtr.Zero, 0, 0)) > 0)
            {
                if (message.message != WmHotkey) continue;
                int index = checked((int)message.wParam.ToInt64() - 1);
                if (index >= 0 && index < RegisteredActions.Count && !actionQueue.Writer.TryWrite(RegisteredActions[index]))
                    Console.Error.WriteLine("Action queue is full; hotkey action was ignored.");
            }
            if (messageResult == -1)
            {
                int error = Marshal.GetLastWin32Error();
                Console.Error.WriteLine($"Windows message loop failed: {new Win32Exception(error).Message} (error {error}).");
                return 5;
            }
        }
        finally
        {
            UnregisterAll();
            actionQueue.Writer.TryComplete();
            shutdown.Cancel();
            try { await actionWorker; }
            catch (OperationCanceledException) { }
        }
        return 0;
    }

    private static async Task ProcessActionQueueAsync(ChannelReader<HotkeyAction> reader, CancellationToken cancellationToken)
    {
        await foreach (var action in reader.ReadAllAsync(cancellationToken))
            await ExecuteActionAsync(action, cancellationToken);
    }

    private static async Task ExecuteActionAsync(HotkeyAction action, CancellationToken cancellationToken)
    {
        await ApiLock.WaitAsync(cancellationToken);
        try
        {
            if (action.ToggleBypass)
            {
                var s = await GetStatusAsync(cancellationToken);
                bool requestedBypass = !s.Bypass;
                await GetJsonAsync($"set_config?bypass={(requestedBypass ? "true" : "false")}", cancellationToken);
                var deadline = DateTime.UtcNow.AddSeconds(2);
                do
                {
                    s = await GetStatusAsync(cancellationToken);
                    if (s.Bypass == requestedBypass) break;
                    await Task.Delay(50, cancellationToken);
                } while (DateTime.UtcNow < deadline);
                if (s.Bypass != requestedBypass)
                    throw new TimeoutException("Beatrice did not report the requested LIVE/BYPASS state.");
                Console.WriteLine(requestedBypass ? "BYPASS enabled." : "LIVE enabled.");
                return;
            }

            if (action.SpeakerIndex is null) return;
            if (action.Model is not null)
            {
                var s = await GetStatusAsync(cancellationToken);
                if (!string.Equals(s.CurrentModel, action.Model, StringComparison.OrdinalIgnoreCase))
                {
                    await GetJsonAsync($"set_model?model={Uri.EscapeDataString(action.Model)}", cancellationToken);
                    var deadline = DateTime.UtcNow.AddSeconds(25);
                    do
                    {
                        await Task.Delay(250, cancellationToken);
                        s = await GetStatusAsync(cancellationToken);
                    } while ((!string.Equals(s.CurrentModel, action.Model, StringComparison.OrdinalIgnoreCase)
                              || (ExpectedSpeakerCounts.TryGetValue(action.Model, out var expected) && s.SpeakerCount != expected))
                             && DateTime.UtcNow < deadline);
                    if (!string.Equals(s.CurrentModel, action.Model, StringComparison.OrdinalIgnoreCase))
                        throw new TimeoutException($"Model '{action.Model}' did not finish loading.");
                    if (ExpectedSpeakerCounts.TryGetValue(action.Model, out var expectedCount) && s.SpeakerCount != expectedCount)
                        throw new TimeoutException($"Model '{action.Model}' reported {s.SpeakerCount} voices; expected {expectedCount}.");
                }
            }

            var ready = await GetStatusAsync(cancellationToken);
            if (action.SpeakerIndex is not int voiceIndex || voiceIndex < 0 || voiceIndex >= ready.SpeakerCount)
                throw new ArgumentOutOfRangeException(nameof(action.SpeakerIndex), $"Index {action.SpeakerIndex} is invalid for {ready.CurrentModel} ({ready.SpeakerCount} voices).");
            await GetJsonAsync($"set_config?speaker_index={voiceIndex}", cancellationToken);
            var applyDeadline = DateTime.UtcNow.AddSeconds(3);
            do
            {
                await Task.Delay(25, cancellationToken);
                ready = await GetStatusAsync(cancellationToken);
            } while (ready.SpeakerIndex != voiceIndex && DateTime.UtcNow < applyDeadline);
            if (ready.SpeakerIndex != voiceIndex)
                throw new TimeoutException("Beatrice audio callback did not accept the requested voice.");
            // The upstream callback updates the reported index before its native worker
            // finishes applying embeddings. The REST API exposes no completion acknowledgement.
            // Allow a short settling interval, but report this as a queued request, not a verified switch.
            await Task.Delay(150, cancellationToken);
            Console.WriteLine($"Voice #{voiceIndex + 1} requested for {ready.CurrentModel}; Beatrice reports the new index.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex) { Console.Error.WriteLine($"Action failed: {ex.Message}"); }
        finally { ApiLock.Release(); }
    }

    private static async Task<BackendStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync("status", cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidDataException("Empty Beatrice status response.");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Beatrice status response must be a JSON object.");

        // System.Text.Json otherwise silently leaves missing value-type properties at
        // their defaults (for example, a missing bypass field becomes false).
        if (!root.TryGetProperty("bypass", out var bypass) ||
            (bypass.ValueKind != JsonValueKind.True && bypass.ValueKind != JsonValueKind.False))
            throw new InvalidDataException("Beatrice status is missing a valid 'bypass' boolean.");
        if (!root.TryGetProperty("current_model", out var model) ||
            model.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(model.GetString()))
            throw new InvalidDataException("Beatrice status is missing a valid 'current_model' string.");
        if (!root.TryGetProperty("n_speakers", out var speakers) ||
            !speakers.TryGetInt32(out var speakerCount) || speakerCount < 0)
            throw new InvalidDataException("Beatrice status is missing a valid non-negative 'n_speakers' integer.");
        if (!root.TryGetProperty("speaker_index", out var speaker) ||
            !speaker.TryGetInt32(out var speakerIndex) || speakerIndex < -1)
            throw new InvalidDataException("Beatrice status is missing a valid 'speaker_index' integer.");

        if (speakerCount > 0 && speakerIndex >= speakerCount)
            throw new InvalidDataException($"Beatrice status reports speaker_index {speakerIndex} outside the {speakerCount}-voice model.");

        return JsonSerializer.Deserialize<BackendStatus>(json, JsonOptions)
               ?? throw new InvalidDataException("Empty Beatrice status response.");
    }

    private static async Task GetJsonAsync(string path, CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>(JsonOptions, cancellationToken);
        if (body is null) throw new InvalidDataException("Empty Beatrice API response.");
        if (body.TryGetValue("error", out var error)) throw new InvalidOperationException(error.ToString());
        if (body.TryGetValue("status", out var status) && string.Equals(status.ToString(), "error", StringComparison.OrdinalIgnoreCase))
        {
            var message = body.TryGetValue("message", out var detail) ? detail.ToString()
                : body.TryGetValue("error_message", out var errorMessage) ? errorMessage.ToString()
                : "Beatrice API reported an error.";
            throw new InvalidOperationException(message);
        }
    }

    private static void ValidateConfig(BridgeConfig config)
    {
        if (config.Hotkeys.Count == 0 || config.Hotkeys.Count > 50) throw new InvalidDataException("Configure between 1 and 50 hotkeys.");
        var registeredCombinations = new HashSet<(uint Modifiers, uint Key)>();
        foreach (var (key, action) in config.Hotkeys)
        {
            if (action is null) throw new InvalidDataException($"'{key}' has an empty action.");
            var combination = ParseHotkey(key);
            if (!registeredCombinations.Add(combination))
                throw new InvalidDataException($"'{key}' duplicates another hotkey combination (modifier order does not matter).");
            if (action.ToggleBypass == (action.SpeakerIndex is not null))
                throw new InvalidDataException($"'{key}' must define either toggleBypass or speakerIndex.");
            if (action.ToggleBypass && action.Model is not null)
                throw new InvalidDataException($"'{key}' cannot specify a model for a bypass toggle.");
            if (action.SpeakerIndex is < 0) throw new InvalidDataException($"'{key}' has a negative speakerIndex.");
            if (action.Model is not null && action.Model is not ("jvs" or "official_1" or "old_tts"))
                throw new InvalidDataException($"'{key}' has unsupported model '{action.Model}'.");
            if (action.Model is not null && action.SpeakerIndex is int configuredIndex
                && ExpectedSpeakerCounts.TryGetValue(action.Model, out var modelSpeakerCount)
                && configuredIndex >= modelSpeakerCount)
                throw new InvalidDataException($"'{key}' selects speakerIndex {configuredIndex}, but model '{action.Model}' has only {modelSpeakerCount} voices.");
        }
    }

    private static (uint Modifiers, uint Key) ParseHotkey(string value)
    {
        uint modifiers = 0, key = 0;
        foreach (var part in value.Split('+', StringSplitOptions.TrimEntries))
        {
            switch (part.ToLowerInvariant())
            {
                case "ctrl": case "control":
                    if ((modifiers & ModControl) != 0) throw new InvalidDataException($"'{value}' repeats Ctrl.");
                    modifiers |= ModControl; break;
                case "alt":
                    if ((modifiers & ModAlt) != 0) throw new InvalidDataException($"'{value}' repeats Alt.");
                    modifiers |= ModAlt; break;
                case "shift":
                    if ((modifiers & ModShift) != 0) throw new InvalidDataException($"'{value}' repeats Shift.");
                    modifiers |= ModShift; break;
                default:
                    if (key != 0) throw new InvalidDataException($"'{value}' contains multiple keys.");
                    key = part.ToUpperInvariant() switch
                    {
                        "F1" => 0x70, "F2" => 0x71, "F3" => 0x72, "F4" => 0x73,
                        "F5" => 0x74, "F6" => 0x75, "F7" => 0x76, "F8" => 0x77,
                        "F9" => 0x78, "F10" => 0x79, "F11" => 0x7A, "F12" => 0x7B,
                        "A" => 0x41, "B" => 0x42, "C" => 0x43, "D" => 0x44, "E" => 0x45,
                        "F" => 0x46, "G" => 0x47, "H" => 0x48, "I" => 0x49, "J" => 0x4A,
                        "K" => 0x4B, "L" => 0x4C, "M" => 0x4D, "N" => 0x4E, "O" => 0x4F,
                        "P" => 0x50, "Q" => 0x51, "R" => 0x52, "S" => 0x53, "T" => 0x54,
                        "U" => 0x55, "V" => 0x56, "W" => 0x57, "X" => 0x58, "Y" => 0x59, "Z" => 0x5A,
                        _ => throw new InvalidDataException($"Unsupported key '{part}'. Use Ctrl/Alt/Shift + F1-F12 or A-Z.")
                    };
                    break;
            }
        }
        if (key == 0) throw new InvalidDataException($"'{value}' has no key.");
        if (modifiers == 0) throw new InvalidDataException($"'{value}' must include Ctrl, Alt, or Shift.");
        return (modifiers, key);
    }

    private static string Describe(HotkeyAction a) => a.ToggleBypass ? "toggle LIVE/BYPASS" :
        $"select {a.Model ?? "current model"} voice #{a.SpeakerIndex!.Value + 1}";

    private static void UnregisterAll()
    {
        foreach (var id in RegisteredIds) UnregisterHotKey(IntPtr.Zero, id);
        RegisteredIds.Clear();
        RegisteredActions.Clear();
    }

    [DllImport("user32.dll", SetLastError = true)] private static extern bool RegisterHotKey(IntPtr hWnd, uint id, uint fsModifiers, uint vk);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool UnregisterHotKey(IntPtr hWnd, uint id);
    [DllImport("user32.dll", SetLastError = true)] private static extern int GetMessage(out NativeMessage lpMsg, IntPtr hWnd, uint min, uint max);
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll", SetLastError = true)] private static extern bool PostThreadMessage(uint threadId, uint message, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeMessage { public IntPtr hWnd; public uint message; public IntPtr wParam; public IntPtr lParam; public uint time; public System.Drawing.Point point; }
}

internal sealed class BridgeConfig
{
    [JsonPropertyName("hotkeys")] public Dictionary<string, HotkeyAction> Hotkeys { get; set; } = new();
}
internal sealed class HotkeyAction
{
    [JsonPropertyName("model")] public string? Model { get; set; }
    [JsonPropertyName("speakerIndex")] public int? SpeakerIndex { get; set; }
    [JsonPropertyName("toggleBypass")] public bool ToggleBypass { get; set; }
}
internal sealed class BackendStatus
{
    [JsonPropertyName("bypass")] public bool Bypass { get; set; }
    [JsonPropertyName("current_model")] public string CurrentModel { get; set; } = "";
    [JsonPropertyName("n_speakers")] public int SpeakerCount { get; set; }
    [JsonPropertyName("speaker_index")] public int SpeakerIndex { get; set; }
}
