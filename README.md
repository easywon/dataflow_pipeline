dataflow_pipeline


7/30/19 -
Basic premise of the multiple output 
- Create buffer blocks
- set Completion to continue until parallel output completes as well. Example: ContinueWith(_ => parallelBlock.Complete());
- set the targets of outputs.

https://social.msdn.microsoft.com/Forums/en-US/0e089f2a-4a78-4760-8452-a4537f942093/best-practise-for-multiple-outputs-from-a-transformmanyblock?forum=tpldataflow

Program outputs even and odd numbered words to their respective ActionBlocks.
Needs better understanding of how the process operates.

Look into replicating the output with a different class constructor.
Or look into creating separate pipelines for each of the "outputs".

7/31/19 -
Look into building separate pipelines for each output.
- Relatively easy to build.
- Need to pass the original pipe completion to the next pipe. Example: ContinueWith(_ => newPipeline.Complete());

Attempted to recreate / restructure the custom dataflow block class.
Results seem to point toward either developing a stronger understanding of C# or making better use of the predefined blocks.

Benefits of creating a custom dataflow block -
- Allows for a clean implementation (Strong abstraction)
- potentially more powerful input-output relationships.
- Better readability as the dataflow network grows in size.

Benefits of utilizing separate pipes -
- Stronger distinction between the two dataflow. 
- Easier to implement.
- The interaction between the pipelines are flexible.
- Utilizes the pre-defined blocks. 

<<<<<<< HEAD
Focusing on separate dataflow pipes. Try to create issues/errors with the process and see what happens to the dataflow.

8/1/19 -
Successfully maintained full process completion even with varying levels of delays.
- Utilizing "await async" and Task.Delays

Implemented another way of maintaining multiple outputs.
Send all messages to a single BufferBlock and link to action blocks on a condition.

    LinkCondition must have PropagateCompletion
    Bufferblock.LinkTo(TargetBlock, LinkCondition, anonymous function);
    FilterBlock.ContinueWith(_ => BufferBlock.Complete());

Another way to create multiple pipelines. 
Filtering ON buffer load instead of sending TO buffer.

Implement a logging pipeline that exists across the entire process.
=======
Focusing on separate dataflow pipes. Try to create issues/errors with the process and see what happens to the dataflow.
>>>>>>> single-buffer
