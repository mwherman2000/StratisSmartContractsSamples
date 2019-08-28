﻿using Stratis.SmartContracts;

public class Player : SmartContract
{
    public Player(ISmartContractState state, Address player, Address opponent, string gameName)
        : base(state)
    {
        PlayerAddress = player;
        Opponent = opponent;
        GameName = gameName;
        State = (uint)StateType.Provisioned;
    }

    public enum StateType : uint
    {
        Provisioned = 0,
        SentPing = 1,
        ReceivedPing = 2,
        Finished = 3
    }

    public uint State
    {
        get => PersistentState.GetUInt32(nameof(State));
        private set => PersistentState.SetUInt32(nameof(State), value);
    }

    public Address Opponent
    {
        get => PersistentState.GetAddress(nameof(Opponent));
        private set => PersistentState.SetAddress(nameof(Opponent), value);
    }

    public Address PlayerAddress
    {
        get => PersistentState.GetAddress(nameof(PlayerAddress));
        private set => PersistentState.SetAddress(nameof(PlayerAddress), value);
    }

    public string GameName
    {
        get => PersistentState.GetString(nameof(GameName));
        private set => PersistentState.SetString(nameof(GameName), value);
    }

    public uint PingsSent
    {
        get => PersistentState.GetUInt32(nameof(PingsSent));
        private set => PersistentState.SetUInt32(nameof(PingsSent), value);
    }

    public uint PingsReceived
    {
        get => PersistentState.GetUInt32(nameof(PingsReceived));
        private set => PersistentState.SetUInt32(nameof(PingsReceived), value);
    }

    public void ReceivePing()
    {
        Assert(Message.Sender == Opponent);
        Assert(State == (uint)StateType.SentPing || State == (uint)StateType.Provisioned);

        State = (uint)StateType.ReceivedPing;

        // We want to overflow the counter here.
        unchecked
        {
            PingsReceived += 1;
        }
    }

    public void SendPing()
    {
        Assert(Message.Sender == PlayerAddress);     
        Assert(State == (uint)StateType.ReceivedPing || State == (uint)StateType.Provisioned);

        var isFinishedResult = Call(Opponent, 0, nameof(Player.IsFinished));

        Assert(isFinishedResult.Success);

        // End the game if the opponent is finished.
        if ((bool)isFinishedResult.ReturnValue)
        {
            State = (uint)StateType.Finished;
            return;
        }

        // We want to overflow the counter here.
        unchecked
        {
            PingsSent += 1;
        }

        var callResult = Call(Opponent, 0, nameof(Player.ReceivePing));
        Assert(callResult.Success);
        State = (uint)StateType.SentPing;
    }

    public bool IsFinished()
    {
        return State == (uint)StateType.Finished;
    }

    public void FinishGame()
    {
        Assert(Message.Sender == PlayerAddress);
        State = (uint)StateType.Finished;
    }
}