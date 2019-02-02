REF "Runtime.IO"

Item x = 1
Item y = 1

Type(x)
Repeat 15
[
	Type(y)
	Item z = x + y
	x = y
	y = z
]	