using System;
using System.IO;
using UnityEngine;
// Original deterministic additive synthesis; no recordings, samples or external melodies.
public static class SakuraAudioSynthesis
{
    const int Rate=44100;
    static double F(int midi) => 440*Math.Pow(2,(midi-69)/12.0);
    static double S(double phase) => Math.Sin(2*Math.PI*phase);
    static void Note(float[] data,double start,double length,double hz,double gain,int voice=0) {
        int count=(int)(length*Rate), offset=(int)(start*Rate);
        for(int i=0;i<count;i++) {
            double t=(double)i/Rate;
            double attack=Math.Min(1,t/.008), release=Math.Min(1,(length-t)/.06);
            double wave= voice==0 ? (S(hz*t)+.32*S(hz*2.01*t)+.12*S(hz*3.98*t))*Math.Exp(-t*4) :
                voice==1 ? (S(hz*t+.002*S(5*t))+.15*S(2*hz*t))*Math.Exp(-t*.6) :
                S(hz*t+8*(1-Math.Exp(-t*20)))*Math.Exp(-t*12);
            int index=(offset+i)%data.Length;
            data[index]+=(float)(wave*gain*attack*release);
        }
    }
    static void Write(string path,float[] data) {
        float peak=0; foreach(float value in data) peak=Math.Max(peak,Math.Abs(value));
        float scale=peak>.65f ? .65f/peak : 1;
        using(var writer=new BinaryWriter(File.Create(path))) {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36+data.Length*2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);
            writer.Write(Rate);writer.Write(Rate*2);writer.Write((short)2);writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(data.Length*2);
            foreach(float sample in data) writer.Write((short)(sample*scale*32767));
        }
    }
    public static void Generate() {
        Music("MenuGarden",80,false); Music("SakuraJourney",120,true);
        foreach(SakuraAudio.Cue cue in Enum.GetValues(typeof(SakuraAudio.Cue))) {
            double duration=cue==SakuraAudio.Cue.Victory || cue==SakuraAudio.Cue.Defeat ? 2.2 : cue==SakuraAudio.Cue.Death ? .9 : .45;
            var data=new float[(int)(Rate*duration)]; var random=new System.Random(1200+(int)cue);
            switch(cue) {
                case SakuraAudio.Cue.Pickup: Note(data,0,.22,F(86),.3); Note(data,.1,.3,F(93),.25); break;
                case SakuraAudio.Cue.Click: Note(data,0,.08,900,.22); break;
                case SakuraAudio.Cue.Windup: Note(data,0,.20,440,.3,1); Note(data,.21,.22,660,.32,1); break;
                case SakuraAudio.Cue.Victory: for(int n=0;n<5;n++) Note(data,n*.19,1.2,F(new[]{62,65,69,74,81}[n]),.27); break;
                case SakuraAudio.Cue.Defeat: for(int n=0;n<4;n++) Note(data,n*.3,1.1,F(new[]{62,60,57,50}[n]),.24,1); break;
                case SakuraAudio.Cue.Portal: Note(data,0,.44,523,.25,1); Note(data,.12,.3,784,.2); break;
                default:
                    for(int i=0;i<data.Length;i++) {
                        double t=(double)i/Rate, x=t/duration;
                        double envelope=Math.Min(1,t/.006)*Math.Pow(1-x,3);
                        double noise=random.NextDouble()*2-1;
                        double tone=cue==SakuraAudio.Cue.Jump ? S(360*t+600*t*t) :
                            cue==SakuraAudio.Cue.Death ? S(280*t-115*t*t) :
                            cue==SakuraAudio.Cue.Shot ? S(750*t-650*t*t) : S(110*t+2*(1-Math.Exp(-t*30)));
                        data[i]=(float)((tone*.32+noise*(cue==SakuraAudio.Cue.Attack?.28:.08))*envelope);
                    } break;
            }
            Write("Assets/Audio/SFX/"+cue+".wav",data);
        }
    }
    static void Music(string name,int bpm,bool energetic) {
        double beat=60.0/bpm; var data=new float[(int)Math.Round(64*beat*Rate)];
        int[] tune={74,77,81,77,74,69,70,74, 77,81,84,81,77,74,70,69,
                    74,81,86,84,81,77,74,70, 69,70,74,77,74,70,69,62};
        for(int bar=0;bar<16;bar++) {
            int bass=new[]{50,46,53,48}[bar%4]; double start=bar*4*beat;
            Note(data,start,4*beat,F(bass),.09,1); Note(data,start,3.8*beat,F(bass+12),.045,1);
            for(int n=0;n<4;n++) {
                int note=tune[(bar*2+n/2)%tune.Length]+(n%2==1 && energetic?12:0);
                if(energetic || n%2==0) Note(data,start+n*beat,Math.Min(1.5,3*beat),F(note),energetic?.19:.16);
                if(energetic) { Note(data,start+n*beat,.22,n%2==0?68:110,.17,2); Note(data,start+(n+.5)*beat,.13,420,.035,2); }
            }
        }
        // Wrapped tails make the composed 16-bar phrase loop continuously.
        Write("Assets/Audio/Music/"+name+".wav",data);
    }
}
