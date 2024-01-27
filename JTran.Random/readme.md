# Jtran.Random
   A set of extension functions for JTran that return random data. Useful for generating test data.

- [pickrandom](#pickrandom)
- [pickrandomunique](#pickrandomunique)
- [randomnumber](#randomnumber)
- [randominteger](#randominteger)


##### <a id="pickrandom">pickrandom</a>(expr)

Selects a single item from the expression/list at random

    #(pickrandom([1, 2, 3]))

Result could be 2<br><br>

  
##### <a id="pickrandomunique">pickrandomunique</a>(expr)

Selects a single item from the expression/list at random but never returns the same item (unless called more than the count of the list). You must pass in a unique name for the list.

    #(pickrandomunique([1, 2, 3], 'numbers'))

Result could be 2<br><br>

    
##### <a id="randomnumber">randomnumber</a>(min, max)

Returns a random floating point number within the given range (inclusive)

    #(randomnumber(10, 100))

Result could be 73.5<br><br>

      
##### <a id="randominteger">randominteger</a>(min, max)

Returns a random integer number within the given range (inclusive)

    #(randominteger(10, 100))

Result could be 73<br><br>

  