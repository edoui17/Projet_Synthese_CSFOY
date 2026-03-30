namespace Core.Interfaces;

using System;
using Core.Utils;

public interface ISignalManager
{
    // === EXAMPLE OF HOW TO IMPLEMENT A SIGNAL ===
    //
    // // 1. Define the strongly-typed WeakEvent property
    // WeakEvent<ScoreChangedEventArgs> OnScoreChanged { get; }
    //
    // // 2. Define the emit method
    // void EmitScoreChanged(object p_sender, int p_newScore);
    //
    // // 3. Define the EventArgs class
    // public class ScoreChangedEventArgs : EventArgs
    // {
    //     public int NewScore { get; }
    //     public ScoreChangedEventArgs(int p_newScore) => NewScore = p_newScore;
    // }

    public class MaterialDestroyedEventArgs : EventArgs
    {
        public string MaterialType { get; }
        public int MaterialQuantity { get; }

        public MaterialDestroyedEventArgs(string p_type, int p_quantity)
        {
            MaterialType = p_type;
            MaterialQuantity = p_quantity;
        }
    }

    WeakEvent<MaterialDestroyedEventArgs> OnMaterialDestroyed { get; }
    void EmitMaterialDestroyed(object p_sender, string p_type, int p_quantity);
}
