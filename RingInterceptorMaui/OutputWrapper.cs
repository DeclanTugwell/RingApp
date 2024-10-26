using RingInterceptorMaui.Enums;

namespace RingInterceptorMaui
{
    public class OutputWrapper()
    {
        public delegate void WriteToOutput(string textValue, OutputType outputType);
        public event WriteToOutput WriteToOutputEvent = DefaultWriteToOutput;

        public OutputWrapper(WriteToOutput writeToOutputEvent) : this()
        {
            WriteToOutputEvent = writeToOutputEvent;
        }

        public async Task InvokeWriteToOutput(string textValue, OutputType outputType)
        {
            WriteToOutputEvent.Invoke(textValue, outputType);
            await Task.Delay(5);
        }

        private static void DefaultWriteToOutput(string outputText, OutputType outputType)
        {
        }
    }
}
