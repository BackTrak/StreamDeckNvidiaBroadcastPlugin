namespace com.zaphop.nvidiabroadcast.Entities
{
    internal enum CameraEffectType : uint
    {
        // Computer\HKEY_CURRENT_USER\SOFTWARE\NVIDIA Corporation\NVIDIA Broadcast\Setting
        None = 0xffffffff,
        BackgroundBlur = 0,
        BackgroundReplacement = 1,
        BackgroundRemoval = 2,
        AutoFrame = 3,
        VideoNoiseRemoval = 6,
        EyeContact = 9,
        Vignette = 17
    }

    internal enum MicrophoneEffectType : uint
    {
        None = 3,
        NoiseRemoval = 0,
        RoomEchoRemoval = 1
    }

    internal enum SpeakerEffectType : uint
    {
        None = 3,
        NoiseRemoval = 0,
        RoomEchoRemoval = 1
    }
}
