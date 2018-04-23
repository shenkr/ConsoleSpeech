using System;
using System.Globalization;
using System.IO;
using System.Net;
using Microsoft.Speech.Recognition;
using System.Threading;
using NAudio.Wave;

namespace ConsoleSpeech
{
    class Program
    {
        // Indicate whether asynchronous recognition is complete.
        static bool completed;

        static void Main(string[] args)
        {
            Stream ms = new MemoryStream();

            using (WebResponse response = WebRequest.Create("http://localhost/playback.mp3")
                .GetResponse())
            {
                Stream stream = response.GetResponseStream();
                
                byte[] buffer = new byte[response.ContentLength];
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
            }
            ms.Position = 0;

            CultureInfo myCultureInfo = new CultureInfo("en-US");

            using (SpeechRecognitionEngine recognizer =
              new SpeechRecognitionEngine(myCultureInfo))
            {

                // Create a grammar, construct a Grammar object, and load it to the recognizer.

                Choices numbers = new Choices(new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" });
                //Choices numbers = new Choices(new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" });

                GrammarBuilder test = new GrammarBuilder();
                test.Append(numbers);
                test.Culture = myCultureInfo;
                Grammar testing = new Grammar(test);
                testing.Name = "123";

                recognizer.LoadGrammar(testing);
                // Configure the input to the recognizer.
                //recognizer.SetInputToWaveFile(@"C:\test\qweqw.wav");
                recognizer.SetInputToWaveStream(ms);
                // Attach event handlers.
                recognizer.SpeechRecognized +=
                  new EventHandler<SpeechRecognizedEventArgs>(
                    SpeechRecognizedHandler);
                recognizer.RecognizeCompleted +=
                  new EventHandler<RecognizeCompletedEventArgs>(
                    RecognizeCompletedHandler);

                // Perform recognition of the whole file.
                Console.WriteLine("Starting asynchronous recognition...");
                completed = false;
                recognizer.RecognizeAsync(RecognizeMode.Multiple);

                while (!completed)
                {
                    Thread.Sleep(500);
                }
                Console.WriteLine("Done.");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        // Handle the SpeechRecognized event.
        static void SpeechRecognizedHandler(
          object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result != null && e.Result.Text != null)
            {
                Console.WriteLine("  Recognized text = {0}", e.Result.Text);
            }
            else
            {
                Console.WriteLine("  Recognized text not available.");
            }
        }

        // Handle the RecognizeCompleted event.
        static void RecognizeCompletedHandler(
          object sender, RecognizeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Console.WriteLine("  Error encountered, {0}: {1}",
                  e.Error.GetType().Name, e.Error.Message);
            }
            if (e.Cancelled)
            {
                Console.WriteLine("  Operation cancelled.");
            }
            if (e.InputStreamEnded)
            {
                Console.WriteLine("  End of stream encountered.");
            }

            completed = true;
        }
    }
}