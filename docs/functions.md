# JTran
   JTran is a .Net Standard Library for doing JSON to JSON transformations.

  <br>

## Function Reference
   Functions are used in expressions to return a value or convert values in some way. All parameters to functions can be expressions.

- [String Functions](#String-Functions)
- [Aggregate/Array Functions](#Aggregate-and-Array-Functions)
- [Math Functions](#Math-Functions)
- [Date/Time Functions](#DateTime-Functions)
- [General Purpose Functions](#General-Purpose-Functions)
- [Document Function](#Document-Function)

<br>

#### String Functions

- [contains](#contains)
- [endswith](#endswith)
- [empty](#empty)
- [indexof](#indexof)
- [lowercase](#lowercase)
- [normalizespace](#normalizespace)
- [padleft](#padleft)
- [padright](#padright)
- [remove](#remove)
- [removeany](#removeany)
- [removeanyending](#removeanyending)
- [removeending](#removeending)
- [replace](#replace)
- [replaceending](#replaceending)
- [string](#string)
- [stringlength](#stringlength)
- [startswith](#startswith)
- [substring](#substring)
- [substringafter](#substringafter)
- [substringbefore](#substringbefore)
- [trim](#trim)
- [trimend](#trimend)
- [trimstart](#trimstart)
- [uppercase](#uppercase)

##### <a id="contains">contains</a>(expr, substr)

Returns true if the given string contains the given substring.

    #(contains('abc123', 'bc1'))

Result is true<br><br>


##### <a id="endswith">endswith</a>(expr, substr)

Returns true if the given string ends with the given substring.

    #(endswith('abc123', '123'))

Result is true<br><br>


##### <a id="indexof">indexof</a>(expr, substr)

Returns the index of the first instance of the given substr. If the given substring does not exist the result will be -1.
    
    #(indexof('abcdefgh', 'cde'))

Result is 2<br><br>

##### <a id="lowercase">lowercase</a>(expr)

Converts a string to all lowercase appropriate to the current language.
    
    #(lowercase('BOB'))

Result is 'bob'<br><br>


##### <a id="normalizespace">normalizespace</a>(expr)

Trims spaces off of the beginning and end and converts multiple instances of spaces into a single space.
    
    #(normalizespace('  ab   c    d  '))

Result is "ab c d"<br><br>

##### <a id="padleft">padleft</a>(expr)

Adds additional characters to the left of the given string to fill to the specified length.
    
    #(padleft('abc', 'x', 5))

Result is "xxabc"<br><br>

##### <a id="padright">padright</a>(expr)

Adds additional characters to the right of the given string to fill to the specified length.
    
    #(padright('abc', 'x', 5))

Result is "abcxx"<br><br>

##### <a id="remove">remove</a>(expr)

Searches for all instances of a substring and removes them
    
    #(remove('123abc456', 'abc'))

Result is "123456"<br><br>

##### <a id="removeany">removeany</a>(expr)

Searches for all instances of substrings in a list and removes them
    
    #variable(lists):
    {   
        keywords:
        [
            'abc',
            'cde'
        ]
    },

    #(removeany('123abc456cde789', lists.keywords))

Result is "123456789"<br><br>

##### <a id="removeanyending">removeanyending</a>(expr)

Searches for all instances of substrings in a list tht occur at the end of the string and removes them
    
    #variable(lists):
    {   
        keywords:
        [
            'abc',
            'cde'
        ]
    }

    #(removeanyending('123abc', lists.keywords))

Result is "123"<br><br>

##### <a id="removeending">removeending</a>(expr)

Searches for all instances of a substring if the occur at the end of the string and removes them
    
    #(removeending('123abc', 'abc'))

Result is "123"<br><br>

##### <a id="replace">replace</a>(expr)

Searches for all instances of a substring and replaces them with another string
    
    #(replace('123abc456', 'xyz'))

Result is "123xyz456"<br><br>

##### <a id="replaceending">replaceending</a>(expr)

Searches for all instances of a substring if the occur at the end of the string and replaces them with another string
    
    #(replaceending('123abc', 'abc', 'xyz'))

Result is "123xyz"<br><br>


##### <a id="string">string</a>(expr)

Converts value to string. Most places on JTran will interpret a json value as a number even if it's quoted. USe the function to treat the value as a string.

    #(string(1) + string(2) + string(3))

Result is "123"<br><br>


##### <a id="stringlength">stringlength</a>(expr)

Returns the length of the given string.

    #(stringlength('abc123'))

Result is 6<br><br>


##### <a id="startswith">startswith</a>(expr, substr)

Returns true if the given string starts with the given substring.

    #(startswith('abc123', 'abc'))

Result is true<br><br>


##### <a id="substring">substring</a>(expr, start, length)

Returns a portion of the given string starting at given location and returns "length" number of characters. If "length" is omitted it will return the remaining string.

    #(substring('abc123', 3, 2))

Result is "12"

    #(substring('abc123', 3))

Result is "123"<br><br>


##### <a id="substringafter">substringafter</a>(expr, substr)

Returns a portion of the given string found after the given substring.

    #(substringafter('abc123', 'abc'))

Result is "123"<br><br>


##### <a id="substringbefore">substringbefore</a>(expr, substr)

Returns a portion of the given string found before the given substring.

    #(substringbefore('abc123', '123'))

Result is "abc"<br><br>

##### <a id="trim">trim</a>(expr)

Removes all white space from the beginning and end of a strig

    #(trim('  abc  '))

Result is "abc"<br><br>

##### <a id="trimend">trimend</a>(expr)

Removes all white space from the end of a strig

    #(trimend('  abc  '))

Result is "   abc"<br><br>

##### <a id="trimstart">trimstart</a>(expr)

Removes all white space from the beginning and end of a strig

    #(trimstart('  abc  '))

Result is "abc   "<br><br>

##### <a id="uppercase">uppercase</a>(expr)

Converts a string to all uppercase appropriate to the current language.
    
    #(uppercase('BOB'))

Result is 'bob'<br><br>

#### Math Functions

- [ceiling](#ceiling)
- [floor](#floor)
- [pi](#pi)
- [pow](#pow)
- [precision](#precision)
- [round](#round)
- [sqrt](#sqrt)
- Trigonometric Functions
  - [acos](#acos)
  - [asin](#asin)
  - [atan](#atan)
  - [atan2](#atan2)
  - [cos](#cos)
  - [cosh](#cosh)
  - [sin](#sin)
  - [sinh](#sinh)


##### <a id="ceiling">ceiling</a>(expr)

Rounds up the given val

    #(ceiling(4.3))

Result is 5<br><br>

##### <a id="floor">floor</a>(expr)

Rounds down the given val

    #(floor(4.3))

Result is 4<br><br>

##### <a id="pi">pi</a>(expr)

Returns the value of pi

##### <a id="pow">pow</a>(expr)

Returns the value of the given base to a power

    #(pow(10, 3))

Result is 1000<br><br>

##### <a id="precision">precision</a>(expr)

Reduces the precision of the given decimal value to the the given numbe of decimal places


    #(precision(12.7342, 2))

Result is 12.73

##### <a id="round">round</a>(expr)

Rounds off the given val

    #(round(4.3))

Result is 4

    #(round(4.5))

Result is 5<br><br>

#### Aggregate and Array Functions

These functions operate on a list of values

- [any](#any)
- [avg](#avg)
- [contains](#contains_list)
- [empty](#empty)  
- [first](#first)
- [last](#last)
- [count](#count)
- [min](#min)
- [min](#min)
- [sort](#sort)
- [sum](#sum)

##### <a id="any">any</a>(expr)

Returns true if the given expression is a non-empty list (or a single object). Given this data:

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

    #(any(Employees))

Result is true<br><br>

##### <a id="avg">avg</a>(expr)

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

##### <a id="max">max</a>(expr)

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

##### <a id="min">min</a>(expr)

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

##### <a id="contains_list">contains</a>(expr)

Given this data:

    {
        Employees
        [
            "Bob",
            "Fred"
        ]
    }

Then this expression:

    #(contains(Employees, "Fred"))

Result is true<br><br>

##### <a id="first">first</a>(expr)

Returns the first item in the array. If the array is null or empty then a null will be returned. Given this data:

    {
        Employees
        [
            "Fred",
            "Wilma",
            "Pebbles",
            "Dino"
        ]
    }

Then this expression:

    #(first(Employees))

Result is "Fred"<br><br>

##### <a id="last">last</a>(expr)

Returns the last item in the array. If the array is null or empty then a null will be returned. Given this data:

    {
        Employees
        [
            "Fred",
            "Wilma",
            "Pebbles",
            "Dino"
        ]
    }

Then this expression:

    #(last(Employees))

Result is "Dino"<br><br>

##### <a id="count">count</a>(expr)

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

##### <a id="sort">sort</a>(expr)

Sorts the array

Given this data:

    {
        Employees
        [
            { 
                Name: "Zelda",
                Salary: 900
            },
            { 
                Name: "Fred",
                Salary: 1000
            },
            { 
                Name: "Alan",
                Salary: 900
            }
        ]
    }

Then this expression:

    #(sort(Employees, 'Name', 'asc'))

Result is:

        [
            { 
                Name: "Alan",
                Salary: 900
            },
            { 
                Name: "Fred",
                Salary: 1000
            },
            { 
                Name: "Zelda",
                Salary: 900
            }
        ]

Sort on multiple properties:

    #(sort(Employees, 'Salary', 'desc', 'Name', 'asc'))

Result is:

        [
            { 
                Name: "Fred",
                Salary: 1000
            },
            { 
                Name: "Alan",
                Salary: 900
            },
            { 
                Name: "Zelda",
                Salary: 900
            }
        ]

Note that the last sort field can omit the 'asc' or 'desc', in which case it will default to 'asc':

    #(sort(Employees, 'Name'))

<br>

##### <a id="sum">sum</a>(expr)

The sum of all the values

Using the data from the previous example:

    #(sum(Employees.Salary))

Result is 1900<br><br>

#### DateTime Functions

- Current DateTime
  - [currentdatetime](#currentdatetime)
  - [currentdatetimeutc](#currentdatetimeutc)
  - [currentdateutc](#currentdateutc)
  - [daydex](#daydex)
 - [date](#date)
- Date Arithmetic
  - [addyears](#addyears)
  - [addmonths](#addmonths)
  - [adddays](#adddays)
  - [addhours](#addhours)
  - [addhours](#addhours)
  - [addminutes](#addminutes)
  - [addseconds](#addseconds)
- [formatdatetime](#formatdatetime)
- Date Components
    - [day](#day)
    - [dayofweek](#dayofweek)
    - [dayofweekoccurrence](#dayofweekoccurrence)
    - [hour](#dayofweekoccurrence)
    - [minute](#minute)
    - [month](#month)
    - [second](#second)
    - [year](#year)
<br><br>

##### <a id="currentdatetime">currentdatetime</a>()

Returns the current date and time

    "#(currentdatetime())"

Result is "2020-06-10T11:30:00"<br><br>

##### <a id="currentdate">currentdate</a>()

Returns the current date

    "#(currentdate())"

Result is "2020-06-10"<br><br>

##### <a id="currentdatetimeutc">currentdatetimeutc</a>()

Returns the current UTC date and time 

    "#(currentdatetimeutc())"

Result is "2020-06-10T19:30:00"<br><br>

##### <a id="currentdateutc">currentdateutc</a>()

Returns the current UTC date 

    "#(currentdateutc())"

Result is "2020-06-10"<br><br>

##### <a id="date">date</a>(expr)

Returns the date portion of the given expression 

    "#(date('2020-06-10T19:30:00'))"

Result is "2020-06-10"<br><br>

##### <a id="addyears">addyears</a>(expr, amount)

Returns the date portion of the given expression 

    "#(addyears('2020-06-10T11:30:00', 2))"

Result is "2022-06-10T11:30:00"<br><br>

##### <a id="addmonths">addmonths</a>(expr, amount)

Returns the date portion of the given expression 

    "#(addmonths('2020-06-10T11:30:00', 3))"

Result is "2020-09-10T11:30:00"<br><br>

##### <a id="adddays">adddays</a>(expr, amount)

Returns the date portion of the given expression 

    "#(adddays('2020-06-10T11:30:00', 5))"

Result is "2020-06-15T11:30:00"<br><br>

##### <a id="addhours">addhours</a>(expr, amount)

Returns the date portion of the given expression 

    "#(addhours('2020-06-10T11:30:00', 7))"

Result is "2020-06-15T18:30:00"<br><br>

##### <a id="addminutes">addminutes</a>(expr, amount)

Returns the date portion of the given expression 

    "#(addminutes('2020-06-10T11:30:00', 22))"

Result is "2020-06-10T11:52:00"<br><br>

##### <a id="addseconds">addseconds</a>(expr, amount)

Returns the date portion of the given expression 

    "#(addseconds('2020-06-10T11:30:00', 45))"

Result is "2020-06-10T11:30:45"<br><br>

##### <a id="formatdatetime">formatdatetime</a>(expr, format)

Formats the given datetime using the format string. The format string uses the [standard formatting](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings) and the [custom formatting](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings) defined in the .Net Framework

    "#(formatdatetime('2020-06-10T11:30:00', 'MMM d, yyy'))"

Result is "June 10, 2020"<br><br>

##### <a id="daydex">daydex</a>(expr)

Returns the number of days since Jan 1, 1900

    "#(daydex('2000-07-01T10:00:00'))"

Result is 36706<br><br>

##### <a id="year">year</a>(expr)

Returns the year component of the given date/time

    "#(year('2020-06-10T11:30:00'))"

Result is 2020<br><br>

##### <a id="month">month</a>(expr)

Returns the month component of the given date/time

    "#(month('2020-06-10T11:30:00'))"

Result is 6<br><br>

##### <a id="day">day</a>(expr)

Returns the day component of the given date/time

    "#(day('2020-06-10T11:30:00'))"

Result is 10<br><br>

##### <a id="dayofweek">dayofweek</a>(expr)

Returns the day of week component of the given date/time where Sunday is 0, Monday is 1, etc

    "#(dayofweek('2020-06-10T11:30:00'))"

Result is 3<br><br>

##### <a id="dayofweekoccurrence">dayofweekoccurrence</a>(expr)

Returns the occurrence of day of week component of the given date/time where the first occurrence is 1

    "#(dayofweekoccurrence('2020-06-10T11:30:00'))" 

Result is 2 (June, 1, 2020 is 2nd Wednesday of the month)<br><br>

##### <a id="hour">hour</a>(expr)

Returns the hour component of the given date/time

    "#(hour('2020-06-10T11:30:00'))"

Result is 11<br><br>

##### <a id="minute">minute</a>(expr)

Returns the minute component of the given date/time

    "#(minute('2020-06-10T11:30:00'))"

Result is 30<br><br>

##### <a id="second">second</a>(expr)

Returns the second component of the given date/time

    "#(second('2020-06-10T11:30:45'))"

Result is 45<br><br>

#### General Purpose Functions

  - [document](#document)
  - [coalesce](#coalesce)
  - [coalescenumber](#coalescenumber)
  - [empty](#empty)
  - [errorcode](#errorcode)
  - [errormessage](#errormessage)
  - [iif](#iif)
  - [name](#name)
  - [not](#not)
  - [number](#number)
  - [position](#position)
  - [required](#required)
<br><br>

##### <a id="coalesce">coalesce</a>()

Returns the first parameter that is a non-null, non empty or non whitespace string

    "#(coalesce('', ' ', null, 'bob'))"

Result is "bob"<br><br>

##### <a id="coalescenumber">coalescenumber</a>()

Returns the first parameter that evaluates to a non-zero value. Note that if the value does not evaluate to a number it will be treated as a zero

    "#(coalescenumber('bob', 0, null, 34))"

Result is 34<br><br>

##### <a id="empty">empty</a>()

Returns true if the specified expression is "empty". An expression is empty in the following cases:

* If the property or object does not exist
* If the property or object is null
* If the property is a string and is empty or contains only whitespace
* If the property is a number and the value is zero
* If the item is an object and has no properties
* If the item is an array and the array is empty (length == 0)

##### <a id="errorcode">errorcode</a>()

Returns the error code thrown from a [#throw](reference.md#throw) directive

##### <a id="errormessage">errormessage</a>()

Returns the error message thrown from a [#throw](reference.md#throw) directive

##### <a id="iif">iif</a>()

Evaluates the first parameter as a condition and returns the second parameter if true otherwise the third

    "#(iif(7 == 8, 'frank', 'bob'))"

Result is "bob"<br><br>

##### <a id="name">name</a>()

Returns the name of the current object that is in scope

    {
        "#bind(Driver)":
        {
            DriverField:   "#(name())"
        }
    }

The value of DriverField would be "Driver"<br><br>

##### <a id="not">not</a>(expr)

Returns opposite of a bool expression

    "#(not(true))"

Result is false<br><br>


##### <a id="number">number</a>(expr)

Converts expression to number

    "#(number('55'))"

Result is 55<br><br>


##### <a id="position">position</a>()

Returns the index of the object within an array. Indices are always zero based. If this function is not called in the context of #foreach it will always return a zero:

    "#(position())"

<br>

##### <a id="required">required</a>(expr, message)

If the given expression returns a null, an empty array, an empty string or all whitespace string then the function will throw an exception with the given message, otherwise it will the result of the passed in expression.

    "Name": "#(required(Name, 'Name is required'))"

<br>


#### Document Function

##### <a id="document">document</a>(repoName, docName)

This function returns an external document. The first parameter is the name of the document repository and second parameter is the name of the document. Note that these parameters are not expressions but literal values. this function can only be used to set the value of a variable:

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
