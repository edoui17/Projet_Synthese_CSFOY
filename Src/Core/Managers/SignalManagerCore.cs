namespace Core.Managers;

using System;
using Core.Interfaces;
using Core.Utils;

public class SignalManagerCore : ISignalManager
{
    private readonly WeakEvent<ISignalManager.MaterialDestroyedEventArgs> m_onMaterialDestroyed = new WeakEvent<ISignalManager.MaterialDestroyedEventArgs>();
    public WeakEvent<ISignalManager.MaterialDestroyedEventArgs> OnMaterialDestroyed => m_onMaterialDestroyed;

    public void EmitMaterialDestroyed(object p_sender, string p_type, int p_quantity)
    {
        m_onMaterialDestroyed.Invoke(p_sender, new ISignalManager.MaterialDestroyedEventArgs(p_type, p_quantity));
    }

    // === EXAMPLE OF HOW TO IMPLEMENT A SIGNAL IN CORE ===
    //
    // // 1. Instantiate the WeakEvent
    // private readonly WeakEvent<ISignalManager.ScoreChangedEventArgs> m_onScoreChanged = new WeakEvent<ISignalManager.ScoreChangedEventArgs>();
    //
    // // 2. Expose the public getter
    // public WeakEvent<ISignalManager.ScoreChangedEventArgs> OnScoreChanged => m_onScoreChanged;
    //
    // // 3. Implement the emit method
    // public void EmitScoreChanged(object p_sender, int p_newScore)
    // {
    //     m_onScoreChanged.Invoke(p_sender, new ISignalManager.ScoreChangedEventArgs(p_newScore));
    // }
}
