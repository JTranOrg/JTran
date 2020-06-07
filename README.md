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
            var transformer = new JTran.Transformer(_transform1);
            var context     = new TransformContext { Arguments = new Dictionary<string, object>() };

            return transformer.Transform(_data1, context);
        }
    }

    
<br>

### Syntax

JTran is made up of processing instructions. All processing instructions start with a "#". Processing instructions operate on the current object, known as the "scope". The starting scope is the json object passed in to the Transform method. There are two types of processing instructions:<br><br>

- <strong>[Output Expressions](#Output Expressions)</strong> - Output expressions are simply expressions which are evaluated and written to the output document
- <strong>[Elements](#Elements)</strong> - Elements are akin to programming constructs, e.g foreach and if. 

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

This example shows the "dot" syntax that refers to a child object.

A dollar sign "$" refers to a variable (see below) or argument passed into the Transform function (see [Getting Started] above).

    #($Venue)

If the variable refers to an object then the "dot" syntax can be used:

    #($Venue.City)


<br>

#### Elements

Elements are akin to programming constructs, e.g foreach and if. <br><br>

##### #bind

The only purpose of #bind is to change the scope:

###### Transform

    {
        #bind(Driver)
        {
            DriverName:   #(Name)
        }
    }

###### Source Document

    {
        Car:
        {
           Make: "Chevy",
           Model : "Corvette"
        }
        Driver:
        {
           Name: "Joe Smith"
        }
    }

The result is that any instruction inside the #bind block will operate on the contents of the Driver object instead of the root object.<br><br>


##### #foreach

#foreach iterates over an array (or just a single object) and processes the contents of the foreach block changing the scope to that child object. #foreach takes 1 or 2 parameters. The first parameter is the expression to evaluate to return an array of objects. The second parameter is optional and is the name of the output array. If the second parameter is not provided then no array is output and the contents of #foreach are simply output as is.

###### Transform

    {
        #foreach(Cars, Vehicles)
        {
            Make:    #(Make)
            Model:   #(Model)
            Driver:  #(//Driver.Name)
        }
    }

###### Source Document

    {
        Cars:
        [
            {
               Make: "Chevy",
               Model : "Corvette"
            },
            {
               Make: "Pontiac",
               Model : "Firebird"
            }
        }
        Driver:
        {
           Name: "Joe Smith"
        }
    }

###### Output Document

    {
        Vehicles:
        [
            {
               Make: "Chevy",
               Model : "Corvette",
               Driver: "Joe Smith"
            },
            {
               Make: "Pontiac",
               Model : "Firebird",
               Driver: "Joe Smith"
            }
        }
    }
