namespace TestArgumentsProvider
{
    public class MyArgs : Dictionary<string, object>
    {
        public MyArgs() 
        {
            this.Add("Phrase", "bobs your uncle");
        }
    }
}
