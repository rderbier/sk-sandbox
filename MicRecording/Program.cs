using System;
using StereoKit;

namespace MicRecording
{
    class Program
    {
        private const Int32 MAX_SOUND_LENGTH = 48000 * 5; // 5 seconds of voice
        
        
        static void Main(string[] args)
        {
            float[] micBuffer = new float[MAX_SOUND_LENGTH]; // 5 seconds;
            float[] soundChunk = new float[48000]; // 1 sec max ;
            Int32 micIndex = 0;
            Sound memo = null;
        // Initialize StereoKit
        SKSettings settings = new SKSettings
            {
                appName = "MicRecording",
                assetsFolder = "Assets",
            };
            if (!SK.Initialize(settings))
                Environment.Exit(1);

            Sprite micSprite = Sprite.FromFile("microphone.png", SpriteType.Single);
            Sprite speakerSprite = Sprite.FromFile("speaker.png", SpriteType.Single);
            // Create assets used by the app
            
            Matrix floorTransform = Matrix.TS(0, -1.5f, 0, new Vec3(30, 0.1f, 30));
            Material floorMaterial = new Material(Shader.FromFile("floor.hlsl"));
            floorMaterial.Transparency = Transparency.Blend;
            Pose windowAdminPose = new Pose(-.2f, 0, -0.65f, Quat.LookAt(new Vec3(-.2f, 0, -0.5f), Input.Head.position, Vec3.Up));

            // Core application loop
            while (SK.Step(() =>
            {
                if (SK.System.displayType == Display.Opaque)
                    Default.MeshCube.Draw(floorMaterial, floorTransform);


                UI.WindowBegin("Voice Memo", ref windowAdminPose, new Vec2(25, 0) * U.cm, UIWin.Normal);
                if (Microphone.IsRecording)
                {

                    int unread = Microphone.Sound.UnreadSamples;
                    int samples = 0;
                    if (unread > 0)
                    {
                        samples = Microphone.Sound.ReadSamples(ref soundChunk);
                    }

                    if (UI.ButtonRound("StopRecord", micSprite))
                    {
                        Microphone.Stop();
                        memo = Sound.CreateStream((float)micIndex / 24000f);
                        memo.WriteSamples(micBuffer, micIndex);
                    }
                    UI.SameLine(); UI.Label("Stop recording");


                    if (samples > 0)
                    {
                        // copy the chunk to our main buffer
                        int i = 0;
                        while ((micIndex < MAX_SOUND_LENGTH) && (i < samples))
                        {
                            micBuffer[micIndex] = soundChunk[i];
                            i += 1;
                            micIndex += 1;
                        }

                    }
                    if (micIndex >= MAX_SOUND_LENGTH)
                    {
                        Microphone.Stop();
                        memo = Sound.CreateStream((float)micIndex / 48000f);
                        memo.WriteSamples(micBuffer, micIndex);
                    }
                }
                else // Not Recording
                {
                    if (UI.ButtonRound("StartRecord", micSprite))
                    {
                        micIndex = 0;
                        Microphone.Start();
                    }
                    UI.SameLine(); UI.Label("record a memo");
                    if (memo != null)
                    {
                        if (UI.ButtonRound("Playback", speakerSprite))
                        {
                            memo.Play(Input.Head.position + 0.5f * Input.Head.Forward); // 
                        }
                        UI.SameLine(); UI.Label("Play");
                    }

                }
                UI.WindowEnd();
                
            })) ;
            SK.Shutdown();
        }
    }
}
