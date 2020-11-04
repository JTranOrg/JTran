# jtrancmd (console app)
   jtrancmd.exe is a console app for doing json to json transformations using the [JTran](../../readme.md) language

Sample call

    jtrancmd /c config.json /t c:\jtran\transforms\transform1.json /s c:\jtran\source\mydata.json /o c:\jtran\outout\newfile.json



### Command Line Reference

    /help -- Show help screen
    /c -- Specify a config file
    /t -- Specify a transform. Must point to a single file.
    /s -- Specify a source document. Can include wildcards. Each source file will be transformed using the transform specified above.
    /o -- Specify an output document.
    /i -- Specify an include folder.
    /d -- Specify a documents folder. When this is used the document source name is "all"

<br><br>

### Config File Schema

A config file can be specified to configure the include and document paths. Note if both a config file and a command line parameter is specified the command line parameter takes precedence.

    {
      "IncludePath":    "C:\\JTran\\Includes",
      "DocumentSources": 
      [
        {
          "Name":   "race",
          "Path":   "C:\\JTran\\RaceDocuments" // These are paths that can contain multiple files. 
        },
        {
          "Name":   "drivers",
          "Path":   "C:\\JTran\\DriverRegistry"
        }
      ]
    }

In the above sample config you would access the files like this:

    {
       "#include":              "racetemplate.json",

        "#variable(races)":     "#(document(race, 'july2020.json'))",
        "#variable(drivers)":   "#(document(drivers, '2020_drivers.json'))",

        ...
    }
