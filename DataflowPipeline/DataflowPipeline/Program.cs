using System;
using System.Collections.Generic;
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

            // Removes duplicates.
            var filterWordList = new TransformBlock<string[], string[]>(words =>
            {
                Console.WriteLine("Filtering word list...");

                return words
                   .Where(word => word.Length > 8)
                   .Distinct()
                   .ToArray();
            });

            var OddBuffer = new BufferBlock<string>();
            var EvenBuffer = new BufferBlock<string>();

            // Designate proper pipelines for Odd and Even lettered words
            var filterOddEven = new ActionBlock<string[]>(wordArray =>
            {
                Console.WriteLine("Pushing messages to the right pipeline");

                foreach(string word in wordArray)
                {
                    if(word.Length % 2 == 0)
                    {
                        EvenBuffer.Post(word);
                    }
                    else
                    {
                        OddBuffer.Post(word);
                    }
                }
            });

            // Creates actions for the appropriate buffers
            var printOddWords = new ActionBlock<string>(word =>
            {
                Console.WriteLine("Odd word: {0} {1}", word, word.Length);
            });

            var printEvenWords = new ActionBlock<string>(word =>
            {
                Console.WriteLine("Even word: {0} {1}", word, word.Length);
            });

            //
            // Connect the dataflow blocks to form a pipeline.
            //

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            downloadString.LinkTo(createWordList, linkOptions);
            createWordList.LinkTo(filterWordList, linkOptions);
            filterWordList.LinkTo(filterOddEven, linkOptions);

            OddBuffer.LinkTo(printOddWords, linkOptions);
            EvenBuffer.LinkTo(printEvenWords, linkOptions);


            // Process "The Iliad of Homer" by Homer.
            downloadString.Post("http://www.gutenberg.org/files/6130/6130-0.txt");

            // Mark the head of the pipeline as complete.
            downloadString.Complete();

            // Create a task array to wait for all tasks to finish.
            Task[] pipelineTask = {printOddWords.Completion, printEvenWords.Completion};

            Task.WaitAll(pipelineTask);
        }
    }
}
