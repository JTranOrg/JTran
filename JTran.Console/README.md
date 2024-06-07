# jtrancmd (console app)
   jtrancmd.exe is a console app for doing json to json transformations using the [JTran](https://github.com/JTranOrg/JTran/blob/master/README.md) language

Sample call

    jtrancmd /c config.json /t c:\jtran\transforms\transform1.json /s c:\jtran\source\mydata.json /o c:\jtran\outout\newfile.json



### Command Line Reference

    /help -- Show help screen
    /p -- Specify a project file
    /t -- Specify a transform. Must point to a single file.
    /s -- Specify a source document. Can include wildcards. Each source file will be transformed using the transform specified above.
    /o -- Specify an output document or folder.
    /i -- Specify an include folder.
    /d -- Specify a documents folder. When this is used the document source name is "all"
    /m -- Specify multiple (split) output
    /a -- Specifies an argument provider. Must be in the format <dll_path::class_name>. Class must be derived from IDictionary<string, object> and constructor must have no parameters.

    A project file allows you to specify all of the other items in a single file. 

<br><br>

### Project File Schema

A project file can be specified to spcify all of the input and outputs. All of the items are optional (the transform, source and destination paths are mandatory and must be specified here or on the command line). Note if both a project file and a command line parameter is specified the command line parameter takes precedence.

    {
      "Name":              "Project1",            
      "TransformPath":     "C:\\Documents\\JTran\\Transforms\\Race.jtran",    
      "SourcePath":        "C:\\Documents\\JTran\\Sources\\Races.json",   // Can also specify wildcards here to transform multiple files    
      "DestinationPath":   "C:\\Documents\\JTran\\Tests\\races.json",
      "IncludePaths":    
      {
         "":                "C:\\Documents\\JTran\\Includes" 
      },
      "DocumentPaths":    
      {
         "race":           "C:\\Documents\\JTran\\Documents\\Races" 
         "drivers":        "C:\\Documents\\JTran\\Documents\\Drivers" 
      },

      // Specify any extension libraries
      "ExtensionPaths":    
      [
         "C:\\Documents\\JTraExtensions\\Extensions.dll" 
      ],

      // These are passed as arguments to the transform
      "Arguments": 
      {
         "country":       "US"
      }
    }

In the above sample config you would access the files like this:

    {
        "#include":             "racetemplate.json",

        "#variable(races)":     "#(document(race,    'july2020.json'))",
        "#variable(drivers)":   "#(document(drivers, '2020_drivers.json'))",

        ...
    }
