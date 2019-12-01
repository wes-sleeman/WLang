REF 'IO', 'Math', 'Util' FROM 'Runtime'

Item x = <[ Collect(0, 1, 2, 3, 4, 5) => Pow($, 2) ]>
Item y = Collect(0, 1, 4, 9, 16, 25)
If x = y
[ Type("Success!") ]
[ Type("FAILED") ]