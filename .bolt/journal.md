## 2024-05-20 - [AnimationPlayer Track Order Execution Bug]
 **Learning:** Godot evaluates Animation tracks sequentially based on index. When swapping grid sizes during animations, placing the `frame` track before `vframes` and `hframes` tracks can cause 'set_frame: Index p_frame is out of bounds' errors, breaking playback state and stalling code dependent on `AnimationFinished` signals.
 **Action:** Reorder tracks in the `.tscn` file so that `texture`, `hframes`, and `vframes` evaluate before the `frame` track.
