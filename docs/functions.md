# JTran
   JTran is a .Net Standard Library for doing JSON to JSON transformations.

## Document Reference
   Documents are used in expressions to return a value or convert values in some way.

#### String Functions

##### substr

##### string

Converts value to string. Most places on JTran will interpret a json value as a number even if it's quoted. USe the function to treat the value as a string.

    #(string(1) + string(2) + string(3))

Result is "123"

#### Math Functions
