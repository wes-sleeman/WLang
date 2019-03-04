REF 'IO', 'Util' FROM 'Runtime'

Item data = Collect(4, 8, 2, 3, 5, 1, 6, 7, 9, 0)
Item correctData
Repeat 10 [ correctData = correctData.concat(#) ]

If Mergesort(data) = correctData [ Type("Success!") ] [ Type("FAILED") ]


Func Mergesort
[
	? An array with a single item is trivially sorted
	If args.num <= 1 [ Return args ]
	
	Item left
	Item right
	
	? Split the array in half
	Repeat args.num
	[
		If # < (args.num / 2)
		[ left = left.concat(args.(#)) ]
		[ right = right.concat(args.(#)) ]
	]
	
	? Recursively sort each half
	left = Mergesort(left)
	right = Mergesort(right)
	
	? Merge the two halves
	Item result
	
	Repeat
	[
		? Break when a list empties
		If left.num = 0 or right.num = 0 [ Escape ]
		
		If left.0 <= right.0
		[
			? Add left.0
			result = result.concat(left.0)
			left = Skip(left)
		]
		[
			? Add right.0
			result = result.concat(right.0)
			right = Skip(right)
		]
	]
	
	? Empty
	Repeat left.num [ result = result.concat(left.(#)) ]
	Repeat right.num [ result = result.concat(right.(#)) ]
	
	Return Result
]

? Returns the array without the first item
Func Skip [ Item out Repeat args.num - 1 [ out = out.concat(args.(# + 1)) ] Return out ]