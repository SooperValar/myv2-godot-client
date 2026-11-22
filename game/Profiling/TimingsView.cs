using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Hud;

namespace TheLastGate.Profiling;

public partial class TimingsView : PanelContainer
{
    private const double RefreshSeconds = 0.25;
    private const string MonitorCategory = "Timings/";
    private const string PacketMonitorCategory = "Packets/";
    private const string OverlaySection = "profiling.overlay";

    private static readonly string[] SectionHeadings =
        ["Section", "Calls/frame", "Avg ms", "Peak ms", "Avg/tick", "Peak/tick"];

    private static readonly string[] PacketHeadings = ["Sender", "Type", "Per second", "Total"];

    private readonly HashSet<string> monitors = [];
    private readonly Dictionary<Label, TextColour> shownColours = [];
    private IFontSet? fonts;
    private PacketCounter? packets;
    private Func<DebugFacts?>? readFacts;
    private TimingRecorder? recorder;
    private double sinceRefresh = RefreshSeconds;

    public void Watch(
        TimingRecorder watchedRecorder,
        PacketCounter packetCounter,
        Func<DebugFacts?>? factsSource,
        IFontSet fontSet)
    {
        recorder = watchedRecorder;
        packets = packetCounter;
        readFacts = factsSource;
        fonts = fontSet;
    }

    public void Toggle()
    {
        Visible = !Visible;
        sinceRefresh = RefreshSeconds;
    }

    public override void _Process(double delta)
    {
        if (recorder == null || packets == null)
        {
            return;
        }

        var frame = recorder.EndFrame(Stopwatch.GetTimestamp());
        if (HitchLog.Describe(frame) is { } hitch)
        {
            GD.Print(hitch);
        }

        AddMonitors(recorder, frame, packets);
        sinceRefresh += delta;
        if (Visible && sinceRefresh >= RefreshSeconds)
        {
            sinceRefresh = 0;
            var startedAt = Stopwatch.GetTimestamp();
            ShowTimings(recorder, packets);
            recorder.Record(OverlaySection, Stopwatch.GetTimestamp() - startedAt);
        }
    }

    public override void _ExitTree()
    {
        foreach (var monitor in monitors)
        {
            Performance.RemoveCustomMonitor(monitor);
        }

        monitors.Clear();
    }

    private void AddMonitors(TimingRecorder watched, FrameSummary frame, PacketCounter counted)
    {
        AddMonitor(
            MonitorCategory + TimingRecorder.WholeFrameName,
            () => watched.WholeFrame().AverageFrameMilliseconds);
        foreach (var (name, _) in frame.Sections)
        {
            AddMonitor(MonitorCategory + name, () => watched.AverageFrameMilliseconds(name));
        }

        foreach (var sender in counted.Senders)
        {
            AddMonitor(PacketMonitorCategory + sender, () => counted.PerSecond(sender, Stopwatch.GetTimestamp()));
        }
    }

    private void AddMonitor(string monitor, Func<double> read)
    {
        if (monitors.Contains(monitor) || Performance.HasCustomMonitor(monitor))
        {
            return;
        }

        Performance.AddCustomMonitor(monitor, Callable.From(read));
        monitors.Add(monitor);
    }

    private void ShowTimings(TimingRecorder watched, PacketCounter counted)
    {
        var factLines = readFacts?.Invoke() is { } facts ? DebugInformation.Lines(facts) : [];
        var factTexts = new List<(string, TextColour)>();
        foreach (var line in factLines)
        {
            factTexts.Add((line.Text, (TextColour)line.Font));
        }

        Fill(GetNode<Container>("%Facts"), factTexts);
        var cells = new List<(string, TextColour)>();
        foreach (var heading in SectionHeadings)
        {
            cells.Add((heading, TextColour.Yellow));
        }

        foreach (var row in (SectionTiming[])[watched.WholeFrame(), .. watched.Snapshot()])
        {
            cells.Add((row.Name, TextColour.White));
            cells.Add((Number(row.CallsPerFrame), TextColour.White));
            cells.Add((Number(row.AverageFrameMilliseconds), TextColour.White));
            cells.Add((Number(row.PeakFrameMilliseconds), TextColour.White));
            cells.Add((row.AverageTickMilliseconds is { } average ? Number(average) : "", TextColour.White));
            cells.Add((row.PeakTickMilliseconds is { } peak ? Number(peak) : "", TextColour.White));
        }

        Fill(GetNode<Container>("%Sections"), cells);
        var packetCells = new List<(string, TextColour)>();
        foreach (var heading in PacketHeadings)
        {
            packetCells.Add((heading, TextColour.Yellow));
        }

        foreach (var row in counted.Snapshot(Stopwatch.GetTimestamp()))
        {
            packetCells.Add((row.Sender, TextColour.White));
            packetCells.Add((row.Type, TextColour.White));
            packetCells.Add((Number(row.PerSecond), TextColour.White));
            packetCells.Add((row.Total.ToString(CultureInfo.InvariantCulture), TextColour.White));
        }

        Fill(GetNode<Container>("%Packets"), packetCells);
    }

    private void Fill(Container container, List<(string Text, TextColour Colour)> cells)
    {
        while (container.GetChildCount() < cells.Count)
        {
            container.AddChild(new Label { MouseFilter = MouseFilterEnum.Ignore });
        }

        for (var index = 0; index < container.GetChildCount(); index++)
        {
            var label = container.GetChild<Label>(index);
            label.Visible = index < cells.Count;
            if (label.Visible)
            {
                ShowCell(label, cells[index].Text, cells[index].Colour);
            }
        }
    }

    private void ShowCell(Label label, string text, TextColour colour)
    {
        label.Text = text;
        if (fonts == null || (shownColours.TryGetValue(label, out var shown) && shown == colour))
        {
            return;
        }

        shownColours[label] = colour;

        var font = fonts.For(colour);
        label.AddThemeFontOverride("font", font.Font);
        label.AddThemeFontSizeOverride("font_size", font.Size);
        label.AddThemeColorOverride("font_color", font.Tint);
    }

    private static string Number(double value) => value.ToString("F2", CultureInfo.InvariantCulture);
}
