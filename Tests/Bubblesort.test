REF 'IO', 'Util' FROM 'Runtime'

Item data = Collect(4, 8, 2, 3, 5, 1, 6, 7, 9, 0)
Item correctData
Repeat 10 [ correctData = correctData.concat(#) ]

If Bubblesort(data) = correctData [ Type("Success!") ] [ Type("FAILED") ]

Func Bubblesort
[
	Item n = args.num
	
	Repeat
	[
		Item swapped = False
		
		Repeat n - 1
		[
			If args.(#) > args.(# + 1)
			[
				Item tmp = args.(#)
				args.(#) = args.(# + 1)
				args.(# + 1) = tmp
				swapped = true
			]
		]
		
		n = n - 1
		If not swapped [ Escape ]
	]
	
	Return args
]