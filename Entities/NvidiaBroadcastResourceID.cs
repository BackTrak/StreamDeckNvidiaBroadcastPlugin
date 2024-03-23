namespace com.zaphop.nvidiabroadcast.Entities
{
    /// <summary>
    /// These IDs are found by looking at the combobox in NVidia Broadcast under the camera effects tab, and then 
    /// searching the string table in Resource Hacker to get the resource ID. These IDs should be static over builds, so they shoulnd't change.
    /// </summary>
    public enum NvidiaBroadcastResourceID
    {
        BackgroundRemoval = 32832, // Background removal
        AutoFrame = 188, // Auto frame
        BackgroundBlur = 186, // Background blur
        BackgroundReplacement = 187, // Background replacement
        EyeContact = 33079, // Eye contact 32873
        Vignette = 33048,
        VideoNoiseRemoval = 33078, // Video noise removal (beta)
        NoiseRemoval = 228, // Noise removal
        RoomEchoRemoval = 33083, // Room echo removal
        Microphone = 32932, // MICROPHONE
        Speakers = 32933, // SPEAKERS
        Camera = 32934 // CAMERA
    }
}
