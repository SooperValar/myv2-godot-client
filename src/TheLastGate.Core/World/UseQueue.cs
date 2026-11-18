using System;
using System.Collections.Generic;
using TheLastGate.Assets;
using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.World;

public enum UseQueueAdd
{
    Ignored,
    Full,
    Queued,
}

// Uses queued with Ctrl+Shift, done one after another: the first is sent at once when the player is idle, and the
// next when the server says the use before it has ended. A queued use stays first until the server starts it.
public sealed class UseQueue(TextLog chat)
{
    public const int MostQueued = 5;

    private const string FullMessage = "Your queue is full.";
    private const int Idle = 0;
    private const int Using = 4;

    private readonly List<(int X, int Y)> queued = [];
    private bool enabled;

    public bool Enabled
    {
        get => enabled;
        set
        {
            enabled = value;
            if (!value)
            {
                queued.Clear();
            }
        }
    }

    public Action<byte[]> Send { get; set; } = _ => { };

    public int Count => queued.Count;

    // 1 for the next use, 0 when the tile isn't queued.
    public int PositionOf(int x, int y) => queued.IndexOf((x, y)) + 1;

    public UseQueueAdd Add(int x, int y, PlayerTarget target)
    {
        if (target.MiscAction == Using && (target.MiscTarget1, target.MiscTarget2) == (x, y))
        {
            return UseQueueAdd.Ignored;
        }

        if (queued.Count >= MostQueued)
        {
            chat.Add(new LogLine(FullMessage, (int)TextColour.Red));
            return UseQueueAdd.Full;
        }

        if (queued.Contains((x, y)))
        {
            return UseQueueAdd.Ignored;
        }

        queued.Add((x, y));
        if (target.MiscAction == Idle && queued.Count == 1)
        {
            SendNext();
        }

        return UseQueueAdd.Queued;
    }

    public void TargetChanged(PlayerTarget previous, PlayerTarget now)
    {
        if (previous.MiscAction == Using)
        {
            if (now.MiscAction != Using)
            {
                SendNext();
            }
        }
        else if (now.MiscAction == Using && queued.Count > 0
            && queued[0] == (now.MiscTarget1, now.MiscTarget2))
        {
            queued.RemoveAt(0);
        }
    }

    // The commands the current client sends through its cmd(): those aimed at a tile, a shop or the smith, the
    // fighting mode, Escape's reset and stat raises.
    public void Sent(byte[] packet)
    {
        if ((ClientCommand)packet[0] is ClientCommand.Move or ClientCommand.Turn or ClientCommand.PickUp
            or ClientCommand.Drop or ClientCommand.Use or ClientCommand.LookItem or ClientCommand.Shop
            or ClientCommand.Smith or ClientCommand.Mode or ClientCommand.Reset or ClientCommand.Stat)
        {
            queued.Clear();
        }
    }

    private void SendNext()
    {
        if (queued.Count > 0)
        {
            Send(MapCommandPackets.ForTile(ClientCommand.Use, queued[0].X, queued[0].Y));
        }
    }
}
