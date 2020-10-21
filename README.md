# JTran
   JTran is a .Net Standard Library for doing JSON to JSON transformations.

   JTran is heavily influenced by XSLT but whereas XSLT does XML to XML transformations, JTran does JSON to JSON transformations.

### Getting started

#### Installing via NuGet

    Install-Package JTran


A transform is a JSON file that contains JTran processing instructions. To transform a source JSON document you provide the source JSON and the transform:


    public class JTranSample
    {
        public string Transform(string transform, string source)
        {
            var transformer = new JTran.Transformer(transform1);
            var context     = new TransformContext { Arguments = new Dictionary<string, object>() };

            return transformer.Transform(source, context);
        }
    }

    
<br>

### Syntax

JTran is made up of processing instructions. All processing instructions start with a "#". Processing instructions operate on the current object, known as the "scope". The starting scope is the json object passed in to the Transform method. There are two types of processing instructions:<br><br>

- <strong>[Output Expressions](#Output-Expressions)</strong> - Output expressions are simply expressions which are evaluated and written to the output document
    - <strong>[Function Reference](docs/functions.md)</strong> - Functions are called within expressions.
    - <strong>[Extension Functions](docs/extensions.md)</strong> - Adding custom Extension Functions.
- <strong>[Elements](#Elements)</strong> - Elements are akin to programming constructs, e.g foreach and if. 
- <strong>[Templates](#Templates)</strong> - Templates are reusable snippets of JTran code 

<br>

#### Output Expressions

An output expression can be used for a property value:

    {
       Name:   "#(Driver.Name)"
    }

It can also be used for the property name:

    {
       "#(DriverProperty)":   "Bob"
    }

An output expression starts with an "#" and then any expression surrounded by parenthesis:

    #(Driver.Name)

Expressions are based on C-derived languages (C, C++, C#, Java, Javascript) with a few changes. 

Any bare text in the expression refers to an object within the scope:

    #(Driver.Name)

This example shows the "dot" syntax that refers to a child object.<br><br>

##### Variables

A dollar sign "$" refers to a variable (see below) or argument passed into the Transform function (see [Getting Started] above).

    #($Venue)

If the variable refers to an object then the "dot" syntax can be used:

    #($Venue.City)

<br>

##### Current Scope

The current scope is the current object within the source document being evaluated. The '@' symbol refers to that scope

    {
        "#foreach(Cars, Vehicles)":
        {
            Car: "#copyof(@)"
        }
    }

<br>

##### Array Indexers

Indexers can be added to arrays. Note: if an expression expects an array and it evaluates to a single object then that single object is treated as an array with that one object in it. This is true everywhere in JTran:

    #(Venues[1])

If the expression inside the brackets evaluates to an integer then the indexer is treated as a classic array index (note that arrays are zero based). In the example above returns the second object in the Venues array.

If the expression inside the brackets does not evaluate to an integer then it is considered a boolean expression and the indexer acts like a "where" clause. Note the scope inside the brackets will then be on each child object in the array:

    #(Venues[City == 'Los Angeles'])

###### Source Document

    {
        Venues
        [
            {
               Name: "SoCal Race Tracks",
               City: "Los Angeles"
            },
            {
               Name: "West LA Speedway",
               City: "Los Angeles"
            },
            {
               Name: "West Texas Speedway",
               City: "Pecos"
            },
            {
               Name: "East Texas Speedway",
               City: "Pecos"
            }
        ]
    }

Then the expression above would return the first two objects in the array.

You can also combine both types of array indexers:

    #(Venues[City == 'Pecos'][1])

This would return the "East Texas Speedway" object from the example above.


If an expression contains an array in the middle and either there is no indexer or the indexer returns more than one object then the remaining expression is evaluated on each object in that array. Consider this example:

    {
        States:
        [
            {
               Name: "California",
               Region: "Southwest",
               Cities:
               [
                 {
                   Name:  "Los Angeles",
                   Theaters:
                   [
                       {
                          Name: "Wilshire Blvd Drive-In"
                       }
                       {
                          Name: "Century 12 in West LA"
                       }
                   ]
                 }
                 {
                   Name:  "San Francisco",
                   Theaters:
                   [
                       {
                          Name: "Downtown Cinema Plaza"
                       }
                       {
                          Name: "Nob Hill Cinema"
                       }
                   ]
                 }
              ]
            },
            {
               Name: "Washington",
               Region: "Northwest",
               Cities:
               [
                 {
                   Name:  "Seattle",
                   Theaters:
                   [
                       {
                          Name: "Capital Hill Movie Complex"
                       }
                       {
                          Name: "Westlake Cinemas"
                       }
                   ]
                 }
                 {
                   Name:  "Spokane",
                   Theaters:
                   [
                       {
                          Name: "Downtown Cinema Plaza"
                       }
                       {
                          Name: "Westside Theaters"
                       }
                   ]
                 }
              ]
            }
         }
       }
    ]
  }
 
Now given an expression:

    #(States.Cities.Theaters)

The result would be all theaters in all cities in all states.

You can also do indexers in the middle:

    #(States[Region == "Northwest"].Cities.Theaters)

The result would be all the theaters in every city that is in Washington state. 

If the final property in a "dot" list is a simple property then an array of those values are returned:

    #(States[Region == "Northwest"].Cities.Theaters.Name)

The result would be string array with the names of all the theaters in every city that is in Washington state.

##### Ancestor Indicators

Consider the following example source data:

    {
        Driver:
        {
           Name: "Joe Smith"
        },
        Car:
        {
            Make: "Chevy",
            Model: "Corvette",
            Year: 1966
        }
    }

If you were processing this document and the current scope was the Car then:

    #(/Driver)

would return the parent's Driver object and you can then refer to the Driver's properties:

    #(/Driver.Name)

More than one slash can be specified. Each slash refers to one level up the parent hierarchy.

    #(///Driver.Name)


<br>

### Elements

Elements are akin to programming constructs, e.g foreach and if. <br><br>

- <strong>[array](#array)</strong> - Outputs an array 
- <strong>[arrayitem](#arrayitem)</strong> - Outputs an array item
- <strong>[bind](#bind)</strong> - Changes the scope to a new object
- <strong>[calltemplate](#Templates)</strong> - Calls a template
- <strong>[copyof](#copyof)</strong> - Copies on object as-is
- <strong>[else](#else)</strong> - Outputs it's children when previous #if and #elseif do not process
- <strong>[elseif](#elseif)</strong> - Outputs it's children when previous #if and #elseif do not process
- <strong>[foreach](#foreach)</strong> - Iterates over an array 
- <strong>[if](#if)</strong> - Conditionally evaluates it's children 
- <strong>[include](#include)</strong> - Loads an external file 
- <strong>[template](#Templates)</strong> - A reusable snippet of JTran code
- <strong>[variable](#variable)</strong> - Creates a variable 

#### array

Outputs an array. This element allows you to create arrays and use JTran elements within it. You can, of course, output an array using simple json syntax but that would not allow you to use elements like #foreach, and #if/#else

###### Transform

    {
        "#array(Cars)":
        {
            "#arrayitem"
            {
               Make:  "Chevy",
               Model: "Corvette"
            }
        }
    }

###### Output

    {
        Cars:
        [
            {
               Make: "Chevy",
               Model: "Corvette",
            }
        }
    }

Using #foreach

###### Transform

    {
        "#array(Cars)":
        {
            "#foreach(Vehicles, {})" // The "{}" tells foreach to output an object. This is only works within #array
            {
               Make:  "#(Make)",
               Model: "#(Model)"
            }
        }
    }

###### Source

    {
        Vehices:
        [
            {
               Make:  Chevy",
               Model: "Corvette"
            },
            {
               Make:  "Pontiac",
               Model: "Firebird"
            }
        }
    }

###### Output

    {
        Cars:
        [
            {
               Make: "Chevy",
               Model: "Corvette",
            },
            {
               Make:  "Pontiac",
               Model: "Firebird"
            }
        }
    }

#### arrayitem

Outputs an arrayitem within an #array element. See samples above.

Because json doesn't allow more than one property with the same name if you needed to output more than #arrayitem you can add dummy parameters. 

###### Transform

    {
        "#array(Cars)":
        {
            "#arrayitem(1)":
            {
               Make:  "Chevy",
               Model: "Corvette"
            }
            "#arrayitem(2)":
            {
               Make:  "Ford",
               Model: "Cobra"
            }
            "#arrayitem(3)":
            {
               Make:  "Dodge",
               Model: "Challenger"
            }
        }
    }

This gets around the json constraint by making each "property" name be unique. Note that the value of the parameter is ignored.

#### #bind

The only purpose of #bind is to change the scope:

###### Transform

    {
        "#bind(Driver)":
        {
            DriverName:   "#(Name)"
        }
    }

###### Source Document

    {
        Car:
        {
           Make: "Chevy",
           Model: "Corvette"
        },
        Driver:
        {
           Name: "Joe Smith"
        }
    }

The result is that any instruction inside the #bind block will operate on the contents of the Driver object instead of the root object.<br><br>

#### #foreach

#foreach iterates over an array (or just a single object) and processes the contents of the foreach block changing the scope to that child object. #foreach takes 1 or 2 parameters. The first parameter is the expression to evaluate to return an array of objects. The second parameter is optional and is the name of the output array.

###### Transform

    {
        "#foreach(Cars, Vehicles)":
        {
            Make:    "#(Make)",
            Model:   "#(Model)",
            Driver:  "#(//Driver.Name)"
        }
    }

###### Source

    {
        Cars:
        [
            {
               Make:  Chevy",
               Model: "Corvette"
            },
            {
               Make:  "Pontiac",
               Model: "Firebird"
            }
        },
        Driver:
        {
           Name: "Joe Smith"
        }
    }

###### Output

    {
        Vehicles:
        [
            {
               Make: "Chevy",
               Model: "Corvette",
               Driver: "Joe Smith"
            },
            {
               Make: "Pontiac",
               Model: "Firebird",
               Driver: "Joe Smith"
            }
        }
    }

If no array name is specified then no new array is created and contents are output for each child

###### Transform

    {
        "#foreach(Cars)":
        {
            "#(Make)":
            {
                Model:   "#(Model)",
                Driver:  "#(//Driver.Name)"
            }
        }
    }

###### Source Document

    {
        Cars:
        [
            {
               Make:  "Chevy",
               Model: "Corvette"
            },
            {
               Make:  "Pontiac",
               Model: "Firebird"
            }
        ],
        Driver:
        {
           Name: "Joe Smith"
        }
    }

###### Output

    {
        Chevy:
        {
            Model: "Corvette",
            Driver: "Joe Smith"
        },
        Pontiac:
        {
            Model: "Firebird",
            Driver: "Joe Smith"
        }
    }

#### #include

#include provides a way to include JTran code from external files. Templates are the only things that can be in included files.

    {
        "#include":     "mytemplates.json",  // The value cannot be an expression. It must be a hardcoded value

        "#calltemplate(Automobile)":  {}  // Automobile would be a template defined in mytemplates.json
    }

You must specify how the included files are loaded when instantiating the transformer:


    public class JTranSample
    {
        public string Transform(string transform, string source)
        {
            var transformer = new JTran.Transformer(transform1,
                                                    null, 
                                                    new Dictionary<string, string> { "mytemplates.json", "{ ... }"} ); // You can implement your own IDictionary to to do deferred loading, 
                                                                                                                       //     loading from file or cloud storage, etc

            var context = new TransformContext { Arguments = new Dictionary<string, object>() };

            return transformer.Transform(source, context);
        }
    }

#### #if

#if conditionally processes it's contents based on the evaluation of it's expression.

###### Transform

    {
        Driver:      "#(Driver.Name)",

        "#if(Car.Make == 'Chevy')":
        {
            Car:    "#('Chevy ' + Car.Make)"
        }
    }

###### Source

    {
        Driver:
        {
           Name: "Joe Smith"
        },
        Car:
        {
            Make:  "Chevy",
            Model: "Corvette"
        }
    }

###### Output

    {
        Driver: "Joe Smith",
        Car: "Chevy Corvette"
    }

Using the same transform:


###### Source

    {
        Driver:
        {
           Name: "Joe Smith"
        },
        Car:
        {
            Make:  "Pontiac",
            Model: "Firebird"
        }
    }

Would then output this:

###### Output

    {
        Driver: "Joe Smith"
    }

<br>

#### #elseif

#elseif conditionally processes it's contents based on the evaluation of it's expression and only if the previous #if expression evaluated to false.

###### Transform

    {
        Driver:      "#(Driver.Name)",

        "#if(Car.Make == 'Chevy')":
        {
            Car:    "#('Chevy ' + Car.Make)"
        },
        "#elseif(Car.Make == 'Pontiac')":
        {
            Car:    "#('Pontiac ' + Car.Make)"
        }
    }

###### Source

    {
        Driver:
        {
           Name: "Joe Smith"
        },
        Car:
        {
            Car: "Pontiac",
            Model: "Firebird"
        }
    }

###### Output

    {
        Driver: "Joe Smith",
        Car: "Pontiac Firebird"
    }

#### #else

#else processes only if the previous #if and #elseif expressions evaluated to false.

###### Transform

    {
        Driver:      "#(Driver.Name)",

        "#if(Car.Make == 'Chevy')"v
        {
            Car:    "#('Chevy ' + Car.Make)"
        },
        "#elseif(Car.Make == 'Pontiac')":
        {
            Car:    "#('Pontiac ' + Car.Make)"
        },
        "#else":
        {
            Car:    "#(Car.Model + Car.Make)"
        }
    }

###### Source

    {
        Driver:
        {
           Name: "Joe Smith"
        },
        Car:
        {
            Car: "Dodge",
            Model: "Charger"
        }
    }

###### Output

    {
        Driver: "Joe Smith",
        Car: "Dodge Charger"
    }

Because json doesn't allow more than one property with the same name if you needed to output more than #if/#else in the same scope you can simply use #elseif with a different condition that evaluates to true. 

###### Transform
    {
        "#if(Car.Make == 'Chevy')":
        {
            Car:    "#('Chevy ' + Car.Make)"
        },
        "#else": 
        {
            Car:    "#(Car.Model + Car.Make)"
        },

        "#if(Car.Make == 'Audi')":
        {
            Car:    "#('Audi ' + Car.Make)"
        },
        "#elseif(1==1)": // If this was simply #else and then it would clash with the #else above and it would be invalid json
        {
            Car:    "#(Car.Model + Car.Make)"
        }
    }
<br>

#### #variable

#variable allows you to specify a placeholder for data, A variable has a single parameter that is the name of the variable. 

###### Transform

    {
        "#variable(Driver)":    "#(Driver)",

        "#foreach(Cars, Vehicles)":
        {
            Make:    "#(Make)",
            Model:   "#(Model)",
            Driver:  "#($Driver.Name)"
        }
    }

###### Source

    {
        Cars:
        [
            {
               Make: "Chevy",
               Model: "Corvette"
            },
            {
               Make: "Pontiac",
               Model: "Firebird"
            }
        ],
        Driver:
        {
           Name: "Joe Smith"
        }
    }

###### Output

    {
        Vehicles:
        [
            {
               Make: "Chevy",
               Model: "Corvette",
               Driver: "Joe Smith"
            },
            {
               Make: "Pontiac",
               Model: "Firebird",
               Driver: "Joe Smith"
            }
        }
    }

<br>

#### #copyof

#copyof copies the contents of the specified object.

###### Transform

    {
        Car:
        {
            Make:       "#(Make)",
            Model:      "#(Model)",
            Drivetrain: "#copyof(Parts)"
        }
    }

###### Source

    {
        Car:
        {
            Make: "Chevy",
            Model: "Corvette",
            Parts:
            {
                Tires:  "BFR2000-S4",
                Wheels: "Acme AC87-D49S"
            }
        }
    }

###### Output

    {
        Car:
        {
            Make: "Chevy",
            Model: "Corvette",
            Drivetrain:
            {
                Tires:  "BFR2000-S4"
                Wheels: "Acme AC87-D49S"
            }
        }
    }

### Templates

Templates are reusable snippets of JTran code that can be called by other JTran code.

###### Transform

    {
        "#foreach(Cars, Vehicles)":
        {
            "#calltemplate(Automobile)":  {}  
        }

        // Templates must be defined in the root object but can be defined before or after it's use
        "#template(Automobile)":
        {
            "Make":   "#(Make)",
            "Model":  "#(Model)",
            "Active":  true
        }

    }

###### Source Document

    {
        Cars:
        [
            {
               Make:  "Chevy",
               Model: "Corvette"
            },
            {
               Make:  "Pontiac",
               Model: "Firebird"
            }
        ]
    }

###### Output

    {
        Vehicles:
        [
            {
               Make:    "Chevy",
               Model:   "Corvette",
               Active:  true
            },
            {
               Make:    "Pontiac",
               Model:   "Firebird",
               Active:  true
           }
        ]
    }


Templates can be included from external files (see #include):

###### Transform

    {
        "#include":          "mytemplate.json",

        "#foreach(Cars, Vehicles)":
        {
            "#calltemplate(Automobile)":  {}  
        }
    }

You can pass parameters to a template

###### Transform

    {
        "foreach(Cars, Vehicles)":
        {
            "#calltemplate(Automobile)":  
            {
                "Make":   "#(Make)"
                "Model":   "#(Model)"
            }  
        }

        // Templates must be defined in the root object but can be defined before or after it's use
        "#template(Automobile, Make, Model)":
        {
            "Make":   "#($Make)",
            "Model":  "#($Model)",
            "Active":  true
        }
    }
