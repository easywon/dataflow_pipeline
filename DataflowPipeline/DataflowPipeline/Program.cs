using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowPipeline
{
    class Program
    {
        static void Main(string[] args)
        {
            //
            // Create members of the pipeline.
            //

            // Download the requested resource as a string
            var downloadString = new TransformBlock<string, string>(async uri =>
            {
                Console.WriteLine("Downloading '{0}'...", uri);

                return await new HttpClient().GetStringAsync(uri);
            });

            // Separates the specified text into an array of words.
            var createWordList = new TransformBlock<string, string[]>(text =>
            {
                Console.WriteLine("Creating word list... " + text.Length);

                // Remove common punctuation by replacing all non-letter characters 
                // with a space character.
                char[] tokens = text.Select(c => char.IsLetter(c) ? c : ' ').ToArray();
                text = new string(tokens);

                // Separate the text into an array of words.
                return text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            });

            var WordBuffer = new BufferBlock<string>();

            // Removes duplicates.
            var filterWordList = new ActionBlock<string[]>(words =>
            {
                Console.WriteLine("Filtering word list...");

                var wordArray = words
                   .Where(word => word.Length > 8)
                   .Distinct()
                   .ToArray();

                foreach(string w in wordArray)
                {
                    WordBuffer.SendAsync(w);
                }
            });

            // Creates actions for the appropriate buffers
            var printOddWords = new ActionBlock<string>(word =>
            { 
                using (StreamWriter file =
                    new StreamWriter(@"C:\Users\Peter.Lee\Desktop\temp\oddwords.txt", true))
                {
                    file.WriteLine(word);
                }
            });

            var printEvenWords = new ActionBlock<string>(async word =>
            {
                using (StreamWriter file =
                    new StreamWriter(@"C:\Users\Peter.Lee\Desktop\temp\evenwords.txt", true))
                {
                    file.WriteLine(word);
                }
            },
            new ExecutionDataflowBlockOptions
            {
                MaxMessagesPerTask = 5
            });

            //
            // Connect the dataflow blocks to form a pipeline.
            //

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            // Linking the first pipe line.
            downloadString.LinkTo(createWordList, linkOptions);
            createWordList.LinkTo(filterWordList, linkOptions);

            WordBuffer.LinkTo(printOddWords, linkOptions, word => word.Length % 2 == 1);
            WordBuffer.LinkTo(printEvenWords, linkOptions, word => word.Length % 2 == 0);

            // Creating a completion link between original pipeline and two output pipelines
            filterWordList.Completion.ContinueWith(_ => WordBuffer.Complete());


            // Process "The Iliad of Homer" by Homer.
            downloadString.Post("http://www.gutenberg.org/files/6130/6130-0.txt");
            File.WriteAllText(@"C:\Users\Peter.Lee\Desktop\temp\oddwords.txt", string.Empty);
            File.WriteAllText(@"C:\Users\Peter.Lee\Desktop\temp\evenwords.txt", string.Empty);

            // Mark the head of the pipeline as complete.
            downloadString.Complete();          

            // Create a task array to wait for all tasks to finish.
            // Simply writing down Completion.Wait() for all output pipes should suffice?
            Task[] pipelineTask = { printOddWords.Completion, printEvenWords.Completion };
            Task.WaitAll(pipelineTask);

            // Alternate waitall
            Task.WaitAll(printOddWords.Completion, printEvenWords.Completion);

            Console.WriteLine("Done");
        }
    }
}
