REF 'IO', 'Util' FROM 'Runtime'

Item data = Collect(4, 8, 2, 3, 5, 1, 6, 7, 9, 0)
Item correctData
Repeat 10 [ correctData = correctData.concat(#) ]

If RadixSort(data) = correctData [ Type("Success!") ] [ Type("FAILED") ]

Func GetMax
[
	Item mx = args.0
	Repeat args.num - 1
	[
		Item x = args.(# + 1)
		If x > mx [ mx = x ]
	]
	
	Return mx
]

Func CountSort
[
	Item exp = args.1
	args = args.0
	Item output Repeat args.num [ output = output.concat(0) ]
	Item count Repeat 10 [ count = count.concat(0) ]
	
	Repeat args.num
	[
		count.((args.(#) \ exp) % 10) = count.((args.(#) \ exp) % 10) + 1
	]
	
	Repeat 9 [ count.(# + 1) = count.(# + 1) + count.(#) ]
	
	Repeat args.num
	[
		Item i = args.num - # - 1
		output.(count.((args.(i) \ exp) % 10) - 1) = args.(i)
		count.((args.(i) \ exp) % 10) = count.((args.(i) \ exp) % 10) - 1
	]
	
	Return output
]

Func RadixSort
[
	Item m = GetMax(args)
	Item exp = 1
	
	Repeat
	[
		If m \ exp <= 0 [ Escape ]
		
		args = CountSort(args, exp)
		
		exp = exp * 10
	]
	
	Return args
]