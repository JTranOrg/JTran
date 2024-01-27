# JTran Language Reference
   JTran is a language for doing JSON to JSON transformations. JTran is itself a JSON schema.

   JTran is heavily influenced by XSLT but whereas XSLT does XML to XML transformations, JTran does JSON to JSON transformations.


### Syntax

JTran is made up of processing instructions. All processing instructions start with a "#". Processing instructions operate on the current object, known as the "scope". The starting scope is the json object passed in to the Transform method.<br><br>

- <strong>[Output Expressions](#Output-Expressions)</strong> - Output expressions are simply expressions which are evaluated and written to the output document
    - <strong>[Function Reference](functions.md)</strong> - Functions are called within expressions.
    - <strong>[Extension Functions](extensions.md)</strong> - Adding custom Extension Functions.
- <strong>[Elements](#Elements)</strong> - Elements are akin to programming constructs, e.g foreach and if. 
- <strong>[Templates](#Templates)</strong> - Templates are reusable snippets of JTran code 

<br>

#### <a id="Output-Expressions">Output Expressions</a>

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

The result would be string array with the names of all the theaters in every city that is in the Northwest region.

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

Note that if the current scope is an array item then the parent is the array itself.

<br>

### <a id="Elements">Elements</a>

Elements are akin to programming constructs, e.g foreach and if. <br><br>

- <strong>[array](#array)</strong> - Outputs an array 
- <strong>[arrayitem](#arrayitem)</strong> - Outputs an array item
- <strong>[bind](#bind)</strong> - Changes the scope to a new object
- <strong>[break](#break)</strong> - Breaks out of the the current iteration in a foreach loop
- <strong>[calltemplate](#Templates)</strong> - Calls a template
- <strong>[catch](#trycatch)</strong> - Part of a try/catch block
- <strong>[copyof](#copyof)</strong> - Copies on object as-is
- <strong>[else](#else)</strong> - Outputs it's children when previous #if and #elseif do not process
- <strong>[elseif](#elseif)</strong> - Outputs it's children when previous #if and #elseif do not process
- <strong>[exclude](#exclude)</strong> - Outputs the specified expression and excludes the listed properties
- <strong>[foreach](#foreach)</strong> - Iterates over an array 
- <strong>[foreachgroup](#foreachgroup)</strong> - Iterates over an array and creates groups of subarrays
- <strong>[if](#if)</strong> - Conditionally evaluates it's children 
- <strong>[iif](#iif)</strong> - Conditionally evaluates it's children 
- <strong>[include](#include)</strong> - Loads an external file 
- <strong>[include (as a property)](#include (as a property))</strong> - Outputs the specified properties of an object 
- <strong>[iterate](#iterate)</strong> - Create an array by looping over the contents
- <strong>[function](#function)</strong> - Create a function in JTran that can be called from expressions
- <strong>[map](#map)</strong> - Maps a set of a keys or expressions to a single output value 
- <strong>[mapitem](#map)</strong> - Maps a key or expression to a single output value 
- <strong>[message](#message)</strong> - Writes a message to the console 
- <strong>[template](#Templates)</strong> - A reusable snippet of JTran code
- <strong>[throw](#throw)</strong> - Throws an exception
- <strong>[try](#trycatch)</strong> - Part of a try/catch block
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
            "#foreach(Vehicles, {})" // The "{}" tells foreach to output a nameless object. This is only works within #array
            {
               Make:  "#(Make)",
               Model: "#(Model)"
            }
        }
    }

###### Source

    {
        Vehicles:
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

You can output single values, e.g. strings, numbers:

###### Transform

    {
        "#array(Cars)":
        {
            "#arrayitem(1)":  "#(Automobiles[0].Make)",
            "#arrayitem(2)":  "#(Automobiles[1].Make)",
            "#arrayitem(3)":  "#(Automobiles[2].Make)"
        }
    }

You can also output single values using a foreach:

###### Transform

    {
        "#array(Cars)":
        {
            "#foreach(Vehicles)":
            {
                "#arrayitem(1)":  "#(Make)",
                "#arrayitem(2)":  "#(Model)"
            }
        }
    }

###### Source

    {
        Vehicles:
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
            "Chevy",
            "Corvette",
            "Pontiac",
            "Firebird"
        }
    }


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

#### #break

Stops the processing of a #foreach:

###### Transform

   {
     "#foreach(Cars, Cars)": 
     {  
       "Make":    "#(Surname)",
       "Model:    "#(Name)", 
       "#break":  "" 
     }
   }

###### Source Document

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
      ]
    }

    ###### Output

    {
      Cars:
      [
        {
           Make: "Chevy",
           Model: "Corvette"
        }
      ]
    }

#### #exclude 

Takes on object and outputs the properties of that object except for those that are listed. If the expression is not an object than a null is returned.


###### Transform

    {
        "Supervisor": "#exclude(Employees[Title == 'Supervisor'], Title)":
    }

###### Source Document

    {
        Employees:
        [
            {
               FirstName:  "Linda",
               LastName:   "Smith",
               Title:      "Supervisor"
            },
            {
               FirstName:  "Jack",
               LastName:   "Ramirez",
               Title:      "Lineman"
            }
        ]
    }

###### Output

    {
        Supervisor:
        {
            FirstName: "Linda",
            LastName:  "Smith"
        }
    }

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


#### #foreachgroup

#foreachgroup iterates over an array and groups them into subarrays

###### Transform

    {
        // Groups cars into subarrays by Make. This will create an array called 'Makes'
        "#foreachgroup(Drivers, Make, Makes)":
        {
            Make: '#(Make)', // Make here is the grouped by value from the foreachgroup parameter

            // currentgroup() returns the list of Cars that belong to the current group (Make)
            '#foreach(currentgroup(), Drivers)':
            {
                Name:  '#(Name)',
                Model: '#(Model)'
            }
        }
    }

###### Source

    {
        Drivers:
        [
            {
                Name:      'John Smith',
                Make:      'Chevy',
                Model:     'Corvette',
            },
            {
                Name:      'Fred Jones',
                Make:      'Pontiac',
                Model:     'Firebird',
            },
            {
                Name:      'Mary Anderson',
                Make:      'Chevy',
                Model:     'Camaro',
            },
            {
                Name:      'Amanda Ramirez',
                Make:      'Pontiac',
                Model:     'GTO',
            },
        ]
    }

###### Output

    {
        Makes:
        [
            {
                Make:      'Chevy',
                Drivers:   
                [
                    {
                        Name:      'John Smith',
                        Model:     'Corvette',
                    },
                    {
                        Name:      'Mary Anderson',
                        Model:     'Camaro',
                    }
                ]
            },
            {
                Make:      'Pontiac',
                Drivers:   
                [
                    {
                        Name:      'Fred Jones',
                        Model:     'Firebird',
                    },
                    {
                        Name:      'Amanda Ramirez',
                        Model:     'GTO',
                    }
                ]
            }
        ]
    }


#### #include

#include provides a way to include JTran code from external files. Templates and functions are the only things that can be in included files.

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

#### #include (as a property)

#include takes on object and outputs only the properties of that object that are listed. If the expression is not an object than a null is returned.

###### Transform

    {
        "Supervisor": #include(Employees[Title == 'Supervisor'], FirstName, LastName)":
    }

###### Source Document

    {
        Employees:
        [
            {
               FirstName:  "Linda",
               LastName:   "Smith",
               Title:      "Supervisor"
            },
            {
               FirstName:  "Jack",
               LastName:   "Ramirez",
               Title:      "Lineman"
            }
        ]
    }

###### Output

    {
        Supervisor:
        {
            FirstName: "Linda",
            LastName:  "Smith"
        }
    }

#### #iterate

#iterate will create an array with the given name and iterate the number of times (derived from the expression) over the contents

###### Transform

    {
        "#iterate(length(Cars), Cars)":
        {
            Index: "#(position() + 1)",
            Make:  "Cars[position()]"
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
            },
            {
               Make:  "Dodge",
               Model: "Charger"
            }
        ]
    }

###### Output

    {
        Cars:
        [
            {
                Index: "1",
                Make:  "Chevy"
            },
            {
                Index: "2",
                Make:  "Pontiac"
            },
            {
                Index: "3",
                Make:  "Dodge"
            }
       ]
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

#### #iif
Evaluates the first parameter as a condition and returns the second parameter if true otherwise the third

    "#iif(7 == 8, 'frank', 'bob'))"

Result is "bob"<br><br>

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

#### <a id="function">#function</a>

#function creates a function using JTran that can be called from any expression.

###### Transform

    {
        "#function(DisplayName, person)": // First parameter is name of function, the rest are the parameters to the function
        {
            // To return a single value, e.g. string, number, etc, create a property named "return"

            return:   "#($person.FirstName + ' ' + $person.LastName)"  // Access parameters using the same syntax as variables
        },
        "Driver":
        {
            Name: "#(DisplayName(Drivers[Winning][0]))"
        },
    }

If no "return" property is created then the whole object is returned

    {
        "#function(DriverInfo, person)": 
        {
            Name:    "#($person.FirstName + ' ' + $person.LastName)",
            Make:    "#($person.Car.Make)",
            Model:   "#($person.Car.Model)"
        },

        "FirstPlace":    "#(DriverInfo(Drivers[Placement == 1])"   
        "SecondPlace":   "#(DriverInfo(Drivers[Placement == 2])"   
        "ThirdPlace":    "#(DriverInfo(Drivers[Placement == 3])"   
    }

###### Output

    {
        FirstPlace:
        {
            Name:   "Bob Jones"
            Make:   "Pontiac"
            Model:  "Firebird"
        },
        SecondPlace:
        {
            Name:   "Mary Kelly"
            Make:   "Chevy"
            Model:  "Camaro"
        },
        ThirdPlace:
        {
            Name:   "Frank Anderson"
            Make:   "Dodge"
            Model:  "Challenger"
        }
    }

#functions can also be placed in #include files. As with #templates #functions are scoped.

#### <a id="throw">#throw</a>

#throw throws an exception (error). When you throw an exception any processing innstructions from the last #try are thrown away. If there is no #try/#catch (see below) then the exception is propagated to your .Net code. An exception of Transformer.UserError will be thrown.

###### Transform

    {
        "#try":
        {
            Make: "Chevy",
              
            "throw":                "This is an error message"                // No Error code
            "throw(123)":           "This is an error message"                // 123 is an error code.
            "throw(ErrorCodes[0])": "#(ErrorMessage[code == ErrorCodes[0]])"  // Use expressions for the message and/or the error code.
        },
        "#catch":
        {
            Make: "Pontiac"
        }
    }

#### <a id="trycatch">#try and #catch</a>

A try/catch is a way to test for exception. If while processing a try block and an exception is thrown then the entire output of the try is throw away and the catch is processed instead. You can have more than one cath and you specifiy conditions on the cacth.

###### Transform

    {
        "#try":
        {
            Make: "Chevy",
              
            "throw(123)": "This is an error message"  // The parameter to the throw is an error code. It is optional.
        },
        "#catch":
        {
            Make: "Pontiac"
        }
    }


###### Output

    {
        Make:    "Pontiac"
    }

Use conditions with more than one #catch

###### Transform

    {
        "#try":
        {
            Make: "Chevy",
              
            "throw(123)": "Chevy has been disqualified"  // The parameter to #throw is an error code. It is optional.
        },
        "#catch(errorcode() == 456)":
        {
            Make: "Pontiac",
        },
        "#catch(errorcode() == 123)": // the errorcode() function returns the error code passed into the #throw
        {
            Make: "Dodge",
            Reason: "#(errormessage())"
        },
        "#catch":
        {
            Make: "Lincoln",
        }
    }


###### Output

    {
        Make:    "Dodge"
        Reason:   "Chevy has been disqualified"
    }

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

#variable can also be an object 

###### Transform

    {
        "#variable(Driver)":    
        {
            "Name": #(Driver.Name),
            "Sponsor": "Mt Dew"
        }

        "#foreach(Cars, Vehicles)":
        {
            Make:    "#(Make)",
            Model:   "#(Model)",
            Driver:  "#($Driver.Name)"
            Sponsor: "#($Driver.Sponsor)"
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
               Driver: "Joe Smith",
               Sponsor: "Mt Dew"
            },
            {
               Make: "Pontiac",
               Model: "Firebird",
               Driver: "Joe Smith",
               Sponsor: "Mt Dew"
            }
        }
    }

#variable can also be an object that evaluates to a single property. The only children where this works are #if/elseif/else and [#map](#map).

###### Transform

    {
        "#variable(Driver)":    
        {
            "#if($DOW == 'Saturday)": #(AlternateDriver.Name),
            "#else": #(Driver.Name),
        }

        "#foreach(Cars, Vehicles)":
        {
            Make:    "#(Make)",
            Model:   "#(Model)",
            Driver:  "#($Driver)"
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
        },
        AlternateDriver:
        {
           Name: "Elena Martinez"
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

#### #map

Maps an input value to an output value. #map can only be used inside a #variable or for a property value.

###### Transform

    {
        "#foreach(Addresses, Addresses)":
        {
            #variable(Region):     
            {   
                #map(State):
                {
                    "#mapitem('CA')": "West",
                    "#mapitem('WA')": "PNW",
                    "#mapitem('OR')": "PNW",
                    "#mapitem('NH')": "New England",
                    "#mapitem('NY')": "East",
                    "#mapitem('MO')": "Midwest",
                    "#mapitem":       "South"
                }
            },

            "Street": "#(Street)
            "City":   "#(City)
            "State":  "#(State)
            "Region": "#($region)
            "Region2": 
            {   
                #map(State):
                {
                    "#mapitem('CA')": "West",
                    "#mapitem('WA')": "PNW",
                    "#mapitem('OR')": "PNW",
                    "#mapitem('NH')": "New England",
                    "#mapitem('NY')": "East",
                    "#mapitem('MO')": "Midwest",
                    "#mapitem":       "South"
                }
            }
        }
    }

###### Source

    {
        Addresses:
        [
            {
               Street: "123 Elm St"
               City:   "Seattle"
               State:  "WA"
            },
            {
               Street: "123 Maple Ave"
               City:   "Concorde"
               State:  "NH"
            }
        ]   
    }

###### Output

    {
        Addresses:
        [
            {
               Street:  "123 Elm St"
               City:    "Seattle"
               State:   "WA",
               Region:  "PNW",
               Region2: "PNW"
            },
            {
               Street:  "123 Maple Ave"
               City:    "Concorde"
               State:   "NH",
               Region:  "New England",
               Region2: "New England"
            }
        ]   
    }

You can also use expressions in the #mapitem but then you'll need to use the scope operator to access the value being mapped. Note that a #mapitem with no key or expression is used as the fallback (if all other keys or expressions fail).

###### Transform

    {
        "#foreach(Addresses, Addresses)":
        {
            #variable(Region):     
            {   
                #map(State):
                {
                  "#mapitem('CA')": "West",
                  "#mapitem(@ == 'WA' && $City == 'Seattle')": "Puget Sound",
                  "#mapitem('WA')": "PNW",
                  "#mapitem('OR')": "PNW",
                  "#mapitem('NH')": "New England",
                  "#mapitem('NY')": "East",
                  "#mapitem('MO')": "Midwest",
                  "#mapitem":       "South"
                }
            }

            "Street": "#(Street)
            "City":   "#(City)
            "State":  "#(State)
            "Region": "#($region)
        }
    }

###### Source

    {
        Addresses:
        [
            {
               Street: "123 Elm St"
               City:   "Seattle"
               State:  "WA"
            },
            {
               Street: "123 Maple Ave"
               City:   "Concorde"
               State:  "NH"
            }
        ]   
    }

###### Output

    {
        Addresses:
        [
            {
               Street: "123 Elm St"
               City:   "Seattle"
               State:  "WA",
               Region:  "Puget Sound"
            },
            {
               Street: "123 Maple Ave"
               City:   "Concorde"
               State:  "NH"
               Region: "New England"
            }
        ]   
    }


### <a id="Templates">Templates</a>

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
        "#include":  "mytemplate.json",

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
