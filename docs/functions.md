# JTran
   JTran is a .Net Standard Library for doing JSON to JSON transformations.

  <br>

## Function Reference
   Functions are used in expressions to return a value or convert values in some way. All parameters to functions can be expressions.

   <ul>
       <li>[String Functions](#String-Functions)</li>
       <li>[Aggregate/Array Functions](#Aggregate-Functions)</li>
       <li>[Math Functions](#Math-Functions)</li>
       <li>[Date/Time Functions](#Elements)</li>
       <li>[General Purpose Functions](#General-Purpose-Functions)</li>
       <li>[Document Function](#Document-Function)</li>
   </ul>

#### String Functions


##### contains(expr, substr)

Returns true if the given string contains the given substring.

    #(endswith('abc123', 'bc1'))

Result is true<br><br>


##### endswith(expr, substr)

Returns true if the given string ends with the given substring.

    #(endswith('abc123', '123'))

Result is true<br><br>


##### indexof(expr, substr)

Returns the index of the first instance of the given substr. If the given substring does not exist the result will be -1.
    
    #(indexof('abcdefgh', 'cde'))

Result is 2<br><br>


##### normalizespace(expr)

Trims spaces off of the beginning and end and converts multiple instances of spaces into a single space.
    
    #(normalizespace('  ab   c    d  '))

Result is "ab c d"<br><br>


##### string(expr)

Converts value to string. Most places on JTran will interpret a json value as a number even if it's quoted. USe the function to treat the value as a string.

    #(string(1) + string(2) + string(3))

Result is "123"<br><br>


##### stringlength(expr)

Returns the length of the given string.

    #(stringlength('abc123'))

Result is 6<br><br>


##### startswith(expr, substr)

Returns true if the given string starts with the given substring.

    #(startswith('abc123', 'abc'))

Result is true<br><br>


##### substring(expr, start, length)

Returns a portion of the given string starting at given location and returns "length" number of characters. If "length" is omitted it will return the remaining string.

    #(substring('abc123', 3, 2))

Result is "12"

    #(substring('abc123', 3))

Result is "123"<br><br>


##### substringafter(expr, substr)

Returns a portion of the given string found after the given substring.

    #(substringafter('abc123', 'abc'))

Result is "123"<br><br>


##### substringbefore(expr, substr)

Returns a portion of the given string found before the given substring.

    #(substringbefore('abc123', '123'))

Result is "abc"<br><br>


#### Math Functions


##### ceiling(expr)

Rounds up the given val

    #(ceiling(4.3))

Result is 5<br><br>


##### floor(expr)

Rounds down the given val

    #(floor(4.3))

Result is 4<br><br>


##### round(expr)

Rounds off the given val

    #(round(4.3))

Result is 4

    #(round(4.5))

Result is 5<br><br>


#### Aggregate Functions

These functions operate on a list of values

##### avg(expr)

The average of all the values

Given this data:

    {
        Employees
        [
            { 
                Name: "Bob",
                Salary: 1000
            },
            { 
                Name: "Fred",
                Salary: 900
            }
        ]
    }

Then this expression:

    #(avg(Employees.Salary))

Result is 950<br><br>

##### max(expr)

Given this data:

    {
        Employees
        [
            { 
                Name: "Bob",
                Salary: 1000
            },
            { 
                Name: "Fred",
                Salary: 900
            }
        ]
    }

Then this expression:

    #(max(Employees))

Result is 1000<br><br>


##### min(expr)

Given this data:

    {
        Employees
        [
            { 
                Name: "Bob",
                Salary: 1000
            },
            { 
                Name: "Fred",
                Salary: 900
            }
        ]
    }

Then this expression:

    #(min(Employees))

Result is 900<br><br>


##### count(expr)

Given this data:

    {
        Employees
        [
            { 
                Name: "Bob",
                Salary: 1000
            },
            { 
                Name: "Fred",
                Salary: 900
            }
        ]
    }

Then this expression:

    #(count(Employees))

Result is 2<br><br>


##### sum(expr)

The sum of all the values

Using the data from the previous example:

    #(sum(Employees.Salary))

Result is 1900<br><br>


#### General Purpose Functions


##### not(expr)

Returns opposite of a bool expression

    "#(not(true))"

Result is false<br><br>


##### number(expr)

Converts expression to number

    "#(number('55'))"

Result is 55<br><br>


##### position()

Returns the index of the object within an array. Indices are always zero based. If this function is not called in the context of #foreach it will always return a zero:

    "#(position())"

<br>


#### Document Function

##### document(repoName, docName)

This function returns an external document. The first parameter is the name of the document respository and second parameter is the name of the document. Note that these parameters are not expressions but literal values. this function can only be used to set the value of a variable:

    '#variable(Products)':   '#(document(MyDocs, Products))'

You can then access your document from the variable:

    "#($Products.Fruit[Category == 'stone']))"


To give access to JTran to external documents you must implement the <i>IDocumentRepository</i> interface in your code:

    public interface IDocumentRepository
    {
        string GetDocument(string name);
    }

The return value of GetDocument must be a valid JSON document. 

Pass in your document repository into the TransformContext:

    var transformer = new JTran.Transformer(_transform1);
    var context     = new TransformContext { DocumentRepositories = new Dictionary<string, IDocumentRepository> { {"MyDocs", new MyDocRepository() }} };

    return transformer.Transform(_data1, context);


<br>
