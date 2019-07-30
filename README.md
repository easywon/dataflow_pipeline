dataflow_pipeline


7/30/19 -
Basic premise of the multiple output 
- Create buffer blocks
- set Completion to continue until parallel block completes as well. Example: ContinueWith(_ => parallelBlock.Complete());
- set the targets of outputs.

https://social.msdn.microsoft.com/Forums/en-US/0e089f2a-4a78-4760-8452-a4537f942093/best-practise-for-multiple-outputs-from-a-transformmanyblock?forum=tpldataflow