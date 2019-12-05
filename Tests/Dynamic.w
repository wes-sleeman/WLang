REF 'IO', 'Class' FROM 'Runtime'

Item obj = Dynamic()
P(obj, "TestProp", 3 + 4)

If obj.TestProp = 7
[ Type("Success!") ]
[ Type("FAILED") ]