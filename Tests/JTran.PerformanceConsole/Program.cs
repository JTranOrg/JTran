// See https://aka.ms/new-console-template for more information

    using var input = File.OpenRead($"c:\\Documents\\Testing\\JTran\\largefile_input_200000.json");
    var transformer  = new JTran.Transformer( "{ 'foreach(@, [])}': { '#noobject': '#copyof(@)' } }" );
    using var output = new MemoryStream();

    transformer.Transform(input!, output);
