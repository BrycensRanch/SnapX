using Gst;
using SnapX.Core;
using Xdg.Directories;
using Stream = System.IO.Stream;
using Task = System.Threading.Tasks.Task;

namespace SnapX.GTK4;

public class SnapXGTK4 : Core.SnapX
{
    // static long NumSamples;   // Number of samples generated so far (for timestamp generation)

    public override async Task PlaySound(Stream stream)
    {
        // using var pipeline = Pipeline.New("SnapXGTK4Sound");
        // using var appSrc = ElementFactory.Make("appsrc", "source")!;
        // using var decodebin = ElementFactory.Make("decodebin", "decodebin")!;
        // using var audioconvert = ElementFactory.Make("audioconvert", "audioconvert")!;
        // using var audioresample = ElementFactory.Make("audioresample", "audioresample")!;
        // using var autoaudiosink = ElementFactory.Make("autoaudiosink", "autoaudiosink")!;
        //
        // pipeline.Add(appSrc);
        // pipeline.Add(decodebin);
        // pipeline.Add(audioconvert);
        // pipeline.Add(audioresample);
        // pipeline.Add(autoaudiosink);
        //
        //
        // appSrc.Link(decodebin);
        // decodebin.Link(audioconvert);
        // audioconvert.Link(audioresample);
        // audioresample.Link(autoaudiosink);



        // Create a new empty buffer

        // Push data from the stream into appsrc in chunks
        // var buffer = new byte[4096]; // Buffer size for reading the stream
        // while (stream.Read(buffer, 0, buffer.Length) > 0)
        // {
        //     // Create a GStreamer buffer from the byte array
        //     var gstBuffer = Gst.Buffer.NewWrappedBytes(GLib.Bytes.New(buffer));
        //     appSrc.PushBuffer(buffer);
        // }
        // pipeline.SetState(State.Playing);


        var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".flac");
        // TODO: Directly use Stream instead of saving to file
        // Gstreamer is hard, okay?!?! I'm trying to go and see my family.
        await using var tempFileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write);
        // Copy the contents of the input stream to the temporary file
        await stream.CopyToAsync(tempFileStream);
        var escapedFilePath = tempFilePath.Replace(@"\", @"\\").Replace("\"", "\\\"");

        var pipeline =
            $"filesrc location=\"{escapedFilePath}\" ! decodebin ! audioconvert ! audioresample ! autoaudiosink";
        using var ret = Functions.ParseLaunch(pipeline);
        using var bus = ret.GetBus()!;
        ret.SetState(Gst.State.Playing);
        bus.TimedPopFiltered(Gst.Constants.CLOCK_TIME_NONE, MessageType.Eos | MessageType.Error);
        ret.SetState(Gst.State.Null);
    }
}
