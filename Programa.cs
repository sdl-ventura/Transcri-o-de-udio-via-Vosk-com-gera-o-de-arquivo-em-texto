using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using Vosk;
using NAudio.Wave;

namespace Teste
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string modelPath = @"[Diretório onde o Vosk se encontra]";
            string transcriptDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Transcripts");
            Directory.CreateDirectory(transcriptDir);

            string transcriptFile = Path.Combine(transcriptDir, $"transcription_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            var writer = File.AppendText(transcriptFile);

            var model = new Model(modelPath);
            var rec = new VoskRecognizer(model, 16000.0f);

            var waveIn = new WaveInEvent();
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new WaveFormat(16000, 1);
            waveIn.BufferMilliseconds = 1000;

            waveIn.DataAvailable += (sender, a) =>
            {
                if (rec.AcceptWaveform(a.Buffer, a.BytesRecorded))
                {
                    string result = rec.Result();
                    string text = ExtractTextManual(result);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        string entry = $"[{DateTime.Now:HH:mm:ss}] {text}";
                        Console.WriteLine(entry);
                        writer.WriteLine(entry);
                        writer.Flush();
                    }
                }
            };

            Console.WriteLine("Gravando reunião... Pressione qualquer tecla para parar.");
            waveIn.StartRecording();
            Console.ReadKey();
            waveIn.StopRecording();

            string final = ExtractTextManual(rec.FinalResult());
            if (!string.IsNullOrWhiteSpace(final))
            {
                string finalEntry = $"[{DateTime.Now:HH:mm:ss}] {final}";
                writer.WriteLine(finalEntry);
                writer.Flush();
                Console.WriteLine(finalEntry);
            }

            writer.Close();
            Console.WriteLine($"\nTranscrição salva em: {transcriptFile}");
        }

        // Faz um parse simples do JSON para pegar o campo "text"
        private static string ExtractTextManual(string json)
        {
            var match = Regex.Match(json, "\"text\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}
