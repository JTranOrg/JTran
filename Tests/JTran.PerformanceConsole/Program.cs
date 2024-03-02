// See https://aka.ms/new-console-template for more information

    //using var input = File.OpenRead($"c:\\Documents\\Testing\\JTran\\largefile_input_200000.json");
    //var transformer  = new JTran.Transformer( "{ 'foreach(@, [])}': { '#noobject': '#copyof(@)' } }" );
    //using var output = new MemoryStream();
    //
    //transformer.Transform(input!, output);

 namespace JTran.PerformanceConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
          Test1();
        }

        static void Test1()
        {        
            using var input = File.OpenRead($"c:\\Documents\\Testing\\JTran\\largefile_input_200000.json");
            var transformer  = new JTran.Transformer( "{ 'foreach(@, [])}': { '#noobject': '#copyof(@)' } }" );
            using var output = new MemoryStream();
                
            transformer.Transform(input!, output);
        }

        static void Test2()
        {
            var list = new List<Automobile> 
            { 
                new Automobile
                {
                    Make  = "Chevy",
                    Model = "Camaro",
                    Year  = 1969,
                    Color = "Blue",
                },
                new Automobile
                {
                    Make  = "Pontiac",
                    Model = "GTO",
                    Year  = 1969,
                    Color = "Blue",
                },
                new Automobile
                {
                    Make  = "Audi",
                    Model = "RS5",
                    Year  = 1969,
                    Color = "Blue",
                },
                new Automobile
                {
                    Make  = "Chevy",
                    Model = "Corvette",
                    Year  = 1969,
                    Color = "Blue",
                },
                new Automobile
                {
                    Make  = "Chevy",
                    Model = "Silverado",
                    Year  = 1969,
                    Color = "Blue",
                }
            };

            var transformer  = new JTran.Transformer( "{ 'foreach(@, [])}': { '#noobject': '#copyof(@)' } }" );
            using var output = new MemoryStream();
            
            transformer.Transform(list, output);
        }

        public class Automobile
        {
            public string  Make     { get; set; } = "";
            public string  Model    { get; set; } = "";
            public int     Year     { get; set; } 
            public string  Color    { get; set; } = "";
        }
    }
}

