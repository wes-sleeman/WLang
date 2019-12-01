REF 'IO', 'Math', 'Util' FROM 'Runtime'

If <[ Collect(0, 1, 2, 3, 4, 5) => Pow($, 2) ]> = Collect(0, 1, 4, 9, 16, 25)
[ Type("Success!") ]
[ Type("FAILED") ]