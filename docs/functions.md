# JTran
   JTran is a .Net Standard Library for doing JSON to JSON transformations.

  <br>

## Function Reference
   Functions are used in expressions to return a value or convert values in some way. All parameters to functions can be expressions.

- [String Functions](#String-Functions)
- [Aggregate/Array Functions](#Aggregate-Functions)
- [Math Functions](#Math-Functions)
- [Date/Time Functions](#Elements)
- [General Purpose Functions](#General-Purpose-Functions)
- [Document Function](#Document-Function)

<br>

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


#### Date/Time Functions


##### currentdatetime()

Returns the current date and time

    "#(currentdatetime())"

Result is "2020-06-10T11:30:00"<br>


##### currentdate()

Returns the current date

    "#(currentdate())"

Result is "2020-06-10"<br><br>


##### currentdatetimeutc()

Returns the current UTC date and time 

    "#(currentdatetimeutc())"

Result is "2020-06-10T19:30:00"<br><br>


##### currentdateutc()

Returns the current UTC date 

    "#(currentdateutc())"

Result is "2020-06-10"<br><br>


##### date(expr)

Returns the date portion of the given expression 

    "#(date('2020-06-10T19:30:00'))"

Result is "2020-06-10"<br><br>

##### addyears(expr, amount)

Returns the date portion of the given expression 

    "#(addyears('2020-06-10T11:30:00', 2))"

Result is "2022-06-10T11:30:00"<br><br>

##### addmonths(expr, amount)

Returns the date portion of the given expression 

    "#(addmonths('2020-06-10T11:30:00', 3))"

Result is "2020-09-10T11:30:00"<br><br>

##### adddays(expr, amount)

Returns the date portion of the given expression 

    "#(adddays('2020-06-10T11:30:00', 5))"

Result is "2020-06-15T11:30:00"<br><br>


##### addhours(expr, amount)

Returns the date portion of the given expression 

    "#(addhours('2020-06-10T11:30:00', 7))"

Result is "2020-06-15T18:30:00"<br><br>


##### addminutes(expr, amount)

Returns the date portion of the given expression 

    "#(addminutes('2020-06-10T11:30:00', 22))"

Result is "2020-06-10T11:52:00"<br><br>


##### addseconds(expr, amount)

Returns the date portion of the given expression 

    "#(addseconds('2020-06-10T11:30:00', 45))"

Result is "2020-06-10T11:30:45"<br><br>


##### formatdatetime(expr, format)

Formats the given datetime using the format string. The format string uses the [standard formatting](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings) and the [custom formatting](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings) defined in the .Net Framework

    "#(formatdatetime('2020-06-10T11:30:00', 'MMM d, yyy'))"

Result is "June 10, 2020"<br><br>


##### daydex(expr)

Returns the number of days since Jan 1, 1900

    "#(daydex('2000-07-01T10:00:00'))"

Result is 36706<br><br>


##### year(expr)

Returns the year component of the given date/time

    "#(year('2020-06-10T11:30:00'))"

Result is 2020<br><br>


##### month(expr)

Returns the month component of the given date/time

    "#(month('2020-06-10T11:30:00'))"

Result is 6<br><br>


##### day(expr)

Returns the day component of the given date/time

    "#(day('2020-06-10T11:30:00'))"

Result is 10<br><br>


##### dayofweek(expr)

Returns the day of week component of the given date/time where Sunday is 0, Monday is 1, etc

    "#(dayofweek('2020-06-10T11:30:00'))"

Result is 3<br><br>

##### dayofweekoccurrence(expr)

Returns the occurrence of day of week component of the given date/time where the first occurrence is 1

    "#(dayofweekoccurrence('2020-06-10T11:30:00'))" 

Result is 2 (June, 1, 2020 is 2nd Wednesday of the month)<br><br>


##### hour(expr)

Returns the hour component of the given date/time

    "#(hour('2020-06-10T11:30:00'))"

Result is 11<br><br>


##### minute(expr)

Returns the minute component of the given date/time

    "#(minute('2020-06-10T11:30:00'))"

Result is 30<br><br>


##### second(expr)

Returns the second component of the given date/time

    "#(second('2020-06-10T11:30:45'))"

Result is 45<br><br>


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
