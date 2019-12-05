REF 'IO', 'Math', 'Util' FROM 'Runtime'

Item data = Collect(4, 8, 2, 3, 5, 1, 6, 7, 9, 0)
Item correctData
Repeat 10 [ correctData = correctData.concat(#) ]

Item maxdepth = Log(data.num, 2) * 0.5 \ 1

If Introsort(data, maxdepth) = correctData
[ Type("Success!") ]
[ Type("FAILED") ]

Func Introsort
[
	Item maxdepth = args.1
	args = args.0
	
	If args.num <= 1 [ Return args ]
	
	If maxdepth = 0 [ Return Heapsort(args) ]

	Item pivot = args.0

	Item highlist
	Item lowlist

	Repeat args.num - 1
	[
		If args.(# + 1) < pivot
		[ lowlist = lowlist.concat(args.(# + 1)) ]
		[ highlist = highlist.concat(args.(# + 1)) ]
	]
	
	Item retval = Introsort(lowlist, maxdepth - 1).concat(pivot)
	Item h = Introsort(highlist, maxdepth - 1)
	Repeat h.num [ retval = retval.concat(h.(#)) ]
	
	Return retval
]

Func Heapsort
[
	Item i
	Item tmp
	
	args = BuildMaxHeap(args.0)
	Item heapsize = args.num
	
	Repeat heapsize - 1
	[
		i = args.num - # - 1
		tmp = args.(i)
		args.(i) = args.(0)
		args.(0) = tmp
		heapsize = heapsize - 1
		args = MaxHeapify(args, 0, heapsize)
	]
	
	Return args
]

Func BuildMaxHeap
[
	args = args.0
	Repeat args.num \ 2 + 1
	[
		args = MaxHeapify(args, (args.num \ 2) - #, args.num)
	]
	Return args
]

Func MaxHeapify
[
	Item i = args.1
	Item heapsize = args.2
	args = args.0
	
	Item tmp
	Item largest
	Item l = (2 * i) + 1
	Item r = (2 * i) + 2
	
	If l < heapsize
	[
		If args.(l) > args.(i)
		[ largest = l ]
		[ largest = i ]
	]
	[ largest = i ]
	
	If r < heapsize [ If args.(r) > args.(largest) [ largest = r ] ]
	
	If not (largest = i)
	[
		tmp = args.(i)
		args.(i) = args.(largest)
		args.(largest) = tmp
		args = MaxHeapify(args, largest, heapsize)
	]
	
	Return args
]