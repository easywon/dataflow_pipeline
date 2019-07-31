using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowPipeline
{
    class CustomDataflow
    {
        public class SeparateByLength<Input, Odd, Even> : ITargetBlock<Input>
        {
            private readonly ITargetBlock<Input> m_target;

            public ISourceBlock<Odd> OddSource { get; private set; }
            public ISourceBlock<Even> EvenSource { get; private set; }

            public SeparateByLength(Func <Input, Even, Action<Even>, Odd, Action<Odd>> assign)
            {
                var evenBlock = new BufferBlock<Even>();
                var oddBlock = new BufferBlock<Odd>();
                var actionBlock =
                    new ActionBlock<Input>(input => assign(input, even => evenBlock.Post(even), odd => oddBlock.Post(odd)));

                actionBlock.Completion.ContinueWith(_ => evenBlock.Complete());

                m_target = actionBlock;
                OddSource = oddBlock;
                EvenSource = evenBlock;
            }

            public Task Completion
            {
                get { return Task.WhenAll(OddSource.Completion, EvenSource.Completion); }
            }

            public void Complete()
            {
                m_target.Complete();
            }

            public void Fault(Exception exception)
            {
                m_target.Fault(exception);
            }

            public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, Input messageValue, ISourceBlock<Input> source, bool consumeToAccept)
            {
                return m_target.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
            }
        }
    }
}
